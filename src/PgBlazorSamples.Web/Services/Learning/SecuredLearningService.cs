using Microsoft.EntityFrameworkCore;
using System.Data;
using PgBlazorSamples.Web.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step66〜70：認可を画面のボタン表示に委ねず、業務操作の入口で再確認する。
/// 旧Step31〜60のサービスは開発用の学習専用。Windows認証有効時は利用不可。
/// </summary>
public sealed class SecuredLearningService(
    IDbContextFactory<LearningDbContext> factory,
    WindowsUserAccessService access,
    IConfiguration configuration,
    LearningAuditService audit,
    ILogger<SecuredLearningService> logger)
{
    private const string Refused = "Windows認証と既存台帳による操作権限が確認できないため、処理を拒否しました。";
    public const int MaxPageSize = 50;
    public const int MaxCsvRows = 100;

    /// <summary>一度の画面表示だけで許可を確定せず、業務操作ごとに台帳を照会する。</summary>
    public async Task<(string? Unit, string Message)> AuthorizeAsync(
        bool forEdit = false, CancellationToken ct = default)
    {
        if (!configuration.GetValue<bool>("WindowsAuthentication:Enabled") ||
            string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL")))
            return (null, Refused);
        // 別DBやFDWでの権限変更と業務更新を単一トランザクションにできない場合、
        // ローカル台帳との排他契約が実機で確認されるまで更新は拒否する。
        if (forEdit && !configuration.GetValue<bool>("Authorization:TransactionalLedgerConfirmed"))
            return (null, "権限正本のトランザクション連動を確認できないため、更新を拒否しました。");
        // 本人と部署が合っても、監査DBに永続記録できない運用では行データを公開しない。
        if (!configuration.GetValue<bool>(LearningAuditService.AuditConfirmedSetting))
            return (null, "監査の永続化を確認できないため、処理を拒否しました。");
        var state = await access.ReadCurrentAsync(ct);
        var needed = forEdit ? "learning.edit" : WindowsUserAccessService.ReadPermission;
        if (!state.CanRead || state.Permissions?.Contains(needed) != true ||
            state.UnitCode is null ||
            !Regex.IsMatch(state.UnitCode, @"^[A-Za-z0-9_-]{1,32}$", RegexOptions.CultureInvariant))
            return (null, Refused);

        return (state.UnitCode, "既存台帳の権限と所属部署を確認しました。");
    }

    /// <summary>
    /// Step73：正本と業務テーブルが同一PostgreSQL上にあり、ロック可能な単純ビューの場合のみ使用。
    /// SELECT FOR UPDATEで正本の利用者行をロックし、トランザクションが確定するまで失効を競合させる。
    /// FDW/別DBなどロックできない正本では例外となり、更新を拒否する（許可へのフォールバックなし）。
    /// </summary>
    private async Task<bool> CheckLockedEditAsync(LearningDbContext db, string unit, CancellationToken ct)
    {
        var login = await access.ReadTrustedCurrentLoginNameAsync();
        if (login is null) return false;

        // FromSqlInterpolatedを使い、loginは必ずSQLパラメータとして渡す。
        // Take(2)によりビューの正規化衝突も拒否。行ロックは同一DbContextのトランザクションで保持。
        var rows = await db.ExistingUserAccess.FromSqlInterpolated(
                $"SELECT * FROM learning_user_access_v WHERE login_name = {login} FOR UPDATE")
            .AsNoTracking().Take(2).ToListAsync(ct);
        return rows.Count == 1 && rows[0].IsActive && rows[0].CanRead && rows[0].CanEdit &&
               string.Equals(rows[0].UnitCode, unit, StringComparison.Ordinal);
    }

    private static IQueryable<LearningTask> Visible(LearningDbContext db, string unit) =>
        db.Tasks.Where(t => t.UnitCode == unit);

    public async Task<LearningPageResult> LoadPageAsync(
        LearningPageQuery request, CancellationToken ct = default)
    {
        LearningPageService.Validate(request);
        var permit = await AuthorizeAsync(ct: ct);
        if (permit.Unit is null) return new([], 0, permit.Message);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var query = Visible(db, permit.Unit).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
                query = query.Where(t => t.Title.Contains(request.Keyword.Trim()));
            if (request.IsDone is { } completed)
                query = query.Where(t => t.IsDone == completed);
            if (request.DueFrom is { } from)
                query = query.Where(t => t.DueDate >= from);
            if (request.DueTo is { } to)
                query = query.Where(t => t.DueDate <= to);

            // Count/Itemsへ同じ部署・検索条件を適用する。余計な他部署の総件数も返さない。
            var total = await query.CountAsync(ct);
            var rows = await query.OrderBy(t => t.Id)
                .Skip(checked(request.Page * request.PageSize)).Take(request.PageSize)
                .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
                    t.StartTime, t.IsDone, t.Version)).ToListAsync(ct);
            // 監査の永続化失敗時は結果を公開しない。Readは監査を記録してから応答する。
            await audit.WriteRequiredAsync(db, "task.list", permit.Unit, 0, rows.Count, "success", ct);
            return new(rows, total, "許可された部署内の一覧を取得しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "認可済み一覧の取得に失敗");
            return new([], 0, "一覧を取得できませんでした。");
        }
    }

    public async Task<LearningDetailResult> GetDetailAsync(long id, CancellationToken ct = default)
    {
        var permit = await AuthorizeAsync(ct: ct);
        if (permit.Unit is null) return new(null, permit.Message);
        if (id <= 0) return new(null, "正しい番号を入力してください。");
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var detail = await Visible(db, permit.Unit).AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new LearningTaskDetail(t.Id, t.Title,
                    t.Category == null ? "未分類" : t.Category.Name,
                    t.DueDate, t.StartTime, t.IsDone, t.Version))
                .SingleOrDefaultAsync(ct);
            await audit.WriteRequiredAsync(db, "task.detail", permit.Unit, id,
                detail is null ? 0 : 1, detail is null ? "not_found" : "success", ct);
            return detail is null
                ? new(null, "対象データは存在しないか、閲覧権限がありません。")
                : new(detail, "許可された部署の詳細を取得しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "認可済み詳細の取得に失敗");
            return new(null, "詳細を取得できませんでした。");
        }
    }

    public async Task<LearningSummaryResult> CountByDoneAsync(CancellationToken ct = default)
    {
        var permit = await AuthorizeAsync(ct: ct);
        if (permit.Unit is null) return new([], permit.Message);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var groups = await Visible(db, permit.Unit).AsNoTracking()
                .GroupBy(t => t.IsDone)
                .Select(g => new LearningCountRow(g.Key, g.Count())).ToListAsync(ct);
            await audit.WriteRequiredAsync(db, "task.summary", permit.Unit, 0,
                groups.Sum(g => g.Count), "success", ct);
            return new(groups, "許可部署の行のみ集計しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "認可済み集計に失敗");
            return new([], "集計できませんでした。");
        }
    }

    public async Task<LearningCsvResult> ExportCsvAsync(
        string? keyword = null, CancellationToken ct = default)
    {
        if (keyword?.Length > LearningPageService.MaxKeywordLength)
            throw new ArgumentOutOfRangeException(nameof(keyword));
        var permit = await AuthorizeAsync(ct: ct);
        if (permit.Unit is null) return new(null, 0, permit.Message);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var query = Visible(db, permit.Unit).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(t => t.Title.Contains(keyword.Trim()));
            var rows = await query.OrderBy(t => t.Id).Take(MaxCsvRows + 1)
                .Select(t => new { t.Id, t.Title, t.IsDone }).ToListAsync(ct);
            if (rows.Count > MaxCsvRows)
            {
                await audit.WriteRequiredAsync(db, "task.csv", permit.Unit, 0,
                    rows.Count, "limit_rejected", ct);
                return new(null, 0, "101件以上は出力しません。条件を絞ってください。");
            }
            var csv = new StringBuilder("\uFEFF番号,課題名,完了\r\n");
            foreach (var t in rows)
                csv.Append(t.Id).Append(',').Append(LearningRangeExportService.EscapeCsv(t.Title))
                    .Append(',').Append(t.IsDone ? "はい" : "いいえ").Append("\r\n");
            await audit.WriteRequiredAsync(db, "task.csv", permit.Unit, 0, rows.Count, "success", ct);
            logger.LogInformation("CSV出力処理を完了しました。");
            return new(Convert.ToBase64String(Encoding.UTF8.GetBytes(csv.ToString())),
                rows.Count, "許可された行だけをCSVへ出力しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "認可済みCSVの生成に失敗");
            return new(null, 0, "CSVを生成できませんでした。");
        }
    }

    public async Task<LearningEditResult> SaveAsync(
        LearningEditInput input, CancellationToken ct = default)
    {
        var permit = await AuthorizeAsync(forEdit: true, ct: ct);
        if (permit.Unit is null) return new(LearningEditStatus.Forbidden, permit.Message);
        if (input.Id <= 0 || input.Version == 0 ||
            string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 100)
            return new(LearningEditStatus.Invalid, "入力内容が正しくありません。");
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            // 認可確認と更新を同一トランザクションへ固定する。不可なら安全に拒否。
            if (!await CheckLockedEditAsync(db, permit.Unit, ct))
                return new(LearningEditStatus.Forbidden, Refused);
            var task = await Visible(db, permit.Unit).SingleOrDefaultAsync(t => t.Id == input.Id, ct);
            if (task is null) return new(LearningEditStatus.Missing, "対象が存在しないか、編集権限がありません。");
            if (task.Version != input.Version)
                return new(LearningEditStatus.Conflict, "別の利用者が更新しました。再読込してください。");

            // 利用者行のロックを保持中のため、正本の更新はこのコミットまで待機する。
            task.Title = input.Title.Trim();
            task.IsDone = input.IsDone;
            // 監査は同じDBトランザクションにINSERTする。監査保存失敗で課題更新もロールバック。
            await audit.WriteRequiredAsync(db, "task.update", permit.Unit, input.Id, 1, "success", ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("業務監査 課題更新 部署={Unit} 行ID={Id}", permit.Unit, input.Id);
            return new(LearningEditStatus.Success, "認可とxmin競合検査を経て保存しました。",
                new LearningEditInput { Id = task.Id, Title = task.Title,
                    IsDone = task.IsDone, Version = task.Version });
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(LearningEditStatus.Conflict, "更新直前に競合しました。再読込してください。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "認可済み更新に失敗");
            return new(LearningEditStatus.Error, "保存できませんでした。");
        }
    }

    public async Task<LearningEditResult> CompleteSelectedAsync(
        IReadOnlyCollection<LearningSelection> selections, CancellationToken ct = default)
    {
        var permit = await AuthorizeAsync(forEdit: true, ct: ct);
        if (permit.Unit is null) return new(LearningEditStatus.Forbidden, permit.Message);
        if (selections.Count is < 1 or > 50 || selections.Any(s => s.Id <= 0 || s.Version == 0)
            || selections.Select(s => s.Id).Distinct().Count() != selections.Count)
            return new(LearningEditStatus.Invalid, "重複のない1〜50件を選んでください。");
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            if (!await CheckLockedEditAsync(db, permit.Unit, ct))
                return new(LearningEditStatus.Forbidden, Refused);
            var expected = selections.ToDictionary(s => s.Id, s => s.Version);
            var ids = expected.Keys.ToArray();
            var rows = await Visible(db, permit.Unit)
                .Where(t => ids.Contains(t.Id)).ToListAsync(ct);
            if (rows.Count != selections.Count || rows.Any(t => expected[t.Id] != t.Version))
                return new(LearningEditStatus.Conflict, "権限外・削除・競合があるため全件取り消しました。");
            foreach (var row in rows) row.IsDone = true;
            // 1行ごとに同じ相関IDを記録し、一括更新と監査を一つのコミットにまとめる。
            var correlationId = Guid.NewGuid();
            foreach (var row in rows)
                await audit.WriteRequiredAsync(db, "task.bulk_complete", permit.Unit,
                    row.Id, 1, "success", ct, correlationId);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("業務監査 一括更新 部署={Unit} 件数={Count}", permit.Unit, rows.Count);
            return new(LearningEditStatus.Success, $"{rows.Count}件を更新しました。");
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(LearningEditStatus.Conflict, "競合したため一括操作を確定できませんでした。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "認可済み一括更新に失敗");
            return new(LearningEditStatus.Error, "一括操作を確定できませんでした。");
        }
    }
}
