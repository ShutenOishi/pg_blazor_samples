using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Data;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>Step46〜50：PostgreSQL 18の架空データでJOIN・キーセット・仮想スクロールを検証する。</summary>
public sealed class LearningAdvancedIntegrationTests
{
    [Fact]
    public async Task PostgreSql18_Join_Keyset_VirtualWindow_Validation_Cancellation()
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var settings = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:PostgreSQL"] = connection }).Build();
        using var provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(settings)
            .AddLogging()
            .AddDbContextFactory<LearningDbContext>(options => options.UseNpgsql(connection))
            .AddScoped<LearningDbService>()
            .AddScoped<LearningAdvancedPageService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var pages = scope.ServiceProvider.GetRequiredService<LearningAdvancedPageService>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LearningDbContext>>();
        var prefix = "高度一覧試験_" + Guid.NewGuid().ToString("N")[..12] + "_";

        try
        {
            int categoryId;
            await using (var db = await factory.CreateDbContextAsync())
            {
                var category = new LearningCategory { Name = prefix };
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                categoryId = category.Id;
                db.Tasks.AddRange(Enumerable.Range(0, 73).Select(i => new LearningTask
                {
                    Title = prefix + i.ToString("D2"),
                    CategoryId = i % 3 == 0 ? categoryId : null,
                    DueDate = new DateOnly(2026, 11, 1).AddDays(i % 4)
                }));
                await db.SaveChangesAsync();
            }

            // 関連表のDTO射影：未分類と結合された分類名の両方を確認。
            await using (var db = await factory.CreateDbContextAsync())
            {
                var projection = await db.Tasks.AsNoTracking()
                    .Where(t => t.Title.StartsWith(prefix))
                    .OrderBy(t => t.Id)
                    .Select(t => new CategoryTaskRow(t.Id, t.Title,
                        t.Category == null ? "未分類" : t.Category.Name))
                    .ToListAsync();
                Assert.Equal(73, projection.Count);
                Assert.Equal(prefix, projection[0].CategoryName);
                Assert.Equal("未分類", projection[1].CategoryName);
            }
            var categories = await pages.LoadCategoriesAsync();
            Assert.True(categories.TotalCount >= 73);
            Assert.InRange(categories.Rows.Count, 1, 20);

            // IDキーセット：深いページでも重複・欠落がなく、74件目が存在しない。
            var seen = new List<long>();
            long? cursor = null;
            bool hasMore;
            do
            {
                var page = await pages.LoadAfterIdAsync(cursor, 10, prefix);
                Assert.InRange(page.Rows.Count, 1, 10);
                seen.AddRange(page.Rows.Select(x => x.Id));
                Assert.Equal(seen.Count, seen.Distinct().Count());
                Assert.True(page.Rows.Zip(page.Rows.Skip(1), (a, b) => a.Id < b.Id).All(x => x));
                hasMore = page.HasMore;
                cursor = page.NextId;
            } while (hasMore);
            Assert.Equal(73, seen.Count);

            // VirtualizeServerData：StartIndexとCountをDB側へ渡し、範囲が重複しない。
            var first = await pages.LoadWindowAsync(0, 5);
            var next = await pages.LoadWindowAsync(5, 5);
            Assert.True(first.TotalCount >= 73);
            Assert.Equal(first.TotalCount, next.TotalCount);
            Assert.Equal(5, first.Rows.Count);
            Assert.Equal(5, next.Rows.Count);
            Assert.Empty(first.Rows.Select(x => x.Id).Intersect(next.Rows.Select(x => x.Id)));

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => pages.LoadAfterIdAsync(null, 51));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => pages.LoadAfterIdAsync(-1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => pages.LoadWindowAsync(-1, 10));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => pages.LoadWindowAsync(0, 201));
            using var cancel = new CancellationTokenSource();
            cancel.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => pages.LoadWindowAsync(0, 5, cancel.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => pages.LoadAfterIdAsync(null, token: cancel.Token));
        }
        finally
        {
            await using var db = await factory.CreateDbContextAsync();
            await db.Tasks.Where(t => t.Title.StartsWith(prefix)).ExecuteDeleteAsync();
            await db.Categories.Where(c => c.Name == prefix).ExecuteDeleteAsync();
        }
    }
}
