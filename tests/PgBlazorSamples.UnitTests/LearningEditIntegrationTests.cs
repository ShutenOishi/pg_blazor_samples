using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>Step51〜55：PostgreSQL 18の実DBで競合、一括原子性、詳細JOINを否定試験する。</summary>
public sealed class LearningEditIntegrationTests
{
    [Fact]
    public async Task PostgreSql18_Edit_StaleVersion_BulkAtomicity_Detail()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:PostgreSQL"] = connection }).Build();
        using var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(options => options.UseNpgsql(connection))
            .AddScoped<LearningDbService>()
            .AddScoped<LearningEditService>()
            .BuildServiceProvider();
        using var scope = services.CreateScope();
        var edits = scope.ServiceProvider.GetRequiredService<LearningEditService>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var prefix = "編集試験_" + Guid.NewGuid().ToString("N")[..10] + "_";
        long[] ids = [];
        try
        {
            await using (var db = await factory.CreateDbContextAsync())
            {
                var category = new LearningCategory { Name = prefix + "分類" };
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                var tasks = Enumerable.Range(0, 3).Select(i => new LearningTask
                {
                    Title = prefix + i, IsDone = false, CategoryId = category.Id,
                    DueDate = new DateOnly(2026, 11, 1)
                }).ToArray();
                db.Tasks.AddRange(tasks);
                await db.SaveChangesAsync();
                ids = tasks.Select(t => t.Id).ToArray();
            }

            // UI用DTOを読んで、編集成功時にxminが更新されることを確かめる。
            var original = (await edits.LoadAsync(ids[0])).Item!;
            Assert.Equal(prefix + "0", original.Title);
            var update = original.Copy();
            update.Title = prefix + "更新済み";
            var saved = await edits.SaveAsync(update);
            Assert.Equal(LearningEditStatus.Success, saved.Status);
            Assert.True(saved.Current!.Version != original.Version);

            var stale = original.Copy();
            stale.Title = prefix + "意図しない上書き";
            var conflict = await edits.SaveAsync(stale);
            Assert.Equal(LearningEditStatus.Conflict, conflict.Status);
            Assert.Equal(prefix + "更新済み", conflict.Current!.Title);

            var invalid = original.Copy();
            invalid.Title = "  ";
            Assert.Equal(LearningEditStatus.Invalid, (await edits.SaveAsync(invalid)).Status);
            Assert.Equal(LearningEditStatus.Invalid,
                (await edits.CompleteSelectedAsync([new(ids[0], 1), new(ids[0], 1)])).Status);
            Assert.Equal(LearningEditStatus.Invalid,
                (await edits.CompleteSelectedAsync([])).Status);

            // 1件だけ他の画面で更新後、一括操作が一切コミットされないこと。
            var first = (await edits.LoadAsync(ids[0])).Item!;
            var second = (await edits.LoadAsync(ids[1])).Item!;
            var third = (await edits.LoadAsync(ids[2])).Item!;
            await using (var db = await factory.CreateDbContextAsync())
            {
                var changed = await db.Tasks.SingleAsync(t => t.Id == ids[1]);
                changed.Title = prefix + "別ウィンドウで変更";
                await db.SaveChangesAsync();
            }
            var denied = await edits.CompleteSelectedAsync([
                new(ids[0], first.Version),
                new(ids[1], second.Version),
                new(ids[2], third.Version)
            ]);
            Assert.Equal(LearningEditStatus.Conflict, denied.Status);
            await using (var verify = await factory.CreateDbContextAsync())
            {
                Assert.False(await verify.Tasks.Where(t => ids.Contains(t.Id)).AnyAsync(t => t.IsDone));
            }
            second = (await edits.LoadAsync(ids[1])).Item!;
            var accepted = await edits.CompleteSelectedAsync([
                new(ids[0], first.Version),
                new(ids[1], second.Version),
                new(ids[2], third.Version)
            ]);
            Assert.Equal(LearningEditStatus.Success, accepted.Status);
            await using (var verify = await factory.CreateDbContextAsync())
            {
                Assert.Equal(3, await verify.Tasks.CountAsync(t => ids.Contains(t.Id) && t.IsDone));
            }

            var detail = await edits.GetDetailAsync(ids[0]);
            Assert.Equal(prefix + "分類", detail.Item!.CategoryName);
            Assert.True(detail.Item.IsDone);
            Assert.Null((await edits.GetDetailAsync(long.MaxValue)).Item);

            // 削除された行は保存を拒否し、取消はデータに触れない。
            var beforeDelete = (await edits.LoadAsync(ids[2])).Item!;
            await using (var db = await factory.CreateDbContextAsync())
                await db.Tasks.Where(t => t.Id == ids[2]).ExecuteDeleteAsync();
            Assert.Equal(LearningEditStatus.Missing,
                (await edits.SaveAsync(beforeDelete)).Status);

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => edits.LoadAsync(ids[0], cancellation.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => edits.GetDetailAsync(ids[0], cancellation.Token));
        }
        finally
        {
            await using var db = await factory.CreateDbContextAsync();
            await db.Tasks.Where(t => t.Title.StartsWith(prefix)).ExecuteDeleteAsync();
            await db.Categories.Where(t => t.Name.StartsWith(prefix)).ExecuteDeleteAsync();
        }
    }
}
