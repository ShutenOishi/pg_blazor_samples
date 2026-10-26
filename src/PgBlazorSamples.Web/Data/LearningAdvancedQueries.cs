namespace PgBlazorSamples.Web.Data;

/// <summary>Step46：関連テーブルとDTOの射影。</summary>
public sealed record CategoryTaskRow(long Id, string Title, string CategoryName);
public sealed record CategoryPageResult(IReadOnlyList<CategoryTaskRow> Rows, int TotalCount, string Message);

/// <summary>Step47：キーセットはページ番号ではなく最後に見た一意なIDを渡す。</summary>
public sealed record CursorPageResult(IReadOnlyList<LearningTaskRow> Rows, bool HasMore, long? NextId, string Message);
