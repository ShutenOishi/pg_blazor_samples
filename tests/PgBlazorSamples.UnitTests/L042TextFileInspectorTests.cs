using System.Text;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L042TextFileInspectorTests
{
    [Theory]
    [InlineData("練習.txt", "text/plain")]
    [InlineData("練習.CSV", "text/csv")]
    [InlineData("練習.csv", "application/vnd.ms-excel")]
    [InlineData("練習.csv", "text/plain; charset=utf-8")]
    public void AllowsMatchingMetadata(string name, string mime)
        => Assert.Null(L042TextFileInspector.ValidateMetadata(name, mime, 65536));

    [Theory]
    [InlineData("拒否.png", "image/png", 1)]
    [InlineData("拒否.txt", "text/csv", 1)]
    [InlineData("拒否.csv", "image/png", 1)]
    [InlineData("拒否.csv", "", 1)]
    [InlineData("拒否.txt", "text/plain", 65537)]
    [InlineData("拒否.txt", "text/plain", -1)]
    public void RejectsInvalidMetadata(string name, string mime, long length)
        => Assert.StartsWith("拒否：", L042TextFileInspector.ValidateMetadata(name, mime, length));

    [Fact]
    public void AcceptsValidUtf8AndExactMaximumSize()
    {
        var japanese = Encoding.UTF8.GetBytes("番号,名前\n1,架空\n");
        Assert.Null(L042TextFileInspector.ValidateContent(japanese, japanese.Length));
        Assert.Null(L042TextFileInspector.ValidateContent(Encoding.UTF8.GetBytes(new string('a', 65536)), 65536));
    }

    [Fact]
    public void RejectsInvalidActualBytes()
    {
        Assert.Contains("制御文字", L042TextFileInspector.ValidateContent(new byte[] { 0, 65 }, 2));
        Assert.Contains("UTF-8", L042TextFileInspector.ValidateContent(new byte[] { 0xC3, 0x28 }, 2));
        Assert.Contains("長さ", L042TextFileInspector.ValidateContent(new byte[] { 65 }, 2));
        Assert.Contains("64 KiB", L042TextFileInspector.ValidateContent(Encoding.UTF8.GetBytes(new string('a', 65537)), 65537));
    }
}
