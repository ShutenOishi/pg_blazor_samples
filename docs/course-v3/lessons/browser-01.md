# 学習順041：JavaScript interopの利用判断

## このステップの到達目標
C#で実装できるイベントにはJSを使わない。ブラウザー固有APIだけモジュールに分離する。JSは事前レンダリング中に呼べず、回線切断・JS例外に備える。

## 前提・動作環境
学習順031〜040のMudBlazor・DI・Interactive Serverを学んでいる想定です。Visual Studio 2026、.NET 10のサンプルWebアプリを使い、Active Directory・PostgreSQLや実際の業務データは不要です。

## 完成画面を操作する
Webアプリの「/learn/browser-01」を開いてください。画面上の数字をC#のイベントで増やした後、JSモジュールでHTMLのlang属性を読んでください。

## WinForms・Dapper経験者向けの比較
WinFormsのイベント・ファイル操作は通常同じPC上で動きます。Interactive ServerではC#イベントがサーバーで動きます。ブラウザーのDOM、ファイル選択やダウンロードなどのAPIはWeb特有の処理場所・制約を考えて設計します。

## 処理の流れ
C#のイベントがサーバーで処理され、JS interopを通してブラウザーのDOMを読む。取得結果はサーバーへ返され、Razorが表示を更新する。

## 関連するC#またはRazorの実装
```csharp
module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/browser-learning.js");
var language = await module.InvokeAsync<string>("readLanguage");
```
教材のコードは主要部分の抜粋です。実際の画面は制約・例外処理や状態表示も含みます。

## 失敗時の確認
JSモジュールを開けない場合は接続状態とブラウザーConsole・Networkを確認します。

## 演習
取得ボタンを押す前の初期HTMLにブラウザーの言語値が直接入らない理由を説明する。

## 解答・確認ポイント
DOMを読むJSはブラウザー側で実行されます。この教材では対話イベント中にサーバーC#からJSを呼び出すためです。

## 完了確認
- [ ] 完成画面を操作し、結果を確認した。
- [ ] ブラウザーとサーバーのどちらで動くか説明できる。
- [ ] 失敗した場合の切り分けと安全上の注意点を説明できる。

[次の学習順042](browser-02.md)へ進みます。
