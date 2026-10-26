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
                $"SELECT * FROM learning_task WHERE title = {attack}").AsNoTracking().ToListAsync();
            Assert.Empty(found);
            var counted = await db.Database.SqlQuery<int>(
                $"SELECT COUNT(*)::int AS \"Value\" FROM learning_task WHERE title = {attack}").SingleAsync();
            Assert.Equal(0, counted);
            var expected = await db.Tasks.FromSqlInterpolated(
                $"SELECT * FROM learning_task WHERE title = {title}").AsNoTracking().SingleAsync();
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
}
