namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順015の一時状態。InstanceTokenはcircuit IDや本人IDではない。
/// </summary>
public sealed class L015CircuitCounter
{
    public string InstanceToken { get; } = Guid.NewGuid().ToString("N");
    public int Count { get; private set; }
    public void Increment() => Count++;
}
