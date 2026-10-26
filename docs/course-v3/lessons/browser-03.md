# 学習順043：CSV・PDFファイルのダウンロード

## このステップの到達目標
大量データを一度に全件読み込まない。実業務の出力前にはサーバー側で権限、件数、出力対象を再検査する。CSVは表計算ソフトの数式注入にも注意する。

## 前提・動作環境
学習順031〜040のMudBlazor・DI・Interactive Serverを学んでいる想定です。Visual Studio 2026、.NET 10のサンプルWebアプリを使い、Active Directory・PostgreSQLや実際の業務データは不要です。

## 完成画面を操作する
Webアプリの「/learn/browser-03」を開いてください。学習用CSVとPDFの両方を保存し、CSVが日本語の架空データであることと、PDFが1ページの技術見本として開けることを確認します。

## WinForms・Dapper経験者向けの比較
WinFormsのイベント・ファイル操作は通常同じPC上で動きます。Interactive ServerではC#イベントがサーバーで動きます。ブラウザーのDOM、ファイル選択やダウンロードなどのAPIはWeb特有の処理場所・制約を考えて設計します。

## 処理の流れ
C#で固定のデータを生成し、DotNetStreamReferenceでブラウザー側JSへ渡す。JSがBlobと一時URLを作成し、ダウンロード用リンクを押した後に一時URLを解放する。

## 関連するC#またはRazorの実装
```csharp
using var stream = new MemoryStream(bytes);
using var reference = new DotNetStreamReference(stream);
await module.InvokeVoidAsync("downloadStream", fileName, mediaType, reference);
```
教材のコードは主要部分の抜粋です。実際の画面は制約・例外処理や状態表示も含みます。

## 失敗時の確認
ポップアップや保存禁止設定、回線切断が原因で保存できない場合があります。ブラウザー側NetworkとConsoleで確認します。

## 演習
保存リンクのUIを非表示にするだけで、業務データの出力を制限できますか？

## 解答・確認ポイント
できません。UIの表示制御は認可ではないため、サーバー側の出力処理で認可を実施します。

## 完了確認
- [ ] 完成画面を操作し、結果を確認した。
- [ ] ブラウザーとサーバーのどちらで動くか説明できる。
- [ ] 失敗した場合の切り分けと安全上の注意点を説明できる。

[次の学習順044](browser-04.md)へ進みます。
