namespace PgBlazorSamples.Web.Data;

/// <summary>学習順058。利用者台帳を参照しない固定の架空ビュー行。</summary>
public sealed class LearningKeylessDemoRow
{
    public int SortOrder { get; set; }
    public string Label { get; set; } = "";
}
