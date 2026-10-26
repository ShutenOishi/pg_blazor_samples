# 学習順53：Dapperの接続／ExecuteとDbContext・追跡・SaveChanges

## 到達目標
追跡状態を押してDetached、Added、Detachedを確認します。SaveChangesを呼ばず、保存しません。

## 前提・操作
Visual Studio 2026、.NET 10、EF Core 10、Npgsqlを使用します。Webアプリの `/learn/db-bridge-03` を開いて操作してください。モデル定義とSQL生成は実DBへの接続を伴いません。登録や検索の実試験は専用PostgreSQL 18で別に行います。

## WinForms・Dapperとのつながり
Dapperで書いたSQLや接続操作を、EF CoreのLINQ・モデル・DbContext・SaveChangesに対応付けます。Interactive ServerではC#処理はサーバー上で動きます。

## 実装を読む
```csharp
using var db = factory.CreateDbContext();
var entity = new LearningTask { Title = "架空課題" };
var before = db.Entry(entity).State;
db.Tasks.Add(entity); // Added
db.ChangeTracker.Clear(); // 保存せずDetached
```
これは主要部分の抜粋です。対応するRazor画面は入力、状態表示、例外の扱いも含みます。

## 注意とトラブル時の確認
- SQL本文へ利用者の入力を連結せず、パラメーターとして渡します。
- Keylessな台帳のメタデータを確認しても、実際の台帳参照権限を得たわけではありません。
- FDW越しの更新・監査が原子的か不明なら、更新を拒否します。
- 実際のデータを読むには専用DBの設定と本人・業務別認可が必要です。

## 練習問題
Added状態になればDBへ保存できていますか？

## 解答
いいえ。SaveChangesを正常に実行するまで永続化されません。

## 完了条件
- [ ] 操作画面で結果を確認した。
- [ ] ブラウザー/サーバー/DBの処理を区別できた。
- [ ] 安全なSQLのパラメーターと操作単位の短命DbContextを説明できた。
