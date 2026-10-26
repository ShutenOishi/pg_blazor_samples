# ステップ21：MudBlazorと日本語翻訳を導入する

## できるようになること
WinFormsのボタンや表と同様、Webでも共通UI部品を組み合わせて業務画面を作ります。MudBlazorはBlazor向けのUIコンポーネントライブラリです。ここでは、**NuGetパッケージ、DI登録、Provider、静的ファイル、Interactive Server、日本語翻訳**がそろって初めて画面が動くことを理解します。

## 学習の前提
Step11〜20のDIと[特別コラム05](../columns/column-library-di.md)が前提です。PostgreSQL・ADは不要です。`/learn/21`を開いてください。

## 実際の画面を操作する
1. `MudPaper`の枠内に「日本語UIの実習」と説明が表示される。
2. 「日本語の通知を表示する」を押す。
3. Snackbarとして「日本語の通知が表示されました。」が現れ、画面内の状態は「通知サービスを使いました。」になる。
4. Step01〜20の画面へ戻り、引き続き利用できることを確認する。

## 新規プロジェクトなら公式テンプレート
すでにある学習アプリを消さずに、別フォルダーへ生成して構成を比べます。コマンド例は次のとおりです（テンプレートの選択肢は導入済みバージョンのヘルプでも確認してください）。

```powershell
dotnet new install MudBlazor.Templates
dotnet new mudblazor --name MudBlazorPractice --output MudBlazorPractice --interactivity Server --all-interactive --empty
dotnet run --project MudBlazorPractice
```

テンプレートはアプリ全体の構成を学ぶためのものです。既存Step01〜20を作り直す必要はありません。

## 既存Webアプリの構成
この教材で使うバージョンは`MudBlazor 9.11.0`と`MudBlazor.Translations 3.6.0`です。Webプロジェクトに両パッケージを導入しています。

```powershell
dotnet add src/PgBlazorSamples.Web/PgBlazorSamples.Web.csproj package MudBlazor --version 9.11.0
dotnet add src/PgBlazorSamples.Web/PgBlazorSamples.Web.csproj package MudBlazor.Translations --version 3.6.0
```

`Program.cs`の登録は次のとおりです。

```csharp
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddMudTranslations();
```

さらに`ja-JP`の文化設定と`UseRequestLocalization`、`App.razor`でのMudBlazor CSS・JavaScriptの読み込み、`InteractiveServer`の描画指定を行っています。`MainLayout.razor`には次のProviderを配置しています。

```razor
<MudThemeProvider IsDarkMode="@ThemePreference.IsDarkMode" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

これらの部品は単なる見た目の飾りではありません。ポップオーバー、ダイアログ、通知などを表示する位置や仕組みを提供します。

## Razorの通知コード
実際の`MudIntroductionPage.razor`は`ISnackbar`を注入します。

```razor
@page "/learn/21"
@inject ISnackbar Snackbar

<MudPaper Class="pa-4" Elevation="1">
    <MudText Typo="Typo.h6">日本語UIの実習</MudText>
    <MudButton OnClick="Notify">日本語の通知を表示する</MudButton>
    <p role="status">@message</p>
</MudPaper>

@code {
    private string message = "まだ通知していません。";

    private void Notify()
    {
        Snackbar.Add("日本語の通知が表示されました.", Severity.Success);
        message = "通知サービスを使いました。";
    }
}
```

上記は説明用の短縮例です。実コードでは通知の文章の句点が「。」、`MudButton`の`Variant`と`Color`、`MudAlert`も指定されています。表示文言の確認には実コードを正としてください。

## ボタンから通知までの順序
1. ブラウザーで`MudButton`を押す。
2. サーバー上のRazorで`Notify`が動く。
3. DIで注入した`ISnackbar`へメッセージを渡す。
4. 共通レイアウトの`MudSnackbarProvider`が通知を表示する。
5. 画面内の`message`も更新し、表示へ反映する。

UI部品を導入しても利用者の認証やデータの保存は行われません。

## 日本語化で混同しないこと
`ja-JP`は日付・数値などの文化設定、`MudBlazor.Translations`は部品に組み込まれた共通文言の翻訳です。「日本語UIの実習」「通知サービスを使いました。」のような**業務固有のラベルはアプリ自身で日本語にします**。パッケージを追加するだけで全画面の任意の文字列が翻訳されるわけではありません。

## よくある問題
- 通知が出ない場合は`AddMudServices`だけでなく`MudSnackbarProvider`・対話型描画・静的ファイルを確認する。
- Providerを重複配置しない。アプリのレイアウト上の配置場所を確認する。
- テンプレート生成時と既存アプリの手動導入を混同しない。

## 演習と解答
**問：** `Snackbar.Add`で状態通知を追加しても、`MudSnackbarProvider`が存在しない場合、何を確認すべきですか。

**答え：** 共通レイアウトへのProvider配置と対話型描画の指定です。DI登録のみでは画面への通知表示が成立しません。

## 完了確認
- [ ] 通知ボタンと画面内メッセージを確認した。
- [ ] DI・Provider・CSS/JS・描画モードの役割を説明できる。
- [ ] 文化設定・共通翻訳・独自ラベルの違いを説明できる。

次は[ステップ22：レイアウト](step22.md)です。
