using System.Security.Claims;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順081：実際の信頼名検証関数を架空Principalで呼ぶ隔離サービスの正常・拒否・失効試験。</summary>
public sealed class L081AuthorizationExerciseTests
{
    private static ClaimsPrincipal Principal(string? login, string? scheme)
    {
        if (scheme is null)
            return new ClaimsPrincipal(new ClaimsIdentity());
        return new ClaimsPrincipal(new ClaimsIdentity(
            login is null ? [] : [new Claim(ClaimTypes.Name, login)], scheme));
    }

    [Fact]
    public void LearningPrincipal_ReadAllowed_ThenRevoked_ThenReset()
    {
        var service = new L081AuthorizationExercise();
        var principal = Principal(L081AuthorizationExercise.DemoLogin, "Negotiate");
        var first = service.Read(principal);
        Assert.True(first.Allowed);
        Assert.Equal(1, first.RowCount);
        service.RevokeRead();
        var revoked = service.Read(principal);
        Assert.False(revoked.Allowed);
        Assert.Equal(0, revoked.RowCount);
        service.Reset();
        Assert.True(service.Read(principal).Allowed);
        Assert.Equal(1, service.Checks);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("TRAINING\\READER", "Cookies")]
    [InlineData("OTHER\\USER", "Negotiate")]
    [InlineData("reader@example.test", "Negotiate")]
    public void UntrustedOrUnknownIdentity_AlwaysRefused(string? name, string? scheme)
    {
        var result = new L081AuthorizationExercise().Read(Principal(name, scheme));
        Assert.False(result.Allowed);
        Assert.Equal(0, result.RowCount);
    }

    [Fact]
    public void InactiveOrAuditUnavailable_NeverReturnsRows()
    {
        var principal = Principal(L081AuthorizationExercise.DemoLogin, "Negotiate");
        var service = new L081AuthorizationExercise();
        Assert.False(service.Read(principal, auditAvailable: false).Allowed);
        service.DisableAccount();
        Assert.False(service.Read(principal).Allowed);
        Assert.Equal(0, service.Read(principal).RowCount);
    }
}
