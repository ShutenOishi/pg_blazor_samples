using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L110AcceptanceLedgerTests
{
    [Fact]
    public void PreAndSiteCases_AllRemainNotRun_WithNoInferredPass()
    {
        Assert.Equal(7, L110AcceptanceLedger.PreCases.Count);
        Assert.Equal(15, L110AcceptanceLedger.SiteCases.Count);
        Assert.Equal(Enumerable.Range(1, 7).Select(i => $"PRE-{i:D2}"),
            L110AcceptanceLedger.PreCases.Select(x => x.Id));
        Assert.Equal(Enumerable.Range(1, 15).Select(i => $"SITE-{i:D2}"),
            L110AcceptanceLedger.SiteCases.Select(x => x.Id));
        Assert.All(L110AcceptanceLedger.PreCases, x => Assert.Equal("NOT_RUN", x.State));
        Assert.All(L110AcceptanceLedger.SiteCases, x => Assert.Equal("NOT_RUN", x.State));
        Assert.DoesNotContain(L110AcceptanceLedger.SiteCases, x => x.State == "PASS");
    }

    [Fact]
    public void AllSelfCheckBoxesChecked_DoNotCreateEvidenceOrAcceptSite()
    {
        var result = L110AcceptanceLedger.AssessSelfCheck(true, true, true, true);
        Assert.Contains("本番受入を宣言できません", result);
        Assert.All(L110AcceptanceLedger.SiteCases, x => Assert.Equal("NOT_RUN", x.State));
        Assert.Equal("NOT_VERIFIED", L110AcceptanceLedger.NotVerified);
        Assert.Contains("受入に不足", L110AcceptanceLedger.AssessSelfCheck(false, false, false, false));
    }
}
