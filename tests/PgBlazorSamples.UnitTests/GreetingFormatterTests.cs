using PgBlazorSamples.Web.Services.Clock;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順025：Razorから呼び出すFormatterとコンストラクター注入先の連携を検査する。</summary>
public sealed class GreetingFormatterTests
{
    [Theory]
    [InlineData(11, "練習花子さん、おはようございます。")]
    [InlineData(12, "練習花子さん、こんにちは。")]
    public void 注入した時計の時刻境界で挨拶が変わる(int hour, string expected)
    {
        var service = new GreetingService(new TestClock(hour));
        var formatter = new GreetingFormatter(service);
        Assert.Equal(expected, formatter.Format("練習花子"));
    }

    [Fact]
    public void 空欄はFormatter自体では入力拒否しない()
    {
        var formatter = new GreetingFormatter(new GreetingService(new TestClock(12)));
        Assert.Equal("さん、こんにちは。", formatter.Format(""));
    }

    private sealed class TestClock(int hour) : IClock
    {
        public DateTimeOffset GetNow() => new(2026, 1, 1, hour, 0, 0, TimeSpan.FromHours(9));
    }
}
