using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step51〜55：Web内だけで編集・排他・一括処理・詳細取得を学習するサービス。
/// 認証・業務認可はStep61〜70で学ぶ。実業務へ転用する場合は読み取りと保存の
/// 双方に対象行単位の認可判定を追加すること。本教材は開発用PostgreSQL専用。
/// </summary>
public sealed class LearningEditService(
    IDbContextFactory<LearningDbContext> factory,
    LearningDbService settings,
    ILogger<LearningEditService> logger)
{
    public const int MaximumSelection = 50;

    public async Task<LearningEditLoadResult> LoadAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return new(null, "正しい番号を指定してください。");
        if (!settings.Configured) return new(null, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var row = await db.Tasks.AsNoTracking().Where(t => t.Id == id)
                .Select(t => new LearningEditInput
                {
                    Id = t.Id, Title = t.Title, IsDone = t.IsDone, Version = t.Version
                }).SingleOrDefaultAsync(ct);
            return row is null
                ? new(null, "対象が削除されています。再読み込みしてください。")
                : new(row, "編集用のDTOを読み込みました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "学習用編集DTOの取得に失敗");
            return new(null, "読み込みに失敗しました。");
        }
    }

    public async Task<LearningEditResult> SaveAsync(LearningEditInput input, CancellationToken ct = default)
    {
        if (!settings.Configured)
            return new(LearningEditStatus.NotConfigured, LearningDbService.NotConfigured);
        if (input.Id <= 0 || input.Version == 0 || string.IsNullOrWhiteSpace(input.Title)
            || input.Title.Trim().Length > 100)
            return new(LearningEditStatus.Invalid, "課題名は1〜100文字、番号と版は有効な値が必要です。");

        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var task = await db.Tasks.SingleOrDefaultAsync(x => x.Id == input.Id, ct);
            if (task is null)
                return new(LearningEditStatus.Missing, "編集中に対象が削除されました。");
            if (task.Version != input.Version)
                return new(LearningEditStatus.Conflict,
                    "他の操作で更新されました。最新状態を確認してから編集し直してください。",
                    ToInput(task));

            task.Title = input.Title.Trim();
            task.IsDone = input.IsDone;
            await db.SaveChangesAsync(ct); // DBでxminが変わった場合もEFが検出する。
            return new(LearningEditStatus.Success, "保存しました。", ToInput(task));
        }
        catch (DbUpdateConcurrencyException)
        {
            // 読み取り後〜UPDATEまでの短い競合も、勝手に再試行して上書きしない。
            var latest = await LoadAsync(input.Id, ct);
            return new(latest.Item is null ? LearningEditStatus.Missing : LearningEditStatus.Conflict,
                "保存直前に競合しました。最新状態を確認してください。", latest.Item);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "学習用編集の保存に失敗");
            return new(LearningEditStatus.Error, "保存できませんでした。DBを確認してください。");
        }
    }

    public async Task<LearningEditResult> CompleteSelectedAsync(
        IReadOnlyCollection<LearningSelection> selections, CancellationToken ct = default)
    {
        if (!settings.Configured)
            return new(LearningEditStatus.NotConfigured, LearningDbService.NotConfigured);
        if (selections.Count is < 1 or > MaximumSelection
            || selections.Any(x => x.Id <= 0 || x.Version == 0)
            || selections.Select(x => x.Id).Distinct().Count() != selections.Count)
            return new(LearningEditStatus.Invalid, "選択は重複のない1〜50件にしてください。");

        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var expected = selections.ToDictionary(s => s.Id, s => s.Version);
            var ids = expected.Keys.ToArray();
            var tasks = await db.Tasks.Where(t => ids.Contains(t.Id)).ToListAsync(ct);

            // 1件でも削除・変更されていれば何も保存しない。部分成功を隠さない。
            if (tasks.Count != selections.Count
                || tasks.Any(t => expected[t.Id] != t.Version))
                return new(LearningEditStatus.Conflict,
                    "削除または更新された行があります。0件更新しました。選択を解除して読み直してください。");

            foreach (var task in tasks) task.IsDone = true;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return new(LearningEditStatus.Success, $"{tasks.Count}件をまとめて完了にしました。");
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(LearningEditStatus.Conflict,
                "一括保存直前に競合しました。0件確定しました。再読み込みしてください。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "学習用一括更新に失敗");
            return new(LearningEditStatus.Error, "一括処理を確定できませんでした。");
        }
    }

    public async Task<LearningDetailResult> GetDetailAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return new(null, "正しい番号を指定してください。");
        if (!settings.Configured) return new(null, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            // AsNoTracking + Selectで必要な列だけをDBでJOIN。行ごとに追加SQLを発行しない。
            var detail = await db.Tasks.AsNoTracking().Where(t => t.Id == id)
                .Select(t => new LearningTaskDetail(t.Id, t.Title,
                    t.Category == null ? "未分類" : t.Category.Name,
                    t.DueDate, t.StartTime, t.IsDone, t.Version))
                .SingleOrDefaultAsync(ct);
            return detail is null
                ? new(null, "詳細対象が削除されています。")
                : new(detail, "詳細を1回の問い合わせで取得しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "学習用の詳細取得失敗");
            return new(null, "詳細を取得できませんでした。");
        }
    }

    private static LearningEditInput ToInput(LearningTask task) => new()
    {
        Id = task.Id, Title = task.Title, IsDone = task.IsDone, Version = task.Version
    };
}
