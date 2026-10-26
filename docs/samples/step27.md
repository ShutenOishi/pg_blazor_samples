# ステップ27：確認ダイアログで実行と中止を分ける

## できるようになること
WinFormsの`MessageBox.Show`に相当する確認画面を、MudBlazorの`IDialogService`で表示します。`MudDialogProvider`との関係、`bool?`で返る結果の意味、確認と保存・認可が別であることを学びます。

## 前提と操作
Step21のDI登録とProvider、Step15の`@inject`が前提です。DB・ADは不要です。`/learn/27`を開きます。

1. 初期表示は「まだ選択していません。」です。
2. 「確認ダイアログを開く」を押すと「架空の処理を実行しますか？」と表示される。
3. 「中止」を選ぶと「中止しました。」になる。
4. 再び開いて「実行する」を選ぶと「実行を選びました（保存はしていません）。」になる。

## 実装の中心部分

```razor
@page "/learn/27"
@inject IDialogService Dialogs

<MudButton OnClick="ConfirmAsync">
    確認ダイアログを開く
</MudButton>
<p role="status">@_result</p>

@code {
    private string _result = "まだ選択していません。";

    private async Task ConfirmAsync()
    {
        bool? confirmed = await Dialogs.ShowMessageBoxAsync(
            "練習用の確認",
            "架空の処理を実行しますか？",
            yesText: "実行する",
            cancelText: "中止");

        _result = confirmed == true
            ? "実行を選びました（保存はしていません）。"
            : "中止しました。";
    }
}
```

`IDialogService`は`Program.cs`の`AddMudServices()`で利用できるようになり、共通レイアウトの`MudDialogProvider`が画面上へダイアログを表示します。この例は`bool?`で結果を受け取り、**明示的な`true`の場合だけ「実行を選んだ」と扱います**。中止や閉じる操作は実行扱いにしません。

## 操作の処理順
1. ブラウザーでボタンを押す。
2. サーバーの`ConfirmAsync`が`ShowMessageBoxAsync`を呼ぶ。
3. MudBlazorがダイアログを描画し、選択結果を非同期で返す。
4. `true`なら実行選択のメッセージ、その他なら中止のメッセージを設定する。
5. Razorの`_result`表示を更新する。

ここでは確認結果を表示するだけで、ファイルやDBを書き換える処理はありません。

## 重要な区別
確認ダイアログで「実行する」が選ばれても、認証済みの本人か、DBに操作権限があるか、二重送信ではないかは分かりません。実際の更新処理では**確認後のサービス呼び出しで改めて入力・本人・業務権限・競合を検査**する必要があります。

## よくある問題
- ダイアログが開かない場合は`IDialogService`の登録と`MudDialogProvider`、描画モードを確認する。
- `confirmed != false`で実行扱いにすると、`null`の中止も実行する可能性があります。`confirmed == true`を使います。
- 確認UIがあるだけでDB更新が安全になるわけではありません。

## 演習と解答
**問：** ダイアログを閉じる操作を「実行する」とみなしてもよいですか。

**答え：** いけません。このコードは`confirmed == true`の場合だけ実行選択とし、それ以外を中止とします。

## 完了確認
- [ ] 実行・中止の両方の表示を確認した。
- [ ] DIサービスとProviderの役割を説明できる。
- [ ] 確認と業務の認可・保存が別だと説明できる。

次は[ステップ28：通知と処理中表示](step28.md)です。
