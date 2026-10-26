namespace PgBlazorSamples.Web.Data;

/// <summary>Step56・58・59：画面へEF Entityや接続を渡さず、読み取り専用の結果を返す。</summary>
public sealed record LearningRangeResult(IReadOnlyList<LearningTaskRow> Rows, int TotalCount, string Message);
public sealed record LearningCountRow(bool IsDone, int Count);
public sealed record LearningSummaryResult(IReadOnlyList<LearningCountRow> Rows, string Message);
public sealed record LearningCsvResult(string? Base64, int Count, string Message);
public sealed record LearningPerformanceResult(double P95Milliseconds, double P99Milliseconds,
    IReadOnlyList<string> PlanLines, string Message);
