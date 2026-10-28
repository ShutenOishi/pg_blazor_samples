using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順059：固定SQL識別子と値パラメーターによる実DB照合。業務台帳は扱わない。</summary>
public sealed class L059ParameterizedSqlService(
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<L059ParameterizedSqlService> logger)
{
    public bool Configured =>
        !string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL"))
        && !configuration.GetValue<bool>("WindowsAuthentication:Enabled");

    public async Task<L059SqlResult> QueryAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        if (!Configured)
            return new(LearningDbService.NotConfigured, [], [], 0, false, false);

        var term = keyword?.Trim() ?? "";
        if (term.Length is < 1 or > 60)
            return new("検索値は1〜60文字で入力してください。", [], [], 0, false, false);

        try
        {
            // WHERE値をSQL文字列に連結しない。識別子は固定値のみ。
            const string sql = """
                SELECT id AS "Id", title AS "Title"
                FROM learning_task
                WHERE title = @Term
                ORDER BY id
                LIMIT 10
                """;
            await using var connection = new NpgsqlConnection(
                configuration.GetConnectionString("PostgreSQL"));
            var dapper = (await connection.QueryAsync<L059TaskRow>(
                new CommandDefinition(sql, new { Term = term },
                    cancellationToken: cancellationToken))).ToList();

            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            // Entityのxminを明示し、EF Coreがモデル全列を安全に読めるようにする。
            var ef = await db.Tasks.FromSqlInterpolated(
                    $"SELECT xmin, * FROM learning_task WHERE title = {term}")
                .AsNoTracking().OrderBy(x => x.Id).Take(10)
                .Select(x => new L059TaskRow { Id = x.Id, Title = x.Title })
                .ToListAsync(cancellationToken);
            var count = await db.Database.SqlQuery<int>(
                    $"SELECT COUNT(*)::int AS \"Value\" FROM learning_task WHERE title = {term}")
                .SingleAsync(cancellationToken);
            var same = count >= ef.Count && dapper.Count == ef.Count
                && dapper.Zip(ef).All(pair =>
                    pair.First.Id == pair.Second.Id && pair.First.Title == pair.Second.Title);
            return new(same
                ? $"学習DBでDapper/FromSql/SqlQueryを実行しました（取得{ef.Count}件、COUNT {count}件）。"
                : "学習DBで照合結果が一致しません。DBの更新状況を確認してください。",
                dapper, ef, count, same, true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習順059のSQL実検索に失敗");
            return new("学習DBの検索に失敗しました。設定とDDLを確認してください。",
                [], [], 0, false, false);
        }
    }
}

public sealed class L059TaskRow
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
}

public sealed record L059SqlResult(
    string Status, IReadOnlyList<L059TaskRow> DapperRows,
    IReadOnlyList<L059TaskRow> EfRows, int EfCount, bool Matches, bool Queried);
