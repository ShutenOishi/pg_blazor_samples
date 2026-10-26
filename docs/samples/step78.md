# ステップ78：ヘルスチェック・構造化ログ・運用監視

## 到達目標

Windows Server 2025のIIS上で.NET 10のWebアプリの**プロセス稼働**と、PostgreSQL 18・既存台帳・永続監査の**業務可用性**を区別し、機微情報を出さない監視の入口を学びます。

## 実装と確認方法

Program.csのAddHealthChecksとMapHealthChecksにより、HTTP GET /health/live を公開します。200はアプリのlivenessを示すだけで、DB接続・Windows認証・権限・FDW・監査成功を**保証しません**。CIはこのURLをHTTPで検査します。

~~~
builder.Services.AddHealthChecks();
// ルーティングの登録はUseAuthorizationの後でも可能
app.MapHealthChecks("/health/live").AllowAnonymous();
~~~

一方、Step78の画面の「業務利用の現在の認可を確認」ボタンでは、既存の業務サービスを通して毎回Windows認証済み本人・既存台帳・監査の設定が成立するかを検証します。匿名であれば結果を拒否します。

本番でreadinessを追加する場合は、監視アカウント・接続タイムアウト・負荷制御・非公開のアクセス経路を別設計します。DBを絶えず高頻度で叩くlivenessにしたり、業務データを診断応答へ出力したりしません。

## 見るべき観測量

- HTTP要求の応答時間・失敗率と、SignalR接続・切断・再接続。
- Npgsqlプールの物理接続数・待機時間、PostgreSQLのロック待機とFDW遅延。
- 既存台帳照合失敗、権限拒否、監査INSERT失敗の件数。
- 重要な業務操作の相関ID・操作コード・結果コード・UTC。パスワード・Cookie・CSV本文・DB接続パスワードは**ログに書かない**。

## 否定試験

監査DB停止中でもプロセスが生きていれば/health/liveは成功する場合があります。だからこそ、監視の成功だけでビジネスの操作成功を主張せず、認可・監査障害は業務をフェイルクローズさせます。livenessはプロキシ/IIS側で監視ネットワークに限定してください。

## 確認問題と解答

**Q1. /health/liveの200でログインも正しい？** いいえ。稼働確認と本人認証は無関係です。

**Q2. application_nameを利用者名にすると監査不要？** いいえ。プール接続の識別であり永続的な操作証跡ではありません。

**Q3. 認証失敗の詳細を匿名HTTPへ返してよい？** いいえ。原因の詳細は制限付きログへ残します。

## 次へ

Step79では実DB・偽Principal・Chrome・実IISを分けた試験の受入を設計します。
