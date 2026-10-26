# 第5段階の概要：DapperからEF Core 10・PostgreSQL 18へ

> 教材v3の**完成後の構成案**。この段階の新規項目が未実装の場合、現行教材へ実装済みとして掲載しません。

## この段階でできるようになること
既存SQLとLINQ、Npgsqlの接続プール、DbContextFactory、CRUD、型、更新競合を対応付けられる。

## この段階の前提
第4段階の入力/UI。学習用PostgreSQL 18が必要な操作を明示する。

## WinForms・Dapperとのつながり
DapperのSQL/DTO/Executeに対してEF Coreのモデル・追跡・SaveChanges・生成SQLを実例で比較する。

## 学習の順番と到達点
- 学習順046：DapperとEF Coreの役割・選定を比較する（新規実装予定：db-bridge-01）
- 学習順047：モデルとDTOを理解する（旧Step31に対応）
- 学習順048：PostgreSQLで学習テーブルを定義する（旧Step32に対応）
- 学習順049：EF Core 10をWebへ登録する（旧Step33に対応）
- 学習順050：DbContextFactoryと接続プール（旧Step34に対応）
- 学習順051：LINQ検索とAsNoTracking（旧Step35に対応）
- 学習順052：SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL（新規実装予定：db-bridge-02）
- 学習順053：Dapperの接続／ExecuteとDbContext・追跡・SaveChanges（新規実装予定：db-bridge-03）
- 学習順054：EF Coreの登録・更新・削除（旧Step36に対応）
- 学習順055：DateOnly・TimeOnly・Rangeの型変換（旧Step37に対応）
- 学習順056：トランザクションを理解する（旧Step38に対応）
- 学習順057：xmin楽観的排他を確認する（旧Step39に対応）
- 学習順058：既存DB・ビュー・キーなしモデル・FDWを扱う（新規実装予定：db-bridge-04）
- 学習順059：FromSql／SqlQueryのパラメーターとDapper併用（新規実装予定：db-bridge-05）
- 学習順060：PostgreSQL 18の実接続確認（旧Step40に対応）

## 5ステップごとに確認すること
- 学習順050の後：第10組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順055の後：第11組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順060の後：第12組の復習。直前の5テーマの関係を説明し、動作を再確認する。

## 練習課題と解答の確認点
同じ検索をSQLとLINQで表現し、パラメーター、トランザクション、xminによる競合を確認する。

**確認の基準：** 画面操作の結果だけでなく、処理がブラウザー・サーバー・DBのどこで動くか、例外や拒否をどう確認するかを説明できること。未実装の教材では学習結果の達成を宣言しません。

## 次の段階への準備
一覧の取得方式を件数と操作要件で選ぶ。

全体の順番は[新学習順表](../learning-sequence.md)を参照してください。
