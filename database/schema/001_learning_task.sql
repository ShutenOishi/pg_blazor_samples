-- Step32：開発専用DBにのみ適用する。実運用の権限・監査テーブルとは別。
-- 実行前に必ず開発用DBへ接続していることを確認する。
CREATE TABLE IF NOT EXISTS learning_task
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title varchar(100) NOT NULL,
    due_date date NULL,
    start_time time without time zone NULL,
    available_dates daterange NULL,
    is_done boolean NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_learning_task_due_date ON learning_task (due_date);
CREATE INDEX IF NOT EXISTS ix_learning_task_available_dates ON learning_task USING gist (available_dates);
-- xmin はPostgreSQLのシステム列。DDLで明示的に作成しない。
-- 期間の例：INSERT INTO learning_task(title, available_dates)
-- VALUES ('期間の練習', daterange('2026-10-01', '2026-11-01', '[)'));
