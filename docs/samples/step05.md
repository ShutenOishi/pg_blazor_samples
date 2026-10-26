# ステップ05：双方向バインディングと日本語入力

**学習テーマ：** `@bind`、`@bind:event="oninput"`、文字列表示

**実装ソース：** [BindingPage.razor](../../src/PgBlazorSamples.Web/Components/Samples/Step05/BindingPage.razor)

## 前提

Step04までを理解していること。AD、DB、MudBlazorは不要です。

## 動作手順

1. `/learn/05`へ移動します。
2. 「表示する名前（架空の名前を入力）」という入力欄が空であることを確認します。
3. 下部に「まだ名前が入力されていません。」と表示されます。
4. 入力欄へ`テスト太郎`と入力すると「こんにちは、テスト太郎 さん。」へ変わります。
5. 入力の途中でも表示が更新されることを確認します。
6. 「入力を消去する」を押すと再び入力前の日本語案内に戻ります。

## 実コードの説明

```razor
<input id="learner-name" class="text-input" type="text"
       autocomplete="off" maxlength="40"
       @bind="learnerName" @bind:event="oninput" />
```

```csharp
// 入力値はこのページの状態に保持します。ADやDBには送信しません。
private string learnerName = "";

// 次の練習のために値を初期状態へ戻します。
private void ClearName() => learnerName = "";
```

- `@bind`により入力値とC#の`learnerName`を結びます。
- `@bind:event="oninput"`は入力のたびにC#の値を変更します。通常の`onchange`との違いを確認してください。
- `maxlength="40"`は入力欄の長さを制限します。実務では、サーバー側の入力検証も別途必要です。
- 入力値は通常のRazor表示でHTMLエンコードされます。`MarkupString`等を用いて無条件にHTML化しません。
- `@if`で未入力時に日本語の説明を表示しています。`@if`の詳しい学習はStep06です。

## 演習と否定テスト

- 漢字、ひらがな、カタカナを組み合わせて入力し、表示を確認してください。
- `<b>学習用</b>`と入力し、太字タグとして処理されず文字列のまま表示されることを確認してください。
- 入力の消去後に画面が初期状態へ戻ることを確認してください。
- **本名、アカウント、パスワードなどを入力しないでください。**

## 完了の判定

- [ ] 入力が表示に即時反映される
- [ ] 日本語文字列が正しく表示される
- [ ] 入力の消去とHTMLエンコードを確認できる

前：[ステップ04](step04.md)／[学習コラム01：ステップ01〜05を復習](../columns/column01-05.md)／次：[ステップ06](step06.md)
