using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>Step61〜65：認証主体の改ざん防止と実DBの未登録・無効・ロール拒否。</summary>
public sealed class WindowsAccessIntegrationTests
{
    private static ClaimsPrincipal Principal(string? sid, string? scheme = "Negotiate")
    {
        var claims = sid is null ? [] : new[] { new Claim(ClaimTypes.PrimarySid, sid) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
    }

    [Fact]
    public void AuthenticationAndSid_NegativeCases()
    {
        const string sid = "S-1-5-21-10101-20202-30303-40404";
        Assert.Null(WindowsUserAccessService.ReadTrustedSid(new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.PrimarySid, sid) }))));
        Assert.Null(WindowsUserAccessService.ReadTrustedSid(Principal(sid, "Cookies")));
        Assert.Null(WindowsUserAccessService.ReadTrustedSid(Principal(null)));
        Assert.Null(WindowsUserAccessService.ReadTrustedSid(Principal("admin")));
        Assert.Equal(sid, WindowsUserAccessService.ReadTrustedSid(Principal(sid)));
    }

    [Fact]
    public async Task PostgreSql18_Unregistered_Disabled_WithoutRole_AndRevoked_FailClosed()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:PostgreSQL"] = connection }).Build();
        var auth = new FakeAuthenticationStateProvider();
        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<AuthenticationStateProvider>(auth)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(x => x.UseNpgsql(connection))
            .AddScoped<WindowsUserAccessService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var access = scope.ServiceProvider.GetRequiredService<WindowsUserAccessService>();
        var guid = Guid.NewGuid().ToByteArray();
        var sid = $"S-1-5-21-{BitConverter.ToUInt32(guid, 0)}-{BitConverter.ToUInt32(guid, 4)}-{BitConverter.ToUInt32(guid, 8)}-{BitConverter.ToUInt32(guid, 12)}";
        auth.User = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.Equal("anonymous", (await access.ReadCurrentAsync()).Code);
        auth.User = Principal(sid);
        Assert.Equal("unknown", (await access.ReadCurrentAsync()).Code);

        long id = 0;
        try
        {
            await using (var db = await factory.CreateDbContextAsync())
            {
                var user = new AppUser { Sid = sid, DisplayName = "架空の閲覧者", IsActive = false };
                db.AppUsers.Add(user);
                await db.SaveChangesAsync();
                id = user.Id;
            }
            Assert.Equal("inactive", (await access.ReadCurrentAsync()).Code);
            await using (var db = await factory.CreateDbContextAsync())
            {
                var user = await db.AppUsers.SingleAsync(x => x.Id == id);
                user.IsActive = true;
                await db.SaveChangesAsync();
            }
            Assert.Equal("forbidden", (await access.ReadCurrentAsync()).Code);

            await using (var db = await factory.CreateDbContextAsync())
            {
                var viewer = await db.AppRoles.SingleAsync(x => x.Code == "viewer");
                db.AppUserRoles.Add(new AppUserRole { UserId = id, RoleId = viewer.Id });
                await db.SaveChangesAsync();
            }
            var permitted = await access.ReadCurrentAsync();
            Assert.Equal("allowed", permitted.Code);
            Assert.Contains("learning.read", permitted.Permissions!);
            Assert.Equal("架空の閲覧者", permitted.DisplayName);
            Assert.DoesNotContain(sid, permitted.SidHint);

            // 付与済みロールを剥奪した直後の判定が拒否に戻る。
            await using (var db = await factory.CreateDbContextAsync())
            {
                await db.AppUserRoles.Where(x => x.UserId == id).ExecuteDeleteAsync();
            }
            Assert.Equal("forbidden", (await access.ReadCurrentAsync()).Code);
        }
        finally
        {
            if (id != 0)
            {
                await using var db = await factory.CreateDbContextAsync();
                await db.AppUsers.Where(x => x.Id == id).ExecuteDeleteAsync();
            }
        }
    }

    private sealed class FakeAuthenticationStateProvider : AuthenticationStateProvider
    {
        public ClaimsPrincipal User { get; set; } = new(new ClaimsIdentity());
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(User));
    }
}
