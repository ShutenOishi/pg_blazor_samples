# ステップ21：MudBlazorを導入して日本語化する

## できるようになること
既存のBlazor Web AppへMudBlazorと共通の日本語翻訳を導入し、DI・Provider・対話型描画のつながりを説明できるようになります。

## 操作して確認する
1. 学習アプリの /learn/21 を開きます。
2. 「日本語の通知を表示する」を押します。
3. 画面に「通知サービスを使いました。」と表示され、通知が現れます。
4. ステップ01〜20も操作できることを確認します。

## 公式テンプレートを別アプリで体験する
新規アプリなら公式テンプレートでDI、レイアウト、静的ファイル設定を自動生成できます。Visual Studio 2026の新規プロジェクト画面からも選べます。既存教材を作り直さず、別のフォルダーへ作成して比較してください。

```powershell
dotnet new install MudBlazor.Templates
dotnet new mudblazor --name MudBlazorPractice --output MudBlazorPractice --interactivity Server --all-interactive --empty
dotnet run --project MudBlazorPractice
```

テンプレートの更新確認は `dotnet new update --check-only`、登録済みの一覧は `dotnet new list mudblazor` で行います。学習環境の.NET SDKは10系です。

## 既存アプリへの組み込み
この教材のWebプロジェクトには検証対象の固定バージョンとして以下を使用します。

```powershell
dotnet add src/PgBlazorSamples.Web/PgBlazorSamples.Web.csproj package MudBlazor --version 9.11.0
dotnet add src/PgBlazorSamples.Web/PgBlazorSamples.Web.csproj package MudBlazor.Translations --version 3.6.0
```

```csharp
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddMudTranslations();
```

起動時に ja-JP 文化を設定し、UseRequestLocalizationでも対応言語を設定します。MudBlazorのCSS/JSを参照し、共通レイアウトへMudThemeProvider、MudPopoverProvider、MudDialogProvider、MudSnackbarProviderを配置します。アプリ全体をInteractiveServerで描画します。

## 重要な考え方
MudBlazorの通知は独自の通知クラスを作らず、`@inject ISnackbar Snackbar`で受け取ります。標準UIの共通文言はMudBlazor.Translationsを優先し、業務固有のラベルやエラーだけを教材が日本語で定義します。パッケージの追加だけでは翻訳・Provider・描画のすべては完成しません。

## 確認
- [ ] 通知サービスとProviderの役割を区別できる
- [ ] テンプレートと既存アプリへの手動導入を区別できる
- [ ] ja-JP文化設定とMudBlazor翻訳を説明できる

次：ステップ22
