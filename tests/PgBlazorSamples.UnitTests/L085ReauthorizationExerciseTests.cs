using System.Security.Claims;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>085：架空再接続の前後でサービスが都度台帳を読む正常・拒否・境界試験。</summary>
public sealed class L085ReauthorizationExerciseTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 0, 0, 0, TimeSpan.Zero);

    private static ClaimsPrincipal Principal(string? login = L081AuthorizationExercise.DemoLogin,
        string scheme = "Negotiate") =>
        new(new ClaimsIdentity(
            login is null ? [] : [new Claim(ClaimTypes.Name, login)], scheme));

    [Fact]
    public void Reconnect_DoesNotRestoreRevokedPermission()
    {
        var service = new L085ReauthorizationExercise();
        var user = Principal();
        Assert.Equal(1, service.Read(user, Now, Now.AddMinutes(10)).RowCount);
        service.SimulateReconnect();
        Assert.True(service.Read(user, Now, Now.AddMinutes(10)).Allowed);
        service.RevokeRead();
        service.SimulateReconnect();
        var revoked = service.Read(user, Now, Now.AddMinutes(10));
        Assert.False(revoked.Allowed);
        Assert.Equal(0, revoked.RowCount);
        Assert.Equal(3, service.Checks);
        Assert.Equal(2, service.Reconnects);
        service.Reset();
        Assert.True(service.Read(user, Now, Now.AddMinutes(10)).Allowed);
        Assert.Equal(1, service.Checks);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ExpiredOrBoundaryTicket_IsAlwaysRejected(int seconds)
    {
        var service = new L085ReauthorizationExercise();
        var response = service.Read(Principal(), Now, Now.AddSeconds(seconds));
        Assert.False(response.Allowed);
        Assert.Equal(0, response.RowCount);
    }

    [Theory]
    [InlineData("TRAINING\\READER", "Cookies")]
    [InlineData("OTHER\\USER", "Negotiate")]
    [InlineData(null, "Negotiate")]
    public void UntrustedIdentity_NeverReturnsRows(string? login, string scheme)
    {
        var result = new L085ReauthorizationExercise().Read(
            Principal(login, scheme), Now, Now.AddMinutes(10));
        Assert.False(result.Allowed);
        Assert.Equal(0, result.RowCount);
    }

    [Fact]
    public void OldTicketVersionOrAuditFailureOrInactiveLedger_Rejects()
    {
        var service = new L085ReauthorizationExercise();
        Assert.False(service.Read(Principal(), Now, Now.AddMinutes(10),
            ticketAuthorizationVersion: 1).Allowed);
        Assert.Equal(0, service.Read(Principal(), Now, Now.AddMinutes(10),
            auditAvailable: false).RowCount);
        service.DisableAccount();
        service.SimulateReconnect();
        Assert.Equal(0, service.Read(Principal(), Now, Now.AddMinutes(10)).RowCount);
    }

    [Fact]
    public void RevocationDoesNotDependOnPageCheckboxOrReconnectCount()
    {
        var service = new L085ReauthorizationExercise();
        service.RevokeRead();
        for (var i = 0; i < 3; i++)
        {
            service.SimulateReconnect();
            Assert.False(service.Read(Principal(), Now, Now.AddMinutes(10)).Allowed);
        }
        Assert.Equal(3, service.Checks);
    }
}
