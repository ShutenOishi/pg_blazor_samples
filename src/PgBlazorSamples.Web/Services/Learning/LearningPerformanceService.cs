using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>Step59：開発用DBに対する読み取り専用EXPLAINと小規模な遅延観察。</summary>
public sealed class LearningPerformanceService(
    IDbContextFactory<LearningDbContext> factory,
    LearningDbService settings,
    ILogger<LearningPerformanceService> logger)
{
    public const int Trials = 20;

    public async Task<LearningPerformanceResult> ObserveAsync(CancellationToken ct = default)
    {
        if (!settings.Configured)
            return new(0, 0, [], LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var times = new List<double>();
            // 接続確立を含む実測。キャッシュ・負荷で変動するので本番SLOを表さない。
            for (var i = 0; i < Trials; i++)
            {
                var timer = Stopwatch.StartNew();
                _ = await db.Tasks.AsNoTracking().OrderBy(t => t.DueDate).ThenBy(t => t.Id)
                    .Take(20).Select(t => new { t.Id, t.Title }).ToListAsync(ct);
                timer.Stop();
                times.Add(timer.Elapsed.TotalMilliseconds);
            }
            times.Sort();
            var plan = new List<string>();
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync(ct);
            try
            {
                await using var command = connection.CreateCommand();
                // 値・SQLとも固定。EXPLAIN ANALYZEではなくEXPLAINだけ実行して副作用を防ぐ。
                command.CommandText = "EXPLAIN SELECT id, title FROM learning_task ORDER BY due_date, id LIMIT 20";
                await using var reader = await command.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct)) plan.Add(reader.GetString(0));
            }
            finally { await connection.CloseAsync(); }
            return new(
                times[(int)Math.Ceiling(Trials * 0.95) - 1],
                times[(int)Math.Ceiling(Trials * 0.99) - 1],
                plan, "20回の小規模観察です。計測環境やデータ量で結果が変わります。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "性能観察に失敗");
            return new(0, 0, [], "性能観察に失敗しました。開発用DBを確認してください。");
        }
    }
}
