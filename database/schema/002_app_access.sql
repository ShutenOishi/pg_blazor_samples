-- Step63〜65のCI検証専用：既存の人事/業務利用者台帳を模した外部データ。
-- 実運用環境でこのファイルを実行しない。Webアプリ自身は利用者登録・更新をしない。
CREATE TABLE IF NOT EXISTS learning_existing_user_ci
(
    login_name varchar(129) PRIMARY KEY,
    display_name varchar(100) NOT NULL,
    is_active boolean NOT NULL DEFAULT false,
    can_read boolean NOT NULL DEFAULT false,
    can_edit boolean NOT NULL DEFAULT false,
    unit_code varchar(32) NULL,
    authorization_version bigint NOT NULL DEFAULT 0
);

-- 1万人以上の既存利用者を想定した検索を開発用DBで再現するための架空データ。
-- 上位システムの既存テーブル / FDW 外部テーブルも同じ契約のビューへ射影可能。
INSERT INTO learning_existing_user_ci
    (login_name, display_name, is_active, can_read, can_edit, unit_code)
SELECT
    'TRAINING\\USER' || lpad(i::text, 5, '0'),
    '架空利用者' || i::text,
    TRUE, TRUE, FALSE, 'DEMO'
FROM generate_series(1, 10001) AS i
ON CONFLICT (login_name) DO NOTHING;

-- ビューの列定義を契約化する。条件のpushdownと元表の一意索引を別途検証すること。
CREATE OR REPLACE VIEW learning_user_access_v AS
SELECT login_name, display_name, is_active, can_read, can_edit, unit_code, authorization_version
FROM learning_existing_user_ci;

-- アプリに必要なのは読み取りだけ。CI用のfixture操作はテストランナーに限る。
