# PostgreSQL 18 データベース設計案

## 原則

PostgreSQLは業務ユーザーと操作権限を保持します。**ADのパスワード、NTLMハッシュ、Kerberosチケットは保存しません。** ユーザー識別の根拠はサーバーで検証されたWindows主体です。データベース接続はNpgsql、CRUDとLINQはlinq2dbを基本とします。

以下は**初期設計案**であり、実行済みDDLではありません。型・制約・権限・監査をレビューしてから`database/schema`へSQLを配置します。

## テーブル案

| テーブル | 主な列 | 用途 |
| --- | --- | --- |
| `app_user` | `user_id uuid PK`、`ad_sid text UNIQUE NOT NULL`、`account_name text`、`display_name text`、`is_active boolean`、`created_at timestamptz`、`updated_at timestamptz` | AD主体とアプリ利用者の対応 |
| `app_role` | `role_id uuid PK`、`role_code text UNIQUE`、`role_name text` | ロールの定義 |
| `app_permission` | `permission_code text PK`、`description text` | 操作権限の定義 |
| `app_user_role` | `user_id FK`、`role_id FK`、複合PK | ユーザーの役割 |
| `app_role_permission` | `role_id FK`、`permission_code FK`、複合PK | 役割ごとの許可 |
| `audit_event` | `event_id bigint PK`、`user_id uuid?`、`action text`、`target text`、`outcome text`、`occurred_at timestamptz` | 監査情報 |

## 推奨する初期DDL（レビュー対象）

```sql
CREATE TABLE app_user (
    user_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    ad_sid text NOT NULL UNIQUE,
    account_name text NOT NULL,
    display_name text NOT NULL,
    is_active boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE app_role (
    role_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    role_code text NOT NULL UNIQUE,
    role_name text NOT NULL
);

CREATE TABLE app_permission (
    permission_code text PRIMARY KEY,
    description text NOT NULL
);

CREATE TABLE app_user_role (
    user_id uuid NOT NULL REFERENCES app_user(user_id),
    role_id uuid NOT NULL REFERENCES app_role(role_id),
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE app_role_permission (
    role_id uuid NOT NULL REFERENCES app_role(role_id),
    permission_code text NOT NULL REFERENCES app_permission(permission_code),
    PRIMARY KEY (role_id, permission_code)
);

CREATE TABLE audit_event (
    event_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id uuid REFERENCES app_user(user_id),
    action text NOT NULL,
    target text,
    outcome text NOT NULL,
    occurred_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_audit_event_occurred_at ON audit_event (occurred_at DESC);
```

このDDLは学習の出発点で、監査ログの改ざん耐性、監査利用者削除時の保持要件、職務分掌、個人情報、ロール管理操作の監査、既存業務テーブルとの結合は実装時に検討します。実行権限は最小限とし、アプリ実行ユーザーがスキーマ変更できないようにします。

## データアクセスの標準

- SQLパラメーター化とLINQのパラメーター化を用い、利用者入力をSQL文字列に結合しない。
- `NpgsqlDataSource`は安全に共有する設計を検討し、接続・コンテキスト・トランザクションは処理ごとに破棄する。
- 検索の絞り込み・件数制限・並べ替えはDB側で実行する。
- 必要な列のみ取得し、一覧で大量のレコードをメモリに展開しない。
- 監査記録を含む複数更新はトランザクション境界を明確にする。
- 日時は`timestamptz`を使用し、UTCと日本時間の変換を明示する。
- 変更スクリプトは番号付きで管理する。既存の適用済みスクリプトを黙って変更しない。

## テストデータ

架空のドメイン・ユーザー名・SIDのみを使います。本番のAD情報や実在個人の名前・アカウント情報を`seed`に入れません。DB結合テストでは毎回隔離された専用DB/スキーマを準備・破棄します。

関連：[認証と認可](authentication.md) / [品質管理](quality.md)
