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

    // PostgreSQLのxminを使った楽観的排他。自分で値を更新しない。
    public uint Version { get; set; }
}

public sealed record LearningTaskRow(
    long Id, string Title, DateOnly? DueDate, TimeOnly? StartTime, bool IsDone, uint Version);

public sealed record LearningTaskResult(
    string Message, IReadOnlyList<LearningTaskRow> Rows, int TotalCount = 0);
