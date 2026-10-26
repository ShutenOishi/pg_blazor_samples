using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NpgsqlTypes;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>Step56〜60：期間の境界、キャッシュ隔離、CSV否定試験、実DB集計とEXPLAIN。</summary>
public sealed class LearningFinalIntegrationTests
{
    [Fact]
    public void InclusivePeriod_CacheAndCsvGuards()
    {
        var range = LearningRangeExportService.CreateInclusiveRange(
            new DateOnly(2088, 6, 1), new DateOnly(2088, 6, 2));
        Assert.Equal(new DateTime(2088, 6, 1), range.LowerBound);
        Assert.Equal(new DateTime(2088, 6, 3), range.UpperBound);
        Assert.Throws<ArgumentException>(() => LearningRangeExportService.CreateInclusiveRange(
            new DateOnly(2088, 6, 2), new DateOnly(2088, 6, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => LearningRangeExportService.CreateInclusiveRange(
            new DateOnly(2088, 1, 1), DateOnly.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => LearningRangeExportService.CreateInclusiveRange(
            new DateOnly(2088, 1, 1), new DateOnly(2089, 4, 1)));

        var cache = new LearningCacheLessonService();
        var a = cache.Read("A", 1);
        Assert.False(a.CacheHit);
        Assert.Contains("部署Aの作業", a.Rows);
        Assert.DoesNotContain("部署Bの作業", a.Rows);
        Assert.True(cache.Read("A", 1).CacheHit);
        var b = cache.Read("B", 1);
        Assert.False(b.CacheHit);
        Assert.DoesNotContain("部署Aの作業", b.Rows);
        Assert.False(cache.Read("A", 2).CacheHit);
        cache.Clear();
        Assert.False(cache.Read("A", 1).CacheHit);
        Assert.Throws<ArgumentException>(() => cache.Read("unknown", 1));

        Assert.Equal("\"'=2+2\"", LearningRangeExportService.EscapeCsv("=2+2"));
        Assert.Equal("\"' +SUM(1,2)\"", LearningRangeExportService.EscapeCsv(" +SUM(1,2)"));
        Assert.Equal("\"a\"\"b\"", LearningRangeExportService.EscapeCsv("a\"b"));
    }

    [Fact]
    public async Task PostgreSql18_DateRange_CsvLimit_Aggregate_Performance()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var settings = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:PostgreSQL"] = connection }).Build();
        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(settings)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(o => o.UseNpgsql(connection))
            .AddScoped<LearningDbService>()
            .AddScoped<LearningRangeExportService>()
            .AddScoped<LearningPerformanceService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var service = scope.ServiceProvider.GetRequiredService<LearningRangeExportService>();
        var performance = scope.ServiceProvider.GetRequiredService<LearningPerformanceService>();
        var prefix = "最終編試験_" + Guid.NewGuid().ToString("N")[..12] + "_";
        try
        {
            await using (var db = await dbFactory.CreateDbContextAsync())
            {
                var tasks = Enumerable.Range(0, 105).Select(i => new LearningTask
                {
                    Title = prefix + (i < 2 ? "in" : "other") + i.ToString("D3"),
                    DueDate = new DateOnly(2088, 6, 1).AddDays(i % 3),
                    IsDone = i % 2 == 0,
                    AvailableDates = i switch
                    {
                        0 => new NpgsqlRange<DateTime>(new DateTime(2088, 6, 2), true,
                            new DateTime(2088, 6, 4), false),
                        1 => new NpgsqlRange<DateTime>(new DateTime(2088, 6, 4), true,
                            new DateTime(2088, 6, 6), false),
                        _ => null
                    }
                }).ToArray();
                db.Tasks.AddRange(tasks);
                await db.SaveChangesAsync();
            }

            // UI期間を6/2〜6/3にすると、半開区間の6/4以降のみの行は非該当。
            var range = await service.FindOverlapsAsync(new DateOnly(2088, 6, 2), new DateOnly(2088, 6, 3));
            Assert.Contains(range.Rows, r => r.Title == prefix + "in000");
            Assert.DoesNotContain(range.Rows, r => r.Title == prefix + "in001");
            Assert.True(range.TotalCount >= 1);

            var counts = await service.CountByDoneAsync();
            Assert.True(counts.Rows.Sum(x => x.Count) >= 105, counts.Message);

            var small = await service.ExportSmallCsvAsync(prefix + "in");
            Assert.Equal(2, small.Count);
            Assert.NotNull(small.Base64);
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(small.Base64!));
            Assert.Contains(prefix + "in000", decoded);
            Assert.StartsWith("\uFEFF番号", decoded);

            var tooMany = await service.ExportSmallCsvAsync(prefix);
            Assert.Null(tooMany.Base64);
            Assert.Contains("101件以上", tooMany.Message);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.ExportSmallCsvAsync(new string('x', 101)));

            var observed = await performance.ObserveAsync();
            Assert.True(observed.P95Milliseconds >= 0);
            Assert.True(observed.P99Milliseconds >= observed.P95Milliseconds);
            Assert.NotEmpty(observed.PlanLines);

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.FindOverlapsAsync(new DateOnly(2088, 6, 1),
                    new DateOnly(2088, 6, 3), cancellation.Token));
        }
        finally
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            await db.Tasks.Where(x => x.Title.StartsWith(prefix)).ExecuteDeleteAsync();
        }
    }
}
