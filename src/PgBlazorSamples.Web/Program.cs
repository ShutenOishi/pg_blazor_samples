using System.Globalization;
using PgBlazorSamples.Web.Components;
using PgBlazorSamples.Web.Services.Clock;
using PgBlazorSamples.Web.Services.Lifetime;

// 日本語専用の教材なので、サーバーのOS設定によらず日本語表示を標準にする。
// 日時や数値の既定書式、今後導入する入力検証などにも適用する。
var japaneseCulture = CultureInfo.GetCultureInfo("ja-JP");
CultureInfo.DefaultThreadCurrentCulture = japaneseCulture;
CultureInfo.DefaultThreadCurrentUICulture = japaneseCulture;

var builder = WebApplication.CreateBuilder(args);

// 最初の15ステップはWebプロジェクトのみを使用する。
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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // 本番の未処理例外は利用者向けの日本語ページへ案内する。
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// HTTPS終端・証明書などの本番設定は、Windows Server 2025の章で検証する。
app.UseHttpsRedirection();
app.UseAntiforgery();

// .NET 10の静的アセットを登録し、対話型サーバー側の描画を有効にする。
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
