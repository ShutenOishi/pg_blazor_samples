# ステップ07：親子コンポーネントとParameter

**実装：** [親画面](../../src/PgBlazorSamples.Web/Components/Samples/Step07/ParametersPage.razor)／[子部品](../../src/PgBlazorSamples.Web/Components/Samples/Step07/GreetingCard.razor)

## 到達目標
同じRazorコンポーネントを複数の場所で利用し、親の値を`[Parameter]`で子へ渡す。データの所有者を説明できる。

## 前提
Step01〜06。AD、DB、DI、外部ライブラリは不要。`/learn/07`を開く。

## 実際に動かす
1. 入力欄に`練習花子`を入力する。
2. 上のカードに「こんにちは、練習花子 さん。」が表示される。
3. 下のカードは「架空の利用者」のまま変化しない。
4. 入力を消しても、下のカードには影響がないことを確認する。

## ソースコードの重要な部分
~~~razor
<GreetingCard LearnerName="@learnerName" Topic="親が入力した名前" />
<GreetingCard LearnerName="架空の利用者" Topic="別の値でも同じ部品" />
~~~

~~~csharp
// Parameterは親が渡す入力です。子コンポーネント内で書き換えません。
[Parameter]
public string LearnerName { get; set; } = "ゲスト";
~~~

### なぜ必要？
同じ表示処理を複数の親で使える。部品をコピーして増やすより、修正の対象が明確になる。親は値の持ち主、子は受け取って表示する役割にする。

### よくある間違い
子が`LearnerName = "別の名前"`と書き換えること。親が次に描画すると値が上書きされ、責任分担が不明瞭になる。子から親への通知はStep08で扱う。

## 演習・確認
- [ ] 2枚のカードで同じ部品を使っていると説明できる
- [ ] 親の入力と、子の表示が連動する
- [ ] 子の`[Parameter]`を勝手に変更しない理由を説明できる

前：[Step06](step06.md)／次：[Step08](step08.md)