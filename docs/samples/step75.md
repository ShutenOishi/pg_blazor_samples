# 学習順100：IIS・Kerberos・負荷・障害の実機受入

**第8段階・第20組／旧Step75・旧URL /learn/75**

## 到達目標

Windows Server 2025 + IISのドメイン環境で、.NET 10 Blazor Interactive Server・AD・PostgreSQL18・既存DB/FDW・監査を正しく運用できるかの受入手順を理解します。**CIの偽PrincipalとChrome成功は実AD・IISの動作保証ではありません。**

## 実機受入の手順

1. **HTTPS/IIS/Kerberos**：サーバー証明書の信頼・HSTS・SPN・Negotiateを確認する。通常のWindows認証と、明示ログイン時の匿名案内・Windows必須HTTP経路を分離し、匿名・Cookie偽装で本人の資格を取得できないことを試す。
2. **Cookie**：専用HTTPの確認GETで認証し、CSRF付きPOSTで10分Cookieを発行する。期限切れ・ログアウト・タブ間・端末変更・SignalR再接続・別サーバー接続を検証する。Data Protection鍵は本番アプリプール限定ACL、バックアップ・保存時暗号化、鍵ローテーションを受け入れる。
3. **既存利用者台帳**：1万人以上のログイン名を全件取得せず、正規化索引と最大2行の重複拒否を確認する。無効・改名・名前再利用・FDW停止/遅延・索引条件pushdownを実DBで検査し、P95/P99とタイムアウトを記録する。
4. **更新と監査**：正本と業務データの行ロックを同一DBで保証できる場合だけ `Authorization:TransactionalLedgerConfirmed=true`。FDW原子性が未確認なら更新は拒否する。監査のINSERT、アクセス権分離・保存期間・改ざん検出・バックアップ/復元を受け入れてから `Authorization:DurableAuditConfirmed=true`。監査SQL失敗で書込みがロールバックされることを試験する。
5. **負荷/回線**：登録済み10,001人、実同時利用者数、Blazor circuit数、Npgsql物理プール数を独立の数として扱う。同時接続を段階的に増やし、メモリ・CPU・SignalR再接続・DB接続待機・P95/P99・エラー率を記録する。

## 安全側の初期設定

```json
{
  "WindowsAuthentication": { "Enabled": true, "ExplicitLoginEnabled": false },
  "Authorization": {
    "TransactionalLedgerConfirmed": false,
    "DurableAuditConfirmed": false
  }
}
```

運用承認前に確認フラグをtrueへ変更しないでください。明示Cookieモードでは `WindowsAuthentication:DataProtectionKeysPath` を設定し、IISアプリプールだけが読み書きできる共有鍵保管先と暗号化を実地確認します。接続文字列やADパスワードはソースへ置きません。

## 否定試験と合格条件

| 障害/攻撃 | 検証 | 合格条件 |
| --- | --- | --- |
| 匿名・任意Claim | Windows認証必須HTTPを要求 | Cookie発行不可、業務データなし |
| 権限剥奪・重複 | ログイン名・部署変更、更新前の失効 | 他部署データを返さず更新拒否 |
| FDW停止/遅延 | 原本切断・タイムアウト | フェイルクローズ、復旧方法明確 |
| 監査INSERT失敗 | 保存・CSV・一覧 | 保存はロールバック、データ非公開 |
| Cookie期限/鍵消失 | 期限後・別ノード・ログアウト | 再認証が必要、安全に拒否 |
| 1万人台帳と高負荷 | 同時circuit・プール・メモリ | 合意したSLO内、計測証跡あり |

担当者・環境・実行日時・ビルドSHA・試験操作・計測値・証跡を受入票へ残してください。CIが成功しても未実施の実機試験を「完了」としないでください。

## 確認問題と解答

**Q1. 台帳10,001人なら同時接続数は10,001？** いいえ。登録人数と同時circuit・DB接続数は別です。

**Q2. actionのapplication_nameがあれば個人監査の永続化は不要？** いいえ。業務本人・操作・対象・成否は別に保存します。

**Q3. Windows/LinuxのCI成功でKerberosも完成？** いいえ。IIS/ADを使う現地検証が不可欠です。

## 学習順100：実機で受け入れる条件を仕様化する

`IisAcceptancePage.razor`の「本番運用の認可ゲートを照合」は`SecuredLearningService.AuthorizeAsync`を呼ぶ学習操作です。画面のチェックが通っても実IIS・AD・Kerberos・FDWの検証が終わった証拠にはなりません。CIはWindows/Linux Release、xUnit、Chromeの画面操作、PostgreSQL18の専用架空DBを検証します。現地の機器構成、サービスアカウント、SPN、ロードバランサー、証明書、既存正本の保護運用は別の受入作業です。

実務では、対象構成図、設定の管理者、変更日時、検証アカウント、失敗時の戻し方を先に確定します。AD本人認証はIISとNegotiateが検証した主体に限定し、Cookieモードでも都度DB台帳を確認します。外部正本FDWが切断・遅延・失効した場合に成功扱いしないことを最優先にします。1万人超という条件は登録アカウント数であり、同時SignalR circuitやNpgsqlの物理接続数とは独立の負荷です。

### 実コード例1：受入前は書込みを閉じる構成

```json
{
  "WindowsAuthentication": {
    "Enabled": true,
    "ExplicitLoginEnabled": false
  },
  "Authorization": {
    "TransactionalLedgerConfirmed": false,
    "DurableAuditConfirmed": false
  }
}
```

この設定は業務更新と監査の確認を宣言していない状態です。検証を省くために`true`へ変更してはいけません。明示Cookieモードが本当に必要な場合だけ、保護されたData Protection鍵保管先、アプリプールACL、鍵ローテーションと復元を実機で検証します。

### 実コード例2：同時接続と台帳照合の測定を分ける

```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT login_name, unit_code, can_read, can_edit
FROM learning_user_access_v
WHERE login_name = 'TRAINING\USER00001'
LIMIT 2;
-- CIの架空台帳の例。本番FDWでは元表と通信・計画を別途計測。
```

上記の検索計画だけから数万人のcircuit負荷を予測できません。別試験で仮想利用者の同時数を段階的に増やし、`p95`、`p99`、HTTP・SignalRの再接続率、CPU、使用メモリ、GC、Npgsql接続待ち、DB側のロック待ち、監査容量を測定します。利用者全員が同時に接続するという前提も、接続数を勝手に小さく見積もる前提も採用せず、仕様で必要な到達目標を定義します。

| 設計項目 | WinForms＋Dapper | Blazor Interactive Server |
| --- | --- | --- |
| 認証 | 各端末のWindows主体を安全に使用 | IISのSPN/Negotiate/プロキシ経路を検証 |
| 接続 | アプリ起動台数とDB接続数を管理 | 同時利用者・circuit・DBプールを別々に計測 |
| 鍵 | クライアント側機密設定を管理 | Cookie/Data Protection鍵の共有・復元 |
| DB | 既存台帳と業務SQLを直接検証 | FDW・EF Core変換・遅延と失効を確認 |
| 復旧 | プロセス再起動と再接続 | 再接続後の本人再確認と安全拒否 |

### 操作手順と障害の切り分け

1. 本番相当IISでHTTPS証明書の有効性、HSTS、SPN、認証プロバイダー、アプリプールのアクセス権を確認します。匿名・別ドメイン・不正形式の本人情報を拒否します。
2. 認証GETとPOST、CSRF、短命Cookieの期限切れ、ログアウト、鍵を失った別サーバー、複数タブとSignalR再接続を順に検証します。実ADを用いる場合は運用管理者の許可した専用検証利用者を使います。
3. 既存台帳について改名、重複、権限剥奪、部署変更、FDW停止・遅延を再現し、正規化条件と正本ロックの実効性を調べます。監査障害では一覧も更新も安全拒否します。
4. 10,001人の架空登録台帳で検索時間を測定し、別に合意した同時ユーザー数でcircuit、DB接続、メモリ、負荷と障害復旧を測ります。実業務データを負荷用に転用しません。
5. 実施日時・担当者・環境・SHA・設定差分・合否・測定値・証跡・戻し手順を受入票に記録し、未実施を合格へ変えません。

| 異常 | 切り分け | 受入条件 |
| --- | --- | --- |
| Windowsログイン失敗 | SPN、DNS、IIS、資格交渉経路 | 正規本人のみ認証 |
| 接続プール枯渇 | 同時circuit、DbContext寿命、Npgsql設定 | SLO内で回復しデータ漏えいなし |
| 失効後のデータ取得 | 既存DB台帳のキャッシュ・通信 | 正本に従い閲覧・更新を拒否 |
| 監査DB停止 | DB障害と例外ハンドリング | 業務データ非公開・更新取消 |
| FDWロック不可 | 外部正本のトランザクション契約 | 原子性未確認なら書込み拒否 |

### 演習と詳しい解答

**演習1．** PostgreSQL18の実DB結合試験とChromeが成功したため、実IISのSPNとCookie鍵の復旧を未試験のまま本番受入を宣言できますか。

**詳しい解答1．** できません。CIが証明する範囲を明記し、現地のKerberos認証と鍵の共有、FDW・監査、復旧、権限失効を別に受け入れます。現地証跡がなければ未完了のままです。

**演習2．** 10,001人が登録されているテストDBの検索が高速なら、10,001人の同時利用にも耐えられますか。

**詳しい解答2．** 直接は判断できません。登録人数と同時ブラウザー数、circuit数、物理DB接続数は別の尺度です。各資源の段階負荷、p95/p99、失敗率、再接続とプールの測定記録を比較して初めてSLOに照らせます。

## 次へ

コラム16で5つのステップを復習し、Step76〜80で監視、障害復旧、運用を発展的に学びます。
