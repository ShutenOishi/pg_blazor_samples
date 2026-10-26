namespace PgBlazorSamples.Web.Services.Clock;

/// <summary>時刻を取得する機能の約束です。実際の時計か固定時計かは問いません。</summary>
public interface IClock
{
    DateTimeOffset GetNow();
}
