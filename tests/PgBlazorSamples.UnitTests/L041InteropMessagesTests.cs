using Microsoft.JSInterop;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L041InteropMessagesTests
{
    [Fact]
    public void JsErrorProducesSafeMessageWithoutErrorDetails()
    {
        var output = L041InteropMessages.Describe(new JSException("secret JS stack trace"));
        Assert.Contains("ブラウザーの処理に失敗", output);
        Assert.DoesNotContain("secret", output);
        Assert.DoesNotContain("stack", output);
    }

    [Fact]
    public void DisconnectedExceptionIsNotMisclassifiedAsJavascriptError()
    {
        // 例外クラスの分類試験。実ネットワーク切断の再現ではない。
        var output = L041InteropMessages.Describe(new JSDisconnectedException("secret circuit"));
        Assert.Contains("再接続してください", output);
        Assert.DoesNotContain("secret", output);
        Assert.DoesNotContain("ブラウザーの処理に失敗", output);
    }

    [Fact]
    public void OtherFailuresUseSafeDefault()
    {
        var output = L041InteropMessages.Describe(new InvalidOperationException("secret detail"));
        Assert.Contains("処理に失敗", output);
        Assert.DoesNotContain("secret", output);
    }
}
