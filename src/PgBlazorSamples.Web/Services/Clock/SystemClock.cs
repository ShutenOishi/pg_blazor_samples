namespace PgBlazorSamples.Web.Services.Clock;

/// <summary>実際のサーバー時刻を取得する実装です。</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset GetNow() => DateTimeOffset.Now;
}
