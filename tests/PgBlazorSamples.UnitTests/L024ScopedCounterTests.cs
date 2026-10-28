using PgBlazorSamples.Web.Services.Lifetime;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順024：ScopedCounterの変更・リセット・インスタンス分離。</summary>
public sealed class L024ScopedCounterTests
{
    [Fact]
    public void 親子が共有する同一インスタンスは加算とリセットを保持する()
    {
        var parent = new ScopedCounter();
        var child = parent;
        Assert.Equal(0, child.Count);

        parent.Increment();
        child.Increment();
        Assert.Equal(2, parent.Count);
        Assert.Equal(2, child.Count);

        child.Reset();
        Assert.Equal(0, parent.Count);
        Assert.Equal(0, child.Count);
        parent.Increment();
        Assert.Equal(1, child.Count);
    }

    [Fact]
    public void 別のインスタンスへカウントが漏れない()
    {
        var first = new ScopedCounter();
        var second = new ScopedCounter();
        first.Increment();
        first.Increment();
        Assert.Equal(2, first.Count);
        Assert.Equal(0, second.Count);
        second.Increment();
        first.Reset();
        Assert.Equal(0, first.Count);
        Assert.Equal(1, second.Count);
        // 実circuitの分離はChromeで別ブラウザーコンテキストを使って検証する。
    }
}
