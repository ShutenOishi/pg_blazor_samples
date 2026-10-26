using PgBlazorSamples.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// 初級教材の段階ではWebプロジェクトだけを使用する。
// DIによるサービスの追加とプロジェクト分割はステップ11以降で学ぶ。
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
