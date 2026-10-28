using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L046OrmComparisonTests
{
    [Fact]
    public async Task WithoutDevelopmentDatabase_RefusesBeforeQuery()
    {
        using var provider = Services(null);
        var comparison = provider.GetRequiredService<L046OrmComparisonService>();
        var result = await comparison.CompareAsync("架空");
        Assert.False(result.Queried);
        Assert.False(result.Matches);
        Assert.Empty(result.DapperRows);
        Assert.Equal(LearningDbService.NotConfigured, result.Status);
    }

    [Fact]
    public async Task PostgreSql18_DapperAndEf_ReadSameRows_AndRejectBoundaries()
    {
        // 実接続の保証はPostgreSQL18 CIジョブだけ。ローカルの未設定環境では未検証。
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;
        using var provider = Services(connection);
        var comparison = provider.GetRequiredService<L046OrmComparisonService>();
        var factory = provider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var prefix = "l046_" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Tasks.AddRange(
                    new LearningTask { Title = prefix + "_a" },
                    new LearningTask { Title = prefix + "_b" });
                await seed.SaveChangesAsync();
            }

            var result = await comparison.CompareAsync(prefix);
            Assert.True(result.Queried);
            Assert.True(result.Matches, result.Status);
            Assert.Equal(2, result.DapperRows.Count);
            Assert.Equal(result.DapperRows.Select(x => (x.Id, x.Title)),
                result.EfRows.Select(x => (x.Id, x.Title)));

            // 入力がSQLの構造にならず、検索値としてのみ使用される。
            var injection = await comparison.CompareAsync("' OR 1=1 --");
            Assert.True(injection.Queried);
            Assert.True(injection.Matches);
            Assert.Empty(injection.DapperRows);
            Assert.Empty(injection.EfRows);

            foreach (var invalid in new[] { "", " ", new string('x', 61) })
            {
                var rejected = await comparison.CompareAsync(invalid);
                Assert.False(rejected.Queried);
                Assert.Empty(rejected.EfRows);
            }
        }
        finally
        {
            await using var cleanup = await factory.CreateDbContextAsync();
            await cleanup.Tasks.Where(x => x.Title.StartsWith(prefix)).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task WindowsAuthenticationEnabled_DoesNotRunAnonymousComparison()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        using var provider = Services(connection, windowsAuth: true);
        var result = await provider.GetRequiredService<L046OrmComparisonService>()
            .CompareAsync("架空");
        Assert.False(result.Queried);
        Assert.Empty(result.DapperRows);
    }

    private static ServiceProvider Services(string? connection, bool windowsAuth = false)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = connection,
                ["WindowsAuthentication:Enabled"] = windowsAuth.ToString()
            }).Build();
        var dbConnection = connection ??
            "Host=127.0.0.1;Database=pg_blazor_learning;Username=pgblazor_learning;Timeout=1";
        return new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(options => options.UseNpgsql(dbConnection))
            .AddScoped<L046OrmComparisonService>()
            .BuildServiceProvider();
    }
}
