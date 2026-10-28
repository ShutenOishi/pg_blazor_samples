# 学習順016：初期化・更新・破棄を理解する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L016ComponentLifecyclePage.razor)。画面の正規URLは `/learn/016` です。

## 到達目標と前提
この学習では、Blazorのコンポーネントが生成されてから破棄されるまでの流れを、親と子の画面を操作して確認します。対象画面は `/learn/016` です。WinFormsでForm.Load、プロパティ変更、Form.Disposeを使った経験を活かしつつ、Blazor固有のライフサイクルの違いを理解してください。特に「画面の表示が変わった」「親から新しい値が届いた」「子のインスタンスを作り直した」は異なる出来事です。

学習順015までで、ParameterとEventCallback、Interactive Server、SignalRを学びました。この学習ではまだ業務用DB、AD認証、永続監査を使いません。Visual Studio 2026、.NET 10、Chromeでアプリを起動してください。操作するのは架空のテーマ名だけです。

## インスタンスを基準に処理を読む
親画面は `showChild` がtrueのときに `LifecycleTracker` という子コンポーネントを表示します。子には `Topic` パラメーターがあり、親の `currentTopic` が渡されます。「テーマを変更する」を押すと親は `topicNumber` を増やして `currentTopic` を更新します。表示中の子を取り除いていなければ、親が新しい値を渡すのは同じ子のインスタンスです。このとき子の初期化をやり直す必要はありません。

一方、「子を非表示にする」を押すと、親の `@if` がfalseになり、レンダーツリーから子が除かれます。子の `IDisposable.Dispose` は所有する `CancellationTokenSource` を取り消して解放します。「子を再表示する」を押した場合は別の子インスタンスが作られ、初期化回数は新しいインスタンスの1回から始まります。テーマ名は親で保持しているため、現在のテーマが新しい子に渡されます。

```razor
@if (showChild)
{
    <LifecycleTracker Topic="@currentTopic" />
}
else
{
    <p role="status" data-testid="step09-hidden">
        子コンポーネントを破棄しました。再表示すると新しいインスタンスになります。
    </p>
}
```

この条件分岐が重要です。単にCSSで非表示にする方法では、コンポーネントがレンダーツリーに残ることがあり、破棄の練習にはなりません。親画面の処理は次のとおりです。

```csharp
private bool showChild = true;
private int topicNumber;
private string currentTopic = "初回のテーマ";

private void ChangeTopic()
{
    topicNumber++;
    currentTopic = "変更後のテーマ " + topicNumber;
}
private void ToggleChild() => showChild = !showChild;
```

ここでは `ChangeTopic` を呼んでも `showChild` を変えていません。したがって子は残り、親の再描画を通じてParameterが渡されます。Parameter更新の回数は必ず一度だけとは限りません。親の描画やパラメーターの設定タイミングに影響されるため、カウンターの値を業務処理の実行回数そのものとして使わないでください。

## 非同期初期化と破棄の責務
子の `OnInitializedAsync` は、各インスタンスの初期化時に実行されます。実装はネットワークアクセスではなく、50ミリ秒のキャンセル可能な `Task.Delay` を使います。処理中に親が子を非表示にするなどしてDisposeが呼ばれたら、中断を要求します。これは業務処理を完了したことの証明ではありません。

```csharp
private readonly CancellationTokenSource lifetimeCancellation = new();
private int initializationCount;
private int parameterSetCount;

protected override async Task OnInitializedAsync()
{
    initializationCount++;
    try
    {
        await Task.Delay(50, lifetimeCancellation.Token);
    }
    catch (OperationCanceledException)
    {
        // ページ移動などによる破棄時のキャンセルは想定内です。
    }
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

`OnParametersSetAsync` はパラメーターの受け取りに対応する処理です。`OnInitializedAsync` と違って同じ子インスタンスでも複数回呼ばれ得ます。画面には両方の回数が表示されるので、同じインスタンスの更新と新しいインスタンスの初期化を比較してください。`Dispose` は購読解除やリソースの解放にも使われますが、この演習で実際に行うのはトークンのCancelとDisposeだけです。破棄処理の途中で `StateHasChanged` を呼ぶ実装を追加しないでください。

## WinForms＋Dapperとの比較
| WinFormsでの経験 | 今回のBlazorの対応 | 同じではない点 |
| --- | --- | --- |
| Form.Load | OnInitializedAsync | 同一インスタンスの初期化。事前レンダリングと対話型では別インスタンスになり得る |
| UserControlへの値の設定 | [Parameter]とOnParametersSetAsync | 親の再描画などでも呼ばれ得る |
| Form.Close、Dispose | IDisposable.Dispose | レンダーツリーから外された子も破棄の対象になる |
| 長い処理を中止する | CancellationTokenSource | キャンセル要求と実際の処理停止は区別する |
| Dapper.Execute | この画面には存在しない | テーマ変更もカウンターもDBには保存されない |

WinFormsでは同じプロセス内のFormとコントロールの寿命を意識して設計します。Interactive Serverでは、コンポーネントのC#はサーバー側で実行され、ブラウザーの表示や接続の影響も受けます。フォームを閉じる操作とページの再読込が同じイベント系列を保証するわけではありません。ライフサイクルに業務上の一度きりの更新を任せない設計が必要です。

## 画面操作と観察
1. `/learn/016` を開き、子の「受け取ったテーマ」「OnInitializedAsync実行回数」「OnParametersSetAsync実行回数」を読み取ります。対話型の表示中の値を記録してください。
2. 「テーマを変更する」を2回押します。テーマが「変更後のテーマ 2」になることを確認し、初期化回数が増えていないか観察します。
3. 「子を非表示にする」を押します。「子コンポーネントを破棄しました」という表示に変わり、子のカウンターが消えることを確認します。
4. 「子を再表示する」を押します。新しく生成された子の初期化回数は1になり、テーマは親が覚えている変更後のものが表示されます。
5. ブラウザーの再読込を行い、親の `topicNumber` も初期状態へ戻ることを確認します。子の一時値もDBに保存されていません。
6. ChromeのNetworkとサーバーログを併せて読み、Documentの取得、対話接続、イベント操作を混同しないようにしてください。

## 障害の切り分け
| 症状 | 最初に調べる場所 | 判断の基準 |
| --- | --- | --- |
| テーマだけ変わり、初期化回数は1 | ChangeTopicとshowChild | 同じ子へのParameter更新なら正常 |
| パラメーター設定回数が思った値と違う | 親の再描画とOnParametersSetAsync | 呼出回数は1操作につき厳密に1とは限らない |
| 非表示でTask.Delayが中止された | DisposeとCancel | この演習では想定された終了 |
| 再表示でカウンターがリセットされた | @ifによる子の生成 | 新しい子のフィールドなので正常 |
| HTMLは見えるがボタンが反応しない | Network、SignalR、サーバーログ | 静的表示だけでは対話接続の成功はわからない |
| 再読込でテーマまで初期値へ戻った | 親のフィールドの保持期間 | DBやブラウザーへの保存は実装していない |

症状を調べるときは、最初に「インスタンスが同じか」、次に「Parameterが変わったか」、最後に「サーバーの実行が失敗したか」を分けて記録します。通信の失敗をC#の条件分岐の不具合と決めつけず、ブラウザー側とサーバー側のログを照合します。

## 演習と詳しい解答
**演習A：** 初期表示からテーマを変更し、その後非表示と再表示を行ったとき、どの場面で子のOnInitializedAsyncが再実行されるでしょうか。

**解答A：** テーマ変更は同じ子インスタンスへのParameter更新です。表示中の子を取り除いていないので初期化をやり直しません。非表示にすると子は破棄され、再表示で新しい子が作られるため初期化が行われます。親のテーマは非表示の間も親が保持し、新しい子へ渡されます。

**演習B：** 一つのテーマ変更でOnParametersSetAsyncは必ず1回だけ呼ばれるでしょうか。

**解答B：** その保証はありません。親の再描画やパラメーター更新に伴って呼ばれるので、表示された回数を確認してください。業務の処理回数を保証したいなら、OnParametersSetAsyncの回数ではなく、明示された要求とサーバー側の重複防止を使います。

**演習C：** 非同期の業務処理が動いている最中に子を非表示にした場合、Cancelを呼べばDBの更新は確実に取り消されたと言えますか。

**解答C：** 言えません。Cancelは協調的な中断要求です。通信やDB処理がすでに完了した可能性もあります。業務用の保存は認可、トランザクション、競合判定、永続監査を伴う独立した処理として設計し、結果を再確認する必要があります。

## 次に進む前に
学習順015のcircuitの寿命と、今回の子インスタンスの寿命を区別できることが目標です。事前レンダリングがある場合はページ表示全体で初期化が複数回になることもあります。画面のカウンターは表示中のインスタンスに限定され、サーバー全体の呼び出し総数を示しません。その仕組みを次の学習順017で詳しく確認します。

前：[学習順015の復習](/learn/column/runtime-15)／次：[学習順017](/learn/017)
