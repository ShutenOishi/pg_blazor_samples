using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>新規総合演習をPostgreSQL18で検証し、検索・取消・xmin競合の安全条件を確認する。</summary>
public sealed class CapstoneIntegrationTests
{
    [Fact]
    public async Task PostgreSql18_FilterCountPaging_RollbackAndConcurrency()
    {
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(conn)) return;
        var options = new DbContextOptionsBuilder<LearningDbContext>().UseNpgsql(conn).Options;
        var unit = "C" + Guid.NewGuid().ToString("N")[..12];
        var prefix = "総合演習_" + Guid.NewGuid().ToString("N")[..12] + "_";
        var ids = new List<long>();
        try
        {
            await using (var db = new LearningDbContext(options))
            {
                Assert.True(await db.Database.CanConnectAsync());
                db.Tasks.AddRange(Enumerable.Range(0, 12).Select(n =>
                    new LearningTask { Title = prefix + n.ToString("D2"), UnitCode = unit, IsDone = n % 2 == 0 }));
                await db.SaveChangesAsync();
                ids = await db.Tasks.Where(t => t.UnitCode == unit && t.Title.StartsWith(prefix))
                    .Select(t => t.Id).ToListAsync();
            }
            await using (var a = new LearningDbContext(options))
            await using (var b = new LearningDbContext(options))
            {
                var query = a.Tasks.AsNoTracking().Where(t => t.UnitCode == unit && t.Title.StartsWith(prefix));
                Assert.Equal(12, await query.CountAsync());
                var rows = await query.OrderBy(t => t.Id).Take(10).ToListAsync();
                Assert.Equal(10, rows.Count);
                Assert.All(rows, row => Assert.Equal(unit, row.UnitCode));
                Assert.True(rows.Zip(rows.Skip(1), (x,y) => x.Id < y.Id).All(x=>x));
                Assert.Empty(await a.Tasks.AsNoTracking().Where(t => t.UnitCode != unit &&
                    t.Title.StartsWith(prefix)).ToListAsync());
                var x = await a.Tasks.SingleAsync(t => t.Id == ids[0]);
                var y = await b.Tasks.SingleAsync(t => t.Id == ids[0]);
                x.IsDone = !x.IsDone;
                await a.SaveChangesAsync();
                y.Title = prefix + "競合";
                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => b.SaveChangesAsync());
            }

            // 更新キャンセルをDBのロールバックとして検証する。途中書込みを残さない。
            await using (var ctx = new LearningDbContext(options))
            {
                await using var tx = await ctx.Database.BeginTransactionAsync();
                var task = await ctx.Tasks.SingleAsync(t => t.Id == ids[1]);
                task.Title = prefix + "保存しない";
                await ctx.SaveChangesAsync();
                await tx.RollbackAsync();
            }
            await using (var verify = new LearningDbContext(options))
                Assert.False(await verify.Tasks.AnyAsync(t => t.UnitCode == unit &&
                    t.Title == prefix + "保存しない"));
        }
        finally
        {
            await using var db = new LearningDbContext(options);
            await db.Tasks.Where(t => t.UnitCode == unit && t.Title.StartsWith(prefix)).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public void SearchRequest_RejectsUnsafeParameters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LearningPageService.Validate(new(PageSize: 51)));
        Assert.Throws<ArgumentException>(() => LearningPageService.Validate(new(Sorts: [new("DROP TABLE")])));
        Assert.Throws<ArgumentOutOfRangeException>(() => LearningPageService.Validate(new(Page: -1)));
    }
}
