using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順084：実Program.csが呼ぶ設定ガードの正常・拒否・境界を検査。</summary>
public sealed class L084StartupPolicyTests
{
    [Theory]
    [InlineData(false, true, false, "any")]
    [InlineData(false, true, true, null)]
    public void ExplicitLoginWithoutWindowsAuthentication_Rejects(
        bool windowsAuth, bool explicitLogin, bool development, string? path)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            L084StartupPolicy.Validate(windowsAuth, explicitLogin, development, path));
        Assert.Contains("Windows認証", ex.Message);
        Assert.DoesNotContain("any", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ProductionExplicitLoginWithoutKeyPath_Rejects(string? path)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            L084StartupPolicy.Validate(true, true, false, path));
        Assert.Contains("DataProtectionKeysPath", ex.Message);
    }

    [Theory]
    [InlineData(true, true, true, null)]
    [InlineData(true, true, false, "架空の保存先")]
    [InlineData(true, false, false, null)]
    [InlineData(false, false, false, null)]
    public void AllowedFlagCombinations_DoNotFailStartupGuard(
        bool windowsAuth, bool explicitLogin, bool development, string? path) =>
        L084StartupPolicy.Validate(windowsAuth, explicitLogin, development, path);

    [Fact]
    public void LearningScenario_NeverReturnsSecretOrPath()
    {
        var refused = L084StartupPolicy.EvaluateLearningScenario(true, true, false, false);
        var allowed = L084StartupPolicy.EvaluateLearningScenario(true, true, false, true);
        Assert.Contains("起動拒否", refused);
        Assert.Contains("通過", allowed);
        Assert.DoesNotContain("架空の保存先", allowed);
        Assert.DoesNotContain("架空の保存先", refused);
    }
}
