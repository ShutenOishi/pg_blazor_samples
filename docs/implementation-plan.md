# 実装計画：80ステップ、Web内のEF Core 10、用途別MudBlazorコラム

## 実装状態

- Step01〜30：Razor・DI・MudBlazorの日本語画面と教材を作成・CI確認済み。
- Step31〜40：**Webプロジェクト内**のDataとServicesを利用するEF Core 10の画面・SQL・教材を追加。DB未設定時には説明を表示。実PostgreSQL 18への接続検証は環境が必要で、SQLの実行履歴のない部分は未実証とする。
- Step41〜60：MudTable/MudDataGridのデータ連携方式を要件から選ぶ20ステップ。計画中。
- Step61〜70：AD Windows統合認証と業務権限。計画中。
- Step71〜80：IIS運用・監視・性能等。計画中。

## EF Core 10の最小構成

1. Web/Data/LearningTask.cs：Entityと表示用DTO。
2. Web/Data/LearningDbContext.cs：EF CoreモデルとPostgreSQL型。
3. Web/Services/Learning/LearningDbService.cs：検索・CRUD・トランザクション・競合の処理。
4. Web/Program.cs：AddDbContextFactory、UseNpgsql、DI登録。
5. database/schema、seed、tests：開発用PostgreSQL 18のDDLと検証SQL。

別のDomain/Infrastructureプロジェクトを設けない。既存のApplicationプロジェクトはStep19の学習のために保持するだけで、EF Coreの導入条件にはしない。

## 学習順序

- Step31〜35：モデル、DBスキーマ、DI、DbContextFactory、LINQ検索 → コラム08
- Step36〜40：CRUD、DateOnly/TimeOnly/Range、トランザクション、xmin競合、DB確認 → コラム09
- **Step40 → [用途別MudBlazor特別コラム](columns/column-mudblazor-by-purpose.md) → Step41**
- Step41〜60：Items・ServerData・VirtualizeServerData・キーセット・編集・CSV・集計の選択

## 受入基準

- [ ] .NET 10のWindows/Linux Releaseビルド・従前の画面の回帰テスト
- [ ] Step31〜40の日本語画面/コラム08・09/特別コラムのルーティングと日本語PDF目次
- [ ] PostgreSQLの型とCRUD、Range、xminの実DB検証（専用環境のみ）
- [ ] PostgreSQLなしでもStep01〜30の画面は起動できる
- [ ] DBの資格情報はリポジトリに保存しない
- [ ] CI未実行・実DB未検証は成功済みと表示しない

## 参照

[80ステップ一覧](sample-catalog.md)／[DataGrid選択ガイド](data-grid-patterns.md)／[EF Core 10](efcore10-data-access.md)／[MudBlazor用途別](columns/column-mudblazor-by-purpose.md)
