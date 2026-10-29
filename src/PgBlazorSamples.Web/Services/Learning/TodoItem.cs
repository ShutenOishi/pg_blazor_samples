namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>1件の練習課題です。更新時は別インスタンスを作ります。</summary>
public sealed record TodoItem(Guid Id, string Title, bool IsComplete);
