using System.ComponentModel.DataAnnotations;

namespace PgBlazorSamples.Web.Components.Samples.Step10;

/// <summary>
/// データベースへ保存しない、フォーム入力検証専用の練習モデルです。
/// </summary>
public sealed class PracticeEntry
{
    [Required(ErrorMessage = "表示名を入力してください。")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "表示名は2文字以上20文字以内で入力してください。")]
    public string DisplayName { get; set; } = "";

    [Required(ErrorMessage = "学習メモを入力してください。")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "学習メモは3文字以上80文字以内で入力してください。")]
    public string LearningMemo { get; set; } = "";
}
