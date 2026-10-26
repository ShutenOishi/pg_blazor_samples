namespace PgBlazorSamples.Web.Services.Lifetime;

/// <summary>同じBlazor回線内の複数部品で共有する練習用状態です。</summary>
public sealed class ScopedCounter
{
    public int Count { get; private set; }
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}
