# ステップ17：非同期処理とログを使う

## このステップでできるようになること
WinFormsの`async/await`による非同期イベント処理を、Blazor Interactive Serverの画面でも扱います。処理中・完了・取消の表示と、ASP.NET CoreがDIで提供する`ILogger<T>`を組み合わせます。

この教材では**外部通信もDB書き込みも実行しません**。サーバー上で800ミリ秒待つ処理を使い、ブラウザーからの操作とサーバー側処理の関係を確認します。

## 前提と期待する動作
Step11〜16を学んでから`/learn/17`を開きます。DB・ADは不要です。

1. 初期表示「まだ実行していません。」を見る。
2. 「非同期処理を開始する」を押し、「処理中です。」から「処理が完了しました。」へ変わるのを確認する。
3. 再度開始し、完了前に「処理を取り消す」を押すと「処理を取り消しました。」になる。
4. 処理していない間、取消ボタンは無効です。取消と完了はタイミングで競合し得るため、すでに完了した後の取消が成功したとは解釈しません。

## Razorと処理の構成
サーバー側のコンポーネントは`IDisposable`を実装し、ログサービスを注入します。

```razor
@page "/learn/17"
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

## 完了確認
- [ ] 正常終了と取消を再現した。
- [ ] `CancellationTokenSource`と`CancellationToken`の役割を説明できる。
- [ ] `ILogger`の受け取りと、ログの保存先は別問題だと理解した。

次は[ステップ18：単体テスト](step18.md)です。
