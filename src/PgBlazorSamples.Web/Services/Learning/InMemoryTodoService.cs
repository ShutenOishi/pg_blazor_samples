using Microsoft.Extensions.Options;
namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>DBを使わない演習用の実装です。Scopedで登録し、利用者間で状態を共有しません。</summary>
public sealed class InMemoryTodoService : ITodoService
{
    private readonly List<TodoItem> _items =
    [
        new(Guid.NewGuid(), "DIの登録を確認する", false),
        new(Guid.NewGuid(), "ライブラリの利用手順を読む", false)
    ];
    private readonly int _limit;

    public InMemoryTodoService(IOptions<LearningOptions> options)
    {
        // 設定値が想定外でも、練習用リストの上限を安全な範囲に収めます。
        _limit = Math.Clamp(options.Value.MaxTasks, 1, 20);
    }

    public IReadOnlyList<TodoItem> GetAll() => _items.AsReadOnly();

    public bool TryAdd(string? title, out string message)
    {
        var trimmed = title?.Trim() ?? "";
        if (trimmed.Length is < 1 or > 40)
        {
            message = "課題名は1〜40文字で入力してください。";
            return false;
        }
        if (_items.Count >= _limit)
        {
            message = $"課題は最大{_limit}件までです。";
            return false;
        }
        _items.Add(new TodoItem(Guid.NewGuid(), trimmed, false));
        message = "課題を追加しました。";
        return true;
    }

    public bool Toggle(Guid id)
    {
        var index = _items.FindIndex(item => item.Id == id);
        if (index < 0) return false;
        _items[index] = _items[index] with { IsComplete = !_items[index].IsComplete };
        return true;
    }
}
