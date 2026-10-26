# ステップ24：MudFormで入力を検証する

## このステップでできるようになること
WinFormsの入力チェックと同様に、ユーザーが必要項目を入力したか確認します。MudBlazorの`MudForm`は複数の入力コンポーネントをまとめて検証し、日本語の必須エラーを表示できます。

## 前提と操作
Step23の入力コンポーネントを学んでから`/learn/24`を開きます。DB・ADは不要です。

1. 空欄のまま「入力を検証する」を押す。
2. 課題名に「課題名を入力してください。」、学習内容に「学習内容を入力してください。」と表示され、画面の結果は「入力内容に不備があります。」となる。
3. それぞれに架空の文字列を入力し、再度検証する。
4. 「入力検証に合格しました。」を確認する。

## Razorの主なコード

```razor
<MudForm @ref="_form">
    <MudTextField T="string" Label="課題名（必須）"
                  @bind-Value="_title" Required="true"
                  RequiredError="課題名を入力してください。" />
    <MudTextField T="string" Label="学習内容（必須）"
                  @bind-Value="_description" Required="true"
                  RequiredError="学習内容を入力してください。" />
    <MudButton OnClick="CheckAsync">入力を検証する</MudButton>
</MudForm>
<p role="status">@_result</p>

@code {
    private MudForm? _form;
    private string _title = "";
    private string _description = "";
    private string _result = "まだ検証していません。";

    private async Task CheckAsync()
    {
        if (_form is null) return;
        await _form.ValidateAsync();
        _result = _form.IsValid
            ? "入力検証に合格しました。"
            : "入力内容に不備があります。";
    }
}
```

これは実装の主要部です。`@ref="_form"`でMudFormのインスタンスを保持し、ボタンのイベントから`ValidateAsync`を呼びます。`RequiredError`に書いた独自の文言はMudBlazorの翻訳パッケージへ任せずアプリが日本語で定義します。

## 操作の処理順
1. ブラウザーから入力値がサーバーの`_title`・`_description`へ結び付く。
2. 検証ボタンを押すと`CheckAsync`が呼ばれる。
3. `ValidateAsync`がMudForm内の入力項目を検査する。
4. 必須欄が空なら`IsValid=false`となり、日本語エラーと不備の結果が表示される。
5. 必要項目を入力した場合は`IsValid=true`になり、合格の結果が表示される。

ここでは**入力の妥当性を確認するだけ**であり、データを保存する処理はありません。

## EditFormとの違い
Step10で扱ったBlazor標準の`EditForm`はデータモデル・バリデーションと結び付けます。`MudForm`はMudBlazorの入力コンポーネントの検証状態をまとめる仕組みです。どちらも画面の入力チェックに使えますが、選択基準、必要な型、検証メッセージの設定が異なります。業務サービス側に必須の制約や認可は別に実装します。

## よくある問題
- `@ref`がまだ作られていなければ`_form`は`null`の可能性があります。実装では早期終了しています。
- `RequiredError`の文字列は業務独自のメッセージです。翻訳を入れれば勝手に置き換わるわけではありません。
- フォームが有効でも、利用者に編集権限があるとは限りません。
- 必須チェックだけでは文字数・重複・DBの一意制約まで満たすことは保証できません。

## 演習と解答
**問：** 全項目を正しく入力した利用者が、DBの更新権限を持たない場合、`MudForm.IsValid`が`true`なら保存を許可してよいですか。

**答え：** いけません。入力検証と業務の認可は別です。後続のサービス側で本人と権限を確認します。

## 完了確認
- [ ] 未入力時の日本語エラーを確認した。
- [ ] 正しい入力で合格することを確認した。
- [ ] `ValidateAsync`と`IsValid`の役割を説明できる。

次は[ステップ25：MudTable](step25.md)です。
