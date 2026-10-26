# ステップ80：Windows Server 2025/IISの配布・切替・復旧

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
