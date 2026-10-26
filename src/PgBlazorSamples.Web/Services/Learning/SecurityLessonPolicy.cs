using System.Text.Encodings.Web;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習用のセキュリティ判定。入力はすべて架空の状態であり、認証・認可サービスには使用しない。
/// </summary>
public static class SecurityLessonPolicy
{
    // HTTPの本人確認、チケット、DB台帳、部署、失効、監査条件をすべて確認する模擬判定。
    // 実業務ではSecuredLearningServiceとWindowsUserAccessServiceがDBを照合する。
    public static bool PracticeAllowsOperation(
        bool authenticated,
        bool ticketValid,
        bool activeInLedger,
        bool hasPermission,
        bool sameUnit,
        bool sessionCurrent,
        bool auditAvailable) =>
        authenticated && ticketValid && activeInLedger && hasPermission
        && sameUnit && sessionCurrent && auditAvailable;

    // ブラウザーからの状態変更POSTを題材にした説明用条件。実際のCSRF対策には
    // ASP.NET CoreのAntiForgeryミドルウェアと検証済みトークンを使う。
    public static bool PracticeAcceptsMutation(
        bool https, bool sameOrigin, bool antiforgeryValidated, bool authorized) =>
        https && sameOrigin && antiforgeryValidated && authorized;

    // Razor通常の文字列表示はHTMLエンコードする。MarkupStringは使用しない。
    public static string EncodedExample(string value) => HtmlEncoder.Default.Encode(value);

    // 学習用のセッション失効は期限に加え、DB台帳の認可版変更を必ず検出する。
    public static bool PracticeSessionCurrent(
        DateTimeOffset now,
        DateTimeOffset expiresAt,
        int ticketAuthorizationVersion,
        int currentLedgerVersion,
        bool activeInLedger) =>
        now < expiresAt && ticketAuthorizationVersion == currentLedgerVersion && activeInLedger;

    // 公開チェックリストには秘密の値そのものを返さず、設定の有無だけを返す。
    public static string PracticeConfigState(bool secretPresent, bool sharedKeyStore, bool https) =>
        secretPresent && sharedKeyStore && https ? "模擬チェック：条件あり（実機受入は別）"
        : "模擬チェック：不足条件があります。運用設定を見直してください。";
}
