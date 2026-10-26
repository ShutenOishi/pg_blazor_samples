namespace PgBlazorSamples.Web.Services.Clock;

/// <summary>通常のC#クラスがコンストラクター経由で別のサービスを受け取る例です。</summary>
public sealed class GreetingFormatter(GreetingService greeting)
{
    public string Format(string name)
        => $"{name}さん、{greeting.GetMessage()}。";
}
