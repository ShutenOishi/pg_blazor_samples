# 学習順38：Snackbar・処理中・完了を使い分ける

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L038NotificationPage.razor)。画面の正規URLは `/learn/038` です。

**実習URL：`/learn/038`。** 第4段階・第8組の3本目です。ここで扱う架空データ・模擬処理は業務DBに保存されません。

## できるようになること
WinFormsでボタンを無効化して処理中を示すように、Blazorでも非同期処理の状態を分かりやすく表示します。`ISnackbar`の通知、`MudProgressLinear`の進捗表示、`aria-live`付きの本文メッセージを組み合わせます。

## 前提と操作
学習順027の`async/await`と学習順031の`ISnackbar`が前提です。`/learn/038`を開きます。DB・ADは不要です。

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
- 取消を含む処理は学習順027の`CancellationToken`も考えますが、この画面には取消機能を実装していません。


## 通知の役割を分ける

| 観点 | WinForms＋Dapperの既知の操作 | このBlazor画面 | 実業務の注意 |
| --- | --- | --- | --- |
| ボタンの二度押し | Enabled=false | Disabledと_running | 別回線からの重複を別途防ぐ |
| 進捗 | ProgressBar | MudProgressLinear | 不明な割合を100%にしない |
| 完了通知 | MessageBoxなど | Snackbar | 消える通知だけに依存しない |
| 結果の記録 | 画面ラベルやログ | role=statusの本文 | DB監査の代用ではない |
| 非同期 | Task.Runやawait | await Task.Delay(400) | 模擬待機と処理実行は別 |

Blazor Interactive Serverのイベントはサーバー側で動き、`Task.Delay(400)`は400ミリ秒の待機を再現するだけです。DapperのExecuteが成功したわけではなく、キューに仕事を登録したわけでもありません。状態を`_running=true`にしたあと、非同期メソッドが未完了の時点でレンダリングされればボタンのDisabledと不確定進捗バーが利用者に見えます。速い処理だと進捗バーは一瞬だけのこともあります。だからこそ完了後の本文メッセージを残す価値があります。

**実際の表示側の最小構成**を切り分けて確認します。

```razor
<MudButton Disabled="@_running" OnClick="StartAsync">
    練習用の処理を開始
</MudButton>
@if (_running)
{
    <MudProgressLinear Indeterminate="true" Color="Color.Info"
                       aria-label="処理を実行中" />
}
<p role="status" aria-live="polite">@_message</p>
```

`aria-live="polite"`は画面の状態更新を読み上げに伝えるための指定です。一方Snackbarは一時的な視覚通知なので、読み上げ・長時間の確認をSnackbarだけに頼りません。状態表示が「完了しました」でも、実際の業務結果を保証するためにはサーバーの戻り値、更新件数、トランザクション成否や必要な監査結果を別に検証します。

## 故障の切り分け

| 症状 | 最初に見る値・箇所 | 判断 |
| --- | --- | --- |
| ボタンが戻らない | finallyの_running=false | 例外でも解除できるか |
| 完了前にもう一度動く | Disabledと先頭のif | UIと処理の両方で抑止する |
| Snackbarが一瞬で消える | 本文のrole=status | 完了結果を後から確認する |
| 処理中バーが消えている | 400ms待機の時刻 | 完了済みなら正常 |
| DB更新履歴がない | Task.Delayを実行しているか | DBは操作していない |
| 切断後に続いたように見える | circuitと処理寿命 | 長期業務は別途ジョブ設計 |

## 再現手順と詳しい演習

1. `/learn/038`を開き、開始前の文章を読む。
2. ボタンを一度押し、押せない状態と進捗バーを観察する。
3. 完了時の本文とSnackbarが同じ結果を示すことを確認する。
4. もう一度開始し、過去の成功通知をDB保存の証拠と解釈しない。
5. 表示が消えた場合は本文メッセージを確かめ、状態の永続化は行っていない点を説明する。

**演習2：** finallyを省略すると、どのような表示不整合が起こりますか。

**詳しい解答：** 非同期処理の途中で例外が発生すると`_running`がtrueのまま残り、ボタンが無効のままになることがあります。finallyは成功・失敗のどちらでも実行されるため、状態の解除場所として適しています。実務では例外ログと利用者向けの失敗メッセージも必要ですが、この教材の実装に存在しない機能を実装済みと取り違えません。

**演習3：** `if (_running) return;`を設けても同時DB更新の競合が防げない理由は何ですか。

**詳しい解答：** `_running`はコンポーネント個別のメモリ状態です。別タブ・別circuit・別端末から送られる要求の衝突は判断できません。実DBでは一意制約、冪等キー、適切な排他、`xmin`による楽観的競合検出などを業務条件に合わせて組み合わせます。UIのボタン制御は誤操作防止であり、整合性保証ではありません。

## 演習と解答
**問：** 画面にSnackbarが表示されれば、本文の結果表示を省略してよいですか。

**答え：** 省略しない方がよいです。通知は短時間で消えるため、本文にも操作結果を残します。

## 完了確認
- [ ] 処理中の無効化と進捗表示を確認した。
- [ ] 本文とSnackbarの違いを説明できる。
- [ ] 模擬待機と実際の業務処理を区別できる。

次は[学習順039：テーマと操作性](L039AccessibilityPage.md)です。
