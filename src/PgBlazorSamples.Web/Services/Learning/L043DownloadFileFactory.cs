using System.Text;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順043の固定架空データのみを生成する。業務データ抽出や出力認可は行わない。
/// CSVの保護は一般的な表計算ソフトに対する一例で、全製品での安全性は未検証。
/// </summary>
public static class L043DownloadFileFactory
{
    public static string QuoteCsvCell(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // 先頭空白等が取り除かれたときも=,+,-,@を式として扱わせない例。
        // CSVの引用符だけでは数式解釈を防げないため、先頭に単一引用符を付ける。
        var first = value.TrimStart(' ', '\t', '\r', '\n');
        if (first.Length > 0 && "=+-@".Contains(first[0]))
            value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    public static byte[] CreateCsv()
    {
        var rows = new[]
        {
            new[] { "番号", "課題名", "完了" },
            new[] { "1", "画面を確認する", "いいえ" },
            new[] { "2", "復習する", "はい" },
            new[] { "3", "=1+1", "未完了" },
            new[] { "4", "カンマ,と\"引用符\"", "いいえ" }
        };
        var text = new StringBuilder("\uFEFF");
        foreach (var row in rows)
            text.Append(string.Join(",", row.Select(QuoteCsvCell))).Append("\r\n");
        return Encoding.UTF8.GetBytes(text.ToString());
    }

    public static byte[] CreateOnePagePdf()
    {
        // ASCIIのみの小さなPDF。改行は固定で、文字位置とバイトオフセットが一致する。
        const string content = "BT /F1 18 Tf 72 750 Td (Blazor PDF sample) Tj ET\n";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {content.Length} >>\nstream\n{content}endstream"
        };
        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(pdf.Length);
            pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }
        var xref = pdf.Length;
        pdf.Append($"xref\n0 {offsets.Count}\n0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
            pdf.Append($"{offsets[i]:D10} 00000 n \n");
        pdf.Append($"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }
}
