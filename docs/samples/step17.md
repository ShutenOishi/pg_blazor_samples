# ステップ17：非同期処理とログを使う

## できるようになること
`async`/`await`、`CancellationToken`、標準DIが提供する`ILogger<T>`の基本を操作して理解します。

## 実際に操作する
1. `/learn/17` を開く。
2. 「非同期処理を開始する」を押す。処理中から完了まで変化する。
3. もう一度開始し、処理中に「処理を取り消す」を押す。
4. 「処理を取り消しました。」と表示されることを確認する。

## 処理の構造
```csharp
@inject ILogger<AsyncLoggingPage> Logger
```

```csharp
await Task.Delay(800, current.Token);
Logger.LogInformation("ステップ17の練習用非同期処理が完了しました。");
```

`ILogger<T>`はASP.NET CoreがすでにDIで提供しています。自作`Logger`を直接生成しません。長時間処理ではキャンセル時の後始末も必要です。

## 注意
ここでは架空の待機だけを行い、外部通信・実データ保存をしません。`CancellationTokenSource`は画面破棄時に取り消します。例外の詳細やパスワードをログへ書かないでください。

## チェック
- [ ] 正常完了・中断の両方を確認した
- [ ] ILoggerがDIから供給される意味を説明できる

次：[ステップ18](step18.md)
