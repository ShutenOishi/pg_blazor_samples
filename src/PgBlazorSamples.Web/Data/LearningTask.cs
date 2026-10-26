using NpgsqlTypes;

namespace PgBlazorSamples.Web.Data;

/// <summary>教材用の架空課題。最初はWebプロジェクトのDataフォルダーに置く。</summary>
public sealed class LearningTask
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public DateOnly? DueDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public NpgsqlRange<DateTime>? AvailableDates { get; set; }
    public bool IsDone { get; set; }
    // Step67：一覧・件数・詳細・CSV・更新で共通に利用する所属部署キー。
    public string UnitCode { get; set; } = "DEMO";
    // Step46：必要な列だけJOINするための任意の関連。
    public int? CategoryId { get; set; }
    public LearningCategory? Category { get; set; }

    // PostgreSQLのxminを使った楽観的排他。自分で値を更新しない。
    public uint Version { get; set; }
}

public sealed record LearningTaskRow(
    long Id, string Title, DateOnly? DueDate, TimeOnly? StartTime, bool IsDone, uint Version);

public sealed record LearningTaskResult(
    string Message, IReadOnlyList<LearningTaskRow> Rows, int TotalCount = 0);
