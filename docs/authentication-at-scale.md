# 実務設計：1万人以上のWindows認証と既存利用者台帳による認可

## 前提と結論

- 社内ドメイン参加端末、Windows Server 2025 + IIS、Blazor Web App (.NET 10 / Interactive Server)、PostgreSQL 18を想定。
- 利用者は1万人以上。ただし**総登録人数と同時接続人数を混同しない**。ピーク同時接続とRPSに従って規模を決める。
- Active DirectoryはWindows統合認証と**認証済みユーザー名を受け取る目的だけ**に使用。LDAP検索・ADロール同期・SID照合・ADへの登録を要件としない。
- アプリは利用者登録を**一切しない**。既存ユーザー台帳（自DBテーブル、既存ビュー、外部テーブル）を読み取り専用で参照する。
- 既存の台帳が持つ有効フラグ、所属、業務別権限、対象データの範囲で認可する。既存の列構造はシステムごとに異なるため、**読み取り専用の統一ビュー**に変換してアプリを簡潔に保つ。

**第一候補**は「IIS Windows認証 → 信頼できる主体の `DOMAIN\user` を取得 → 正規化 → 読み取り専用ビューを検索 → 業務サービスの操作単位で判定」です。ユーザー名を単独の `user` へ切り詰めない。

## 本人識別子の設計上の制約

1. `HttpContext.User.Identity.Name` や `AuthenticationStateProvider` が返す**認証済み** Windows Identity の `Name` のみを採用。任意のヘッダー、画面入力、URLから渡されたユーザー名を信用しない。
2. `DOMAIN\user` を単純なユーザー名 `user` に省略しない。UPN表記 `user@example` と混同せず、**組織内で統一する形式を明文化**する。大小文字は統一するが、DB照合時の照合順序・正規化は同一規則に揃える。
3. ユーザー名はSIDと異なり、アカウント名の変更・再使用・ドメイン改名で同一性が失われる。既存人事台帳への同期と変更時の失効・再照合を運用要件にする。名前の再使用による旧権限継承を防ぐ。**再使用リスクを台帳側で管理できないならユーザー名だけの案は採用不可**。
4. 認証されていても正規化不可・登録0件・一致複数件・無効・接続障害・権限なしは**拒否**（fail closed）。認証と業務認可を必ず分離する。

## 既存利用者台帳の読み取り設計

実テーブルまたは外部テーブルの列構造は変えず、管理者が例のような読み取り専用ビューを用意する。

```sql
-- 以下は契約の列を示す擬似例。実行SQLではない。
-- 既存の人事テーブルまたはFDW外部テーブルから必要列を射影する。
CREATE VIEW learning_user_access_v AS
SELECT
  normalized_domain_login AS login_name, -- DOMAIN\userを統一した値
  enabled AS is_active,
  can_read_business AS can_read,
  can_edit_business AS can_edit,
  department_code AS unit_code,
  permission_revision AS authorization_version
FROM existing_authoritative_user_source;
```

- `login_name`は**ユニーク**であることが必要。ビューは固有制約を定義できない場合があるため、原本の一意制約と起動時・運用監視を併用する。アプリ側では2件まで取得し、重複なら拒否する。
- `unit_code`等の属性を業務対象のSQLに必ず織り込む。`Count`と`Items`に同じ条件を使い、詳細・CSV・一括更新・集計にも適用する。
- ビューを読むPostgreSQL接続ロールは最小限の`SELECT`だけを持つ。アプリは台帳を`INSERT/UPDATE/DELETE`しない。元システムにあるデータ以外へ「補助登録」を行わない。
- `postgres_fdw`等の外部テーブルでは、リモート側のフィルターpushdown、索引、遅延、タイムアウト、照合順序、参照障害を検証する。必要なら管理されたローカル同期スナップショットを選ぶが、**権限反映時間と障害時の拒否条件**を先に定義する。
- 原本に編集権限などの列が無いなら、勝手にアプリ側で許可を作らず、正本側と権限仕様を決める。

## 1万人規模の性能設計

**ユーザー総数1万人は接続1万本を意味しない。** Interactive Serverでは開いているブラウザタブごとにcircuit/SignalR接続が発生し得る。目安をハードコードせず、実際の最大同時circuit・業務操作RPS・同時DBクエリ数を観測してサイジングする。

- PostgreSQLの`max_connections`を利用者数へ合わせて増やさない。Npgsqlの接続プールと`IDbContextFactory`による短命DbContextを使用し、負荷に合わせ`Maximum Pool Size`と必要ならPgBouncer等を調整する。
- 小さな`SELECT`で単一利用者の必要列のみ取得する。外部テーブルの場合も接続タイムアウト・クエリタイムアウト・索引とリモートpushdownを測る。
- 認可のキャッシュは任意。低リスクの表示情報だけ短TTL・ユーザー名+権限版+業務範囲で管理し、重要な更新・CSV・閲覧は操作時に最新の正本を照合する。DB/外部台帳停止はキャッシュを理由に許可しない。
- Web多台数構成はsticky sessions/SignalR、再接続、ロールアウト時のcircuit切断、CPU・GC・メモリ、Data Protectionキー共有（Cookie採用時）を含めて負荷試験する。
- 認可Whereはテーブル全件読込後のLINQ to Objectsで実装しない。DBに翻訳されるクエリを使う。対象データの部署コードなどでフィルタし、読み取り結果にも拒否試験を追加する。

## 二つの認証UI方式

### A. 通常業務：ログイン画面なし（既定）

- IISのWindows認証を有効、匿名認証を無効化。ブラウザーは社内ポリシーで統合認証し、ADパスワードはWebアプリが扱わない。
- 認証の成否はWebサーバーの本人情報に従う。**ログアウトボタンでWindows SSOセッション自体を終了できると誤解させない**。
- アカウント無効化時の挙動（既存circuitを含む）は別の受入試験が必要。DB権限は重要操作ごとに再評価する。

### B. 明示的なログイン導線が必要な業務

- おすすめはログイン前の案内ページ → 明示的な「Windowsアカウントで続ける」操作 → **サーバー側のWindows認証エンドポイント**で本人確認 → 保護された短命Cookieに本人識別情報だけを記録する方式。
- ただしIISサイト全体で匿名認証を無効化すると、ログイン前の案内ページは表示できない。専用の認証エンドポイント/アプリを分けるか、認証方式を経路単位で設計する。**IISの全体設定とアプリのFallbackPolicyは別の層**。
- Cookieの発行はHTTPレスポンスで行う。Blazorの長寿命circuit内イベントから`HttpContext.SignInAsync`を直接呼ばない。
- CookieはSecure/HttpOnly/SameSite/適切な有効期限/絶対上限/CSRF対策/Data Protectionキー共有を構成。ロールを固定保存せずDBから定期・操作時再評価。失効・ユーザー名変更・端末共用を考慮する。
- ログアウトはアプリCookieの削除とcircuitの切断/遷移まで。**Windows認証自体のサインアウトではない**。実運用ではリスク・監査要件次第でAD FS/OIDCなど本物の明示的ログインフローも比較する。
- 自前のID/パスワード入力フォームでADパスワードを受け取り、LDAP bindして使う設計は推奨しない。

## 監査とPostgreSQL application_name

監査の信頼性に優先順位をつける。

1. **主監査**：サービス層で `actor`、操作、対象ID、処理結果、UTC時刻、correlation_id、権限判定の結果を記録。業務データの成功更新に対する監査は**同じDBトランザクション**に格納するか、必要な分離・永続化保証がある監査用経路を採用する。失敗・拒否はロールバック後の独立した監査/ログに送る設計が必要。
2. **DB補助**：接続文字列の `Application Name=PgBlazorSamples.Web` はアプリ識別であって、操作したAD利用者の証明にならない。利用者数だけプールを分割するような可変のconnection stringを作らない。
3. **相関が必要な場合**：同一DBトランザクション内の信頼されたサーバー処理から `SET LOCAL application_name = 'app/...'` 等を設定し、必ずSQL処理と同じ物理接続・トランザクションで用いる。プール上の別操作へ漏らさず、終了時に戻ることを負荷試験する。個人情報をapplication_nameへ直接入れず、ログ相関ID等を使う。設定しても任意文字列なので**監査証跡として単独では信用しない**。
4. PostgreSQLの`log_line_prefix` (`%a`=application_nameなど)やDB監査拡張は運用要件次第で補助。コネクションプール/PgBouncer/FDW跨ぎのログ連携を検証する。

## 受入の最低条件

- [ ] 1万人の**登録総数**とピーク**同時circuit/RPS**を分けて測定
- [ ] Windows認証はサーバーの主体情報のみ・名前変更多重登録と再使用を拒否
- [ ] 既存テーブル/外部ビューは読み取り専用・0件/複数件/停止時は拒否
- [ ] 一覧/Count/詳細/CSV/更新/集計が同じ部署・権限のフィルターを適用
- [ ] キャッシュ失効・Cookie失効・既存circuitからの旧権限利用を拒否
- [ ] パスワードをWebアプリ内に収集・保存しない
- [ ] IIS/Kerberos/SPN/多台数/外部テーブルの障害試験（CIのモックは実機保証ではない）
- [ ] 監査の成功トランザクションと拒否ログの保存経路を検証

## 公式資料

- [Windows認証 (.NET 10)](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/windowsauth?view=aspnetcore-10.0)
- [Blazor Windows認証 (.NET 10)](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/security/blazor-web-app-with-windows-authentication?view=aspnetcore-10.0)
- [Blazor認証状態とcircuit (.NET 10)](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0)
- [ASP.NET Core Cookie認証](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0)
- [PostgreSQL postgres_fdw](https://www.postgresql.org/docs/18/postgres-fdw.html)
- [PostgreSQLログ](https://www.postgresql.org/docs/18/runtime-config-logging.html)
