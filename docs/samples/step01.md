# ステップ01：最初のBlazor画面

**対応するソース：** [FirstPage.razor](../../src/PgBlazorSamples.Web/Components/Samples/Step01/FirstPage.razor)

**状態：コードを作成済み。動作確認結果はCIに従って更新します。**

## 到達目標

Razorコンポーネントの場所、`@page`ディレクティブ、画面タイトルの指定方法を理解し、日本語の最初の画面を表示できるようになること。

## 前提

Visual Studio 2026、.NET 10 SDK。AD・PostgreSQL・MudBlazorは不要です。

## 実行手順

1. ルートにある `PgBlazorSamples.slnx` をVisual Studio 2026で開きます。
2. `PgBlazorSamples.Web` を起動プロジェクトに選びます。
3. F5またはCtrl+F5で起動し、トップページの「ステップ01を学ぶ」を選びます。
4. `/learn/01`に「はじめてのBlazor画面」が表示されることを確認します。

コマンドラインなら、リポジトリのルートから次を実行します。

```powershell
dotnet restore PgBlazorSamples.slnx
dotnet run --project src/PgBlazorSamples.Web
```

表示URLとポートは起動時の出力に従ってください。

## ソースと解説

`FirstPage.razor`の先頭に実際に存在する記述です。

```razor
@page "/learn/01"

<PageTitle>ステップ01：最初の画面｜Blazor ステップアップ学習</PageTitle>
```

`@page`は、この画面を表示するURLを決定します。`PageTitle`はブラウザーに表示するページタイトルです。`<h1>`は見出しを示し、読み上げにも役立ちます。アプリ起動処理は`Program.cs`、ルーティングは`Routes.razor`です。

## 演習

ソースにある「この文章は、Razorコンポーネントから表示しています。」を、自分で考えた日本語の文章に変更し、再実行してください。

## トラブル対策

SDKのエラーでは`dotnet --list-sdks`を確認します。画面が見つからなければ、URLの綴りが`/learn/01`か確認します。

## 完了条件

- [ ] 日本語の画面を表示できる
- [ ] `@page`の意味を説明できる
- [ ] 文章を変更して結果を確認できる

次：[ステップ02](step02.md)
