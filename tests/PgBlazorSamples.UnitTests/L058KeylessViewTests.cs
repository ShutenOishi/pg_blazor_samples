using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L058KeylessViewTests
{
    [Fact]
    public async Task MissingConnection_RefusesBeforeReading()
    {
        using var provider = Services(null);
        var result = await provider.GetRequiredService<L058KeylessViewService>().ReadAsync();
        Assert.False(result.Queried);
        Assert.Empty(result.Rows);
        Assert.Equal(LearningDbService.NotConfigured, result.Status);
    }

    [Fact]
    public async Task WindowsAuthentication_RefusesAnonymousViewReading()
    {
        using var provider = Services(Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL"), true);
        var result = await provider.GetRequiredService<L058KeylessViewService>().ReadAsync();
        Assert.False(result.Queried);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task PostgreSql18_KeylessViewReadsOnlyTwoFictionalRows()
    {
        // 実接続の証明は専用PostgreSQL18 CIのみ。未設定ならDB成功とは数えない。
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;
        using var provider = Services(connection);
        var factory = provider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        var entity = db.Model.FindEntityType(typeof(LearningKeylessDemoRow));
        Assert.NotNull(entity);
        Assert.Null(entity.FindPrimaryKey());
        Assert.Equal("learning_keyless_demo_v", entity.GetViewName());

        var result = await provider.GetRequiredService<L058KeylessViewService>().ReadAsync();
        Assert.True(result.Queried, result.Status);
        Assert.Equal(new[] { 1, 2 }, result.Rows.Select(r => r.SortOrder));
        Assert.Equal(new[] { "キーなしビューの架空行", "読取専用モデルの架空行" },
            result.Rows.Select(r => r.Label));
        Assert.Empty(db.ChangeTracker.Entries<LearningKeylessDemoRow>());
        Assert.False(db.ChangeTracker.AutoDetectChangesEnabled == false);
    }

    private static ServiceProvider Services(string? connection, bool windowsAuth = false)
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
            .AddScoped<L058KeylessViewService>()
            .BuildServiceProvider();
    }
}
