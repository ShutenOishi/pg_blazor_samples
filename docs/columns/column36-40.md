# 学習コラム09：EF Coreの型・変更追跡・トランザクションを結ぶ

## 学習順054〜40のつながり
学習順054で登録・更新・削除、学習順055でDateOnly・TimeOnly・daterange、学習順056でトランザクション、学習順057でxmin競合、学習順060でPostgreSQL 18の実接続と型の検証を学びます。

## データアクセスの順番
画面の入力 → サービスの入力チェック → FactoryからDbContextを生成 → EF Coreで更新 → SaveChangesAsync → Context破棄 → 表示DTOを再取得。

## DateOnlyとRangeの意味
MudDatePickerはDateTime?を扱います。PostgreSQL dateではDateOnly、daterangeではNpgsqlRange<DateTime>を使います。開始日を含み、終了日の翌日を含まない半開期間への変換を確認してください。空範囲や無限境界を勝手に補完しません。

## 競合とトランザクション
2人が同じ行を編集するとき、DBのxminを楽観的排他トークンに使います。SaveChangesAsyncがDbUpdateConcurrencyExceptionを返せば上書きせず再読込を促します。複数行の原子的処理は短いトランザクションで完結します。

## 確認問題
**Q. 操作中ずっとDbContextやトランザクションを維持する？** A. いいえ。画面入力中は維持せず、保存するときに短く作ります。

**Q. InMemoryの単体テストでdaterange SQLの正しさを証明できる？** A. できません。実PostgreSQLの統合試験が必要です。

**Q. これからMudDataGridを選ぶ判断は？** A. 次の「用途から選ぶMudBlazor特別コラム」で、一覧・入力・階層・通知・描画方式を比較します。

次：MudBlazor用途別特別コラム → 学習順061。

## 実DBで発見したRangeの注意

NpgsqlRangeの2引数コンストラクターは両端を含みます。daterangeの上限を含まないようにする場合は4引数で下限true・上限falseを明示します。DBの正規化による終了日のずれを統合テストで検出します。
