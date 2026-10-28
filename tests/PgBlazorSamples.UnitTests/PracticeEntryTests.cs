using System.ComponentModel.DataAnnotations;
using PgBlazorSamples.Web.Components.Samples.Step10;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順018：入力検証用モデルの正常値と境界値を検査する。</summary>
public sealed class PracticeEntryTests
{
    [Fact]
    public void 正しい表示名と学習メモは検証に合格する()
    {
        Assert.Empty(Validate(new PracticeEntry { DisplayName = "練習太郎", LearningMemo = "Blazorを学ぶ" }));
    }

    [Fact]
    public void 必須の項目を省略すると双方のエラーになる()
    {
        var errors = Validate(new PracticeEntry());
        Assert.Contains(errors, x => x.ErrorMessage == "表示名を入力してください。");
        Assert.Contains(errors, x => x.ErrorMessage == "学習メモを入力してください。");
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(20, true)]
    [InlineData(21, false)]
    public void 表示名の文字数境界を検査する(int length, bool valid)
    {
        var errors = Validate(new PracticeEntry { DisplayName = new string('あ', length), LearningMemo = "正常なメモ" });
        Assert.Equal(valid, errors.Count == 0);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(80, true)]
    [InlineData(81, false)]
    public void 学習メモの文字数境界を検査する(int length, bool valid)
    {
        var errors = Validate(new PracticeEntry { DisplayName = "練習太郎", LearningMemo = new string('あ', length) });
        Assert.Equal(valid, errors.Count == 0);
    }

    private static List<ValidationResult> Validate(PracticeEntry entry)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(entry, new ValidationContext(entry), errors, validateAllProperties: true);
        return errors;
    }
}
