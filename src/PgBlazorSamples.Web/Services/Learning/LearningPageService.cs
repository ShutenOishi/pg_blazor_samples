using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step41〜60の一覧取得をWeb内で完結する。Grid固有型を持ち込まず、必要な行だけDBから取得。
/// 本番業務で使う前にこのクエリへ利用者権限によるWhereを追加する必要がある。
/// </summary>
public sealed class LearningPageService(
    IDbContextFactory<LearningDbContext> factory,
    LearningDbService connection,
    ILogger<LearningPageService> logger)
{
    public const int MaxPageSize = 50;
    public const int MaxKeywordLength = 100;

    // UIの列名を動的SQL式へ組み立てない。許可されたプロパティにのみ振り分ける。
    public static bool IsAllowedSort(string column) =>
        column is "Id" or "Title" or "DueDate" or "StartTime" or "IsDone";

    public static void Validate(LearningPageQuery request)
    {
        if (request.Page < 0) throw new ArgumentOutOfRangeException(nameof(request.Page));
        if (request.PageSize is < 1 or > MaxPageSize)
            throw new ArgumentOutOfRangeException(nameof(request.PageSize));
        _ = checked(request.Page * request.PageSize);
        if (request.Keyword?.Length > MaxKeywordLength)
            throw new ArgumentOutOfRangeException(nameof(request.Keyword));
        if (request.DueFrom > request.DueTo)
            throw new ArgumentException("開始日は終了日以前にしてください。");
        if (request.Sorts?.Count > 3)
            throw new ArgumentException("同時ソートは最大3列です。");
        if (request.Sorts is not null && request.Sorts.Any(x => !IsAllowedSort(x.Column)))
            throw new ArgumentException("許可されていないソート列です。");
    }

    public async Task<LearningPageResult> LoadAsync(
        LearningPageQuery request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        if (!connection.Configured)
            return new([], 0, LearningDbService.NotConfigured);

        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var query = db.Tasks.AsNoTracking().AsQueryable();

            // 正規化されていない生のSQL断片を連結しない。EF Coreが値をパラメーター化する。
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(t => t.Title.Contains(keyword));
            }
            if (request.IsDone is { } done)
                query = query.Where(t => t.IsDone == done);
            if (request.DueFrom is { } from)
                query = query.Where(t => t.DueDate >= from);
            if (request.DueTo is { } to)
                query = query.Where(t => t.DueDate <= to);

            // 件数にも検索・権限・期間の同じ絞り込みが適用される必要がある。
            int total = await query.CountAsync(cancellationToken);

            IOrderedQueryable<LearningTask>? sorted = null;
            foreach (var sort in request.Sorts ?? [])
            {
                sorted = AddOrder(sorted, query, sort);
            }
            // 同値でも安定したページ区切りになるよう最後に一意のIDを追加。
            sorted = sorted is null
                ? query.OrderBy(t => t.Id)
                : sorted.ThenBy(t => t.Id);

            var offset = checked(request.Page * request.PageSize);
            var rows = await sorted.Skip(offset).Take(request.PageSize)
                .Select(t => new LearningTaskRow(
                    t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
                .ToListAsync(cancellationToken);
            return new(rows, total, "DB側で絞り込み・ページングしました。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // 次の検索に切り替わったときは古い結果を表示しない。
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "一覧のページ取得に失敗");
            return new([], 0, "一覧の読み込みに失敗しました。開発用DBの状態を確認してください。");
        }
    }

    private static IOrderedQueryable<LearningTask> AddOrder(
        IOrderedQueryable<LearningTask>? sorted,
        IQueryable<LearningTask> source,
        LearningSort sort)
    {
        // 各列に対するEF Coreの式木を静的なswitchで選ぶ。未許可列はValidateで拒否。
        return sort.Column switch
        {
            "Title" => sorted is null
                ? (sort.Descending ? source.OrderByDescending(t => t.Title) : source.OrderBy(t => t.Title))
                : (sort.Descending ? sorted.ThenByDescending(t => t.Title) : sorted.ThenBy(t => t.Title)),
            "DueDate" => sorted is null
                ? (sort.Descending ? source.OrderByDescending(t => t.DueDate) : source.OrderBy(t => t.DueDate))
                : (sort.Descending ? sorted.ThenByDescending(t => t.DueDate) : sorted.ThenBy(t => t.DueDate)),
            "StartTime" => sorted is null
                ? (sort.Descending ? source.OrderByDescending(t => t.StartTime) : source.OrderBy(t => t.StartTime))
                : (sort.Descending ? sorted.ThenByDescending(t => t.StartTime) : sorted.ThenBy(t => t.StartTime)),
            "IsDone" => sorted is null
                ? (sort.Descending ? source.OrderByDescending(t => t.IsDone) : source.OrderBy(t => t.IsDone))
                : (sort.Descending ? sorted.ThenByDescending(t => t.IsDone) : sorted.ThenBy(t => t.IsDone)),
            "Id" => sorted is null
                ? (sort.Descending ? source.OrderByDescending(t => t.Id) : source.OrderBy(t => t.Id))
                : (sort.Descending ? sorted.ThenByDescending(t => t.Id) : sorted.ThenBy(t => t.Id)),
            _ => throw new ArgumentException("許可されていないソート列です。")
        };
    }
}
