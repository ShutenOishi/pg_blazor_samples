-- Step32：全件架空データ。再実行すると行が増えるため、必要時だけ適用する。
INSERT INTO learning_task(title, due_date, start_time, is_done) VALUES
 ('EF Coreの接続練習', DATE '2026-10-30', TIME '09:00', false),
 ('DateOnlyの練習', DATE '2026-11-01', TIME '10:15', false),
 ('PostgreSQLの期間検索', DATE '2026-11-03', TIME '13:00', true);
