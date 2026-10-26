using PgBlazorSamples.Web.Services.Clock;
using PgBlazorSamples.Application;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>実際の時刻やブラウザーを使わない単体テストです。</summary>
public sealed class GreetingServiceTests
{
    [Fact]
    public void 午前8時ならおはようございます()
    {
        var service = new GreetingService(new FixedClock());
        Assert.Equal("おはようございます", service.GetMessage());
    }

    [Theory]
    [InlineData(0, "おはようございます")]
    [InlineData(11, "おはようございます")]
    [InlineData(12, "こんにちは")]
    [InlineData(23, "こんにちは")]
    public void 時間帯で挨拶が決まる(int hour, string expected)
    {
        Assert.Equal(expected, new GreetingService(new TestClock(hour)).GetMessage());
    }

    [Fact]
    public void 学習進捗は60から差し引く()
    {
        IStepSummaryService summary = new StepSummaryService();
        Assert.Equal("5ステップを学習済み。残り55ステップです。", summary.Describe(5));
        Assert.Equal("全60ステップを修了しました。", summary.Describe(60));
    }

    [Fact]
    public void 範囲外の進捗を拒否する()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StepSummaryService().Describe(61));
    }

    private sealed class TestClock(int hour) : IClock
    {
        public DateTimeOffset GetNow() => new(2026, 1, 1, hour, 0, 0, TimeSpan.FromHours(9));
    }
}
