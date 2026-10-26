using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step46〜49：既存Web内のEF Core読み取りサービス。
/// DbContextは操作ごとに作成し、UIにはIQueryableを渡さない。
/// 業務認可はStep61〜70で学ぶ。実運用時は各クエリのCountとRowsの双方へ認可条件を適用する。
/// </summary>
public sealed class LearningAdvancedPageService(
    IDbContextFactory<LearningDbContext> factory,
    LearningDbService settings,
    ILogger<LearningAdvancedPageService> logger)
{
    public const int MaxWindowSize = 200;

    public async Task<CategoryPageResult> LoadCategoriesAsync(CancellationToken token = default)
    {
        if (!settings.Configured) return new([], 0, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(token);
            // 関連エンティティを行ごとに照会しない。必要な列だけをLEFT JOINでDTOへ射影。
            var query = db.Tasks.AsNoTracking();
            var total = await query.CountAsync(token);
            var rows = await query.OrderBy(t => t.Id).Take(20)
                .Select(t => new CategoryTaskRow(t.Id, t.Title,
                    t.Category == null ? "未分類" : t.Category.Name))
                .ToListAsync(token);
            return new(rows, total, "関連テーブルをDB側で結合しました（先頭20件）。");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "関連テーブルの一覧取得失敗");
            return new([], 0, "関連テーブルの読み込みに失敗しました。");
        }
    }

    public async Task<CursorPageResult> LoadAfterIdAsync(
        long? afterId, int size = 10, string? keyword = null, CancellationToken token = default)
    {
        if (afterId < 0) throw new ArgumentOutOfRangeException(nameof(afterId));
        if (size is < 1 or > LearningPageService.MaxPageSize)
            throw new ArgumentOutOfRangeException(nameof(size));
        if (keyword?.Length > LearningPageService.MaxKeywordLength)
            throw new ArgumentOutOfRangeException(nameof(keyword));
        if (!settings.Configured) return new([], false, null, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(token);
            var query = db.Tasks.AsNoTracking();
            if (afterId is { } cursor) query = query.Where(t => t.Id > cursor);
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(t => t.Title.Contains(keyword.Trim()));
            // size+1件で次ページの有無を判定。深いSkipや不要な全件Countを避ける。
            var fetched = await query.OrderBy(t => t.Id).Take(size + 1)
                .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate, t.StartTime,
                    t.IsDone, t.Version)).ToListAsync(token);
            var rows = fetched.Take(size).ToArray();
            return new(rows, fetched.Count > size, rows.Length == 0 ? null : rows[^1].Id,
                "一意な番号をカーソルにして取得しました。");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "キーセットページ取得失敗");
            return new([], false, null, "次の行の取得に失敗しました。");
        }
    }

    public async Task<LearningPageResult> LoadWindowAsync(
        int startIndex, int count, CancellationToken token = default)
    {
        if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count is < 1 or > MaxWindowSize) throw new ArgumentOutOfRangeException(nameof(count));
        _ = checked(startIndex + count);
        if (!settings.Configured) return new([], 0, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(token);
            var query = db.Tasks.AsNoTracking();
            var total = await query.CountAsync(token);
            // VirtualizeServerDataのStartIndex/Countを使い、DB側だけで範囲を取得する。
            var rows = await query.OrderBy(t => t.Id).Skip(startIndex).Take(count)
                .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate, t.StartTime,
                    t.IsDone, t.Version)).ToListAsync(token);
            return new(rows, total, "表示範囲のみDBから取得しました。");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "仮想スクロール取得失敗");
            return new([], 0, "スクロール範囲の読み込みに失敗しました。");
        }
    }
}
