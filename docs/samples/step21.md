# 学習順31：MudBlazorと日本語翻訳を導入する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step21/MudIntroductionPage.razor)。画面の正規URLは `/learn/031` です。

## 学ぶ前に
この教材はWinForms＋Dapperを使ってきた開発者が、WebのUIとサーバー側C#の境界を理解するためのものです。学習用の架空データで試し、認証・認可や実業務の保存が成立したとは見なしません。対象画面は /learn/031 です。

## WinForms＋Dapperとの比較
| 観点 | デスクトップでの経験 | MudBlazorでの対応 |
| --- | --- | --- |
| NuGet参照 | Windowsフォーム部品を追加 | MudBlazorとTranslationsを参照しDI登録も必要 |
| 表示と通知 | MessageBox | ISnackbarと共通Providerを組み合わせる |
| 文化設定 | OS文化や書式 | ja-JPと翻訳辞書と独自ラベルを分ける |
| データ処理 | DapperのSQL取得 | この画面にDB取得処理はない |

## 不具合時の切り分け
| 症状 | 確認する場所 |
| --- | --- |
| 通知が出ない | MudSnackbarProvider・ISnackbarを確認 |
| クリックが効かない | 対話型描画とcircuit・Consoleを調べる |
| 表示が英語 | Cultureと共通翻訳・独自ラベルを区別 |
| デザインが崩れる | MudBlazorのCSSとJavaScriptを確認 |

## 仕組みを一段深く理解する
ライブラリ導入は、NuGet参照だけで終了しません。MudBlazorが提供するサービスの登録、レイアウトのProvider、ブラウザーへ読み込む静的資産、対話型描画が揃って初めて利用者の操作へ反応します。WinFormsではフォームのデザイナーで部品とイベントを結びますが、Blazor Serverのイベント処理はサーバー側のC#へ送られます。Dapperはここでは登場しません。通知をDB更新の結果と混同しないでください。

```csharp
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddMudTranslations();
```

```razor
@inject ISnackbar Snackbar
<MudButton OnClick="Notify">日本語の通知を表示する</MudButton>
@code {
    private void Notify() => Snackbar.Add("日本語の通知が表示されました。", Severity.Success);
}
```

`ja-JP`は日付・数値の表示規則、MudBlazor.Translationsはライブラリ側の共通UI文字列を担当します。「日本語UIの実習」のような固有ラベルやRequiredErrorを自動翻訳する機能ではありません。Providerは通知の表示位置、ISnackbarは通知を要求するための契約です。この違いを理解すれば、第三者ライブラリのDIも読みやすくなります。

**演習と詳しい解答：** ボタンを押して画面内の状態だけが変わった場合、何を確認するべきでしょうか。イベント処理は少なくとも一部動いていると考えられるため、ProviderとSnackbarの配置を優先して調べます。逆に状態も更新されなければ、Interactive Serverの接続、イベント指定、ブラウザーConsoleとサーバー側ログを確認します。権限やDBを疑うのはこの段階では適切ではありません。

## 実装に対応した操作・コード・詳細解答

## できるようになること
WinFormsのボタンや表と同様、Webでも共通UI部品を組み合わせて業務画面を作ります。MudBlazorはBlazor向けのUIコンポーネントライブラリです。ここでは、**NuGetパッケージ、DI登録、Provider、静的ファイル、Interactive Server、日本語翻訳**がそろって初めて画面が動くことを理解します。

## 学習の前提
学習順021〜20のDIと[特別コラム05](../columns/column-library-di.md)が前提です。PostgreSQL・ADは不要です。`/learn/031`を開いてください。

## 実際の画面を操作する
1. `MudPaper`の枠内に「日本語UIの実習」と説明が表示される。
2. 「日本語の通知を表示する」を押す。
3. Snackbarとして「日本語の通知が表示されました。」が現れ、画面内の状態は「通知サービスを使いました。」になる。
4. 学習順006〜20の画面へ戻り、引き続き利用できることを確認する。

## 新規プロジェクトなら公式テンプレート
すでにある学習アプリを消さずに、別フォルダーへ生成して構成を比べます。コマンド例は次のとおりです（テンプレートの選択肢は導入済みバージョンのヘルプでも確認してください）。

```powershell
dotnet new install MudBlazor.Templates
dotnet new mudblazor --name MudBlazorPractice --output MudBlazorPractice --interactivity Server --all-interactive --empty
dotnet run --project MudBlazorPractice
```

テンプレートはアプリ全体の構成を学ぶためのものです。既存学習順006〜20を作り直す必要はありません。

## 既存Webアプリの構成
この教材で使うバージョンは`MudBlazor 9.11.0`と`MudBlazor.Translations 3.6.0`です。Webプロジェクトに両パッケージを導入しています。

```powershell
dotnet add package MudBlazor --version 9.11.0
dotnet add package MudBlazor.Translations --version 3.6.0
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
@page "/learn/031"
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
        Snackbar.Add("日本語の通知が表示されました。", Severity.Success);
        message = "通知サービスを使いました。";
    }
}
```

上記は説明用の短縮例です。実コードには`MudButton`の`Variant`と`Color`、`MudAlert`、移動リンクもあります。

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

次は[学習順032：レイアウト](step22.md)です。


## 学習順31の追加確認
- [ ] 画面で操作を実際に行い、状態の変化を確認した。
- [ ] 比較表と切り分け表に沿って、WinFormsとの相違点を説明できる。
- [ ] DapperによるDB検索と画面表示の処理を区別できる。
- [ ] 教材の演習に回答し、失敗時の復旧手順を説明できる。
