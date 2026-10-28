using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L059ParameterizedSqlTests
{
    [Fact]
    public async Task Unconfigured_RefusesQuery()
    {
        using var provider = Services(null);
        var result = await provider.GetRequiredService<L059ParameterizedSqlService>().QueryAsync("架空");
        Assert.False(result.Queried);
        Assert.Empty(result.DapperRows);
        Assert.Equal(LearningDbService.NotConfigured, result.Status);
    }

    [Fact]
    public async Task WindowsAuthentication_RefusesAnonymousQuery()
    {
        using var provider = Services(Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL"), true);
        var result = await provider.GetRequiredService<L059ParameterizedSqlService>().QueryAsync("架空");
        Assert.False(result.Queried);
        Assert.Empty(result.EfRows);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task InvalidKeywords_DoNotQuery(string keyword)
    {
        using var provider = Services("Host=127.0.0.1;Database=unused;Username=unused;Timeout=1");
        var result = await provider.GetRequiredService<L059ParameterizedSqlService>().QueryAsync(keyword);
        Assert.False(result.Queried);
        Assert.Empty(result.DapperRows);
    }

    [Fact]
    public async Task PostgreSql18_DapperFromSqlAndSqlQuery_UseParameters()
    {
        // PG18専用CIだけが実接続の証拠。非DBジョブのreturnは実DB成功を意味しない。
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;
        using var provider = Services(connection);
        var factory = provider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var title = "l059_" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var seed = await factory.CreateDbContextAsync())
            {
                seed.Tasks.Add(new LearningTask { Title = title });
                await seed.SaveChangesAsync();
            }
            var service = provider.GetRequiredService<L059ParameterizedSqlService>();
            var result = await service.QueryAsync(title);
            Assert.True(result.Queried, result.Status);
            Assert.True(result.Matches, result.Status);
            Assert.Single(result.DapperRows);
            Assert.Single(result.EfRows);
            Assert.Equal(1, result.EfCount);
            Assert.Equal(title, result.DapperRows[0].Title);
            Assert.Equal(result.DapperRows[0].Id, result.EfRows[0].Id);

            var injection = await service.QueryAsync("' OR 1=1 --");
            Assert.True(injection.Queried, injection.Status);
            Assert.True(injection.Matches, injection.Status);
            Assert.Empty(injection.DapperRows);
            Assert.Empty(injection.EfRows);
            Assert.Equal(0, injection.EfCount);

            var tooLong = await service.QueryAsync(new string('x', 61));
            Assert.False(tooLong.Queried);
        }
        finally
        {
            await using var cleanup = await factory.CreateDbContextAsync();
            await cleanup.Tasks.Where(x => x.Title == title).ExecuteDeleteAsync();
        }
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
            .AddScoped<L059ParameterizedSqlService>()
            .BuildServiceProvider();
    }
}
