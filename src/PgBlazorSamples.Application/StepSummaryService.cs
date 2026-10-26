namespace PgBlazorSamples.Application;

/// <summary>UIやDBに依存しない、学習進捗の文章化処理です。</summary>
public sealed class StepSummaryService : IStepSummaryService
{
    public string Describe(int completedSteps)
    {
        if (completedSteps is < 0 or > 60)
            throw new ArgumentOutOfRangeException(nameof(completedSteps), "学習済み数は0〜60の範囲です。");
        return completedSteps == 60
            ? "全60ステップを修了しました。"
            : $"{completedSteps}ステップを学習済み。残り{60 - completedSteps}ステップです。";
    }
}
