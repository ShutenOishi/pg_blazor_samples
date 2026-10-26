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
    'TRAINING' || chr(92) || 'USER' || lpad(i::text, 5, '0'),
    '架空利用者' || i::text,
    TRUE, TRUE, FALSE, 'DEMO'
FROM generate_series(1, 10001) AS i
ON CONFLICT (login_name) DO NOTHING;

-- ビューの列定義を契約化する。条件のpushdownと元表の一意索引を別途検証すること。
-- Step71：同じWindows名の大文字・小文字違いをビュー側で同じキーへ正規化する。
-- CI用元表のPRIMARY KEYは元文字列の一意性しか保証しない。
-- 本番では正本の利用者名一意制約と照合方式を管理者が別途受け入れる。
-- PostgreSQL側でupper(login_name)を評価する場合の検索用索引。
CREATE INDEX IF NOT EXISTS learning_existing_user_ci_login_upper_idx
    ON learning_existing_user_ci (upper(login_name));

CREATE OR REPLACE VIEW learning_user_access_v AS
SELECT upper(login_name) AS login_name, display_name, is_active, can_read, can_edit, unit_code, authorization_version
FROM learning_existing_user_ci;

-- アプリに必要なのは読み取りだけ。CI用のfixture操作はテストランナーに限る。
