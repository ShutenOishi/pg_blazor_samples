namespace PgBlazorSamples.Web.Services.Clock;

/// <summary>時計を自分で生成せず、外から受け取って挨拶を作ります。</summary>
public sealed class GreetingService(IClock clock)
{
    public string GetMessage() => clock.GetNow().Hour < 12
        ? "おはようございます"
        : "こんにちは";

    public string GetTimeText() => clock.GetNow().ToString("yyyy/MM/dd HH:mm");
}
