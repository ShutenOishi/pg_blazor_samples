# 第1段階の概要：Web開発の前提

## この段階の目的

この段階は、**C#・WinForms・Dapperの経験はあるがWeb開発は初めて**という学習者を想定しています。Webアプリで新しい構文を覚える前に、どの処理がブラウザーで行われ、どの処理がWebサーバー上のC#で行われるかを理解します。

学習する環境はVisual Studio 2026、.NET 10、Blazor Web AppのInteractive Serverです。PostgreSQL 18やActive Directoryはこの5ステップには必要ありません。学習用画面には架空のカウンター・文字列などを使用し、機密情報は入力しません。

## 学習順と到達目標

| 学習順 | 主要テーマ | 動かすページ | 自分で確認する結果 |
| --- | --- | --- | --- |
| 001 | WinFormsとWebの実行場所 | `/learn/001` | クリック2回でC#の値が2になる |
| 002 | URL、HTTP要求・応答 | `/learn/002` | NetworkのDocument GET、パス、Status |
| 003 | HTML・CSS・フォーム | `/learn/003` | 名前の入力、ラベル関連付け、プレビュー |
| 004 | DOM・Network・Console | `/learn/004` | 条件による`web04-added`要素の追加・削除 |
| 005 | KestrelとWebのデバッグ | `/learn/005` | `BreakpointSample()`でサーバーC#停止 |
| 復習01 | 5つの概念を結び付ける | `/learn/column/web-05` | 各診断道具と実行場所を選べる |

## 最初に知っておく言葉

| 言葉 | この教材での意味 | WinForms経験からの比較 |
| --- | --- | --- |
| ブラウザー | HTML・CSSを描画し入力を受け付ける | Formに近い表示側 |
| Webサーバー | HTTP要求を処理しサーバー側C#を実行 | デスクトップアプリとは別の実行先 |
| HTTP | 要求と応答の通信方式 | 画面内のイベントとは別の通信 |
| HTML | 見出し・本文・ボタン・入力の意味 | コントロールの役割 |
| CSS | 枠線・余白・文字などの外観 | 表示設定 |
| DOM | ブラウザー内で管理されるHTML要素の構造 | 実行中の画面要素 |
| Interactive Server | イベントC#をサーバーで実行するBlazorの方式 | クライアントにC#本体を配布しない |
| Kestrel | ASP.NET CoreのWebサーバー | WinFormsのEXEと起動方式が異なる |

この段階の最小構成は、ページの`@page`ルート、`@onclick`イベント、`@bind`入力、`@if`条件描画です。詳しい概念はそれぞれ対応する画面で観察します。

```razor
@page "/learn/001"
<button type="button" @onclick="Increment">サーバー側で1増やす</button>
@code {
    private int count;
    private void Increment() => count++;
}
```

この実コードの`Increment()`はブラウザーにダウンロードされて実行されるのではなく、Interactive ServerのサーバーC#として実行されます。最初はこの一点を明確にしてください。

## 学び方と作業順

1. 各本文を先に読み、表の用語とサーバー・ブラウザーの役割を整理します。
2. アプリの対応URLを開き、本文に示した順で操作します。
3. コードと実際の表示を照合します。`@page`、`@onclick`、`@bind`などの記述箇所を探します。
4. 開発者ツールのElements・Network・Consoleで、どこまで観察できるかを確かめます。
5. 最後にVisual StudioのデバッガーでサーバーC#の実行を確認し、第1組の復習へ進みます。

## デバッグの役割分担

| 何が起きたか | 初めに確認する場所 | 次に確認する場所 |
| --- | --- | --- |
| URLが開かない | NetworkのURL/Status | サーバー起動ログ |
| HTML構造がおかしい | ElementsのDOM | RazorのHTML部分 |
| クリックできない | Networkの接続 | `@onclick`、サーバーC# |
| 入力が反映されない | 要素の属性と接続 | `@bind`、`@code` |
| C#内部でエラー | Visual Studioの例外・ログ | ブラウザーの結果 |

## 確認問題と解答

**問1：** WinFormsのClickと`@onclick`の実行場所は同じですか。

**解答：** WinFormsは通常利用者のPCで動きますが、この教材のInteractive ServerではC#のイベント処理をWebサーバーで実行します。

**問2：** ページの再読み込みとボタンのクリックは、いつも同じHTTPの処理ですか。

**解答：** いいえ。再読み込みでは主にHTTPのGETでページを取得し、Interactive ServerのクリックはBlazorが管理する対話型接続を通じて処理されます。

**問3：** CSSだけを変更してもDBの内容が変わらないのはなぜですか。

**解答：** CSSは画面の外観を記述するもので、DB操作のためのC#サービスを呼び出していないためです。

## 次の段階へ進む条件

5つの操作を完了し、`@page`、`@onclick`、`@bind`、`@if`が何に使われるかを実例で説明できたら[第1組の復習](../reviews/review-01.md)へ進みます。次の第2段階ではRazorコンポーネントそのものと、Static SSR、Interactive Server、SignalR、画面状態の寿命を詳しく学びます。
