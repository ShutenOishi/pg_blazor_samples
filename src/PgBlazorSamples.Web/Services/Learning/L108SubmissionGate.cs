using System.Threading;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順108：同じ画面の保存イベントが同時に二重起動することを防ぐ。DB排他・認可の代替ではない。</summary>
public sealed class L108SubmissionGate
{
    private int _busy;
    public bool IsBusy => Volatile.Read(ref _busy) != 0;
    public bool TryBegin() => Interlocked.CompareExchange(ref _busy, 1, 0) == 0;
    public void End() => Volatile.Write(ref _busy, 0);
}
