using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順058。学習用固定ビューのみを読み、実在利用者の認可台帳には触れない。</summary>
public sealed class L058KeylessViewService(
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<L058KeylessViewService> logger)
{
    public bool Configured =>
        !string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL"))
        && !configuration.GetValue<bool>("WindowsAuthentication:Enabled");

    public async Task<L058ViewResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!Configured)
            return new(LearningDbService.NotConfigured, [], false);

        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var rows = await db.DemoView.AsNoTracking()
                .OrderBy(row => row.SortOrder)
                .Take(10)
                .ToListAsync(cancellationToken);
            return new($"学習用PostgreSQLビューから架空データ{rows.Count}件を読み取りました（更新なし）。",
                rows, true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習順058の学習用キーなしビュー読取に失敗");
            return new("学習用ビューの読取に失敗しました。接続設定とDDLを確認してください。",
                [], false);
        }
    }
}

public sealed record L058ViewResult(
    string Status, IReadOnlyList<LearningKeylessDemoRow> Rows, bool Queried);
