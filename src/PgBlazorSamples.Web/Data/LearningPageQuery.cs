namespace PgBlazorSamples.Web.Data;

/// <summary>サーバー側のページング・検索・ソートを表す。UI部品には依存しない。</summary>
public sealed record LearningSort(string Column, bool Descending = false);

public sealed record LearningPageQuery(
    int Page = 0,
    int PageSize = 5,
    string? Keyword = null,
    bool? IsDone = null,
    DateOnly? DueFrom = null,
    DateOnly? DueTo = null,
    IReadOnlyList<LearningSort>? Sorts = null);

public sealed record LearningPageResult(
    IReadOnlyList<LearningTaskRow> Rows,
    int TotalCount,
    string Message);
