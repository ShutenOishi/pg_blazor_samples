namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順106：画面の要件選択を業務サービスの入口へ対応付ける。
/// 委譲先はRazorが既存SecuredLearningServiceの認可・監査付き操作として渡す。
/// 不明な選択ではいずれの操作も実行しない。
/// </summary>
public static class L106RequirementFlow
{
    public static Task<string> ExecuteAsync(
        string? requirement,
        Func<Task<string>> list,
        Func<Task<string>> edit,
        Func<Task<string>> export) =>
        requirement switch
        {
            "list" => list(),
            "edit" => edit(),
            "export" => export(),
            _ => Task.FromResult("未知の要件のためサービスを実行せず拒否しました。")
        };
}
