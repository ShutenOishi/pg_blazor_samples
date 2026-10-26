# ステップ10：EditFormとDataAnnotationsによる日本語入力検証

**実装：** [フォーム画面](../../src/PgBlazorSamples.Web/Components/Samples/Step10/FormPage.razor)／[入力モデル](../../src/PgBlazorSamples.Web/Components/Samples/Step10/PracticeEntry.cs)

## 到達目標
`EditForm`、`DataAnnotationsValidator`、`InputText`、`InputTextArea`、`ValidationMessage`、`OnValidSubmit`の役割を説明できる。

## 前提
Step01〜09。DB・AD・DI不要。画面内の架空データのみ使用する。

## 操作手順・否定テスト
1. `/learn/10`を開き、両方空欄で「入力内容を確認する」を押す。
2. 「表示名を入力してください。」「学習メモを入力してください。」が日本語で表示される。
3. 表示名に`あ`、学習メモに`あい`と入力して再送信し、文字数エラーが表示されることを確認する。
4. 表示名`練習太郎`、学習メモ`コンポーネントを復習する`を入力して再送信する。
5. 「入力検証に合格しました（画面内のみ）。」が表示される。
6. 入力不足・文字数不足時には、検証済みの確認結果が新たに確定しないことを確認する。

## 実装のしくみ
`PracticeEntry.cs`に`[Required]`と`[StringLength]`を付け、日本語エラーメッセージを定義する。

~~~csharp
[Required(ErrorMessage = "表示名を入力してください。")]
[StringLength(20, MinimumLength = 2, ErrorMessage = "表示名は2文字以上20文字以内で入力してください。")]
public string DisplayName { get; set; } = "";
~~~

`FormPage.razor`は以下のように使用する。

~~~razor
<EditForm Model="@entry" OnValidSubmit="ConfirmEntry" FormName="step10-practice">
    <DataAnnotationsValidator />
    ...
</EditForm>
~~~

検証に合格すると`ConfirmEntry`が呼び出され、画面内の確認結果に値をコピーする。**この画面はデータベースへ保存しない。**

## 初心者が間違えやすい点
- HTML側の`maxlength`だけに依存すると、サーバー側の入力検証を省略しがちです。
- 入力値が正しい形式であることと、業務データを書き換える権限があることは別です。
- フォームは送信後も編集できます。最後の確認結果と編集中の値は別の状態として扱います。

## 完了条件
- [ ] 必須・文字数不足で日本語のエラーが表示される
- [ ] 正しい値のときだけ確認結果が更新される
- [ ] 入力検証、認証、認可、DB保存の違いを説明できる

前：[Step09](step09.md)／復習：[コラム02](../columns/column06-10.md)

次はStep11でDIの基本を学びます。