# 学習順99：本人・操作・結果を永続監査する

**第8段階・第20組／旧Step74・旧URL /learn/74**

## 到達目標

PostgreSQL 18への業務アクセスについて、単なる`ILogger`やNpgsqlの`application_name`ではなく、サーバー側で本人・操作・対象行・結果・相関ID・UTCの永続証跡を残す仕組みを学びます。

## 画面の確認

1. 認証のない学習環境でStep74の「監査を伴う一覧取得を確認」を押し、データを表示しないことを確認する。
2. CIの架空利用者で一覧・詳細・集計・CSV・更新を実行し、同じ正本の本人・部署・対象行・結果が監査テーブルへ記録されることを確認する。
3. CI専用監査トリガーで `AUDITFAIL` 利用者の監査INSERTを故意に失敗させ、CSV・一覧は空、保存はロールバックになることを確認する。

## サーバー側の実装

`LearningAuditService.WriteRequiredAsync` は認証ハンドラー由来の `DOMAIN\user` を取得し、サーバーが相関ID（Guid）とUTC時刻を発行します。Cookie・HTTPヘッダー・フォームから本人名や監査時刻を自己申告させません。

`SecuredLearningService` は、認可された一覧・詳細・集計・CSVの応答前に監査DBへINSERTします。保存・一括更新では業務データ更新と監査INSERTを**同じDbContext・PostgreSQLトランザクション**に含め、いずれかが失敗するとコミットしません。

監査結果は `success`、`not_found`、`limit_rejected` 等の固定コードで記録し、CSV本文・課題名・ADパスワードは監査列へ記録しません。一括操作は同じ相関IDで行ごとに証跡を保存します。

## 本番DBの準備と障害の扱い

教材の `database/schema/003_learning_audit.sql` は**CI専用**です。本番へそのまま適用せず、管理者が監査テーブル・INSERT専用権限・読み出し権限分離・保存期間・暗号化/バックアップ/監査官による改ざん検出を設計します。監査が書けない状態では業務データを出さず、更新を拒否します。`Authorization:DurableAuditConfirmed=true` は実機受入後にのみ有効化します。

データベース全体が停止している場合には監査を同じDBへ保存できません。この場合はフェイルクローズし、別系統のインフラ障害ログでも原因を追える設計とします。匿名拒否や入力不備も、DB監査が不能なら必ず記録されると誤認しないでください。

## 確認問題と解答

**Q1. application_nameにログイン名を入れれば個人監査を保証できる？** いいえ。物理接続はプールで再利用され、本人・対象行・成否の永続証跡にはなりません。

**Q2. 課題更新が成功し、監査INSERTだけ失敗したら？** 同じトランザクションなので両方をロールバックします。

**Q3. 監査DBが停止中でも読み取り結果だけ返してよい？** いいえ。監査の永続化が条件であるため業務データを公開せず拒否します。

## 学習順099で追う監査レコードの作成

`LearningAuditService.WriteRequiredAsync`は、業務操作のログを画面や接続パラメータに委ねず、検証済みの`DOMAIN\user`、操作コード、対象、件数、結果コード、サーバー生成の相関IDとUTC時刻をPostgreSQLへINSERTします。アプリが端末から受信した任意の監査本人名は採用しません。`ILogger`による障害診断や`application_name`は別の用途です。コネクションプールを再利用すると物理接続が複数人の操作を担当するため、`application_name`は特定個人・対象・結果を証明しません。

学習用の`database/schema/003_learning_audit.sql`はCIの架空監査表です。運用テーブルの保存期間、改ざん耐性、書込み専用DB権限、監査者だけの閲覧権限、暗号化、バックアップと復元、容量増加時の保守は実環境で設計します。匿名で拒否した要求やDB停止中の操作をDB監査に必ず記録できるわけではありません。DBへの書込み自体が失敗したときは、外部インフラ障害ログを併用して原因を追跡します。ただし障害ログの存在をもって機密データの返却を許可してはいけません。

### 実コード例1：監査記録を要求するサービス入口

```csharp
var permit = await AuthorizeAsync(ct: ct);
if (permit.Unit is null)
    return new([], 0, permit.Message);
var query = Visible(db, permit.Unit).AsNoTracking();
var rows = await query.OrderBy(x => x.Id).Take(100).ToListAsync(ct);
await audit.WriteRequiredAsync(
    db, "task.list", permit.Unit, 0, rows.Count, "success", ct);
// 必須監査が成功して初めて閲覧用DTOを返す。
```

この断片はデータ範囲と監査の呼出し順を示し、実際のページングや検索条件は`SecuredLearningService.LoadPageAsync`で確認します。監査を`Task.Run`へ投げて応答を先に返すと、監査失敗時の安全拒否を保証できなくなります。

### 実コード例2：CI用の監査障害を再現するSQL

```sql
CREATE OR REPLACE FUNCTION learning_audit_ci_failure() RETURNS trigger
LANGUAGE plpgsql AS $
BEGIN
    IF position('AUDITFAIL' IN NEW.actor_login) > 0 THEN
        RAISE EXCEPTION 'audit failure for CI';
    END IF;
    RETURN NEW;
END;
$;
-- 本番の監査正本へこの試験用トリガーを適用しない。
```

CIの`AUDITFAIL`は失敗時の安全拒否を検証する架空ユーザーであり、本番で同じ名前に意味を持たせる設計ではありません。`SaveAsync`や`CompleteSelectedAsync`では同じDbContextとトランザクションで更新と監査INSERTを確定し、失敗時にはロールバックします。読み取り系の一覧・詳細・集計・CSVも監査不能ならデータ本文を返しません。

| 比較対象 | WinForms＋Dapper | Blazor＋EF Core |
| --- | --- | --- |
| 本人 | OS検証済みログイン名をサーバー側で使う | AuthenticationStateの信頼済みPrincipal |
| 監査の時刻 | DB/サーバー時刻を統一 | UTCで生成し端末時計を信用しない |
| 操作種別 | UPDATE/SELECTの意図を固定コード化 | task.list・task.csv・task.updateなど |
| 確定順序 | 同一IDbTransactionの更新とINSERT | DbContext/transaction内で両方を保存 |
| 読取失敗 | 監査書込み不可なら行を返さない | DTO/CSVのレスポンスを抑止 |

### 操作手順と障害の切り分け

1. `/learn/74`で匿名時に「監査を伴う一覧取得を確認」を押し、拒否メッセージと空の結果を確認します。画面を開けた事実だけでは業務取得の成功ではありません。
2. CI専用DBに架空の認証済み利用者を用意し、一覧、詳細、CSV、更新ごとの操作コードと対象ID・結果を`learning_operation_audit_ci`で照合します。機密の本文、接続文字列、ADパスワードを監査列へ保存しません。
3. `AUDITFAIL`のトリガーを使い、監査INSERT失敗でデータ取得が抑止され、保存と一括処理がロールバックされることを独立に確認します。
4. 本番受入では監査テーブル権限をINSERT専用に制限し、改ざん・消去検出、ローテーション、復元、保持期限の担当者を記録します。`DurableAuditConfirmed`を単にtrueにするだけでは受入になりません。

| 症状 | 原因候補 | 対応 |
| --- | --- | --- |
| 一覧が拒否 | DB監査不可、台帳無効、匿名 | 先に認可・監査接続を確認 |
| 更新の一部だけ見える | トランザクション境界の分離 | 業務と監査の確定単位を再点検 |
| 本人が誤る | 接続プールのapplication_nameを流用 | リクエストの検証済み本人を採用 |
| 障害時の記録がない | DB停止でINSERT不能 | 別経路の障害ログを確認し業務は拒否 |

### 演習と詳しい解答

**演習1．** 監査書込みが失敗したとき、データ表示だけ先に成功させ後から再送してよいでしょうか。

**詳しい解答1．** 許可できません。この教材の安全条件は閲覧前の永続監査成功です。書込み系は更新と監査を同一トランザクションに含めているため、失敗したら両方とも取り消します。別の非機密な運用障害ログは診断には使えますが業務操作の成功条件の代わりにはなりません。

**演習2．** `application_name=DOMAIN\user`の設定だけで監査要件を満たせますか。

**詳しい解答2．** 満たせません。接続はプールで複数利用者に再利用され、個別の操作コード、対象ID、結果、相関ID、UTC、保存期間を含まないためです。必要な列と権限を持つ永続監査表へサーバーがINSERTします。

## 次へ

Step75ではIIS/Windows Server 2025実機と1万人規模の同時接続・プール運用を検証します。
