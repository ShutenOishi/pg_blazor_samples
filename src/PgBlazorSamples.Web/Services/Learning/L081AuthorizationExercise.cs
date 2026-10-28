using System.Security.Claims;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順081専用の隔離したサーバー側演習。架空Principalとメモリー上の架空台帳だけを扱い、
/// 認証ハンドラー、実DB、業務データへの接続は一切行わない。
/// </summary>
public sealed class L081AuthorizationExercise
{
    public const string DemoLogin = @"TRAINING\READER";
    private bool active = true;
    private bool canRead = true;
    public int Checks { get; private set; }

    /// <summary>毎回架空台帳の現行状態を読み直す。画面側に一度の許可を保存しない。</summary>
    public L081ReadOutcome Read(ClaimsPrincipal principal, bool auditAvailable = true)
    {
        Checks++;
        var login = WindowsUserAccessService.ReadTrustedLoginName(principal);
        if (login is null)
            return new(false, 0, "架空主体の認証方式またはログイン名を確認できないため拒否。");
        if (!string.Equals(login, DemoLogin, StringComparison.Ordinal))
            return new(false, 0, "架空台帳に一致する利用者がいないため拒否。");
        if (!active || !canRead)
            return new(false, 0, "架空台帳の有効性または読取権限が失効したため拒否。");
        if (!auditAvailable)
            return new(false, 0, "架空監査の条件を満たさないため拒否。");
        return new(true, 1, "架空台帳の現行読取権限を確認。固定の架空課題1件を許可。");
    }

    public void RevokeRead() => canRead = false;
    public void Reset() { active = true; canRead = true; Checks = 0; }
    public void DisableAccount() => active = false;
}

/// <summary>件数は架空課題のみ。実DBの結果や業務行は含まない。</summary>
public sealed record L081ReadOutcome(bool Allowed, int RowCount, string Message);
