# 学習順27：非同期処理とログを使う

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L027AsyncLoggingPage.razor)。画面の正規URLは `/learn/027` です。

対象画面は`/learn/027`です。WinForms＋Dapperの経験と結び付けて理解します。

## このステップでできるようになること
WinFormsの`async/await`による非同期イベント処理を、Blazor Interactive Serverの画面でも扱います。処理中・完了・取消の表示と、ASP.NET CoreがDIで提供する`ILogger<T>`を組み合わせます。

この教材では**外部通信もDB書き込みも実行しません**。サーバー上で800ミリ秒待つ処理を使い、ブラウザーからの操作とサーバー側処理の関係を確認します。

## 前提と期待する動作
学習順021〜16を学んでから`/learn/027`を開きます。DB・ADは不要です。

1. 初期表示「まだ実行していません。」を見る。
2. 「非同期処理を開始する」を押し、「処理中です。」から「処理が完了しました。」へ変わるのを確認する。
3. 再度開始し、完了前に「処理を取り消す」を押すと「処理を取り消しました。」になる。
4. 処理していない間、取消ボタンは無効です。取消と完了はタイミングで競合し得るため、すでに完了した後の取消が成功したとは解釈しません。

## Razorと処理の構成
サーバー側のコンポーネントは`IDisposable`を実装し、ログサービスを注入します。

```razor
@page "/learn/027"
@implements IDisposable
@inject ILogger<AsyncLoggingPage> Logger

<p role="status">@state</p>
<button @onclick="StartAsync">非同期処理を開始する</button>
<button @onclick="Cancel" disabled="@(!running)">処理を取り消す</button>
```

実際の`AsyncLoggingPage.razor`の要点は次のとおりです。

```csharp
private string state = "まだ実行していません。";
private CancellationTokenSource? cancellation;
private bool running;

private async Task StartAsync()
{
    cancellation?.Cancel();
    cancellation?.Dispose();
    cancellation = new CancellationTokenSource();
    var current = cancellation;
    running = true;
    state = "処理中です。";
    try
    {
        await Task.Delay(800, current.Token);
        state = "処理が完了しました。";
        Logger.LogInformation("ステップ17の練習用非同期処理が完了しました。");
    }
    catch (OperationCanceledException)
    {
        state = "処理を取り消しました。";
    }
    finally
    {
        if (ReferenceEquals(cancellation, current)) running = false;
    }
}

private void Cancel()
{
    cancellation?.Cancel();
    state = "処理を取り消しました。";
}

public void Dispose()
{
    cancellation?.Cancel();
    cancellation?.Dispose();
}
```

これは教材の対象実装に合わせたコードです。二重開始のときは古い処理をキャンセルし、新しい`CancellationTokenSource`に取り替えます。`ReferenceEquals`によって、古い処理の`finally`が新しい処理の`running`を誤って消さないようにしています。

## クリックから表示更新までの順序
1. ブラウザーからクリックイベントがサーバーのコンポーネントへ届く。
2. `StartAsync`が`state`と`running`を変更する。
3. `Task.Delay(800, current.Token)`を待っている間、サーバーのスレッドを同期的に800ミリ秒占有するわけではありません。
4. 正常終了なら`ILogger`へ機密情報を含まないメッセージを記録して、完了を表示する。
5. 取消なら`Cancel`がトークンへ取消を要求し、待機が`OperationCanceledException`を送出して取消を表示する。
6. 画面破棄時も`Dispose`が取消と解放を行う。

`CancellationToken`は「取り消したい」という通知です。ネットワーク上やDBで進行した処理まで必ず巻き戻す仕組みではありません。実務では取消可能なAPIへトークンを渡し、トランザクションや後片付けも設計します。

## ILoggerを理解する
`ILogger<AsyncLoggingPage>`はASP.NET Coreの標準DIから受け取ります。画面が`new Logger(...)`と書く必要はありません。ログは通常サーバー側のログプロバイダーへ流れ、ブラウザーに自動表示されるわけではありません。記録内容・ログレベル・保存先は環境ごとに確認します。

## 間違えやすいこと
- `await`したからといって処理がブラウザーで動くわけではありません。Interactive ServerのC#はサーバーで動きます。
- キャンセルボタンを押しただけでDBトランザクションが取り消されるとは限りません。
- ログへパスワード、個人情報、機密データを無条件で記録してはいけません。
- `Dispose`中の取消は、永続的な後処理が完了したことを保証しません。

## 演習と解答
**演習1：** `Task.Delay(800, current.Token)`から`current.Token`を外すと、取消の効果はどう変わりますか。

**解答：** 待機に取消が伝わらず、画面で取消表示を出していても800ミリ秒後に正常終了処理が動き得ます。

**演習2：** 非同期処理の完了メッセージを機密情報とともにログへ出してよいですか。

**解答：** いけません。ログに必要な識別子・結果のみを記録し、保存先とアクセス制御も考慮します。

## 学習順027：非同期・ログ・取消の責任を分ける
`/learn/027`のイベントはブラウザー上のJavaScript処理ではありません。Interactive ServerのサーバーC#が`StartAsync`を実行し、`Task.Delay(800, current.Token)`で待機します。`await`はサーバースレッドを800ミリ秒間同期的に占有しないための構文であり、処理が高速になったという意味ではありません。WinFormsで`async void`イベントと`CancellationToken`を扱う経験は役立ちますが、サーバーでは複数利用者のイベントが同時に処理される点も考えます。

| 観点 | WinForms＋Dapper | Blazor Interactive Server | この実装 |
| --- | --- | --- | --- |
| クリック処理 | ローカルPCのC#イベント | サーバーのRazorイベント | `StartAsync` |
| 待機 | `await`でUIを塞がない | `await`で同期的スレッド占有を避ける | 800ミリ秒の待機 |
| DB取消 | SQLのトークンとトランザクションを設計 | 取消だけでDBは戻らない | DBアクセスなし |
| ログ | ローカル出力先を選ぶ | 標準DIから`ILogger<T>`注入 | 完了時の学習ログ |

連続して開始ボタンが押された場合、古い操作の取消処理が新しい操作の開始後に完了する可能性があります。`cancellation`が最新の操作を指すかどうかを`ReferenceEquals(cancellation, current)`で調べてから結果を表示する必要があります。`finally`だけを確認しても十分ではなく、正常完了時や`OperationCanceledException`の処理も同じ照合が必要です。`Dispose`では取消元を解放しますが、外部DBのトランザクションをロールバックする機能ではありません。`ILogger`のメッセージはサーバーログ用で、ブラウザーのConsoleへ自動転送されるものではありません。

| 症状 | 切り分けの場所 | 判断 |
| --- | --- | --- |
| 取消しても完了が出る | `Task.Delay`へトークンを渡したか | 通知が待機へ届かない可能性 |
| 連続開始で前の結果が出る | 完了・取消時の`ReferenceEquals` | 古い操作が画面を書き換えた |
| 取消ボタンを押せない | `running`の真偽 | 処理終了後は押せない |
| ブラウザーConsoleにログがない | `ILogger`の設定 | ログの出力先はサーバー側 |
| DB更新が取り消されない | DB処理自体 | この演習はDB更新なし |

### 追加演習と解答
**演習3：** `Task.Delay(800)`だけに変更したら、`Cancel()`で800ミリ秒の待機は中断されますか。

**解答：** 中断されません。待機処理へ`current.Token`を渡していないためです。画面の取消表示だけでは実際に取消に成功した証拠にはなりません。

**演習4：** 機密業務のSQL条件やパスワードを`Logger.LogInformation`へ書いてよいですか。

**解答：** いけません。必要な操作結果や追跡識別子などを限定して記録し、アクセス権限と保存期間を設計します。ILoggerの登録ができたことを監査の永続化が保証されたこととしてはいけません。

## 完了確認
- [ ] 正常終了と取消を再現した。
- [ ] `CancellationTokenSource`と`CancellationToken`の役割を説明できる。
- [ ] `ILogger`の受け取りと、ログの保存先は別問題だと理解した。

次は[学習順028：単体テスト](step18.md)です。
