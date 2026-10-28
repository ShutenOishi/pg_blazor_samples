using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順082専用の実HTTP演習。データ更新を行わず、正しいトークンでも必ず403。
/// 実AD/IIS/TLS終端・業務認可の受入は行わない。
/// </summary>
public static class L082HttpProbe
{
    public const string Path = "/learn/082/http-probe";

    public static IResult RenderForm(HttpContext context, IAntiforgery antiforgery)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'none'; connect-src 'self'; form-action 'self'; base-uri 'none'; frame-ancestors 'none'";
        var tokens = antiforgery.GetAndStoreTokens(context);
        var field = WebUtility.HtmlEncode(tokens.FormFieldName);
        var value = WebUtility.HtmlEncode(tokens.RequestToken);
        var transport = context.Request.IsHttps ? "HTTPS" : "HTTP（隔離した開発・CI環境）";
        var html = $"""
            <!doctype html><html lang="ja"><head><meta charset="utf-8">
            <title>学習順082：実HTTP検証</title></head><body>
            <h1>学習順082：Antiforgeryの実HTTP検証</h1>
            <p data-testid="l082-transport">サーバーが認識した経路：{transport}</p>
            <p>有効なトークンでもこの学習用POSTは403で拒否されます。業務データは変更しません。</p>
            <form method="post" action="/learn/082/http-probe" data-testid="l082-post-form">
              <input type="hidden" name="{field}" value="{value}">
              <button type="submit">検証済みトークンで送信</button>
            </form>
            <p>この表示はIIS、TLS終端、Forwarded Headersの安全性を証明しません。</p>
            </body></html>
            """;
        return Results.Content(html, "text/html; charset=utf-8");
    }

    public static async Task<IResult> ValidateAndDenyAsync(
        HttpContext context, IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.StatusCode(StatusCodes.Status400BadRequest);
        }
        // トークンは本人認証・業務認可ではない。学習用経路は常に安全拒否。
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }
}
