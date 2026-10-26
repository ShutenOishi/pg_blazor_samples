-- Step63〜65：開発用PostgreSQL 18へのみ適用。実ADの利用者やSIDは投入しない。
-- 未登録・無効・役割なしは拒否する。ロール付与は管理者が別途安全な手段で行う。
CREATE TABLE IF NOT EXISTS app_user
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sid varchar(184) NOT NULL UNIQUE,
    display_name varchar(100) NOT NULL,
    is_active boolean NOT NULL DEFAULT false,
    CONSTRAINT ck_app_user_sid CHECK (sid ~ '^S-1-[0-9]+(-[0-9]+){2,}$')
);
CREATE TABLE IF NOT EXISTS app_role
(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code varchar(80) NOT NULL UNIQUE,
    name varchar(100) NOT NULL,
    is_active boolean NOT NULL DEFAULT true
);
CREATE TABLE IF NOT EXISTS app_permission
(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code varchar(100) NOT NULL UNIQUE,
    name varchar(100) NOT NULL
);
CREATE TABLE IF NOT EXISTS app_user_role
(
    user_id bigint NOT NULL REFERENCES app_user(id) ON DELETE CASCADE,
    role_id integer NOT NULL REFERENCES app_role(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);
CREATE TABLE IF NOT EXISTS app_role_permission
(
    role_id integer NOT NULL REFERENCES app_role(id) ON DELETE CASCADE,
    permission_id integer NOT NULL REFERENCES app_permission(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);
CREATE INDEX IF NOT EXISTS ix_app_user_role_role_id ON app_user_role(role_id);
CREATE INDEX IF NOT EXISTS ix_app_role_permission_permission_id ON app_role_permission(permission_id);

-- 架空のロール・権限定義のみ。利用者登録や許可の自動付与はしない。
INSERT INTO app_role(code, name) VALUES
('viewer', '閲覧担当'), ('editor', '編集担当')
ON CONFLICT(code) DO NOTHING;
INSERT INTO app_permission(code, name) VALUES
('learning.read', '教材の読み取り'), ('learning.edit', '教材の編集')
ON CONFLICT(code) DO NOTHING;
INSERT INTO app_role_permission(role_id, permission_id)
SELECT r.id, p.id FROM app_role r CROSS JOIN app_permission p
WHERE (r.code = 'viewer' AND p.code = 'learning.read')
   OR (r.code = 'editor' AND p.code IN ('learning.read', 'learning.edit'))
ON CONFLICT DO NOTHING;
