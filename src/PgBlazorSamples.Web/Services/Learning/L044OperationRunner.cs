namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順044。副作用のない模擬処理で、取消・二重開始・破棄の状態を観察する。
/// DBの取消・ロールバックや業務再試行の安全性を証明するものではない。
/// </summary>
public sealed class L044OperationRunner : IDisposable
{
    private readonly Func<CancellationToken, Task> operation;
    private CancellationTokenSource? pending;
    private bool disposed;

    public L044OperationRunner(Func<CancellationToken, Task>? operation = null)
    {
        this.operation = operation ?? (token => Task.Delay(2500, token));
    }

    public bool Busy => pending is not null;
    public int StartCount { get; private set; }
    public string State { get; private set; } = "未開始";

    public async Task<bool> TryStartAsync()
    {
        if (disposed || pending is not null) return false;
        var current = new CancellationTokenSource();
        pending = current;
        StartCount++;
        State = "処理中";
        try
        {
            await operation(current.Token);
            State = current.IsCancellationRequested ? "処理を取り消しました。" : "処理が完了しました。";
        }
        catch (OperationCanceledException) when (current.IsCancellationRequested)
        {
            State = "処理を取り消しました。";
        }
        finally
        {
            if (ReferenceEquals(pending, current))
                pending = null;
            current.Dispose();
        }
        return true;
    }

    public bool Cancel()
    {
        if (pending is null) return false;
        pending.Cancel();
        return true;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        pending?.Cancel(); // ページ離脱時は通知する。既確定の更新を巻き戻さない。
    }
}
