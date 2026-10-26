namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習用設定。画面が設定ファイルの読み込み処理を持たなくてよいようにします。</summary>
public sealed class LearningOptions
{
    public string CourseTitle { get; set; } = "Blazorステップアップ学習";
    public int MaxTasks { get; set; } = 5;
}
