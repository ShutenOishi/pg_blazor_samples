using System.Security.Claims;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順085専用。架空Principalとメモリー内の架空台帳だけで毎操作再認可を実演する。
/// circuitの再接続は認証チケット・台帳の復活を意味しない。業務DB、実Cookie、ADは操作しない。
/// </summary>
public sealed class L085ReauthorizationExercise
{
    private int currentLedgerVersion = 2;
    private bool ledgerActive = true;
    private bool canRead = true;
    public int Checks { get; private set; }
    public int Reconnects { get; private set; }

    /// <summary>再接続したという架空の事実だけを記録。権限・Cookie期限を変更しない。</summary>
    public void SimulateReconnect() => Reconnects++;

    /// <summary>サーバー側サービスが毎操作で現在の架空台帳を再評価し、拒否時は件数を返さない。</summary>
    public L085ReadOutcome Read(
        ClaimsPrincipal principal, DateTimeOffset now, DateTimeOffset expiresAt,
        int ticketAuthorizationVersion = 2, bool auditAvailable = true)
    {
        Checks++;
        var login = WindowsUserAccessService.ReadTrustedLoginName(principal);
        if (!string.Equals(login, L081AuthorizationExercise.DemoLogin, StringComparison.Ordinal))
            return new(false, 0, "架空主体の信頼名を検証できないため拒否。");
        if (!SecurityLessonPolicy.PracticeSessionCurrent(now, expiresAt,
                ticketAuthorizationVersion, currentLedgerVersion, ledgerActive))
            return new(false, 0, "架空チケットの期限、台帳の失効または権限版の不一致により拒否。");
        if (!canRead)
            return new(false, 0, "架空台帳の現在の読取権限がないため拒否。");
        if (!auditAvailable)
            return new(false, 0, "架空監査が成立しないため拒否。");
        return new(true, 1, "架空台帳の現在の読取権限を再確認し、固定の架空課題1件を許可。");
    }

    public void RevokeRead()
    {
        canRead = false;
        currentLedgerVersion++;
    }

    public void DisableAccount() => ledgerActive = false;

    public void Reset()
    {
        currentLedgerVersion = 2;
        ledgerActive = true;
        canRead = true;
        Checks = 0;
        Reconnects = 0;
    }
}

/// <summary>実業務行は含まない。拒否結果では必ず0件。</summary>
public sealed record L085ReadOutcome(bool Allowed, int RowCount, string Message);
