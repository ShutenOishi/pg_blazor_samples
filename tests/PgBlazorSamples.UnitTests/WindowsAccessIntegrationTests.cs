using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>既存利用者台帳10001人の検索と未登録・無効・権限変更の実DB拒否。</summary>
public sealed class WindowsAccessIntegrationTests
{
    private static ClaimsPrincipal Principal(string? login, string? scheme = "Negotiate")
    {
        var claims = login is null ? [] : new[] { new Claim(ClaimTypes.Name, login) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, scheme));
    }

    [Fact]
    public void AuthenticationAndTrustedName_NegativeCases()
    {
        const string login = "DOMAIN\\USER";
        Assert.Null(WindowsUserAccessService.ReadTrustedLoginName(new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login) }))));
        Assert.Null(WindowsUserAccessService.ReadTrustedLoginName(Principal(login, "Cookies")));
        Assert.Null(WindowsUserAccessService.ReadTrustedLoginName(Principal(null)));
        Assert.Null(WindowsUserAccessService.ReadTrustedLoginName(Principal("USER")));
        Assert.Null(WindowsUserAccessService.ReadTrustedLoginName(Principal("USER@EXAMPLE")));
        Assert.Equal(login, WindowsUserAccessService.ReadTrustedLoginName(Principal("domain\\user")));
    }

    [Fact]
    public async Task PostgreSql18_ExistingDirectory_Inactive_Revoked_FailClosed()
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
        var login = "TRAINING\\CI" + Guid.NewGuid().ToString("N")[..20].ToUpperInvariant();

        auth.User = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.Equal("anonymous", (await access.ReadCurrentAsync()).Code);
        auth.User = Principal(login);
        Assert.Equal("unknown", (await access.ReadCurrentAsync()).Code);

        // テストが既存台帳を準備する操作であり、業務アプリに登録処理を実装しない。
        await using (var db = await factory.CreateDbContextAsync())
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO learning_existing_user_ci(login_name, display_name, is_active, can_read, can_edit) VALUES ({login}, {"架空の閲覧者"}, {false}, {false}, {false})");
        }
        try
        {
            Assert.Equal("inactive", (await access.ReadCurrentAsync()).Code);
            await using (var db = await factory.CreateDbContextAsync())
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE learning_existing_user_ci SET is_active = {true} WHERE login_name = {login}");
            }
            Assert.Equal("forbidden", (await access.ReadCurrentAsync()).Code);

            await using (var db = await factory.CreateDbContextAsync())
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE learning_existing_user_ci SET can_read = {true}, authorization_version = authorization_version + 1 WHERE login_name = {login}");
            }
            Assert.Equal("allowed", (await access.ReadCurrentAsync()).Code);
            Assert.Contains("learning.read", (await access.ReadCurrentAsync()).Permissions!);
            Assert.DoesNotContain(login, (await access.ReadCurrentAsync()).IdentityHint);

            await using (var db = await factory.CreateDbContextAsync())
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE learning_existing_user_ci SET can_read = {false}, authorization_version = authorization_version + 1 WHERE login_name = {login}");
            }
            Assert.Equal("forbidden", (await access.ReadCurrentAsync()).Code);

            await using (var db = await factory.CreateDbContextAsync())
            {
                Assert.Equal(10001, await db.ExistingUserAccess.CountAsync(x => x.UnitCode == "DEMO"));
                var existing = await db.ExistingUserAccess.Where(x => x.LoginName == "TRAINING\\USER10001")
                    .Take(2).ToListAsync();
                Assert.Single(existing);
            }
        }
        finally
        {
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM learning_existing_user_ci WHERE login_name = {login}");
        }
    }

    private sealed class FakeAuthenticationStateProvider : AuthenticationStateProvider
    {
        public ClaimsPrincipal User { get; set; } = new(new ClaimsIdentity());
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(User));
    }
}
