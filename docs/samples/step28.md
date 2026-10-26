# ステップ28：Snackbar・処理中・完了を使い分ける

## できるようになること
WinFormsでボタンを無効化して処理中を示すように、Blazorでも非同期処理の状態を分かりやすく表示します。`ISnackbar`の通知、`MudProgressLinear`の進捗表示、`aria-live`付きの本文メッセージを組み合わせます。

## 前提と操作
Step17の`async/await`とStep21の`ISnackbar`が前提です。`/learn/28`を開きます。DB・ADは不要です。

1. 初期表示「開始前です。」を見る。
2. 「練習用の処理を開始」を押す。
3. 短い間「練習用の処理を実行中です。」と進捗バーが表示され、ボタンが無効になる。
4. 400ミリ秒の待機後、「練習用の処理が完了しました。」の本文とSnackbarが表示される。

## 主要なRazorコード

```razor
@inject ISnackbar Snackbar

<MudButton Disabled="@_running" OnClick="StartAsync">
    練習用の処理を開始
</MudButton>
@if (_running)
{
    <MudProgressLinear Indeterminate="true"
                       Color="Color.Info" aria-label="処理を実行中" />
}
<p role="status" aria-live="polite">@_message</p>

@code {
    private bool _running;
    private string _message = "開始前です。";

    private async Task StartAsync()
    {
        if (_running) return;
        _running = true;
        _message = "練習用の処理を実行中です。";
        try
        {
            await Task.Delay(400);
            _message = "練習用の処理が完了しました。";
            Snackbar.Add("練習用の処理が完了しました。", Severity.Success);
        }
        finally
        {
            _running = false;
        }
    }
}
```

実装の`NotificationPage.razor`と同じ処理を示す短縮例です。`MudProgressLinear`の`Indeterminate=true`は完了率が分からない処理中表示で、百分率を表していません。

## クリックから表示まで
1. クリックでサーバーの`StartAsync`を実行する。
2. `_running`を`true`にし、本文を処理中にする。ボタンは無効になる。
3. `Task.Delay(400)`で待機する。実業務処理やジョブ投入ではない。
4. 完了後、本文の結果を更新してSnackbarを追加する。
5. `finally`で`_running`を`false`へ戻す。

`if (_running) return;`もあるため、UIの無効化に加えて同じコンポーネント上の処理を二重に開始しにくい構造です。ただし、実DB更新の二重送信・冪等性を保証する処理ではありません。

## なぜ本文にも残すのか
Snackbarは消えることがあるため、重要な結果を本文の`role="status"`と`aria-live="polite"`でも示します。色だけの変化に依存せず、読み上げや見直しにも対応します。

## 誤解しやすいこと
- 待機を`Task.Delay`で再現しただけでは、長い業務処理をバックグラウンドジョブに分離したことにはなりません。
- 処理完了の通知があることとDBのトランザクション成功は別です。
- 取消を含む処理はStep17の`CancellationToken`も考えますが、この画面には取消機能を実装していません。

## 演習と解答
**問：** 画面にSnackbarが表示されれば、本文の結果表示を省略してよいですか。

**答え：** 省略しない方がよいです。通知は短時間で消えるため、本文にも操作結果を残します。

## 完了確認
- [ ] 処理中の無効化と進捗表示を確認した。
- [ ] 本文とSnackbarの違いを説明できる。
- [ ] 模擬待機と実際の業務処理を区別できる。

次は[ステップ29：テーマと操作性](step29.md)です。
