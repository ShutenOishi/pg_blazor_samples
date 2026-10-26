# 学習順082：Middlewareの順序・HTTPS・IISの経路

## 到達目標
ASP.NET Coreのパイプラインはリクエストが処理を通る順序で動きます。認証の前提が成り立たなければ業務操作へ進めません。証明書やTLS終端、Forwarded Headersを信頼してよいプロキシ範囲、IIS上でのNegotiateの構成は実機で確認します。

## 前提条件と学習環境
第6段階のデータ検索・一覧表示と、第5段階のEF Coreの操作単位のDbContextを理解している想定です。Visual Studio 2026・.NET 10・Blazor Interactive Serverで実行します。このページ自体はADやPostgreSQLの実接続を必要としません。

## 画面で確認すること
アプリの `/learn/security-02` を開き、架空の条件を変えてボタンを押してください。**表示される許可・拒否は学習用の模擬判定です。** Windows本人認証や実際の既存台帳認可を代行せず、DBにデータを書き込みません。

## コードを追いかける
```csharp
app.UseHttpsRedirection();
if (windowsAuthEnabled) app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
// 実際のルートは認可要件を確認して構成する。
```
コードは主要部分の抜粋です。画面とサーバーの処理場所、ミドルウェアや認可サービスの責任を区別してください。

## WinForms＋Dapper経験者向けの比較
デスクトップ画面の入力・イベントと違い、Webではブラウザーからの要求を信用できません。Blazorの対話イベントはサーバーのC#で動きますが、画面に表示される操作ボタンや保存済み状態だけで本人認証・部署別権限は保証できません。

## 間違えやすい点・安全な確認方法
画面の4つのチェックはHTTPリクエストを発生させません。実際のAntiforgeryトークンはASP.NET Coreの検証機構へ任せます。ブラウザーから送信したフラグを信頼する設計は避けます。

## 演習問題
HTTPSが正しくても、Antiforgery検証なしの状態変更POSTを許可してよいですか？

## 解答
許可しません。HTTPSは通信を保護しますが、それだけではCSRFへの防御になりません。

## 完了チェック
- [ ] 模擬画面で正常時と少なくとも1つの拒否条件を確認した。
- [ ] ブラウザー・Webサーバー・業務サービス・既存DBの役割を説明できた。
- [ ] 学習用の模擬判定とAD/IIS/DBの実機試験を区別できた。
