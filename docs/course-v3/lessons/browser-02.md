# 学習順042：ファイルアップロードと入力制約

## このステップの到達目標
IBrowserFileのName・Size・ContentTypeを完全に信用しない。acceptは選択補助。サイズ制限はOpenReadStreamで実施し、実環境では内容検査・ウイルス検査・保存先分離が必要。

## 前提・動作環境
学習順031〜040のMudBlazor・DI・Interactive Serverを学んでいる想定です。Visual Studio 2026、.NET 10のサンプルWebアプリを使い、Active Directory・PostgreSQLや実際の業務データは不要です。

## 完成画面を操作する
Webアプリの「/learn/browser-02」を開いてください。.txtまたは.csvの小さなファイルを選び、受信したバイト数とSHA-256を確認します。拡張子を変えたファイルや64 KiB超のファイルも選んで拒否を確認します。

## WinForms・Dapper経験者向けの比較
WinFormsのイベント・ファイル操作は通常同じPC上で動きます。Interactive ServerではC#イベントがサーバーで動きます。ブラウザーのDOM、ファイル選択やダウンロードなどのAPIはWeb特有の処理場所・制約を考えて設計します。

## 処理の流れ
InputFileが選択を受け取り、C#で拡張子とサイズを確認してからOpenReadStreamの最大64 KiBを指定し、内容をメモリで受け取る。メモリ内のSHA-256のみ表示し、保存しない。

## 関連するC#またはRazorの実装
```csharp
await using var stream = file.OpenReadStream(maxAllowedSize: 64 * 1024);
using var output = new MemoryStream();
await stream.CopyToAsync(output);
var hash = SHA256.HashData(output.ToArray());
```
教材のコードは主要部分の抜粋です。実際の画面は制約・例外処理や状態表示も含みます。

## 失敗時の確認
拡張子不一致とファイルサイズ超過で拒否されることを確認。読取失敗時にはサーバーログと通信を確認してください。

## 演習
ブラウザーのaccept属性を.txtにすれば、.exe形式のファイルは確実に拒否できますか？

## 解答・確認ポイント
できません。拡張子、実ファイル内容やサイズをサーバーで検査する必要があります。

## 完了確認
- [ ] 完成画面を操作し、結果を確認した。
- [ ] ブラウザーとサーバーのどちらで動くか説明できる。
- [ ] 失敗した場合の切り分けと安全上の注意点を説明できる。

[次の学習順043](browser-03.md)へ進みます。
