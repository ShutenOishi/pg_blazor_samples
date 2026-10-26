using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習用の認証・DB照合結果。SIDの完全な値は画面に返さない。</summary>
public sealed record WindowsAccessSnapshot(
    string Code,
    string Message,
    string SidHint = "なし",
    string DisplayName = "",
    IReadOnlyList<string>? Roles = null,
    IReadOnlyList<string>? Permissions = null)
{
    public bool CanRead => Code == "allowed";
}

/// <summary>
/// Step61〜65：ASP.NET Coreが認証した主体だけを信頼し、DBで毎回有効性と権限を判定する。
/// 既存の業務データサービスに適用するのはStep66〜70。これは認可の完成を意味しない。
/// </summary>
public sealed class WindowsUserAccessService(
    AuthenticationStateProvider authenticationState,
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<WindowsUserAccessService> logger)
{
    public const string ReadPermission = "learning.read";

    /// <summary>画面入力やHTTPヘッダーの値をSIDとして受け取らない。</summary>
    public static string? ReadTrustedSid(ClaimsPrincipal principal)
    {
        var identity = principal.Identity;
        if (identity?.IsAuthenticated != true) return null;
        // SIDはWindows認証ハンドラーから渡された主体に限る。ユーザー名に代替しない。
        var scheme = identity.AuthenticationType;
        if (identity is not WindowsIdentity &&
            !string.Equals(scheme, "Negotiate", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(scheme, "Windows", StringComparison.OrdinalIgnoreCase))
            return null;

        string? sid = principal.FindFirst(ClaimTypes.PrimarySid)?.Value;
        if (identity is WindowsIdentity windowsIdentity)
        {
            // WindowsIdentityのOS検証済みSIDを優先する。
            sid = windowsIdentity.User?.Value;
        }

        if (sid is null || sid.Length > 184 ||
            !Regex.IsMatch(sid, @"^S-1-[0-9]{1,12}(?:-[0-9]{1,12}){2,14}$", RegexOptions.CultureInvariant))
            return null;
        return sid.ToUpperInvariant();
    }

    public async Task<WindowsAccessSnapshot> ReadCurrentAsync(CancellationToken cancellationToken = default)
    {
        var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
        if (principal.Identity?.IsAuthenticated != true)
            return new("anonymous", "Windows認証されていません。業務利用は拒否します。");

        var sid = ReadTrustedSid(principal);
        if (sid is null)
            return new("no-sid", "Windows認証済みのSIDを確認できません。利用を拒否します。");

        // SIDそのものは画面上の表示・ログに残さない。
        var hint = "末尾 " + sid[^Math.Min(4, sid.Length)..];
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL")))
            return new("no-db", "開発用PostgreSQL接続が未設定です。利用を拒否します。", hint);

        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var user = await db.AppUsers.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Sid == sid, cancellationToken);
            if (user is null)
                return new("unknown", "アプリの利用者台帳に登録がありません。利用を拒否します。", hint);
            if (!user.IsActive)
                return new("inactive", "アプリの利用者が無効です。利用を拒否します。", hint);

            // ロールと許可は操作ごとにDBから再取得。Interactive Server回線へ長期キャッシュしない。
            var roles = await (
                from link in db.AppUserRoles.AsNoTracking()
                join role in db.AppRoles.AsNoTracking() on link.RoleId equals role.Id
                where link.UserId == user.Id && role.IsActive
                orderby role.Code
                select new { role.Id, role.Name }).ToListAsync(cancellationToken);
            var ids = roles.Select(x => x.Id).ToArray();
            var rights = await (
                from link in db.AppRolePermissions.AsNoTracking()
                join permission in db.AppPermissions.AsNoTracking() on link.PermissionId equals permission.Id
                where ids.Contains(link.RoleId)
                orderby permission.Code
                select permission.Code).Distinct().ToListAsync(cancellationToken);

            if (!rights.Contains(ReadPermission, StringComparer.Ordinal))
                return new("forbidden", "利用者は有効ですが、この学習操作の権限がありません。", hint,
                    user.DisplayName, roles.Select(x => x.Name).ToArray(), rights);

            return new("allowed", "Windows認証・SID照合・利用者有効性・読み取り権限が成立しました。", hint,
                user.DisplayName, roles.Select(x => x.Name).ToArray(), rights);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is Npgsql.NpgsqlException or DbUpdateException or InvalidOperationException)
        {
            // DB停止・権限不足・モデル不整合・重複などもフェイルクローズ。
            logger.LogWarning(ex, "Windows本人照合のDB確認に失敗しました。アクセスを拒否します。");
            return new("db-error", "DB照合に失敗しました。利用を拒否します。管理者へ連絡してください。", hint);
        }
    }
}
