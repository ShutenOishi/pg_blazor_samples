using System.Text;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step56・58：daterangeの重なり、DB側集計、件数制限付きCSV。
/// 学習DB専用。実業務では認証済み本人による行単位の認可を全クエリへ追加する。
/// </summary>
public sealed class LearningRangeExportService(
    IDbContextFactory<LearningDbContext> factory,
    LearningDbService settings,
    ILogger<LearningRangeExportService> logger)
{
    public const int CsvRowLimit = 100;
    public const int SearchRowLimit = 20;

    public static NpgsqlRange<DateTime> CreateInclusiveRange(DateOnly from, DateOnly to)
    {
        if (from > to) throw new ArgumentException("開始日は終了日以前にしてください。");
        if (to == DateOnly.MaxValue) throw new ArgumentOutOfRangeException(nameof(to), "終了日の翌日が表現できません。");
        if (to.DayNumber - from.DayNumber > 366)
            throw new ArgumentOutOfRangeException(nameof(to), "検索期間は367日以内にしてください。");
        // UIでは両端を含む。PostgreSQL daterangeは翌日を上端とする半開区間に直す。
        return new NpgsqlRange<DateTime>(from.ToDateTime(TimeOnly.MinValue), true,
            to.AddDays(1).ToDateTime(TimeOnly.MinValue), false);
    }

    public async Task<LearningRangeResult> FindOverlapsAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var range = CreateInclusiveRange(from, to);
        if (!settings.Configured) return new([], 0, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var query = db.Tasks.AsNoTracking()
                .Where(t => t.AvailableDates.HasValue && t.AvailableDates.Value.Overlaps(range));
            var total = await query.CountAsync(ct);
            var rows = await query.OrderBy(t => t.Id).Take(SearchRowLimit)
                .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
                    t.StartTime, t.IsDone, t.Version)).ToListAsync(ct);
            return new(rows, total, "期間の重なりをDBのGiST対応演算で検索しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "daterange検索に失敗");
            return new([], 0, "期間検索に失敗しました。開発用DBを確認してください。");
        }
    }

    public async Task<LearningSummaryResult> CountByDoneAsync(CancellationToken ct = default)
    {
        if (!settings.Configured) return new([], LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            // 全EntityをToListして集計しない。GroupByをSQLへ変換する。
            var grouped = await db.Tasks.AsNoTracking().GroupBy(t => t.IsDone)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToListAsync(ct);
            // SQL集計後に小さな結果だけをDTOへ写す。SQL翻訳できない後段の式を渡さない。
            var rows = grouped.OrderBy(x => x.Status)
                .Select(x => new LearningCountRow(x.Status, x.Total)).ToArray();
            return new(rows, "DBでGROUP BYとCOUNTを実行しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "集計に失敗");
            return new([], "集計に失敗しました。");
        }
    }

    public async Task<LearningCsvResult> ExportSmallCsvAsync(string? keyword, CancellationToken ct = default)
    {
        if (keyword?.Length > LearningPageService.MaxKeywordLength)
            throw new ArgumentOutOfRangeException(nameof(keyword), "検索語は100文字以内です。");
        if (!settings.Configured) return new(null, 0, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var query = db.Tasks.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(t => t.Title.Contains(keyword.Trim()));
            // 件数オーバーなら部分的なCSVを「全件」と誤認させず拒否する。
            var rows = await query.OrderBy(t => t.Id).Take(CsvRowLimit + 1)
                .Select(t => new { t.Id, t.Title, t.DueDate, t.IsDone }).ToListAsync(ct);
            if (rows.Count > CsvRowLimit)
                return new(null, rows.Count, "101件以上のため出力を中止しました。条件を絞るか専用の大量出力処理を設計してください。");

            var csv = new StringBuilder("\uFEFF番号,課題名,期限日,完了\r\n");
            foreach (var row in rows)
            {
                csv.Append(row.Id).Append(',').Append(EscapeCsv(row.Title)).Append(',')
                    .Append(row.DueDate?.ToString("yyyy-MM-dd") ?? "").Append(',')
                    .Append(row.IsDone ? "はい" : "いいえ").Append("\r\n");
            }
            return new(Convert.ToBase64String(Encoding.UTF8.GetBytes(csv.ToString())), rows.Count,
                $"{rows.Count}件のCSVを用意しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "CSV出力失敗");
            return new(null, 0, "CSVの生成に失敗しました。");
        }
    }

    public static string EscapeCsv(string text)
    {
        // Excel系で先頭が=,+,-,@や制御文字のセルを式として扱わせない。
        var trimmed = text.TrimStart();
        if (trimmed.Length > 0 && ("=+-@".Contains(trimmed[0]) || text.StartsWith('\t') || text.StartsWith('\r') || text.StartsWith('\n')))
            text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
