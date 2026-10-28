# 学習順105：Windows Server 2025/IISの配布・切替・復旧

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L105DeploymentPage.razor)。画面の正規URLは `/learn/105` です。

**第8段階・第21組／学習順105・旧URL /learn/105**

## 到達目標

.NET 10 Blazor Interactive Server + MudBlazor + PostgreSQL 18を、社内ドメインのWindows Server 2025/IISへ安全に配置するための作業手順と、異常時の切戻し判断を作成します。設定と運用手順の章であり、CIによって実際のAD/IIS構成を変更することはありません。

## 配布の手順

1. 本番アカウント/サービスを決め、Windows Server 2025にIISと適切な.NET 10 Hosting Bundleを導入します。アプリプールのIDと権限を最小化し、サイトへ有効なHTTPS証明書を割り当てます。
2. アプリをRelease発行して新ディレクトリへ配置します。アプリプールはNo Managed Codeを使用します。アプリプロセスはASP.NET Core Moduleで起動されます。管理者がIIS認証設定（Windows認証有効/匿名認証）とASP.NET CoreのNegotiate・明示Cookieモードの構成の整合を検証します。
3. KerberosのSPN、サービスアカウント、ブラウザーイントラネットゾーン、DNS、HTTPS・HSTSを確認します。認証ダウングレードやNTLMフォールバックの有無を運用要件に応じて試験します。
4. 接続文字列・Data Protection鍵パスをサーバー設定へ隔離し、鍵のACL・保存時暗号化・バックアップ・ローテーションを受け入れます。アプリが個人のADパスワードを受け取ったり、ユーザー登録行を作ったりしないことを検証します。
5. 既存台帳ビューの正規化・一意性・FDW接続と、部署絞り込み・監査INSERT専用ロールを検証します。行ロック・原子性と監査永続性の検証が終わるまではAuthorizationの確認フラグを**false**のままにします。
6. /health/liveと業務の拒否動作を区別して確認し、SignalRの接続/再接続、ブラウザー負荷、Npgsqlプール上限、P95/P99、ログ・監査の整合を記録します。
7. 切替前に旧Releaseと復旧方法を固定します。データベースの破壊的マイグレーションは教材から実施しません。問題が生じた場合は接続停止→旧ビルドへ切戻し→セッション/鍵・監査の整合を確認します。

## 環境設定の例

~~~json
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
~~~

これらの確認フラグは値を書けば安全になるスイッチではありません。現地の権限・ロック・監査保存の**受入証跡**と一致する場合のみ、有資格の運用担当者が変更します。Windows認証はIIS/ASP.NET Core両側で正しく構成してください。

## 本番受入票

- サーバー名、サイト、配布SHA、証明書、SPNの検証結果と担当者。
- 認証・ロック・監査・権限剥奪・FDW停止時の失敗シナリオ。
- 10,001人以上の台帳検索時間と、実同時circuit数・DB物理接続・遅延値。
- 切戻しの実測時間と、監査の欠落がないことを確認した証拠。

## 確認問題と解答

**Q1. CIがWindowsでビルド成功ならIISも正常？** いいえ。IIS/AD/HTTPS/FDWは現地で試験します。

**Q2. 本番DBに教材DDLをそのまま適用してよい？** いいえ。CI用の架空テーブルと本番の既存台帳は分離します。

**Q3. フラグをtrueにするだけで本番受入完了？** いいえ。運用担当者の実機証跡が必要です。

## 次へ

コラム17で競合・キャッシュ・監視・試験・配布の境界を再確認し、最初のステップから実装理由を振り返ります。


## 実装・運用の詳しい検証

この教材はWinForms＋Dapper経験者が、Blazor Interactive Serverのサーバー側処理と既存業務DBの境界を理解するためのものです。ADは本人を認証し信頼済みのドメイン付きログイン名を取得します。業務の閲覧・編集、部署・行単位の認可は既存DB台帳を操作ごとに照合します。Cookieやcircuitの保持値、画面に表示されたボタンだけでは業務権限を証明できません。

### 実コード例1：画面の入口

```csharp
var windowsAuthEnabled = builder.Configuration.GetValue<bool>("WindowsAuthentication:Enabled");
var explicitLoginEnabled = builder.Configuration.GetValue<bool>("WindowsAuthentication:ExplicitLoginEnabled");
if (explicitLoginEnabled && !windowsAuthEnabled) throw new InvalidOperationException("明示ログインにはWindows認証の有効化が必要です。");
```


この抜粋は、実際の呼出し経路を追うためのものです。画面で実行するC#は業務サービスを呼ぶ入口にすぎません。認可を通ったように見えても、対象行の部署、現在の権限、監査書込みを確認できなければ結果を公開しません。特に非同期操作をキャンセルした場合やDBから例外が返った場合、古い許可に戻して成功にする処理を追加しないでください。

### 実コード例2：安全な境界の確認

```csharp
// 初期の受入前フラグはfalseを維持する。
"Authorization": {
  "TransactionalLedgerConfirmed": false,
  "DurableAuditConfirmed": false
}
```

### 実装と判断の詳しい説明

Windows Server 2025でIISと.NET 10 Hosting Bundle、No Managed Codeアプリプール、HTTPS証明書、SPN・Kerberosを検証します。明示Cookieモードでは短命CookieとData Protection鍵のACL・暗号化・バックアップ・ノード共有が必要です。接続文字列と秘密はリポジトリに保存しません。既存DB/FDWの行ロックと監査原子性を受け入れるまでは確認フラグはfalseです。/health/liveの成功と業務成功を区別し、登録10,001人と同時circuit・DB接続数は別に測ります。切戻しは旧ビルドだけでなく監査・台帳・鍵の整合を確認します。

### WinForms＋Dapperとの比較

| 観点 | WinForms＋Dapper | Blazor Interactive Server |
| --- | --- | --- |
| 操作イベント | 各端末のFormで受け付ける | サーバーのRazorとサービスで処理する |
| 権限の正本 | Dapperで既存DBへ毎回照会する | SecuredLearningServiceで操作・行単位を都度判定する |
| 表示状態 | フォーム内の値は古くなり得る | circuitの状態も権限の代わりにならない |
| 監査 | SQLの実行結果と本人・操作を記録 | 永続監査に失敗する場合は業務を安全に拒否 |
| 本番受入 | Windows端末とDB接続を確認 | IIS、Kerberos、SignalR、DBプールと鍵の管理を別途確認 |

### 操作手順

1. 新学習順の目次から旧URLで開く画面を表示し、前後リンクを確認します。
2. 匿名の開発用ブラウザーで業務の確認ボタンを押し、本人とDB台帳の権限が確かめられないときに拒否されることを確認します。
3. 肯定試験はCI専用の架空PrincipalとPostgreSQL18の架空テーブルだけで行います。本番台帳のレコードや資格情報を教材に投入しないでください。
4. 操作前後の期待結果、実際の画面表示、相関ID、実行日時、コミットSHAを記録します。再操作時もDB正本を確認します。
5. 実AD/IIS/Kerberos/FDWおよび1万人超負荷は、CIとは別に現地で受入票を作ります。

### 障害の切り分け

| 現象 | 最初に確かめる内容 | 合格条件 |
| --- | --- | --- |
| 匿名・台帳未登録・重複 | 認証ハンドラー、ドメイン付き名、照合の0/1/2件 | データを返さず拒否 |
| 権限変更・部署違い | 正本の有効状態、can_read/can_edit、部署 | 過去の許可を再利用しない |
| 競合・行ロック不可 | xmin、トランザクション、FDW条件 | 古いデータを自動上書きしない |
| 監査INSERT・DB障害 | 監査権限、接続、相関ID、ロールバック | 機密の行やCSVを公開しない |
| CI成功・実機未検証 | IIS、SPN、Kerberos、FDW、復旧と負荷 | 現地受入は未完了のままにする |

利用者へSQLエラー、認証Cookie、接続文字列、他部署のデータを表示しません。運用ログは利用者画面と切り分けて保護します。権限失効は再ログインだけで直す問題と決めつけず、現在の正本を確かめます。成功、拒否、競合、通信障害を別の結果として記録してください。

### 演習

切替後、監査INSERTに失敗しSignalRが再接続を繰り返した。何を確認して切戻すか。

### 詳しい解答

新規操作を止めてSHAと障害ログを採取し、旧ビルド・設定へ切戻します。DB/監査・鍵・認証の整合と欠落記録、復旧時間を現地で確認するまで合格にしません。

### 現場での確認票

WinForms＋Dapperから移行しても認可の根拠は変わりません。本人を確かめたことと、その本人がどの行へどの操作をできるかを分け、監査が不能な場合は安全に拒否します。設定フラグだけで行ロックや監査の原子性が保証されるわけではありません。実AD、IIS、Kerberos、FDW、鍵の管理、1万人超登録アカウントと同時circuit、DBプールの負荷、復旧手順は現地実機で受入します。CI成功の意味を超えて運用を承認しないでください。
