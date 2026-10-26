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
builder.Services.AddDbContextFactory<LearningDbContext>(options =>
    options.UseNpgsql(string.IsNullOrWhiteSpace(pgConnection)
        ? "Host=127.0.0.1;Database=pg_blazor_learning;Username=pg_blazor_learning;Timeout=2"
        : pgConnection));
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
app.UseAntiforgery();

// .NET 10の静的アセットを登録し、対話型サーバー側の描画を有効にする。
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
