namespace PgBlazorSamples.Web.Services.Clock;

/// <summary>練習用に、必ず午前8時を返す時計です。</summary>
public sealed class FixedClock : IClock
{
    public DateTimeOffset GetNow() => new(2026, 1, 1, 8, 0, 0, TimeSpan.FromHours(9));
}
