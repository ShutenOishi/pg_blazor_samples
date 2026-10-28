using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>第5段階のSQLパラメーター化とKeylessモデルを開発用PostgreSQL18で検証する。</summary>
public sealed class DbBridgeIntegrationTests
{
    [Fact]
    public async Task PostgreSql18_RawSqlParameters_DoNotExecuteInjectedConditions()
    {
        // 通常の単体テストはDB未接続でも実行する。CIの実DBジョブは環境変数を必ず指定する。
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;
        var options = new DbContextOptionsBuilder<LearningDbContext>().UseNpgsql(connection).Options;
        await using var db = new LearningDbContext(options);
        Assert.True(await db.Database.CanConnectAsync());
        var title = "dbbridge_" + Guid.NewGuid().ToString("N");
        var attack = "' OR 1=1 --";
        var id = 0L;
        try
        {
            var task = new LearningTask { Title = title, UnitCode = "DEMO" };
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            id = task.Id;

            // 悪意ある見た目の文字列をSQLの値として渡しても全件取得にならない。
            var found = await db.Tasks.FromSqlInterpolated(
                $"SELECT xmin, * FROM learning_task WHERE title = {attack}").AsNoTracking().ToListAsync();
            Assert.Empty(found);
            var counted = await db.Database.SqlQuery<int>(
                $"SELECT COUNT(*)::int AS \"Value\" FROM learning_task WHERE title = {attack}").SingleAsync();
            Assert.Equal(0, counted);
            var expected = await db.Tasks.FromSqlInterpolated(
                $"SELECT xmin, * FROM learning_task WHERE title = {title}").AsNoTracking().SingleAsync();
            Assert.Equal(id, expected.Id);
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
    public void ExistingAccessView_IsKeylessWithoutReadingUserRows()
    {
        var options = new DbContextOptionsBuilder<LearningDbContext>()
            .UseNpgsql("Host=127.0.0.1;Database=pg_blazor_learning;Username=pgblazor_learning;Timeout=1")
            .Options;
        using var db = new LearningDbContext(options);
        // 認可のない環境ではモデル定義のみ調べ、既存台帳の行を参照しない。
        var metadata = db.Model.FindEntityType(typeof(ExistingUserAccessRow));
        Assert.NotNull(metadata);
        Assert.Null(metadata.FindPrimaryKey());
        Assert.Equal("learning_user_access_v", metadata.GetViewName());
    }

    [Fact]
    public async Task PostgreSql18_LinqWhere_LeftJoin_StableSort_ExecutesOnTrainingDatabase()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;
        var options = new DbContextOptionsBuilder<LearningDbContext>().UseNpgsql(connection).Options;
        var prefix = "dbbridge_join_" + Guid.NewGuid().ToString("N");
        var categoryId = 0;
        try
        {
            await using (var seed = new LearningDbContext(options))
            {
                var category = new LearningCategory { Name = prefix };
                seed.Categories.Add(category);
                await seed.SaveChangesAsync();
                categoryId = category.Id;
                seed.Tasks.AddRange(
                    new LearningTask { Title = prefix + "_b", CategoryId = categoryId },
                    new LearningTask { Title = prefix + "_a", CategoryId = categoryId },
                    new LearningTask { Title = prefix + "_z", CategoryId = null });
                await seed.SaveChangesAsync();
            }
            await using (var db = new LearningDbContext(options))
            {
                var rows = await db.Tasks.AsNoTracking()
                    .Where(t => t.Title.Contains(prefix))
                    .OrderBy(t => t.Category!.Name).ThenBy(t => t.Id)
                    .Select(t => new { t.Id, t.Title,
                        Category = t.Category == null ? "未分類" : t.Category.Name })
                    .Take(10).ToListAsync();
                Assert.Equal(3, rows.Count);
                Assert.Contains(rows, x => x.Category == "未分類");
                Assert.Equal(2, rows.Count(x => x.Category == prefix));
                Assert.All(rows, x => Assert.StartsWith(prefix, x.Title));
                Assert.Equal(rows.Count, rows.Select(x => x.Id).Distinct().Count());
            }
        }
        finally
        {
            await using var cleanup = new LearningDbContext(options);
            await cleanup.Tasks.Where(x => x.Title.StartsWith(prefix)).ExecuteDeleteAsync();
            if (categoryId != 0)
                await cleanup.Categories.Where(x => x.Id == categoryId).ExecuteDeleteAsync();
        }
    }
}
