# 学習順58：既存DB・ビュー・キーなしモデル・FDWを扱う

## 到達目標
キーなしモデルの定義ボタンを押します。台帳の実データは一切検索しません。

## 前提・操作
Visual Studio 2026、.NET 10、EF Core 10、Npgsqlを使用します。Webアプリの `/learn/db-bridge-04` を開いて操作してください。モデル定義とSQL生成は実DBへの接続を伴いません。登録や検索の実試験は専用PostgreSQL 18で別に行います。

## WinForms・Dapperとのつながり
Dapperで書いたSQLや接続操作を、EF CoreのLINQ・モデル・DbContext・SaveChangesに対応付けます。Interactive ServerではC#処理はサーバー上で動きます。

## 実装を読む
```csharp
var access = modelBuilder.Entity<ExistingUserAccessRow>();
access.HasNoKey();
access.ToView("learning_user_access_v");
// 実際の台帳SELECTは匿名画面では行わない。
```
これは主要部分の抜粋です。対応するRazor画面は入力、状態表示、例外の扱いも含みます。

## 注意とトラブル時の確認
- SQL本文へ利用者の入力を連結せず、パラメーターとして渡します。
- Keylessな台帳のメタデータを確認しても、実際の台帳参照権限を得たわけではありません。
- FDW越しの更新・監査が原子的か不明なら、更新を拒否します。
- 実際のデータを読むには専用DBの設定と本人・業務別認可が必要です。

## 練習問題
FDW越しの更新と監査を原子的に実行できると保証できなければ？

## 解答
フェイルクローズして更新を拒否します。

## 完了条件
- [ ] 操作画面で結果を確認した。
- [ ] ブラウザー/サーバー/DBの処理を区別できた。
- [ ] 安全なSQLのパラメーターと操作単位の短命DbContextを説明できた。
