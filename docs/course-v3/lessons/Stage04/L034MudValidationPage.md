# 学習順34：MudFormの入力検証を学ぶ

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L034MudValidationPage.razor)。画面の正規URLは `/learn/034` です。

## 学ぶ前に
この教材はWinForms＋Dapperを使ってきた開発者が、WebのUIとサーバー側C#の境界を理解するためのものです。学習用の架空データで試し、認証・認可や実業務の保存が成立したとは見なしません。対象画面は /learn/034 です。

## WinForms＋Dapperとの比較
| 観点 | デスクトップでの経験 | MudBlazorでの対応 |
| --- | --- | --- |
| 必須検査 | ErrorProviderやValidating | RequiredとRequiredError |
| 実行方法 | 各検査の呼び出し | MudForm.ValidateAsync |
| 結果 | フォームの判定 | MudForm.IsValid |
| 認可 | 業務側で判断 | 入力検証とは独立 |

## 不具合時の切り分け
| 症状 | 確認する場所 |
| --- | --- |
| 必須エラーが出ない | Requiredと@ref、ValidateAsync |
| 文言が英語 | RequiredErrorで独自文言を指定 |
| 再入力後も失敗 | 再検証とIsValidを確認 |
| 合格後の保存拒否 | 認可やDB制約と入力検証を区別 |

## 画面入力の合格と業務操作の許可は違う
WinFormsのErrorProviderは入力エラーの手掛かりを示します。MudFormのRequiredErrorも同様に利用者が入力を直す助けになります。MudFormのValidateAsyncをawaitした後でIsValidを見ます。必須項目の値が整っていても、利用者に更新権限があるとは限りません。実DB保存時はサーバーのサービス側で既存台帳を確認し、競合・権限失効・監査失敗を安全に拒否する設計が必要です。

```razor
<MudForm @ref="_form">
    <MudTextField T="string" Label="課題名（必須）" @bind-Value="_title"
                  Required="true" RequiredError="課題名を入力してください。" />
</MudForm>
```

```csharp
private MudForm? _form;
private async Task CheckAsync()
{
    if (_form is null) return;
    await _form.ValidateAsync();
    _result = _form.IsValid ? "入力検証に合格しました。" : "入力内容に不備があります。";
}
```

@refはコンポーネント参照です。初回表示の前にはnullとなる場合があるのでガードしています。学習順018のEditFormと構造が違っても、UIのエラー検証とサーバー認可は分けて考える点は共通です。

**演習と詳しい解答：** 両欄を空にして検証ボタンを押すと2種類の日本語エラーと全体の不備が表示されます。架空の文字列を入力して再検証すれば必須条件を満たし合格します。エラー文言を試験する際、同一文言が画面の別の説明にもあるならフォームの検証領域を限定して調べる必要があります。合格してもDB保存が許可される保証はなく、DapperなどのDB処理より前に業務上の認可を確認します。

## 実装に対応した操作・コード・詳細解答

## このステップでできるようになること
WinFormsの入力チェックと同様に、ユーザーが必要項目を入力したか確認します。MudBlazorの`MudForm`は複数の入力コンポーネントをまとめて検証し、日本語の必須エラーを表示できます。

## 前提と操作
学習順033の入力コンポーネントを学んでから`/learn/034`を開きます。DB・ADは不要です。

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
学習順018で扱ったBlazor標準の`EditForm`はデータモデル・バリデーションと結び付けます。`MudForm`はMudBlazorの入力コンポーネントの検証状態をまとめる仕組みです。どちらも画面の入力チェックに使えますが、選択基準、必要な型、検証メッセージの設定が異なります。業務サービス側に必須の制約や認可は別に実装します。

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

次は[学習順035：MudTable](L035MudTablePage.md)です。


## 学習順34の追加確認
- [ ] 画面で操作を実際に行い、状態の変化を確認した。
- [ ] 比較表と切り分け表に沿って、WinFormsとの相違点を説明できる。
- [ ] DapperによるDB検索と画面表示の処理を区別できる。
- [ ] 教材の演習に回答し、失敗時の復旧手順を説明できる。
