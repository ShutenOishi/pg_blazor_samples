# PostgreSQL 18 データベース設計案

## 原則

PostgreSQLは業務ユーザーと操作権限を保持します。**ADのパスワード、NTLMハッシュ、Kerberosチケットは保存しません。** ユーザー識別の根拠はサーバーで検証されたWindows主体です。データベース接続はNpgsql、CRUDとLINQはEF Core 10（Npgsql公式プロバイダー）を基本とします。linq2dbは上級の比較題材に限定します。

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
- Blazor Interactive Serverでは`IDbContextFactory<AppDbContext>`を使って操作ごとにDbContextを生成・破棄する。Npgsql物理接続プールとDbContextプールは別。
- `NpgsqlDataSource`の明示的な共有は必要性が出た時に検討し、二重の接続プール構築を避ける。
- 検索の絞り込み・件数制限・並べ替えはDB側で実行する。
- 必要な列のみ取得し、一覧で大量のレコードをメモリに展開しない。
- 監査記録を含む複数更新はトランザクション境界を明確にする。
- 日時は`timestamptz`を使用し、UTCと日本時間の変換を明示する。
- 変更スクリプトは番号付きで管理する。既存の適用済みスクリプトを黙って変更しない。

## EF Coreで扱う日時・期間型（追加予定）

単純な日付は`DateOnly`、時刻は`TimeOnly`、UTC日時は`DateTime`（UTC）を使います。`daterange`/`tstzrange`/`datemultirange`はNpgsqlの型マッピング・境界・包含演算を**PostgreSQL 18で統合試験**してから学習用DDLとC#の型を確定します。UIのMudDatePicker（`DateTime?`）やMudDateRangePickerの`DateRange`をそのままエンティティの型にしません。

詳細：[EF Core 10データアクセス](efcore10-data-access.md)／[一覧表示パターン](data-grid-patterns.md)。

## テストデータ

架空のドメイン・ユーザー名・SIDのみを使います。本番のAD情報や実在個人の名前・アカウント情報を`seed`に入れません。DB結合テストでは毎回隔離された専用DB/スキーマを準備・破棄します。

関連：[認証と認可](authentication.md) / [品質管理](quality.md)

## Step31〜40で実装した簡単なEF Core教材

Web/Data/LearningTask.cs（Entity/DTO）、Web/Data/LearningDbContext.cs（型対応）、Web/Services/Learning/LearningDbService.cs（検索/更新）、Program.cs（Factory DI）を基本とします。**この段階では新しい層別プロジェクトを作らず、1つのWeb内で学びます。** DDLはdatabase/schema/001_learning_task.sql、架空データはdatabase/seed/、日付・Range・xminを確認するSQLはdatabase/tests/です。実PostgreSQLに接続していない場合、UI上の動作とDB処理成功を区別します。
