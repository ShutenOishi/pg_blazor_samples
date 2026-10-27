# 学習順044：ErrorBoundary・取消・再試行

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser04Page.razor)。画面の正規URLは `/learn/044` です。

## 到達目標
このステップでは、画面がエラーを表示すること、非同期処理を取り消すこと、データの更新を元に戻すことを明確に分けます。WinFormsのtry/catchやCancellationTokenの知識はそのまま役立ちますが、Blazor Interactive Serverではコンポーネント、circuit、ブラウザーとの接続も考慮する必要があります。ErrorBoundaryは子コンポーネントの例外を扱い、代わりのUIを表示する部品です。すでに確定したDB操作を自動で取り消すものではありません。

この教材の画面には2つの独立した練習があります。1つ目はBrowserFaultDemo内で意図的にInvalidOperationExceptionを投げ、ErrorBoundaryが一般的な案内へ切り替わる動作です。2つ目はTask.Delayを使う学習用の非同期処理を開始し、CancellationTokenSourceから取消を通知します。実DB接続、外部サービスの呼出し、再試行による重複更新は含みません。この範囲を守って確認します。

## WinForms＋Dapperとの対応
| 既知の構成 | 今回の画面 | 違いと注意点 |
| --- | --- | --- |
| try/catchで例外を捕捉 | ErrorBoundaryで子コンポーネントの失敗を表示 | 全例外やDB操作を包括的に元へ戻すものではない |
| CancelボタンとCancellationTokenSource | pending?.Cancel() | 取消は協調的な通知であり即時強制停止ではない |
| DBトランザクションのCommit/Rollback | この練習にはDB処理なし | 取消とロールバックは別に設計する |
| 画面を閉じるときのDispose | RazorのIDisposable | 離脱後に処理が残り続けないよう通知する |
| エラー後の再実行 | ErrorBoundary.Recover() | 画面の復旧と副作用の再実行を分ける |

## 画面の実コード：子コンポーネントの例外
以下はErrorBoundaryを置いた部分の抜粋です。ChildContentに例外が発生し得る子コンポーネントを配置し、ErrorContentには業務内部の例外文やスタックトレースを出しません。Recoverは境界を復旧させる操作です。

```razor
<ErrorBoundary @ref="boundary">
    <ChildContent>
        <BrowserFaultDemo />
    </ChildContent>
    <ErrorContent>
        <p role="alert" data-testid="browser04-error">
            学習用のエラーを検出しました。操作をやり直してください。
        </p>
    </ErrorContent>
</ErrorBoundary>
<button type="button" @onclick="Recover">エラー表示を復旧する</button>

@code {
    private ErrorBoundary? boundary;
    private void Recover() => boundary?.Recover();
}
```

子のBrowserFaultDemo.razorではクリック時に学習用の例外を発生させます。例外を意図的に投げているのはサンプルの動作確認のためです。本番処理で利用者が自由に例外を発生させられる機能を設けるものではありません。

```razor
<button type="button" @onclick="ThrowSample">学習用の例外を発生させる</button>
@code {
    // 境界の表示切替を確認する学習専用の例外。
    private void ThrowSample() =>
        throw new InvalidOperationException("学習用の例外");
}
```

例外の診断にはサーバーログを用い、利用者には取り得る対応だけを伝えます。ErrorBoundaryの表示が復旧しても、原因が解決した保証はありません。同じ例外が再発する場合は原因の修正が必要です。特に業務の保存ボタンを単純に再試行させると、最初の処理が実は成功していた場合に二重登録するおそれがあります。

## 非同期処理の取消と状態管理
画面には「学習用の処理を開始する」「進行中の処理を取り消す」の2つのボタンがあります。処理中は開始ボタンを無効化し、取消ボタンを有効化します。busyはUIを表示するための状態です。この画面ではpendingを1つだけ保持し、RunAsync開始時にCancellationTokenSourceを作ります。

```razor
<button type="button" @onclick="RunAsync" disabled="@busy">
    学習用の処理を開始する
</button>
<button type="button" @onclick="Cancel" disabled="@(!busy)">
    進行中の処理を取り消す
</button>
<p role="status" data-testid="browser04-state">@state</p>
```

実際のRunAsyncは2500ミリ秒のTask.Delayで処理時間を模擬しています。業務の書き込みは行いません。Tokenを渡しているので、Cancelが通知されるとTask.DelayはOperationCanceledExceptionを投げ、取消専用のメッセージに切り替わります。finallyで参照とbusyを適切に戻し、CancellationTokenSourceを解放します。

```csharp
private async Task RunAsync()
{
    if (busy) return;
    var current = new CancellationTokenSource();
    pending = current;
    busy = true;
    state = "処理中";
    try
    {
        // 実DB操作はせず、キャンセル可能な待機だけを行う。
        await Task.Delay(2500, current.Token);
        state = "処理が完了しました。";
    }
    catch (OperationCanceledException)
    {
        state = "処理を取り消しました。";
    }
    finally
    {
        if (ReferenceEquals(pending, current))
        {
            pending = null;
            busy = false;
        }
        current.Dispose();
    }
}
private void Cancel() => pending?.Cancel();
```

ReferenceEqualsは、finallyで自分が開始した処理のpendingを片付けるための確認です。画面離脱時にはIDisposableのDisposeでpending?.Cancel()を実行しています。Cancelメソッドは処理が実際に完了することを待つ機構ではないため、実サービスで使うときは中断を待つ責務、所有するリソースの寿命、完了前後の競合も設計します。

## 実習を時系列で進める
1. /learn/044を開き、最初に「学習用の例外を発生させる」を押します。
2. 子コンポーネント部分が一般的な案内へ切り替わること、画面にInvalidOperationExceptionの詳細が表示されないことを確認します。Consoleとサーバーログの記録は利用者の案内とは別に観察します。
3. 「エラー表示を復旧する」を押し、同じ子コンポーネントのボタンが再び現れることを確認します。この時点ではDBの復旧処理は何も行われていません。
4. 「学習用の処理を開始する」を押し、表示が「処理中」となったら、完了前に「進行中の処理を取り消す」を押します。「処理を取り消しました。」に変わることを確認します。
5. 再び開始し、今度は取り消さずに待って「処理が完了しました。」を確認します。開始中に開始ボタンが無効になることも確認します。
6. 処理中にページを離れる操作を試し、コンポーネント破棄時のCancelの意味を考えます。画面を閉じたことと、外部システムが必ず処理を停止したことは同義ではありません。

## エラーを分類して診断する
| 症状 | 何が起きた可能性があるか | 確認・対処 |
| --- | --- | --- |
| 子コンポーネントがエラー表示へ切り替わる | 学習用InvalidOperationException | 境界内の例外を確認。一般的なUI案内とサーバーログを区別 |
| 復旧後すぐ再び失敗する | 原因は未修正 | Recoverを繰り返すだけで解決したことにしない |
| 取消を押せない | busyがfalse、すでに終了 | 非同期処理の開始・終了とボタン状態を調べる |
| 取消後に完了表示になる | 取消より完了が先だった可能性 | 協調的取消は競合することを理解する |
| 画面だけ処理中のまま | circuit切断、UI更新停止 | サーバー側処理の生存と表示更新を切り分ける |
| DBに二重登録された | 再試行と副作用の境界不明 | 業務キー、冪等性、トランザクションと監査を再設計 |
| SQL更新が確定した後で取消された | 完了済みの書込みは取消だけでは戻らない | 状態確認と業務上の取消処理を別に用意 |

## 本番の安全条件
業務DBの更新時には、認証済み本人と既存DB/外部台帳に基づく認可を保存直前に再検査し、競合制御や永続監査を同じ処理設計で扱います。取消ボタンを押したときに、一度Commitした更新や外部送信が自動で戻るとは記載してはいけません。通信断で応答だけが失われた場合は、実際の保存結果を確認せず再送すると重複処理の危険があります。再試行の可否は操作が読み取り専用か、冪等か、業務上重複を検出できるかによって判断します。

また例外詳細にSQL、接続文字列、利用者情報を載せたまま公開しません。ログには調査に必要な情報を残す場合も、権限のない利用者が読めないこと、個人情報や秘密情報を出し過ぎないことを確認します。ErrorBoundaryはUI障害の局所化には役立ちますが、認証や権限制御の代替ではありません。

## 演習と詳しい解答
**問1：** ErrorBoundaryのエラー案内をRecoverで消したら、保存途中のDBトランザクションも必ずロールバックしましたか。**解答：** いいえ。Recoverは境界の表示を復旧させます。DBのCommit/RollbackはDB接続・トランザクション処理で管理します。この学習画面にはDB更新そのものがありません。

**問2：** Task.DelayにCancellationTokenを渡さなければ、Cancelボタンだけ追加しても待機は途中で止まりますか。**解答：** 止まりません。取消はトークンを監視する処理が協調して初めて機能します。現在のコードはTask.Delay(2500, current.Token)に渡すため、取消がOperationCanceledExceptionとして扱われます。

**問3：** 画面の「処理中」を消したらサーバーの業務処理も停止したといえますか。**解答：** いえません。表示状態と非同期処理の実態は別です。業務サービス側に取消通知を伝えたか、処理が完了したか、保存が確定したかを必要に応じて確認します。

**問4：** 通信障害で保存結果の応答だけが届かないとき、同じ保存要求を無条件に自動再送できますか。**解答：** できません。実際は保存済みの可能性があり、重複更新や二重監査の原因になります。業務キーや要求IDで結果を確認するなど、重複を防ぐ設計が必要です。

## 完了確認と次への接続
- [ ] 学習用の例外を発生させ、一般的なエラー案内とRecoverを確認した。
- [ ] 非同期処理の開始、取消、完了を別々に確認した。
- [ ] UI復旧・協調的取消・DBロールバックの違いを説明できる。
- [ ] 保存の再試行に重複実行の危険があることを説明できる。

次の[学習順045：HTMLの意味・フォーカス・拡大表示の最終確認](browser-05.md)では、エラーや状態が利用者に実際に伝わるかをキーボードと拡大表示で検証します。