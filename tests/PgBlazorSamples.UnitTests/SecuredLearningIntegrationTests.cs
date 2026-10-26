using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>Step66〜70：PostgreSQL18実DBで匿名・部署横断・権限剥奪・編集可否を検証。</summary>
public sealed class SecuredLearningIntegrationTests
{
    [Fact]
    public async Task PostgreSql18_AllOperations_FailClosed_UnitIsolation_Revocation()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = connection,
                ["WindowsAuthentication:Enabled"] = "true"
            }).Build();
        var auth = new DemoAuthenticationStateProvider();
        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<AuthenticationStateProvider>(auth)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(options => options.UseNpgsql(connection))
            .AddScoped<WindowsUserAccessService>()
            .AddScoped<SecuredLearningService>()
            .AddScoped<LearningDbService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var business = scope.ServiceProvider.GetRequiredService<SecuredLearningService>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var oldService = scope.ServiceProvider.GetRequiredService<LearningDbService>();
        Assert.False(oldService.Configured); // 認証ありモードでは無認可の旧教材DBを無効化

        var suffix = Guid.NewGuid().ToString("N")[..14].ToUpperInvariant();
        var login = "TRAINING\\AUTH" + suffix;
        var unit = "U" + suffix;
        var otherUnit = "X" + suffix;
        var prefix = "権限試験" + suffix;
        long ownId = 0, otherId = 0;

        try
        {
            await using (var db = await factory.CreateDbContextAsync())
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO learning_existing_user_ci (login_name, display_name, is_active, can_read, can_edit, unit_code) VALUES ({login}, {"試験用の架空利用者"}, {true}, {true}, {false}, {unit})");
                var own = new LearningTask { Title = prefix + "自部署", UnitCode = unit };
                var other = new LearningTask { Title = prefix + "他部署", UnitCode = otherUnit };
                db.Tasks.AddRange(own, other);
                await db.SaveChangesAsync();
                ownId = own.Id;
                otherId = other.Id;
            }

            // AD未認証なら一覧/件数/CSV/更新のどこからも業務行を出さない。
            auth.User = new ClaimsPrincipal(new ClaimsIdentity());
            Assert.Null((await business.AuthorizeAsync()).Unit);
            Assert.Equal(0, (await business.LoadPageAsync(new(0, 20, prefix))).TotalCount);
            Assert.Null((await business.ExportCsvAsync(prefix)).Base64);
            Assert.Empty((await business.CountByDoneAsync()).Rows);
            Assert.Equal(LearningEditStatus.Forbidden,
                (await business.SaveAsync(new LearningEditInput { Id = ownId, Title = "拒否", Version = 1 })).Status);

            // Cookieや入力欄から自己申告したユーザー名は信頼しない。
            auth.User = Principal(login, "Cookies");
            Assert.Null((await business.AuthorizeAsync()).Unit);

            // 読み取り権限のある本人は自部署だけ。CountとItemsは同じ検索範囲。
            auth.User = Principal(login, "Negotiate");
            Assert.Equal(unit, (await business.AuthorizeAsync()).Unit);
            var listing = await business.LoadPageAsync(new(0, 20, prefix));
            Assert.Equal(1, listing.TotalCount);
            Assert.Single(listing.Rows);
            Assert.Equal(ownId, listing.Rows[0].Id);
            Assert.NotNull((await business.GetDetailAsync(ownId)).Item);
            Assert.Null((await business.GetDetailAsync(otherId)).Item);
            Assert.Equal(1, (await business.CountByDoneAsync()).Rows.Sum(r => r.Count));
            var csv = await business.ExportCsvAsync(prefix);
            Assert.NotNull(csv.Base64);
            var content = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(csv.Base64!));
            Assert.Contains(prefix + "自部署", content);
            Assert.DoesNotContain(prefix + "他部署", content);

            // 閲覧可能でも編集権限がなければ拒否。
            var dto = listing.Rows[0];
            var edited = new LearningEditInput
            {
                Id = dto.Id, Version = dto.Version, Title = prefix + "更新", IsDone = true
            };
            Assert.Equal(LearningEditStatus.Forbidden, (await business.SaveAsync(edited)).Status);

            // DB上の正本でcan_editを変更すると、古いcircuit/Principalを再利用しても直ちに反映。
            await using (var db = await factory.CreateDbContextAsync())
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE learning_existing_user_ci SET can_edit = {true}, authorization_version = authorization_version + 1 WHERE login_name = {login}");
            Assert.Equal(LearningEditStatus.Success, (await business.SaveAsync(edited)).Status);
            Assert.Equal(LearningEditStatus.Conflict, (await business.SaveAsync(edited)).Status);

            // 他部署のIDを一括更新へ混入させると全体を拒否する。
            await using (var db = await factory.CreateDbContextAsync())
            {
                var ownVersion = (await db.Tasks.AsNoTracking().SingleAsync(t => t.Id == ownId)).Version;
                var otherVersion = (await db.Tasks.AsNoTracking().SingleAsync(t => t.Id == otherId)).Version;
                Assert.Equal(LearningEditStatus.Conflict, (await business.CompleteSelectedAsync(
                    [new(ownId, ownVersion), new(otherId, otherVersion)])).Status);
            }

            await using (var db = await factory.CreateDbContextAsync())
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE learning_existing_user_ci SET can_read = {false}, can_edit = {false}, authorization_version = authorization_version + 1 WHERE login_name = {login}");
            Assert.Equal(0, (await business.LoadPageAsync(new(0, 20, prefix))).TotalCount);
            Assert.Null((await business.GetDetailAsync(ownId)).Item);
            Assert.Null((await business.ExportCsvAsync(prefix)).Base64);
            Assert.Equal(LearningEditStatus.Forbidden, (await business.SaveAsync(edited)).Status);
        }
        finally
        {
            await using var db = await factory.CreateDbContextAsync();
            await db.Tasks.Where(t => t.Title.StartsWith(prefix)).ExecuteDeleteAsync();
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM learning_existing_user_ci WHERE login_name = {login}");
        }
    }

    [Fact]
    public async Task AuthFeatureDisabled_AlwaysRefuses()
    {
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["WindowsAuthentication:Enabled"] = "false" }).Build();
        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(cfg)
            .AddSingleton<AuthenticationStateProvider>(new DemoAuthenticationStateProvider { User = Principal("TRAINING\\USER00001", "Negotiate") })
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(x => x.UseNpgsql("Host=127.0.0.1;Database=unused"))
            .AddScoped<WindowsUserAccessService>()
            .AddScoped<SecuredLearningService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SecuredLearningService>();
        Assert.Null((await service.AuthorizeAsync()).Unit);
        Assert.Equal(0, (await service.LoadPageAsync(new())).TotalCount);
    }

    private static ClaimsPrincipal Principal(string login, string scheme) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Name, login)], scheme));

    private sealed class DemoAuthenticationStateProvider : AuthenticationStateProvider
    {
        public ClaimsPrincipal User { get; set; } = new(new ClaimsIdentity());
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(User));
    }
}
