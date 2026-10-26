using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step74：業務本人・操作・対象行・結果・相関ID・UTCをサーバー側で永続記録する。
/// connection stringのapplication_nameだけで本人監査を代用してはいけない。
/// </summary>
public sealed class LearningAuditService(
    WindowsUserAccessService access,
    IConfiguration configuration)
{
    public const string AuditConfirmedSetting = "Authorization:DurableAuditConfirmed";

    public async Task WriteRequiredAsync(LearningDbContext db, string operation,
        string unit, long targetRowId, int rowCount, string outcome,
        CancellationToken ct, Guid? correlationId = null)
    {
        if (!configuration.GetValue<bool>(AuditConfirmedSetting))
            throw new InvalidOperationException("監査の永続保存が確認されていません。");
        var actor = await access.ReadTrustedCurrentLoginNameAsync();
        if (actor is null)
            throw new UnauthorizedAccessException("監査対象のWindows本人を確認できません。");

        // アプリでは時刻と相関IDを発行し、利用者ヘッダーやフォーム入力は使わない。
        // 既存dbのトランザクションに参加するため、更新の成功と監査保存を同時確定できる。
        var id = correlationId ?? Guid.NewGuid();
        var utc = DateTimeOffset.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO learning_operation_audit_ci
                (actor_login, operation, unit_code, target_row_id, row_count, outcome, correlation_id, occurred_utc)
            VALUES
                ({actor}, {operation}, {unit}, {targetRowId}, {rowCount}, {outcome}, {id}, {utc})
            """, ct);
    }
}
