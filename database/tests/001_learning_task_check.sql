-- Step40：開発用DBのSQL側で日付・時刻・Range・xminを確認する。
BEGIN;
SELECT '日付' AS item, DATE '2026-10-26'::text AS value;
SELECT '時刻' AS item, TIME '09:30'::text AS value;
SELECT daterange('2026-10-01', '2026-11-01', '[)') @> DATE '2026-10-26' AS contains_day;
SELECT datemultirange(daterange('2026-10-01','2026-10-10','[)'), daterange('2026-10-20','2026-10-31','[)')) @> DATE '2026-10-26' AS multirange_contains;
SELECT id, title, xmin FROM learning_task ORDER BY id LIMIT 5;
ROLLBACK;
