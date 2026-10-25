# Active Directory認証とPostgreSQL認可

## 基本方針

**認証**はWindows統合認証（Active Directory / Negotiate / Kerberos等）に任せ、**認可**はPostgreSQL上の利用者・ロール・許可情報に基づいて判定します。Windowsの本人確認が成功しても、アプリケーションの未登録者や無効利用者には業務データを公開しません。

## 想定する認可フロー

1. ブラウザーからアプリへアクセスする。
2. IISのWindows認証または適切に構成されたASP.NET Core Negotiateで本人を認証する。
3. サーバー側の`ClaimsPrincipal`から識別子を取得する。Windows環境では原則として**SID**を安定キーにする。
4. SIDを用いてPostgreSQLの`app_user`を検索する。照合不可、無効、重複など異常があれば拒否する。
5. `app_user_role`と`app_role_permission`から操作許可を求める。
6. 画面ルートと**業務サービスの操作単位**で認可する。
7. 業務操作は利用者識別子・対象・結果・時刻を適切に監査記録する。

`DOMAIN\user`は表示・補助照合用とし、名前変更や同名アカウント再作成に備えて一意な識別子として過信しません。SID取得ができない環境では、環境固有の信頼できる識別子を別途設計・検証する必要があり、ユーザー名だけへ無条件にフォールバックしません。

## 認可ルール

| 状態 | 期待結果 |
| --- | --- |
| Windows認証に失敗 | 業務画面を利用できない |
| AD認証済みだがDB未登録 | 拒否 |
| DB登録済みだが`is_active=false` | 拒否 |
| DB登録済みで権限がない | 対応する操作は拒否 |
| 許可されたユーザー | 許可された操作のみ実行可能 |
| DBに接続できない | 権限を許可せず、安全にエラーを表示 |

匿名閲覧できる教材を設ける場合は、業務データ・内部API・管理機能へ接続できない無害なサンプル画面だけを`AllowAnonymous`で明示します。`FallbackPolicy`は原則として認証要求とし、未指定の業務ルートを無条件公開しません。

## 実装・運用上の注意

- Windows認証は`Microsoft.AspNetCore.Authentication.Negotiate`などの公式機構を利用する。IIS、Kestrel、リバースプロキシそれぞれで設定・信頼境界が異なる。
- リバースプロキシが送った任意の`X-User`ヘッダーを信用して認証してはならない。
- Windows IDの偽装を使う場合は適用範囲を明示する。アプリのDB接続資格情報とWindows本人の扱いを混同しない。
- 画面の`AuthorizeView`による表示制御だけでなく、サービスのメソッドでも許可を確かめる。
- ロールの更新は次の操作に反映されるようにし、長寿命のInteractive Server回線に古い認可結果を永続保持しない。
- サーバー間で認証情報・回線・接続を維持する場合は負荷分散とセッション運用を別途確認する。
- 認証ログには生の資格情報や秘密を残さず、閲覧権限と保管期間を定める。

## 検証条件

成功経路だけでなく、未登録、無効化、権限剥奪、DB障害、接続切断、SID取得失敗、ADの名前変更・再作成、並行更新などの**拒否経路**をテストします。

## 参照

- [Windows認証の公式Blazorガイド](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/security/blazor-web-app-with-windows-authentication?view=aspnetcore-10.0)
- [ASP.NET Core Windows認証](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/windowsauth?view=aspnetcore-10.0)
- [テーブル設計案](database.md)
