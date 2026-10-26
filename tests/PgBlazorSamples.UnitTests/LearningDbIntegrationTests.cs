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
}
