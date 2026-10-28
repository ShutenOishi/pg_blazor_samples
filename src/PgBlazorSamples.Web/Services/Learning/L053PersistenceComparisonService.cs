using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順053：架空データの取引でDapperとEF Coreの書込みを比較し、必ずロールバックする。</summary>
public sealed class L053PersistenceComparisonService(
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<L053PersistenceComparisonService> logger)
{
    public bool Configured => !string.IsNullOrWhiteSpace(
        configuration.GetConnectionString("PostgreSQL"))
        && !configuration.GetValue<bool>("WindowsAuthentication:Enabled");

    public async Task<L053PersistenceResult> RunAsync(CancellationToken ct = default)
    {
        if (!Configured)
            return new(LearningDbService.NotConfigured, false, 0, 0, 0, "", "", false);
        var dapperTitle = "l053_dapper_" + Guid.NewGuid().ToString("N");
        var efTitle = "l053_ef_" + Guid.NewGuid().ToString("N");
        try
        {
            int affected, found;
            await using (var connection = new NpgsqlConnection(
                configuration.GetConnectionString("PostgreSQL")))
            {
                await connection.OpenAsync(ct);
                await using var tx = await connection.BeginTransactionAsync(ct);
                affected = await connection.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO learning_task (title) VALUES (@Title)",
                    new { Title = dapperTitle }, tx, cancellationToken: ct));
                found = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "SELECT COUNT(*) FROM learning_task WHERE title = @Title",
                    new { Title = dapperTitle }, tx, cancellationToken: ct));
                await tx.RollbackAsync(ct);
            }
            int saved;
            string before, after;
            await using (var db = await factory.CreateDbContextAsync(ct))
            {
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                var task = new LearningTask { Title = efTitle };
                db.Tasks.Add(task);
                before = db.Entry(task).State.ToString();
                saved = await db.SaveChangesAsync(ct);
                after = db.Entry(task).State.ToString();
                if (!await db.Tasks.AsNoTracking().AnyAsync(t => t.Title == efTitle, ct))
                    throw new InvalidOperationException("学習用取引内の行が見つかりません");
                await tx.RollbackAsync(ct);
            }
            await using var check = await factory.CreateDbContextAsync(ct);
            var remained = await check.Tasks.AsNoTracking()
                .AnyAsync(t => t.Title == dapperTitle || t.Title == efTitle, ct);
            var matched = affected == 1 && found == 1 && saved == 1
                && before == "Added" && after == "Unchanged" && !remained;
            return new(matched
                ? "DapperとEF Coreの書込みを取引内で確認し、両方ロールバックしました。"
                : "書込みまたはロールバックの結果が一致しません。",
                true, affected, found, saved, before, after, remained);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習順053のDB取引比較に失敗");
            return new("学習DBの操作に失敗しました。設定とDDLを確認してください。",
                false, 0, 0, 0, "", "", false);
        }
    }
}

public sealed record L053PersistenceResult(
    string Status, bool Queried,
    int DapperAffected, int DapperFound, int EfSaved,
    string EfBefore, string EfAfter, bool RemainedAfterRollback);
