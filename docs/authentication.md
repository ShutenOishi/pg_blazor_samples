# Active Directory認証と既存PostgreSQL台帳の認可

## 基本方針

Active Directoryは**Windows統合認証と認証済みのドメイン付きユーザー名取得だけ**に使用します。利用者の登録・有効性・所属・業務別権限の正本は既存PostgreSQLテーブル、ビューまたは外部テーブルです。アプリは新しい利用者を登録・権限付与しません。

## 認証から業務権限まで

1. IIS Windows認証またはASP.NET Core Negotiateが本人を認証。
2. サーバーの認証済みPrincipalから`DOMAIN\\user`を取得。HTTPヘッダー・URL・画面入力の値を信じない。
3. 正規化済みの同形式のキーで、DB管理者が用意した読み取り専用の統一ビューを**最大2件だけ**照合する。
4. 0件/重複/無効/DB障害なら拒否。有効なら`can_read`、`can_edit`、`unit_code`などから必要な業務権限を判定する。
5. Count/一覧/詳細/CSV/更新/集計/APIのすべてで操作単位に同一の業務データ範囲へ権限を適用し、監査を記録する（Step66〜70）。

ADユーザー名はSIDより変更・再使用の問題があるため、**ドメインを含む契約、改名・再使用の検知、台帳同期・旧権限失効を運用前提**とします。UPNとの表記変換は暗黙に行いません。

## 既存台帳の契約

`learning_user_access_v`が`login_name`、`display_name`、`is_active`、`can_read`、`can_edit`、`unit_code`、`authorization_version`を返す読み取り専用ビューとして使用します。

- 実運用では各システムの既存テーブル・外部テーブルからこの列群へ投影するビューをDB管理者が作成。
- アプリに利用者の新規登録・更新機能を置かない。
- 検証用`database/schema/002_app_access.sql`は架空1万1人の**CI専用fixture**。本番には適用しない。
- 業務サービスへの保護が完了するまでは開発用DBに限定する。

## 認可ルール

| 状態 | 期待 |
| --- | --- |
| 未認証・ユーザー名不正 | 拒否 |
| 既存台帳に該当0件 / 2件以上 | 拒否 |
| 既存台帳で無効 | 拒否 |
| 閲覧不可・編集不可・対象部署外 | 該当操作を拒否 |
| 台帳または外部DB停止 | 拒否 |
| 有効かつ対象権限あり | 許可された業務操作のみ |

`FallbackPolicy`による認証要求と`AuthorizeView`によるボタン非表示は、業務データの操作単位の認可の代わりにはなりません。匿名画面の構成、Cookie/SSOの違い、Connection Pool、監査は[1万人規模の実務設計](authentication-at-scale.md)で扱います。

## 実装段階と実機受入

- Step61〜65：Negotiateを選択可能にし、認証済みユーザー名と既存台帳を照合する。DB拒否経路をテストする。
- Step66〜70：画面と全サービスのデータ範囲を保護し、管理・監査・IIS/Kerberosを検証する。
- CIのWindows/Linux/Chrome/PostgreSQL18試験では**本物のAD認証・Kerberos・IIS本番動作は実証できない**。
- 開発でWindows認証を無効にできるが、その状態では本物の業務データを扱ってはならない。

## 参考

- [1万人規模・外部テーブル・Cookieと監査](authentication-at-scale.md)
- [Windows Server 2025とIIS](windows-server-2025-iis.md)
- [ASP.NET Core Windows認証](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/windowsauth?view=aspnetcore-10.0)
