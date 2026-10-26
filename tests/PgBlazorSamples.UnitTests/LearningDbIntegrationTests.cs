using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Services.Learning;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using PgBlazorSamples.Web.Data;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>開発専用PostgreSQL 18でだけ実行する実DB統合テスト。別の層別プロジェクトは作らない。</summary>
public sealed class LearningDbIntegrationTests
{
    [Fact]
    public async Task PostgreSql18_DateOnly_TimeOnly_Range_Xmin_Transaction()
    {
        // 通常のWindows/Linuxのテストでは未接続としてスキップ相当。
        // 専用CIジョブでは接続文字列とDDLを用意し、必ず実DBで検証する。
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection))
            return;

        var options = new DbContextOptionsBuilder<LearningDbContext>()
            .UseNpgsql(connection).Options;
        await using var db = new LearningDbContext(options);
        Assert.True(await db.Database.CanConnectAsync());

        var id = 0L;
        try
        {
            var start = new DateTime(2026, 10, 1);
            var end = new DateTime(2026, 11, 1);
            var task = new LearningTask
            {
                Title = "統合試験用の架空課題",
                DueDate = new DateOnly(2026, 10, 26),
                StartTime = new TimeOnly(13, 45),
                AvailableDates = new NpgsqlRange<DateTime>(start, true, end, false)
            };
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            id = task.Id;
            Assert.True(id > 0);

            await using var contextA = new LearningDbContext(options);
            await using var contextB = new LearningDbContext(options);
            var a = await contextA.Tasks.SingleAsync(x => x.Id == id);
            var b = await contextB.Tasks.SingleAsync(x => x.Id == id);
            Assert.Equal(new DateOnly(2026, 10, 26), a.DueDate);
            Assert.Equal(new TimeOnly(13, 45), a.StartTime);
            Assert.True(a.AvailableDates.HasValue);
            Assert.Equal(start, a.AvailableDates.Value.LowerBound);
            Assert.Equal(end, a.AvailableDates.Value.UpperBound);
            Assert.False(a.AvailableDates.Value.UpperBoundIsInclusive);
            Assert.True(a.Version > 0);

            a.IsDone = true;
            await contextA.SaveChangesAsync();
            b.Title = "競合する変更";
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await contextB.SaveChangesAsync());

            // 別の行を追加後、明示ロールバックで永続化されないことを検証する。
            await using var txContext = new LearningDbContext(options);
            await using (var tx = await txContext.Database.BeginTransactionAsync())
            {
                txContext.Tasks.Add(new LearningTask { Title = "ロールバックを検証する課題" });
                await txContext.SaveChangesAsync();
                await tx.RollbackAsync();
            }
            await using var check = new LearningDbContext(options);
            Assert.False(await check.Tasks.AnyAsync(t => t.Title == "ロールバックを検証する課題"));
        }
        finally
        {
            if (id > 0)
            {
                await using var cleanup = new LearningDbContext(options);
                await cleanup.Tasks.Where(x => x.Id == id).ExecuteDeleteAsync();
            }
        }
    }
    [Fact]
    public async Task PostgreSql18_ServerPaging_Filter_SafeSorting()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = connection
            }).Build();
        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(options => options.UseNpgsql(connection))
            .AddScoped<LearningDbService>()
            .AddScoped<LearningPageService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var pages = scope.ServiceProvider.GetRequiredService<LearningPageService>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LearningDbContext>>();

        // 複数回のテストと他の学習用データが混ざらないよう、固有の架空課題にする。
        var prefix = "一覧結合試験_" + Guid.NewGuid().ToString("N")[..12] + "_";
        try
        {
            await using (var db = await factory.CreateDbContextAsync())
            {
                db.Tasks.AddRange(Enumerable.Range(0, 27).Select(i => new LearningTask
                {
                    Title = prefix + i.ToString("D2"),
                    IsDone = i % 2 == 0,
                    DueDate = new DateOnly(2026, 10, 1).AddDays(i % 3)
                }));
                await db.SaveChangesAsync();
            }
            var page0 = await pages.LoadAsync(new(0, 5, prefix));
            var page1 = await pages.LoadAsync(new(1, 5, prefix));
            Assert.Equal(27, page0.TotalCount);
            Assert.Equal(27, page1.TotalCount);
            Assert.Equal(5, page0.Rows.Count);
            Assert.Equal(5, page1.Rows.Count);
            Assert.Empty(page0.Rows.Select(x => x.Id).Intersect(page1.Rows.Select(x => x.Id)));

            var multi = await pages.LoadAsync(new(0, 50, prefix,
                Sorts: [new("DueDate", true), new("Title", false)]));
            Assert.Equal(27, multi.TotalCount);
            Assert.True(multi.Rows.Zip(multi.Rows.Skip(1), (a, b) =>
                a.DueDate > b.DueDate || a.DueDate == b.DueDate
                    && string.CompareOrdinal(a.Title, b.Title) <= 0).All(x => x));
            var filter = await pages.LoadAsync(new(0, 20, prefix,
                IsDone: true, DueFrom: new DateOnly(2026, 10, 2),
                DueTo: new DateOnly(2026, 10, 3)));
            Assert.True(filter.Rows.Count > 0);
            Assert.Equal(filter.TotalCount, filter.Rows.Count);
            Assert.All(filter.Rows, row =>
            {
                Assert.True(row.IsDone);
                Assert.InRange(row.DueDate!.Value, new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 3));
            });
            Assert.Throws<ArgumentException>(() =>
                LearningPageService.Validate(new(Sorts: [new("unsafe_column")])));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LearningPageService.Validate(new(PageSize: 1000)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LearningPageService.Validate(new(Page: -1)));
            Assert.Throws<ArgumentException>(() =>
                LearningPageService.Validate(new(DueFrom: new DateOnly(2026, 11, 1),
                    DueTo: new DateOnly(2026, 10, 1))));
            using var cancel = new CancellationTokenSource();
            cancel.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => pages.LoadAsync(new(0, 5, prefix), cancel.Token));
        }
        finally
        {
            await using var db = await factory.CreateDbContextAsync();
            await db.Tasks.Where(t => t.Title.StartsWith(prefix)).ExecuteDeleteAsync();
        }
    }

}
