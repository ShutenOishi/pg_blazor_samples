using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習用のDB照合結果。利用者の個人情報やフルアカウント名は画面へ返さない。</summary>
public sealed record WindowsAccessSnapshot(
    string Code,
    string Message,
    string IdentityHint = "なし",
    string DisplayName = "",
    IReadOnlyList<string>? Roles = null,
    IReadOnlyList<string>? Permissions = null)
{
    public bool CanRead => Code == "allowed";
}

/// <summary>
/// Step61〜65：ADは本人認証とWindowsログイン名の提供だけに使い、
/// 既存台帳を投影した読み取り専用ビューから**操作ごとに**権限を確認する。
/// Step66〜70までは既存の一覧・CSV・更新サービスへの適用は未完成。
/// </summary>
public sealed class WindowsUserAccessService(
    AuthenticationStateProvider authenticationState,
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<WindowsUserAccessService> logger)
{
    public const string ReadPermission = "learning.read";

    /// <summary>
    /// 信頼済みのWindows認証ハンドラーが作った主体だけから名前を取る。
    /// DOMAINを落とさず、UPNとの暗黙的な変換や入力値へのフォールバックをしない。
    /// </summary>
    public static string? ReadTrustedLoginName(ClaimsPrincipal principal)
    {
        var identity = principal.Identity;
        if (identity?.IsAuthenticated != true) return null;

        // 別の任意Cookieや任意クレームでは本人を証明できない。
        var scheme = identity.AuthenticationType;
        if (identity is not WindowsIdentity &&
            !string.Equals(scheme, "Negotiate", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(scheme, "Windows", StringComparison.OrdinalIgnoreCase))
            return null;

        var name = identity.Name;
        if (string.IsNullOrWhiteSpace(name) || name.Length > 129)
            return null;

        // この教材はDOMAIN\userを契約とし、それ以外は意図的に拒否する。
        // 記号・UPNなど別表記が必要なら契約と正本のキーを一緒に変更する。
        if (!Regex.IsMatch(name, @"^[A-Za-z0-9._-]{1,64}\\[A-Za-z0-9._$-]{1,64}$",
                RegexOptions.CultureInvariant))
            return null;
        return name.ToUpperInvariant();
    }

    public async Task<WindowsAccessSnapshot> ReadCurrentAsync(CancellationToken cancellationToken = default)
    {
        var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
        if (principal.Identity?.IsAuthenticated != true)
            return new("anonymous", "Windows認証されていません。業務利用は拒否します。");

        var login = ReadTrustedLoginName(principal);
        if (login is null)
            return new("no-login", "認証済みのドメイン付きユーザー名を確認できません。利用を拒否します。");

        var hint = "認証済み（ユーザー名は非表示）";
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL")))
            return new("no-db", "開発用PostgreSQL接続が未設定です。利用を拒否します。", hint);

        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            // ビューに原本の一意制約がなくても重複があれば必ず拒否。全件は読み込まない。
            var matches = await db.ExistingUserAccess.AsNoTracking()
                .Where(x => x.LoginName == login)
                .Take(2)
                .ToListAsync(cancellationToken);

            if (matches.Count == 0)
                return new("unknown", "既存の利用者台帳に該当する記録がありません。利用を拒否します。", hint);
            if (matches.Count != 1)
                return new("ambiguous", "既存の利用者台帳でアカウントが重複しています。利用を拒否します。", hint);

            var user = matches[0];
            if (!user.IsActive)
                return new("inactive", "既存の利用者台帳で無効な利用者です。利用を拒否します。", hint);

            // 現在の既存台帳の権限に基づき、許可コードに投影する。Cookieやcircuitには固定保存しない。
            var permissions = new List<string>();
            if (user.CanRead) permissions.Add(ReadPermission);
            if (user.CanEdit) permissions.Add("learning.edit");
            if (!user.CanRead)
                return new("forbidden", "有効な利用者ですが、この学習操作の閲覧権限がありません。", hint,
                    user.DisplayName, Permissions: permissions);

            return new("allowed", "Windows認証済みユーザー名と既存台帳の有効状態・閲覧権限が成立しました。", hint,
                user.DisplayName, Permissions: permissions);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is Npgsql.NpgsqlException or DbUpdateException or InvalidOperationException)
        {
            // 外部表の停止・権限不足・照合失敗でもフェイルクローズ。秘密とログイン名をログへ出さない。
            logger.LogWarning(ex, "既存利用者台帳の照合に失敗したため、利用を拒否します。");
            return new("db-error", "利用者台帳の照合に失敗しました。利用を拒否します。管理者へ連絡してください。", hint);
        }
    }
}
