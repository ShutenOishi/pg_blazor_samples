# 学習順018：入力フォームを日本語で検証する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L018ValidatedFormPage.razor)。画面の正規URLは `/learn/018` です。

## 到達目標
`/learn/018`でEditForm、DataAnnotationsValidator、InputText、InputTextArea、ValidationMessage、OnValidSubmitの関係を理解します。WinFormsでTextBoxやErrorProviderを使った経験があっても、Blazorではモデルの属性とフォームの送信イベントを組み合わせて検証します。この練習で扱うのは架空の表示名と学習メモだけです。DBへの保存、AD認証、業務権限の付与は行いません。

最初に「文字列の形式が妥当なこと」「画面内で確認したこと」「DBへ登録が完了したこと」を別の状態として考えてください。入力エラーがないだけでは、利用者が更新操作を許可されたことにはなりません。学習順017までのコンポーネント初期化の知識も使います。

## 編集モデルと確認結果は別のもの
FormPage.razorでは `PracticeEntry entry` に入力中の値を保持します。EditFormの `Model="@entry"` がこのモデルを参照し、DataAnnotationsValidatorが属性による検証を実行します。表示名は必須で2〜20文字、学習メモも必須で3〜80文字です。`PracticeEntry.cs`のRequiredとStringLengthには日本語のErrorMessageが指定されています。エラーはそれぞれの入力欄に対応するValidationMessageで表示されます。

```razor
<EditForm Model="@entry" OnValidSubmit="ConfirmEntry" FormName="step10-practice">
    <DataAnnotationsValidator />
    <label for="step10-display">練習用の表示名（2〜20文字・必須）</label>
    <InputText id="step10-display" class="text-input"
               @bind-Value="entry.DisplayName" />
    <ValidationMessage For="@(() => entry.DisplayName)" />
    <label for="step10-memo">学習メモ（3〜80文字・必須）</label>
    <InputTextArea id="step10-memo" class="text-input"
                   @bind-Value="entry.LearningMemo" />
    <ValidationMessage For="@(() => entry.LearningMemo)" />
    <button type="submit">入力内容を確認する</button>
</EditForm>
```

`OnValidSubmit` は検証が成功した場合のみ呼び出されるイベントです。これに対して「ボタンを押した」という事実だけで保存処理を行う設計は誤りです。入力欄で使っている `InputText` などのコンポーネントは、前の学習順で扱った通常のHTML入力と違い、Blazorの編集コンテキストに結び付く仕組みがあります。ラベルのforと入力欄のidを対応させることで、日本語の画面を読み上げやキーボードでも理解しやすくします。

## 属性とサーバー側の処理を読む
```csharp
[Required(ErrorMessage = "表示名を入力してください。")]
[StringLength(20, MinimumLength = 2,
    ErrorMessage = "表示名は2文字以上20文字以内で入力してください。")]
public string DisplayName { get; set; } = "";

[Required(ErrorMessage = "学習メモを入力してください。")]
[StringLength(80, MinimumLength = 3,
    ErrorMessage = "学習メモは3文字以上80文字以内で入力してください。")]
public string LearningMemo { get; set; } = "";
```

画面側は入力中の値と最後に妥当と判断された値を分けています。`ConfirmEntry` はDBを操作するメソッドではありません。

```csharp
private readonly PracticeEntry entry = new();
private string? confirmedName;
private string? confirmedMemo;

private void ConfirmEntry()
{
    // 画面内の確認結果だけを更新する。
    confirmedName = entry.DisplayName;
    confirmedMemo = entry.LearningMemo;
}
```

この違いは業務用の入力画面で特に重要です。妥当な文字列を一度確定して確認欄を表示した後に、不正な文字列を入力して送信しても、前回の確認結果はそのまま残る場合があります。これは新しい不正値で上書きされたという意味ではありません。実際の業務画面を設計するときは「編集中」「検証エラー」「確認済み」「DB保存済み」「競合」「保存拒否」などの状態を混同しない表示を考えます。

## WinForms＋Dapperとの比較
| WinForms＋Dapperの経験 | 今回の対応 | 確認すべき違い |
| --- | --- | --- |
| TextBoxとValidating | InputTextとEditForm | フォームのModelと検証の関係がある |
| ErrorProvider | ValidationMessage | 入力項目に対応した日本語エラーを表示 |
| 属性や条件式での検査 | Required、StringLength | 必須と長さの検証がモデル側にある |
| OKボタンで確定 | OnValidSubmit | 妥当な場合だけ確認欄を更新 |
| Dapper.ExecuteでDBに保存 | 今回は未実装 | 画面内の確認結果は永続化ではない |

WinFormsでも画面側の検証を通れば自動的に利用者の権限が付くわけではありません。Webではフォームの内容やHTTP要求を利用者が変更できるため、サーバー側の業務処理へ渡す前に検証・本人確認・認可を行う責任が一層重要です。この練習では入力の妥当性だけに範囲を絞ります。

## 実際の操作手順
1. `/learn/018`を開き、表示名とメモを空欄のまま「入力内容を確認する」を押します。「表示名を入力してください。」「学習メモを入力してください。」が表示されます。
2. 表示名に「あ」、学習メモに「あい」を入力して送信します。両方とも指定の最小文字数に足りないため、長さに関する日本語のメッセージを確認します。
3. 表示名に「練習太郎」、メモに「フォームの入力検証を復習する」を入力して送信します。妥当な値なら「入力検証に合格しました（画面内のみ）。」と両方の値が確認欄に現れます。
4. その後で表示名だけ1文字に戻し、再び送信します。現在の編集値は不正でも、以前の確認結果が残る場合があります。新しい値が確定していないことを説明してください。
5. ブラウザーを再読込し、編集値と確認欄が永続化されていないことを確かめます。DB検索やINSERTはこの画面では実行されません。

操作の際に実際の名前や業務上の秘密を入力しないでください。画面にも「架空のデータのみを入力してください」と案内があります。テストケースは正しく検証される境界値を選んで記録しましょう。

## 失敗・症状の切り分け
| 症状 | 調べる場所 | 確認と対策 |
| --- | --- | --- |
| 空欄なのに新しい確認結果が出た | DataAnnotationsValidatorとOnValidSubmit | 必須検証と呼出し条件を確認 |
| 1文字でも合格した | StringLengthのMinimumLength | 2文字以上、3文字以上の条件を確認 |
| エラーが英語で表示された | ErrorMessageの指定 | この練習では日本語の固定文言を使用 |
| 入力を直しても以前の結果が見える | entryとconfirmedNameの区別 | 前回の確定表示と現在の編集状態は別 |
| フォーム送信時に画面が動かない | SignalR、Console、サーバーログ | HTML表示だけでは対話イベント成功を保証しない |
| DBに新しい行がない | ConfirmEntryの中身 | 画面内コピーしか行っておらず正常 |

検証が失敗した場合は、まず実際の入力値と属性の条件を比べ、次にEditFormとModel、ValidationMessageを確認します。NetworkのDocument取得と対話通信は別です。フォームが画面に見えることを、クリックイベントやサーバー処理の成功と取り違えないようにしてください。

## より深く考える
HTMLのmaxlengthなどのUI制限だけに依存すると、サーバーへ直接送られた値の検証が抜ける危険があります。ここで使用するDataAnnotationsは入力形式の検証であり、業務アクセスを認可する機能ではありません。後続の実DB連携では、ADから認証済みのドメインユーザー名を取得し、事前登録済みの既存DB・外部台帳で都度認可する方針です。保存直前の権限再確認やxmin競合、監査も別途必要です。今回のOnValidSubmitの成功だけでそれらを合格扱いしません。

また、一度成功した確認欄が残ることは、操作履歴の永続的な証明ではありません。再読込や接続失効で消える可能性があります。利用者に「保存しました」と伝える前には、実際の永続保存が成功したことをサーバーで確かめる必要があります。

## 演習と詳しい解答
**演習A：** 空欄、1文字の表示名、正しい表示名という3条件について、ConfirmEntryが呼ばれる条件を説明してください。

**解答A：** 両項目がRequiredとStringLengthを満たした送信でOnValidSubmitが呼ばれます。空欄や文字数不足ならValidationMessageへ日本語エラーを表示し、現在の不正な入力は確認結果にコピーされません。以前の確認結果が画面に残る可能性がある点にも注意します。

**演習B：** 表示名の検証に通ったことから、ADで本人確認済みと判断できますか。

**解答B：** できません。表示名は利用者が入力する練習用文字列です。認証済みのドメインユーザー名を確認する処理はありません。既存DB台帳に基づく認可や監査も行わないため、入力検証の成功を業務更新の許可と結び付けてはいけません。

**演習C：** 一度「練習太郎」で合格した後に表示名を「あ」に書き換えて送信しました。確認欄が「練習太郎」のままの場合、どう説明すべきでしょうか。

**解答C：** 編集モデルentryの現在値と、確認済みのconfirmedNameは別です。後の送信は検証エラーのためConfirmEntryが呼ばれず、以前の確認結果が残ります。「現在の入力が合格した」でも「DBに練習太郎が登録された」でもありません。必要なら業務UIでは確認欄に確定時点を表示したり、編集再開時に旧結果を消したりする設計を行います。

## 次の学習
次の学習順019では、画面状態のうちURLに残る表示条件と、再読込で初期化されるコンポーネントのカウンターを比べます。フォーム入力の一時値も、保存処理がなければ永続化されないことを思い出してください。

前：[学習順017](/learn/017)／次：[学習順019](/learn/019)
