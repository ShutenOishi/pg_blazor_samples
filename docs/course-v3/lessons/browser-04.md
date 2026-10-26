# 学習順044：ErrorBoundary・取消・再試行

## このステップの到達目標
UIの例外表示とDBトランザクションは別物。CancellationTokenは処理中止の通知であり、完了した書き込みを巻き戻す仕組みではない。例外の詳細を利用者に出さない。

## 前提・動作環境
学習順031〜040のMudBlazor・DI・Interactive Serverを学んでいる想定です。Visual Studio 2026、.NET 10のサンプルWebアプリを使い、Active Directory・PostgreSQLや実際の業務データは不要です。

## 完成画面を操作する
Webアプリの「/learn/browser-04」を開いてください。学習用例外の発生ボタンを押し、例外の詳細が画面に出ずErrorBoundaryの案内に切り替わることを確認します。その後復旧して操作を再実行し、別の非同期処理を開始・取り消します。

## WinForms・Dapper経験者向けの比較
WinFormsのイベント・ファイル操作は通常同じPC上で動きます。Interactive ServerではC#イベントがサーバーで動きます。ブラウザーのDOM、ファイル選択やダウンロードなどのAPIはWeb特有の処理場所・制約を考えて設計します。

## 処理の流れ
ErrorBoundaryが子コンポーネントの例外を受け止める。非同期処理はCancellationTokenSourceのトークンをTask.Delayへ渡す。取消するとOperationCanceledExceptionを区別して表示する。

## 関連するC#またはRazorの実装
```csharp
try { await Task.Delay(2500, current.Token); }
catch (OperationCanceledException) { state = "処理を取り消しました。"; }
finally { current.Dispose(); }
```
教材のコードは主要部分の抜粋です。実際の画面は制約・例外処理や状態表示も含みます。

## 失敗時の確認
学習用の例外は意図して発生させます。異常系のログが記録されても、例外詳細をWeb画面へ表示しません。

## 演習
ErrorBoundaryにエラーが表示されたら、既に確定したDB操作も自動で元に戻りますか？

## 解答・確認ポイント
戻りません。トランザクション・例外処理・再試行ポリシーをサービス側で設計します。

## 完了確認
- [ ] 完成画面を操作し、結果を確認した。
- [ ] ブラウザーとサーバーのどちらで動くか説明できる。
- [ ] 失敗した場合の切り分けと安全上の注意点を説明できる。

[次の学習順045](browser-05.md)へ進みます。
