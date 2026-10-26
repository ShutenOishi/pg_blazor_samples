-- Step32：全件架空データ。再実行すると行が増えるため、必要時だけ適用する。
INSERT INTO learning_task(title, due_date, start_time, is_done) VALUES
 ('EF Coreの接続練習', DATE '2026-10-30', TIME '09:00', false),
 ('DateOnlyの練習', DATE '2026-11-01', TIME '10:15', false),
 ('PostgreSQLの期間検索', DATE '2026-11-03', TIME '13:00', true);

-- Step46：関連テーブルの架空データ。未分類の課題も許容する。
INSERT INTO learning_category (name) VALUES
('接続'), ('日付と期間')
ON CONFLICT (name) DO NOTHING;
UPDATE learning_task SET category_id = (SELECT id FROM learning_category WHERE name = '接続')
WHERE title = 'EF Coreの接続練習';
UPDATE learning_task SET category_id = (SELECT id FROM learning_category WHERE name = '日付と期間')
WHERE title IN ('DateOnlyの練習', 'PostgreSQLの期間検索');
