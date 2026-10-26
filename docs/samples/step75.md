# ステップ75：IIS・Kerberos・負荷・障害の実機受入

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

## 次へ

コラム16で5つのステップを復習し、Step76〜80で監視、障害復旧、運用を発展的に学びます。
