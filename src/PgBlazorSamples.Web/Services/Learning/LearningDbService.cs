using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step31〜40に共通するDB操作。画面へDbContextを渡さず、毎操作Factoryから生成する。
/// サンプルなので外部プロジェクトやRepositoryの階層を増設しない。
/// </summary>
public sealed class LearningDbService(
    IDbContextFactory<LearningDbContext> factory,
    IConfiguration configuration,
    ILogger<LearningDbService> logger)
{
    public bool Configured => !string.IsNullOrWhiteSpace(
        configuration.GetConnectionString("PostgreSQL"))
        && !configuration.GetValue<bool>("WindowsAuthentication:Enabled");

    // Step66：Windows認証を有効化した場合、旧教材の無認可サービスはDB操作を許さない。
    // 実業務の例は必ずSecuredLearningServiceを経由する。

    public const string NotConfigured =
        "PostgreSQLは未設定です。開発用DBを作成し、ConnectionStrings__PostgreSQLを設定してください。";

    public async Task<LearningTaskResult> SearchAsync(
        string? keyword = null, CancellationToken cancellationToken = default)
    {
        if (!Configured) return new(NotConfigured, []);
        try
        {
            await using var db = await factory.CreateDbContextAsync(cancellationToken);
            var query = db.Tasks.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(t => t.Title.Contains(term));
            }
            int count = await query.CountAsync(cancellationToken);
            var rows = await query.OrderBy(t => t.Id).Take(20)
                .Select(t => new LearningTaskRow(
                    t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
                .ToListAsync(cancellationToken);
            return new("DBから最大20件を取得しました。", rows, count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "開発用PostgreSQLの一覧取得に失敗しました。");
            return new("DBへ接続できません。設定とDDLの適用を確認してください。", []);
        }
    }

    public async Task<string> AddAsync(string title, CancellationToken ct = default)
    {
        if (!Configured) return NotConfigured;
        title = title.Trim();
        if (title.Length is < 1 or > 100) return "課題名は1〜100文字で入力してください。";
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            db.Tasks.Add(new LearningTask
            {
                Title = title,
                DueDate = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeOnly.FromDateTime(DateTime.Now)
            });
            await db.SaveChangesAsync(ct);
            return "登録しました。";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習用の登録に失敗しました。");
            return "登録に失敗しました。DB設定と入力を確認してください。";
        }
    }

    public async Task<string> ToggleAsync(long id, uint version, CancellationToken ct = default)
    {
        if (!Configured) return NotConfigured;
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var task = await db.Tasks.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (task is null) return "対象が削除されています。再読み込みしてください。";
            if (task.Version != version)
                return "別の利用者が更新しました。再読み込みしてください。";
            task.IsDone = !task.IsDone;
            await db.SaveChangesAsync(ct);
            return "状態を更新しました。";
        }
        catch (DbUpdateConcurrencyException)
        {
            return "競合を検出しました。再読み込みしてください。";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習用の更新に失敗しました。");
            return "更新できませんでした。DBの状態を確認してください。";
        }
    }

    public async Task<string> DeleteAsync(long id, uint version, CancellationToken ct = default)
    {
        if (!Configured) return NotConfigured;
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == id, ct);
            if (task is null) return "既に削除されています。";
            if (task.Version != version) return "競合しました。再読み込みしてください。";
            db.Tasks.Remove(task);
            await db.SaveChangesAsync(ct);
            return "削除しました。";
        }
        catch (DbUpdateConcurrencyException) { return "競合しました。再読み込みしてください。"; }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習用の削除に失敗しました。");
            return "削除できませんでした。";
        }
    }

    public async Task<string> AddTwoInTransactionAsync(CancellationToken ct = default)
    {
        if (!Configured) return NotConfigured;
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            db.Tasks.Add(new LearningTask { Title = "取引の練習A" });
            db.Tasks.Add(new LearningTask { Title = "取引の練習B" });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return "2件を1つのトランザクションで登録しました。";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "学習用トランザクションに失敗しました。");
            return "トランザクションを完了できませんでした。";
        }
    }
}
