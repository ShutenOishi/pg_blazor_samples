namespace PgBlazorSamples.Application;

/// <summary>学習進捗を文章にする処理の約束です。</summary>
public interface IStepSummaryService
{
    string Describe(int completedSteps);
}
