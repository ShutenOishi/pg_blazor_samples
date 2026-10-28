using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順106：要件ごとに対応する実業務サービス入口だけを呼ぶことを検査。</summary>
public sealed class L106RequirementFlowTests
{
    [Theory]
    [InlineData("list")]
    [InlineData("edit")]
    [InlineData("export")]
    public async Task SelectedRoute_OnlyInvokesMatchingOperation(string selected)
    {
        var calls = new List<string>();
        Task<string> Call(string name)
        {
            calls.Add(name);
            return Task.FromResult(name + " は拒否しました。");
        }

        var result = await L106RequirementFlow.ExecuteAsync(
            selected,
            () => Call("list"),
            () => Call("edit"),
            () => Call("export"));

        Assert.Equal([selected], calls);
        Assert.Contains("拒否", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    public async Task UnknownRequirement_FailsClosedWithoutInvokingService(string? selected)
    {
        var calls = 0;
        Task<string> Unexpected()
        {
            calls++;
            return Task.FromResult("実行されてはいけません");
        }
        var result = await L106RequirementFlow.ExecuteAsync(
            selected, Unexpected, Unexpected, Unexpected);

        Assert.Equal(0, calls);
        Assert.Contains("拒否", result);
    }
}
