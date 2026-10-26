# 学習順46：DapperとEF Coreの役割・選定を比較する

## 到達目標
一般的なCRUD、既存SQL、件数が多い一覧から要件を選び、方式の違いを確認します。

## 前提・操作
Visual Studio 2026、.NET 10、EF Core 10、Npgsqlを使用します。Webアプリの `/learn/db-bridge-01` を開いて操作してください。モデル定義とSQL生成は実DBへの接続を伴いません。登録や検索の実試験は専用PostgreSQL 18で別に行います。

## WinForms・Dapperとのつながり
Dapperで書いたSQLや接続操作を、EF CoreのLINQ・モデル・DbContext・SaveChangesに対応付けます。Interactive ServerではC#処理はサーバー上で動きます。

## 実装を読む
```csharp
// DapperではSQLをパラメーターとともにQuery/Executeへ渡す
// EF Core 10の実装例
await using var db = await factory.CreateDbContextAsync();
var query = db.Tasks.AsNoTracking().Where(x => x.Title.Contains(term));
```
これは主要部分の抜粋です。対応するRazor画面は入力、状態表示、例外の扱いも含みます。

## 注意とトラブル時の確認
- SQL本文へ利用者の入力を連結せず、パラメーターとして渡します。
- Keylessな台帳のメタデータを確認しても、実際の台帳参照権限を得たわけではありません。
- FDW越しの更新・監査が原子的か不明なら、更新を拒否します。
- 実際のデータを読むには専用DBの設定と本人・業務別認可が必要です。

## 練習問題
どちらを選べば認可が自動で成立しますか？

## 解答
どちらも自動では成立しません。サーバーの業務サービスで本人・部署・操作・行範囲を検査します。

## 完了条件
- [ ] 操作画面で結果を確認した。
- [ ] ブラウザー/サーバー/DBの処理を区別できた。
- [ ] 安全なSQLのパラメーターと操作単位の短命DbContextを説明できた。
