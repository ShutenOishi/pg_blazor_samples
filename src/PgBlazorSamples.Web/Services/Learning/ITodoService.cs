namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>画面が利用する課題操作の契約です。保存方法には依存しません。</summary>
public interface ITodoService
{
    IReadOnlyList<TodoItem> GetAll();
    bool TryAdd(string? title, out string message);
    bool Toggle(Guid id);
}
