namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順110：教材内の未実施台帳。実CI・現地証跡を自動取得するものではない。</summary>
public static class L110AcceptanceLedger
{
    public sealed record Case(string Id, string State);
    public const string NotRun = "NOT_RUN";
    public const string NotVerified = "NOT_VERIFIED";

    // 正本：docs/course-v3/on-site-acceptance-plan.md。計画作成のみでPASSにしない。
    public static IReadOnlyList<Case> PreCases { get; } =
        Enumerable.Range(1, 7).Select(i => new Case($"PRE-{i:D2}", NotRun)).ToArray();
    public static IReadOnlyList<Case> SiteCases { get; } =
        Enumerable.Range(1, 15).Select(i => new Case($"SITE-{i:D2}", NotRun)).ToArray();

    public static string AssessSelfCheck(bool source, bool ci, bool denials, bool server)
    {
        var missing = new List<string>();
        if (!source) missing.Add("教材とコードの整合");
        if (!ci) missing.Add("自動CIの証跡");
        if (!denials) missing.Add("拒否・競合・監査失敗の検証");
        if (!server) missing.Add("Windows Server 2025/IIS/ADの実機受入");
        return missing.Count > 0
            ? "受入に不足： " + string.Join("、", missing)
            : "自己点検の項目は選択されました。実機・CIの証跡が独立して確認できるまでは本番受入を宣言できません。";
    }
}
