using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順039：テーマの状態変更通知とインスタンス間の分離を検査する。</summary>
public sealed class ThemePreferenceServiceTests
{
    [Fact]
    public void 初期値はライトモードである()
    {
        Assert.False(new ThemePreferenceService().IsDarkMode);
    }

    [Fact]
    public void 状態が変化したときだけ通知する()
    {
        var service = new ThemePreferenceService();
        var changes = 0;
        service.Changed += () => changes++;

        service.SetDarkMode(false);
        Assert.Equal(0, changes);
        service.SetDarkMode(true);
        Assert.True(service.IsDarkMode);
        Assert.Equal(1, changes);
        service.SetDarkMode(true);
        Assert.Equal(1, changes);
        service.SetDarkMode(false);
        Assert.False(service.IsDarkMode);
        Assert.Equal(2, changes);
    }

    [Fact]
    public void 異なるインスタンスに表示状態が漏れない()
    {
        var firstCircuit = new ThemePreferenceService();
        var otherCircuit = new ThemePreferenceService();
        firstCircuit.SetDarkMode(true);
        Assert.True(firstCircuit.IsDarkMode);
        Assert.False(otherCircuit.IsDarkMode);
    }
}
