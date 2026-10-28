using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L044OperationRunnerTests
{
    [Fact]
    public async Task CompletesSuccessfullyAndAllowsNewRun()
    {
        using var runner = new L044OperationRunner(_ => Task.CompletedTask);
        Assert.True(await runner.TryStartAsync());
        Assert.Equal("処理が完了しました。", runner.State);
        Assert.False(runner.Busy);
        Assert.True(await runner.TryStartAsync());
        Assert.Equal(2, runner.StartCount);
    }

    [Fact]
    public async Task RejectsDoubleStartThenAllowsRetryAfterCancellation()
    {
        using var runner = new L044OperationRunner(token => Task.Delay(Timeout.InfiniteTimeSpan, token));
        var first = runner.TryStartAsync();
        Assert.True(runner.Busy);
        Assert.Equal("処理中", runner.State);
        Assert.False(await runner.TryStartAsync());
        Assert.Equal(1, runner.StartCount);
        Assert.True(runner.Cancel());
        Assert.True(await first);
        Assert.Equal("処理を取り消しました。", runner.State);
        Assert.False(runner.Busy);
        Assert.False(runner.Cancel());

        var retry = runner.TryStartAsync();
        Assert.True(runner.Busy);
        Assert.Equal(2, runner.StartCount);
        Assert.True(runner.Cancel());
        Assert.True(await retry);
        Assert.Equal("処理を取り消しました。", runner.State);
    }

    [Fact]
    public async Task DisposeCancelsInFlightWorkAndPreventsRestart()
    {
        var runner = new L044OperationRunner(token => Task.Delay(Timeout.InfiniteTimeSpan, token));
        var pending = runner.TryStartAsync();
        Assert.True(runner.Busy);
        runner.Dispose();
        Assert.True(await pending);
        Assert.Equal("処理を取り消しました。", runner.State);
        Assert.False(runner.Busy);
        Assert.False(await runner.TryStartAsync());
        Assert.Equal(1, runner.StartCount);
        runner.Dispose(); // 2回呼んでも安全
    }
}
