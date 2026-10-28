using PgBlazorSamples.Web.Services.Learning;

namespace PgBlazorSamples.UnitTests;

public sealed class L015CircuitCounterTests
{
    [Fact]
    public void SameInstanceRetainsState()
    {
        var counter = new L015CircuitCounter();
        var token = counter.InstanceToken;
        counter.Increment();
        counter.Increment();
        Assert.Equal(2, counter.Count);
        Assert.Equal(token, counter.InstanceToken);
        Assert.Matches("^[0-9a-f]{32}$", token);
    }

    [Fact]
    public void NewInstanceStartsAtZero()
    {
        var previous = new L015CircuitCounter();
        previous.Increment();
        var next = new L015CircuitCounter();
        Assert.Equal(1, previous.Count);
        Assert.Equal(0, next.Count);
        Assert.NotEqual(previous.InstanceToken, next.InstanceToken);
        // circuit期限切れや認証セッション失効はこの単体テストの対象外。
    }
}
