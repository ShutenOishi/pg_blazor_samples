using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L108SubmissionGateTests
{
    [Fact]
    public void PendingSave_RejectsDuplicate_UntilReleased()
    {
        var gate = new L108SubmissionGate();
        Assert.False(gate.IsBusy);
        Assert.True(gate.TryBegin());
        Assert.True(gate.IsBusy);
        Assert.False(gate.TryBegin());
        gate.End();
        Assert.False(gate.IsBusy);
        Assert.True(gate.TryBegin());
        gate.End();
    }

    [Fact]
    public void SimultaneousAttempts_OnlyOneCanStart()
    {
        var gate = new L108SubmissionGate();
        var accepted = 0;
        Parallel.For(0, 32, _ =>
        {
            if (gate.TryBegin()) Interlocked.Increment(ref accepted);
        });
        Assert.Equal(1, accepted);
        gate.End();
        Assert.True(gate.TryBegin());
        gate.End();
    }
}
