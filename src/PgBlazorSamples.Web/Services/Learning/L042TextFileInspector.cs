using System.Text;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順042。限定したUTF-8形式の検査であり、マルウェアや業務認可の検査ではない。
/// </summary>
public static class L042TextFileInspector
{
    public const int MaxBytes = 64 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static string? ValidateMetadata(string fileName, string? contentType, long declaredSize)
    {
        var extension = Path.GetExtension(fileName);
        var isText = extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
        var isCsv = extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
        if (!isText && !isCsv)
            return "拒否：.txt または .csv のみ選択できます。";
        if (declaredSize < 0 || declaredSize > MaxBytes)
            return "拒否：64 KiBを超えるファイルは受け付けません。";

        // MIMEは申告値。拡張子と整合しても安全性・真正性を保証しない。
        var mediaType = contentType?.Split(';', 2)[0].Trim();
        var matches = isText
            ? string.Equals(mediaType, "text/plain", StringComparison.OrdinalIgnoreCase)
            : string.Equals(mediaType, "text/csv", StringComparison.OrdinalIgnoreCase)
              || string.Equals(mediaType, "text/plain", StringComparison.OrdinalIgnoreCase)
              || string.Equals(mediaType, "application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase);
        return matches ? null : "拒否：拡張子とMIMEの組み合わせを確認してください。";
    }

    public static string? ValidateContent(byte[] bytes, long declaredSize)
    {
        if (bytes.Length > MaxBytes)
            return "拒否：64 KiBを超えるファイルは受け付けません。";
        if (bytes.Length != declaredSize)
            return "拒否：申告サイズと受信内容の長さが一致しません。";
        try
        {
            var content = StrictUtf8.GetString(bytes);
            if (content.Any(ch => char.IsControl(ch) && ch is not '\r' and not '\n' and not '\t'))
                return "拒否：テキスト以外の制御文字が含まれています。";
        }
        catch (DecoderFallbackException)
        {
            return "拒否：UTF-8として読み取れない内容です。";
        }
        return null;
    }
}
