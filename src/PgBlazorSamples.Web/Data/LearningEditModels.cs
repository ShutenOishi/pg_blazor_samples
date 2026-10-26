using System.ComponentModel.DataAnnotations;

namespace PgBlazorSamples.Web.Data;

/// <summary>Step51：画面入力用DTO。DbContextや追跡中のEntityを回線に保持しない。</summary>
public sealed class LearningEditInput
{
    public long Id { get; set; }

    [Required(ErrorMessage = "課題名を入力してください。")]
    [StringLength(100, ErrorMessage = "課題名は100文字以内で入力してください。")]
    public string Title { get; set; } = "";

    public bool IsDone { get; set; }
    public uint Version { get; set; }

    public LearningEditInput Copy() => new()
    {
        Id = Id, Title = Title, IsDone = IsDone, Version = Version
    };
}

public enum LearningEditStatus { Success, Invalid, Conflict, Missing, NotConfigured, Error }

/// <summary>保存後の状態を曖昧なメッセージ文字列で判定しない。</summary>
public sealed record LearningEditResult(
    LearningEditStatus Status, string Message, LearningEditInput? Current = null);

public sealed record LearningEditLoadResult(LearningEditInput? Item, string Message);

/// <summary>一覧表示時点のxminも一緒に渡す。一括操作の条件はIDだけではない。</summary>
public sealed record LearningSelection(long Id, uint Version);

public sealed record LearningTaskDetail(
    long Id, string Title, string CategoryName, DateOnly? DueDate,
    TimeOnly? StartTime, bool IsDone, uint Version);
public sealed record LearningDetailResult(LearningTaskDetail? Item, string Message);
