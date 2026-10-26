# 第4組の復習：初期化・再読込・状態の復習

## 復習の目標
学習順016〜020では、画面の表示・入力・クリックができることと、状態が永続化されたことを区別して学びました。WinForms＋Dapperの経験者は、Formのフィールド、Loadイベント、Validatingイベント、設定ファイル、DB更新の役割を思い浮かべると理解しやすくなります。ただしBlazorのInteractive Serverでは、C#のコンポーネントがサーバー側で動き、ブラウザーのHTML表示と対話接続、事前レンダリングのインスタンス、circuitの保持期間などを考える必要があります。

この復習はDB・AD・実IISを用意せずに実行できます。画面は架空の値を扱うものです。入力検証に合格しても業務認可は成立せず、ブラウザー上に文字が表示されてもDBに保存されたことにはなりません。各画面で実装された処理だけを根拠に結果を説明してください。

## 5件の学習順と観察対象
| 学習順 | 実画面URL | 画面で行う操作 | 違いを説明する概念 |
| --- | --- | --- | --- |
| 016 | /learn/09 | テーマ変更、子の非表示と再表示 | OnInitializedAsyncとOnParametersSetAsync、Dispose |
| 017 | /learn/runtime-03 | 初期化・描画後・変更の回数 | 事前レンダリングと対話型インスタンス |
| 018 | /learn/10 | 空欄、短い文字、妥当な入力で送信 | EditFormと日本語のDataAnnotations |
| 019 | /learn/runtime-04 | 履歴タブを選択して再読込 | URLのtabと一時的なcount |
| 020 | /learn/runtime-05 | circuit・ブラウザー・DBを選択 | 保存先の性質と安全な選び方 |

## WinForms＋Dapperと対応させる
| WinFormsでの経験 | Blazorの学習対象 | 特に注意する条件 |
| --- | --- | --- |
| Form.LoadとForm.Dispose | コンポーネントの初期化と破棄 | 親の再描画と新規インスタンスは別 |
| 画面のShownイベント | OnAfterRender | 事前レンダリング中は呼ばれない |
| ErrorProviderでの必須検証 | EditFormとDataAnnotationsValidator | 入力妥当性は認可でも保存でもない |
| 起動引数や画面の選択 | URLのクエリパラメーター | 共有できる公開条件だけを置く |
| Formフィールドや設定ファイル | circuitとブラウザーの保存候補 | 保持期間、改ざん可能性が異なる |
| Dapper.Executeとトランザクション | 後続のDB実装 | 今回の画面には書込み処理が存在しない |

## 画面を順番に操作する
1. `/learn/09`で初期化回数を読み、「テーマを変更する」を二度押します。テーマが変わっても同じ子の初期化回数は増えないことを確認します。「子を非表示にする」から「子を再表示する」へ進むと、新しい子が作られます。
2. `/learn/runtime-03`で表示中の初期化回数と初回の描画後処理を読みます。「状態を変更する」を二度押して変更回数が2になることを確かめ、初期化との違いを説明します。
3. `/learn/10`で二つの欄を空にして送信し、日本語の必須エラーを確認します。表示名「あ」、メモ「あい」でも送信し、最小文字数エラーを確認します。最後に「練習太郎」「フォームの入力検証を復習する」で送信し、画面内の確認欄が表示されることを確かめます。
4. `/learn/runtime-04`で「履歴」を選び、操作回数を2へ増やした後に再読込します。URLのtabは残り、countは0に戻ります。ブラウザーの履歴を戻すこととメモリ値の復元は別です。
5. `/learn/runtime-05`で三種類の保存先の説明を切り替えます。「業務の永続データ」を押してもDBへの書込み処理がないことをコードから確認します。再読込後にselectedが初期値へ戻ることも確かめます。

## 実コードを読み取る問題
次は学習順016の子で使用するコードの要点です。

```csharp
protected override async Task OnInitializedAsync()
{
    initializationCount++;
    try { await Task.Delay(50, lifetimeCancellation.Token); }
    catch (OperationCanceledException) { /* 破棄による中断 */ }
}
protected override Task OnParametersSetAsync()
{
    parameterSetCount++;
    return Task.CompletedTask;
}
public void Dispose()
{
    lifetimeCancellation.Cancel();
    lifetimeCancellation.Dispose();
}
```

**問題A：** 親のテーマだけ変えても子を取り除いていないとき、初期化とParameter設定のどちらに関係しますか。

**解答A：** 同じ子インスタンスに新しいTopicが渡されるため、Parameter設定が関係します。初期化はそのインスタンスの作成時に行われます。非表示で子を取り除いた後に再表示したときは新しい子を初期化します。Parameter設定回数は描画条件によって異なり、厳密に1増える保証ではありません。

次は学習順017の実コードです。

```csharp
protected override void OnInitialized() => initialized++;
protected override void OnAfterRender(bool firstRender)
{
    if (firstRender)
    {
        afterFirstRender++;
        _ = InvokeAsync(StateHasChanged);
    }
}
private void Add() => changes++;
```

**問題B：** Addを2回押した後の画面で初期化回数が1だった場合、サーバー全体のOnInitialized総回数も1ですか。

**解答B：** そうとは限りません。画面上の数値は現在の対話型インスタンスのフィールドであり、事前レンダリングに使われた別のインスタンスの回数は含みません。Addはchangesだけを増やします。初期化が総計何回行われたかを判定するには、実行構成とサーバーログなどの別の観測が必要です。

フォームの処理は次のようになっています。

```razor
<EditForm Model="@entry" OnValidSubmit="ConfirmEntry" FormName="step10-practice">
    <DataAnnotationsValidator />
    <ValidationMessage For="@(() => entry.DisplayName)" />
</EditForm>
```

**問題C：** 表示名1文字で送信したとき確認結果が更新されないのはなぜでしょうか。

**解答C：** モデルのStringLengthで最小2文字を要求しているためです。OnValidSubmitは妥当な入力でのみ実行されます。以前に確認した結果が残ることはあっても、その不正な新入力が妥当だったことを示しません。この画面ではDBへは保存していません。

URLの処理と保存先選定のコードは以下です。

```csharp
[SupplyParameterFromQuery(Name = "tab")]
[Parameter] public string? Tab { get; set; }
private int count;
private void Increase() => count++;
private void Choose(string value)
    => Navigation.NavigateTo($"/learn/runtime-04?tab={value}");

// 別画面の状態選択。実際の保存処理ではない。
private string selected = "circuit";
private void Select(string value) => selected = value;
```

**問題D：** URLを `?tab=history` とし、countを2へ増やして再読込するとどうなりますか。

**解答D：** URLの条件から履歴タブが再現されますが、countは新しいコンポーネントの初期値0へ戻ります。URLの表示条件と現在のサーバー側フィールドを別々に保持しているからです。履歴からURLを開き直すことと、画面内のすべての操作を復元することは同じではありません。

**問題E：** runtime-05で「業務の永続データ」を選んだら、DBのINSERTが成功したと判断できますか。

**解答E：** できません。Selectはselectedの文字列を変更するだけであり、DB接続や保存、認可、監査のコードを含みません。画面は保存先の選び方を説明する教材です。実際に永続化するにはサーバー側で本人と権限を確認し、必要な更新・競合制御・監査を行う処理を別途実装します。

## 症状別の切り分け
| 症状 | 確認すること | 誤解しない点 |
| --- | --- | --- |
| 子のテーマだけ変わり初期化回数が同じ | showChildとParameter | 同じ子への更新なら正常 |
| 初期化ログが複数回記録された | 事前レンダリング、対話型のインスタンス | ライフサイクルを一度きりのDB登録に使わない |
| 入力値が短く送信エラー | Required、StringLength、ValidationMessage | エラーを表示しても認可とは無関係 |
| URLの条件が再読込後も残る | tabのクエリ値 | 認証・権限情報をURLに入れない |
| カウンターや選択が再読込後に消える | フィールドの初期値 | DBの障害ではない |
| ブラウザーストレージの値で権限を決めたい | 改ざん可能性と信頼境界 | サーバーの既存台帳で都度確認する |

## 応用演習と詳しい解答
**演習F：** ブラウザーのlocalStorageに「管理者」と保存された文字列があれば、業務データを更新してよいですか。

**解答F：** いけません。ブラウザーストレージは利用者が改ざん可能です。ADで本人認証したドメイン付きユーザー名と、事前整備された既存DBまたは外部台帳を使い、サーバー側で都度認可する必要があります。保存直前の権限再確認、更新競合、永続監査も業務要件に応じて保証します。

**演習G：** 申請書の途中入力を失いたくないとき、すべてをURLへ入れればよいですか。

**解答G：** よくありません。URLは共有・履歴・ログへ残るため、個人情報や秘密を含む入力は不適切です。保存期間、機密性、端末間共有、認可、復元要件を整理して適切な保存先を選びます。circuitのフィールドだけでは再読込時に失われる可能性があります。DBへ保存する場合は本人と権限の確認を伴う実際の保存処理が必要です。

**演習H：** 一つの画面で入力検証、認証、認可、保存、監査を行う場合、成功の判定はすべてOnValidSubmitに任せられますか。

**解答H：** 任せられません。OnValidSubmitはモデルの入力検証を通過したことを示すイベントです。認証済み本人、事前整備されたDB台帳での権限、競合と保存、監査は別のサーバー側処理です。途中で安全条件が成立しなければ拒否する必要があります。この第4組の演習画面はそれらの業務処理をまだ実装していません。

## 次の段階へ進む基準
- [ ] 生成・Parameter更新・破棄の違いを説明できる。
- [ ] 事前レンダリングの別インスタンスと表示中の回数を区別できる。
- [ ] EditFormの検証成功とDB保存・認可の違いを説明できる。
- [ ] URLに残る条件と再読込で消えるフィールドを予測できる。
- [ ] circuit・ブラウザー・DBを機密性と寿命で使い分けられる。
- [ ] 実AD・IIS・FDW・永続監査を今回の演習で検証したと誤解していない。

次は[学習順021：DI（依存性注入）の基本](/learn/11)です。画面と状態の寿命を理解した上で、Webサーバーで使うサービスの作り方と寿命へ進んでください。
