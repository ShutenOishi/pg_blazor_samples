using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L053PersistenceComparisonTests
{
    [Fact]
    public async Task Unconfigured_RefusesWrite()
    {
        using var provider = Build(null);
        var result = await provider.GetRequiredService<L053PersistenceComparisonService>().RunAsync();
        Assert.False(result.Queried);
        Assert.Equal(LearningDbService.NotConfigured, result.Status);
    }

    [Fact]
    public async Task WindowsAuthentication_RefusesAnonymousWrite()
    {
        using var provider = Build(Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL"), true);
        var result = await provider.GetRequiredService<L053PersistenceComparisonService>().RunAsync();
        Assert.False(result.Queried);
        Assert.Equal(0, result.DapperAffected);
    }

    [Fact]
    public async Task PostgreSql18_DapperExecute_EfSaveChanges_BothRolledBack()
    {
        // 実接続を保証するのは専用PostgreSQL18 CI。環境変数なしでは未検証。
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;
        using var provider = Build(connection);
        var result = await provider.GetRequiredService<L053PersistenceComparisonService>().RunAsync();
        Assert.True(result.Queried, result.Status);
        Assert.Equal(1, result.DapperAffected);
        Assert.Equal(1, result.DapperFound);
        Assert.Equal(1, result.EfSaved);
        Assert.Equal("Added", result.EfBefore);
        Assert.Equal("Unchanged", result.EfAfter);
        Assert.False(result.RemainedAfterRollback);
    }

    private static ServiceProvider Build(string? connection, bool windowsAuth = false)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = connection,
                ["WindowsAuthentication:Enabled"] = windowsAuth.ToString()
            }).Build();
        return new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(options => options.UseNpgsql(
                connection ?? "Host=127.0.0.1;Database=pg_blazor_learning;Username=pgblazor_learning;Timeout=1"))
            .AddScoped<L053PersistenceComparisonService>()
            .BuildServiceProvider();
    }
}
