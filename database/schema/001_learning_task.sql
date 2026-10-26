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

-- Step46：関連テーブルの例。既存の開発用テーブルへ繰り返し適用できる。
CREATE TABLE IF NOT EXISTS learning_category
(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(60) NOT NULL UNIQUE
);
ALTER TABLE learning_task ADD COLUMN IF NOT EXISTS category_id integer REFERENCES learning_category(id);
CREATE INDEX IF NOT EXISTS ix_learning_task_category_id ON learning_task (category_id);

-- Step67：架空の課題を部署別に分離する。開発用DB専用で実業務には適用しない。
ALTER TABLE learning_task ADD COLUMN IF NOT EXISTS unit_code varchar(32) NOT NULL DEFAULT 'DEMO';
CREATE INDEX IF NOT EXISTS ix_learning_task_unit_id ON learning_task (unit_code, id);
