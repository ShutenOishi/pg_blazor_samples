using System.Text;
using System.Text.RegularExpressions;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class L043DownloadFileFactoryTests
{
    [Theory]
    [InlineData("=1+1")]
    [InlineData("+SUM(A1:A2)")]
    [InlineData("-1+2")]
    [InlineData("@SUM(1,2)")]
    [InlineData("  =2+2")]
    public void QuotesAndNeutralizesFormulaLikeCells(string value)
    {
        var csvCell = L043DownloadFileFactory.QuoteCsvCell(value);
        Assert.StartsWith("\"'", csvCell);
        Assert.EndsWith("\"", csvCell);
        Assert.Contains(value.Replace("\"", "\"\""), csvCell);
    }

    [Fact]
    public void QuotesCommaDoubleQuoteAndNewline()
    {
        Assert.Equal("\"A,B\"",
            L043DownloadFileFactory.QuoteCsvCell("A,B"));
        Assert.Equal("\"a\"\"b\"", L043DownloadFileFactory.QuoteCsvCell("a\"b"));
        Assert.Equal("\"a\nb\"", L043DownloadFileFactory.QuoteCsvCell("a\nb"));
    }

    [Fact]
    public void CsvIsUtf8BomWithExpectedFixedRows()
    {
        var bytes = L043DownloadFileFactory.CreateCsv();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        var text = new UTF8Encoding(false, true).GetString(bytes);
        Assert.StartsWith("\uFEFF", text);
        Assert.Contains("\"番号\",\"課題名\",\"完了\"\r\n", text);
        Assert.Contains("\"3\",\"'=1+1\",\"未完了\"\r\n", text);
        Assert.Contains("\"カンマ,と\"\"引用符\"\"\"", text);
        Assert.DoesNotContain("\"=1+1\"", text);
    }

    [Fact]
    public void PdfHasSinglePageAndValidCrossReferenceOffsets()
    {
        var bytes = L043DownloadFileFactory.CreateOnePagePdf();
        var text = Encoding.ASCII.GetString(bytes);
        Assert.StartsWith("%PDF-1.4\n", text);
        Assert.Contains("/Type /Pages /Kids [3 0 R] /Count 1", text);
        Assert.Contains("(Blazor PDF sample) Tj", text);
        var match = Regex.Match(text, @"startxref\n(\d+)\n%%EOF\n$");
        Assert.True(match.Success, "startxref と %%EOF が必要");
        var offset = int.Parse(match.Groups[1].Value);
        Assert.Equal("xref\n", text.Substring(offset, 5));
        var lines = text[offset..].Split('\n');
        Assert.Equal("0 6", lines[1]);
        for (var i = 1; i <= 5; i++)
        {
            var position = int.Parse(lines[i + 2][..10]);
            Assert.StartsWith($"{i} 0 obj\n", text[position..]);
        }
    }
}
