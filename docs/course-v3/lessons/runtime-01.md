# 学習順009：Static SSRとInteractive Serverの違い

## 学習の目的
Static SSRとInteractive Serverの違いを、WinFormsのイベント処理と比較しながら理解します。ここではWebサーバー上のC#とブラウザーの表示を区別します。

## 前提と動作環境
学習順001〜008で学んだHTTP、HTML、Razorの基本を使います。.NET 10のInteractive Serverを前提とし、DB/ADは使いません。

## 画面で確かめること
`/learn/runtime-01`を開いてください。NetworkでDocumentの要求を観察した後にボタンを2回押し、サーバー側の回数が2になることを確認する。

## 動作と処理場所
Static SSRはリクエスト時にサーバーがHTMLを生成します。Interactive ServerのC#イベントは接続確立後にサーバー側で実行します。この教材の画面はInteractive Serverであり、Static SSRの別アプリを起動する比較実験ではありません。

## 主要なC#コード
```csharp
private int count;
// サーバー上の状態を更新し、接続中のブラウザーへ結果を反映する。
private void Increment() => count++;
```
Razorの `@onclick` が上のC#メソッドを呼び出します。完成画面を操作しながらRazorの表示とC#メソッドを確認してください。

## WinForms・Dapper経験者が注意する点
WinFormsではフォーム上の変数がPCのプロセスで維持されます。Interactive Serverでは画面状態がサーバーの対話回線と結び付き、ブラウザーやDBに自動保存されるわけではありません。

## よくある失敗と確認方法
画面が動かない場合はブラウザーのNetworkとConsole、サーバーのログ、再接続の案内を確認してください。表示されているHTMLだけでサーバー側処理の成功を判断しないでください。

## 演習と解答
**演習：** NetworkでDocumentの要求を観察した後にボタンを2回押し、サーバー側の回数が2になることを確認する。

**解答：** 要求時にHTMLを返すことと、対話接続を使ってC#イベントを処理することは別の仕組みです。

## 完了確認
- [ ] 画面の表示とボタン操作を確認した。
- [ ] どの状態がサーバー・ブラウザー・DBのどこに存在するか説明できる。
- [ ] 再読込や接続切断に関する注意点を説明できる。

次は[学習順010](../../samples/step04.md)です。
