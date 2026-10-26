using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PgBlazorSamples.Web.Data;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;
using MudBlazor.Translations;
using PgBlazorSamples.Web.Components;
using PgBlazorSamples.Web.Services.Clock;
using PgBlazorSamples.Web.Services.Lifetime;
using PgBlazorSamples.Web.Services.Learning;
using PgBlazorSamples.Application;

// 日本語専用の教材なので、サーバーのOS設定によらず日本語表示を標準にする。
// 日時や数値の既定書式、今後導入する入力検証などにも適用する。
var japaneseCulture = CultureInfo.GetCultureInfo("ja-JP");
CultureInfo.DefaultThreadCurrentCulture = japaneseCulture;
CultureInfo.DefaultThreadCurrentUICulture = japaneseCulture;

var builder = WebApplication.CreateBuilder(args);

// MudBlazorが提供するサービスとコミュニティ翻訳を利用する。
// 独自のMudLocalizerを作り直さず、翻訳パッケージの登録を優先する。
builder.Services.AddMudServices();
builder.Services.AddLocalization();
// Step61：認証状態をInteractive Serverの各コンポーネントへ安全に供給する。
builder.Services.AddCascadingAuthenticationState();

// Windows統合認証はWindows Server 2025/IIS上の構成確認後に有効化する。
// CIや匿名の架空データ教材から、実ADの認証済み主体を偽造しない。
var windowsAuthEnabled = builder.Configuration.GetValue<bool>("WindowsAuthentication:Enabled");
var explicitLoginEnabled = builder.Configuration.GetValue<bool>("WindowsAuthentication:ExplicitLoginEnabled");
if (explicitLoginEnabled && !windowsAuthEnabled)
    throw new InvalidOperationException("明示ログインにはWindows認証の有効化が必要です。");

if (windowsAuthEnabled)
{
    if (explicitLoginEnabled)
    {
        // Step72：業務操作用のCookieはASP.NET Coreの署名・暗号化済み認証チケットのみ。
        // 未認証者にも案内画面を見せ、Windows資格の検証は専用HTTP経路だけで行う。
        builder.Services.AddAuthentication(WindowsUserAccessService.BrowserCookieScheme)
            .AddCookie(WindowsUserAccessService.BrowserCookieScheme, options =>
            {
                options.Cookie.Name = "__Host-PgBlazorBusiness";
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.SlidingExpiration = false;
                options.LoginPath = "/learn/72";
                options.Events.OnValidatePrincipal = context =>
                {
                    // 暗号化チケットが検証済みでも発行元の契約が欠ければ拒否する。
                    if (WindowsUserAccessService.ReadTrustedLoginName(
                        context.Principal ?? new ClaimsPrincipal()) is null)
                        context.RejectPrincipal();
                    return Task.CompletedTask;
                };
            })
            .AddNegotiate();

        // 本番で鍵が消えるとセッションが失効する。共有先のACL/保存時暗号化は運用側で設定。
        var keyPath = builder.Configuration["WindowsAuthentication:DataProtectionKeysPath"];
        if (string.IsNullOrWhiteSpace(keyPath) && !builder.Environment.IsDevelopment())
            throw new InvalidOperationException("明示ログインの本番利用にはDataProtectionKeysPathが必要です。");
        if (!string.IsNullOrWhiteSpace(keyPath))
            builder.Services.AddDataProtection().SetApplicationName("PgBlazorSamples.Business")
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    }
    else
    {
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
    }
}
builder.Services.AddAuthorization(options =>
{
    // 明示ログインモードでは匿名案内ページを許す。業務操作はDB認可サービスで毎回拒否判定。
    if (windowsAuthEnabled && !explicitLoginEnabled)
        options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddMudTranslations();

// Step18でUnitTests、Step19でApplicationを段階的に追加する。
// Step11からDIコンテナーへ時計サービスを登録する。
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Step11〜12：時計の約束と差し替え可能な具象実装を登録する。
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<FixedClock>();
builder.Services.AddTransient<GreetingService>();
builder.Services.AddTransient<GreetingFormatter>();

// Step13〜14：サービス生存期間を観察するための架空の状態。
builder.Services.AddSingleton<SingletonMarker>();
builder.Services.AddScoped<ScopedMarker>();
builder.Services.AddTransient<TransientMarker>();
builder.Services.AddScoped<ScopedCounter>();

// Step16：標準Optionsライブラリのサービス登録。設定を画面で解析しない。
builder.Services.Configure<LearningOptions>(builder.Configuration.GetSection("Learning"));

// Step19〜20：Applicationの契約と実装、画面固有の保存方法を分離する。
builder.Services.AddTransient<IStepSummaryService, StepSummaryService>();
builder.Services.AddScoped<ITodoService, InMemoryTodoService>();
// Step29：利用者ごとのテーマ設定はcircuitごとに保持する。
builder.Services.AddScoped<ThemePreferenceService>();

// Step31〜40：新たな層別プロジェクトは作らず、このWeb内でEF Core 10を使う。
// 未設定時はDB画面で接続前の案内を表示する。接続情報は環境変数で与える。
var pgConnection = builder.Configuration.GetConnectionString("PostgreSQL");
// PostgreSQLの接続にはアプリ名を固定で付ける。利用者別の接続文字列にせず
// Npgsqlのプールを再利用する。実行者の監査は業務サービス側で別途記録する。
var pgApplicationConnection = new Npgsql.NpgsqlConnectionStringBuilder(
    string.IsNullOrWhiteSpace(pgConnection)
        ? "Host=127.0.0.1;Database=pg_blazor_learning;Username=pg_blazor_learning;Timeout=2"
        : pgConnection)
{
    ApplicationName = "PgBlazorSamples.Web"
};
builder.Services.AddDbContextFactory<LearningDbContext>(options =>
    options.UseNpgsql(pgApplicationConnection.ConnectionString));
builder.Services.AddScoped<LearningDbService>();
// Step41〜60：ServerDataのDB側ページングもWeb内の小さなサービスで扱う。
builder.Services.AddScoped<LearningPageService>();
// Step46〜49：JOIN・キーセット・仮想化の各パターンを操作単位のDbContextで実装。
builder.Services.AddScoped<LearningAdvancedPageService>();
// Step51〜55：短命DbContextを使った編集DTO、排他、一括更新と詳細表示。
builder.Services.AddScoped<LearningEditService>();
// Step56〜60：期間検索、集計、学習用キャッシュ、実DB性能の観察をWeb内へ登録。
builder.Services.AddScoped<LearningRangeExportService>();
builder.Services.AddScoped<LearningCacheLessonService>();
builder.Services.AddScoped<LearningPerformanceService>();
// Step61〜65：本人はASP.NET CoreのWindows認証が確定し、ドメイン付きユーザー名とDBを照合する。
builder.Services.AddScoped<WindowsUserAccessService>();
// Step66〜70：本人と既存台帳を毎操作照合し、全SQLの部署条件を共通化する。
builder.Services.AddScoped<SecuredLearningService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // 本番の未処理例外は利用者向けの日本語ページへ案内する。
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// HTTPS終端・証明書などの本番設定は、Windows Server 2025の章で検証する。
app.UseHttpsRedirection();
// リクエスト単位でも日本語の文化と言語を確定する。
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("ja-JP")
    .AddSupportedCultures("ja-JP")
    .AddSupportedUICultures("ja-JP"));
// 認証ミドルウェアはルート実行より前。未認証リクエストの拒否は.NET標準へ任せる。
if (windowsAuthEnabled) app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Step72：Cookie発行/破棄はBlazor circuitではなくHTTPレスポンスで実行する。
// Negotiate経路の本人だけが短命Cookieを取得する。GETは同意用フォームの表示のみ。
if (explicitLoginEnabled)
{
    var windowsOnly = new AuthorizeAttribute
    {
        AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme
    };
    app.MapGet("/auth/windows/confirm", (HttpContext context, IAntiforgery antiforgery) =>
    {
        if (WindowsUserAccessService.ReadTrustedLoginName(context.User) is null)
            return Results.Forbid();
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'none'; form-action 'self'; frame-ancestors 'none'; base-uri 'none'";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        var tokens = antiforgery.GetAndStoreTokens(context);
        var field = WebUtility.HtmlEncode(tokens.FormFieldName);
        var value = WebUtility.HtmlEncode(tokens.RequestToken);
        var html = $"""
            <!doctype html><html lang="ja"><head><meta charset="utf-8">
            <title>Windowsログインの確認</title></head><body>
            <h1>Windows認証を確認しました</h1>
            <p>ADのパスワードは入力しません。業務用の短時間ログインを開始します。</p>
            <form method="post" action="/auth/windows/signin">
              <input type="hidden" name="{field}" value="{value}">
              <button type="submit">ログイン状態を保持する</button>
            </form></body></html>
            """;
        return Results.Content(html, "text/html; charset=utf-8");
    }).RequireAuthorization(windowsOnly);

    app.MapPost("/auth/windows/signin", async (HttpContext context, IAntiforgery antiforgery) =>
    {
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest("確認情報が無効です。"); }
        var login = WindowsUserAccessService.ReadTrustedLoginName(context.User);
        if (login is null) return Results.Forbid();
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, login),
            new Claim(WindowsUserAccessService.BrowserCookieSourceClaim, "Negotiate")
        ], WindowsUserAccessService.BrowserCookieScheme);
        context.Response.Headers.CacheControl = "no-store";
        await context.SignInAsync(
            WindowsUserAccessService.BrowserCookieScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            });
        return Results.Redirect("/learn/72");
    }).RequireAuthorization(windowsOnly);

    app.MapPost("/auth/windows/signout", async (HttpContext context, IAntiforgery antiforgery) =>
    {
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest("確認情報が無効です。"); }
        await context.SignOutAsync(WindowsUserAccessService.BrowserCookieScheme);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Redirect("/learn/72");
    }).RequireAuthorization(new AuthorizeAttribute
    {
        AuthenticationSchemes = WindowsUserAccessService.BrowserCookieScheme
    });
}

// .NET 10の静的アセットを登録し、対話型サーバー側の描画を有効にする。
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
