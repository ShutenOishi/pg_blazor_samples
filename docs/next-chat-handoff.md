# 次のチャットへの引き継ぎ：Step26〜30の確認からEF Core 10へ

## 今回の範囲
- Step26〜30のMudDataGrid Items、Dialog、Snackbar、Scopedテーマ切替、総合UIの日本語ソースと日本語教材、コラム07を追加。
- 共有レイアウト・メニュー・ホーム・日本語PDF・Chromeの操作試験も更新。
- これらは架空データのみで、PostgreSQL 18・AD・IIS実接続ではない。

## 次の実装順序
1. 最新の元HEAD必須CI、Windows/Linuxビルド、Chrome操作、PDF生成を確認し、失敗があれば修正後に正規マージ・postmergeを確認。
2. Step31 Domain、32 PostgreSQL 18 DDL、33 Infrastructure＋EF Core 10とNpgsqlを導入。
3. Step34 DbContextFactory・プール、35 AsNoTracking/DTO、36 CRUD、37 DateOnly/TimeOnly/Range、38 トランザクション、39 xmin競合、40 PostgreSQL統合テスト。
4. 5ステップごとの日本語コラム08・09、日本語PDFをソースと一致させる。
5. Step41〜60ではDataGrid ServerData、VirtualizeServerData、キーセット、権限/集計/編集/CSVの選択を実装。

## 基本原則
- Blazor circuitでDbContextを長寿命のScopedとして共有しない。Factoryから操作単位で生成・破棄する。
- 一覧はItemsの上限、ソートの安定性、認可された総件数、CancellationToken、SQL実行計画を検査する。
- 学習者向けPDFにPR/コミット/リポジトリ説明を表示しない。
- 成功済みCIの不要な再実行禁止。実DB未接続の機能を実証済みと呼ばない。
