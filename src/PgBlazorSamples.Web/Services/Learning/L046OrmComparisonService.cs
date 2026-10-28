using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順046：匿名の開発用データに限定した、DapperとEF Coreの同条件実検索。</summary>
public sealed class L046OrmComparisonService(
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<L046OrmComparisonService> logger)
{
    public bool Configured =>
        !string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL"))
        && !configuration.GetValue<bool>("WindowsAuthentication:Enabled");

    public async Task<L046ComparisonResult> CompareAsync(
        string? keyword, CancellationToken cancellationToken = default)
    {
        if (!Configured)
            return new(LearningDbService.NotConfigured, [], [], false, false);

        var term = keyword?.Trim() ?? "";
        if (term.Length is < 1 or > 60)
            return new("検索語は1〜60文字で入力してください。", [], [], false, false);

        try
        {
            // SQL識別子は固定し、検索文字列は必ずパラメーターとして渡す。
            const string sql = """
                SELECT id AS "Id", title AS "Title"
                FROM learning_task
                WHERE strpos(title, @Term) > 0
                ORDER BY id
                LIMIT 10
                """;
            await using var connection = new NpgsqlConnection(
                configuration.GetConnectionString("PostgreSQL"));
            var dapperRows = (await connection.QueryAsync<L046TaskRow>(
                new CommandDefinition(sql, new { Term = term },
                    cancellationToken: cancellationToken))).ToList();

            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var efRows = await db.Tasks.AsNoTracking()
                .Where(t => t.Title.Contains(term))
                .OrderBy(t => t.Id)
                .Take(10)
                .Select(t => new L046TaskRow { Id = t.Id, Title = t.Title })
                .ToListAsync(cancellationToken);
            var same = dapperRows.Count == efRows.Count
                && dapperRows.Zip(efRows).All(pair =>
                    pair.First.Id == pair.Second.Id
                    && pair.First.Title == pair.Second.Title);
            return new(same
                    ? $"学習DBで両方式の検索を実行しました（各{efRows.Count}件、一致）。"
                    : "両方式の結果が一致しません。DBの更新状況を確認してください。",
                dapperRows, efRows, same, true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習順046のDapper/EF Core比較に失敗");
            // 接続文字列やSQLの内部例外はブラウザーに公開しない。
            return new("学習DBへの検索に失敗しました。設定とDDLを確認してください。",
                [], [], false, false);
        }
    }
}

public sealed class L046TaskRow
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
}

public sealed record L046ComparisonResult(
    string Status,
    IReadOnlyList<L046TaskRow> DapperRows,
    IReadOnlyList<L046TaskRow> EfRows,
    bool Matches,
    bool Queried);
