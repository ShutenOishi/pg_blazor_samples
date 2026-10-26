# Windows Server 2025 + IIS + AD の構築計画

## 対象環境

- サーバーOS: **Windows Server 2025**（ドメイン参加済みのメンバーサーバー）
- Web: **IIS** + **.NET 10 ASP.NET Core Hosting Bundle**
- アプリ: **Blazor Web App / Interactive Server**。サーバー側の処理をSignalR経由で実行
- 認証: **IIS Windows認証**（Negotiateを使用しKerberosを優先、NTLMへの意図しないフォールバックを検査）
- 認可: PostgreSQL 18 の既存テーブルまたは外部テーブルの利用者・操作権限
- DB: PostgreSQL 18（アプリサーバーと分離可能）。EF Core 10（Npgsqlプロバイダー）で接続
- 利用端末: 社内管理のWindowsドメイン参加端末 + 統合認証を使えるEdge等
- 接続: 内部DNSのFQDN、信頼された社内CA等によるHTTPS証明書
- 時刻: ドメイン時刻同期を確認する。日付・時刻の画面表示は日本時間、DBの監査時刻はUTCを基準とする

**開発端末のVisual Studio 2026** と **サーバー上の実行環境** を区別します。運用サーバーにVisual StudioやSDKは原則不要です。IISに適したHosting Bundleを入れます。

## システム構成（目標）

```text
ドメイン参加Windows端末 / ブラウザー
    |
    | HTTPS / Windows統合認証（Negotiate/Kerberos）
    v
社内DNS（例: blazor.intra.example.invalid）
    |
    v
Windows Server 2025（ドメイン参加）
    IIS + Windows認証 + ASP.NET Core Module + .NET 10 Hosting Bundle
    |
    +-- Blazor Web App（Interactive Server + SignalR）
    |       |
    |       +-- アプリケーションサービス
    |       +-- Windows認証済みユーザー名/既存利用者台帳の有効性/権限の照合
    |
    +---- TCP/TLS ----> PostgreSQL 18
    |
Active Directory ドメインコントローラー（認証基盤）
```

AD認証の本人情報はIIS/ASP.NET Coreで取得し、DBにはADパスワードを複製しません。DBへの接続は原則としてWebアプリ用の最小権限の接続IDで行い、利用者本人のWindows認証トークンでDBへ自動的に接続するものとは考えません。

## 構築前に管理者が確認すること

1. サーバーのドメイン参加、ホスト名/FQDN、DNS解決、時刻同期、DNS/ADへの通信。
2. 検証用OU/ユーザー、管理権限、IISインストール権限、社内CA証明書の発行方法。
3. IISアプリプールの実行アカウント。まず専用アプリプールIDを基本とし、AD資源アクセスなどに必要なら**gMSA**を検討。
4. ネットワーク分離、HTTPS、PostgreSQLへの到達性、ファイアウォールと監査対象。
5. 利用者の総数と最大同時接続数を区別する。Interactive Serverは回線状態を保持するため、1万人規模の負荷試験と段階的なスケールアウトが必要。
6. PostgreSQLのバックアップ・復旧・更新手順、Webアプリのログ収集先と保管期間。

## 構築手順（設計時点のチェックリスト）

### 1. IISの役割機能を有効にする

- [ ] Web Server (IIS)を追加する。
- [ ] **Windows Authentication** を役割サービスで有効化する。
- [ ] **WebSocket Protocol** を有効化する（Interactive Serverに推奨）。
- [ ] IIS管理コンソールおよびサーバーの適切なWindows更新状態を確認する。

### 2. ASP.NET Coreをホストできる状態にする

- [ ] .NET 10用 **ASP.NET Core Hosting Bundle** をサーバーへ導入する。
- [ ] IIS導入前にHosting Bundleをインストールした場合は修復する。
- [ ] IISのアプリプールを**No Managed Code**で構成する（.NET CLR選択とASP.NET Coreの実行は別）。
- [ ] 実行IDに配置フォルダーの読み取り権限、ログ格納先の必要最小限の書込み権限を設定する。
- [ ] Visual Studioから検証済みの成果物を `dotnet publish -c Release` 等で作成する。

### 3. Windows統合認証の設定

- [ ] IISサイトで**匿名認証を無効**、**Windows認証を有効**にする（業務サイト）。
- [ ] 本番業務画面で匿名/テスト用認証経路がないことを検査する。
- [ ] サイトFQDNのHTTP SPN、実行アカウント/マシンアカウント、IISのKernel-mode設定などの整合性を環境に合わせて確認する。
- [ ] Edgeの統合認証動作、イントラネットゾーン/ポリシー、Kerberos使用可否を実機で確認する。
- [ ] リバースプロキシを挟む場合は、Windows認証の終端位置・安全な主体情報引き渡しを別途設計する。任意のリクエストヘッダーを身元情報として信じない。

### 4. HTTPS / Blazor回線

- [ ] 社内CA等が発行した証明書をIISのHTTPSバインドへ設定する。
- [ ] TLS設定、証明書更新手順、秘密鍵アクセス権を確認する。
- [ ] ブラウザーとIIS間のWebSocket/SignalRの接続、再接続、切断、アプリプール再起動時の挙動を確認する。
- [ ] 複数サーバー運用ではセッション接続方式、スケールアウト、Data Protectionキーや認証状態への影響を別途設計する。
- [ ] 変更後のアプリプール再起動による回線切断を業務仕様に反映する。

### 5. DB接続と権限

- [ ] PostgreSQL 18を開発・検証・本番で分離する。
- [ ] DB専用ロールを用い、通常のWebアプリ接続IDへDDLやSUPERUSER権限を与えない。
- [ ] 接続文字列・秘密はリポジトリに保存せず、サーバーの保護された構成から受け取る。
- [ ] DB接続でTLSを使用し、証明書検証ポリシーを定める。
- [ ] DB接続障害、権限不足、失効したユーザー、DB停止時は**許可せず拒否**する。

## 受入試験

| ケース | 期待する動作 |
| --- | --- |
| ドメインの認証済み利用者＋DB権限あり | 許可された画面・操作のみ成功 |
| AD認証失敗 | 認証失敗。業務データを返さない |
| AD認証成功・DB利用者未登録 | アプリ使用拒否 |
| AD認証成功・DBで利用者無効 | アプリ使用拒否 |
| DB側で権限剥奪 | 次回の保護対象操作から拒否 |
| DBサーバー停止 | 許可しない。運用ログへ記録 |
| HTTP SPNの設定誤り | Kerberos利用可否を調査し、意図しないNTLM移行を検知 |
| WebSocket切断/IIS再起動 | 利用者に再接続方法を提示、二重保存を防止 |
| TLS証明書期限切れ | 監視で検知し更新できる |
| 日本語入力・長文・キーボードのみの操作 | 表示と入力に問題がない |

この文書は**運用導入設計**です。実機確認したチェック項目だけを「検証済み」に更新します。実在するADドメイン名、サーバー名、SPN、証明書、利用者情報、実際の秘密は記載しません。

## 公式参考資料

- [IISでASP.NET Coreをホストする](https://learn.microsoft.com/ja-jp/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0)
- [ASP.NET CoreのWindows認証](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/windowsauth?view=aspnetcore-10.0)
- [Blazor Web AppのWindows認証](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/blazor-web-app-with-windows-authentication?view=aspnetcore-10.0)
- [Windows Server 2025でのgMSA管理](https://learn.microsoft.com/ja-jp/windows-server/identity/ad-ds/manage/group-managed-service-accounts/group-managed-service-accounts/manage-group-managed-service-accounts)

## Step75：実機受入チェック

IIS + Windows認証の通常経路・明示Cookie経路を別々に検証し、SPN・Kerberos・HTTPS、Cookie鍵共有・失効・SignalR再接続、台帳FDW切断・遅延、権限行ロック、監査INSERT失敗で業務更新を中止できることを受け入れます。10,001人の登録件数を同時接続人数やDBプール接続数と混同しません。**Actionsの偽PrincipalやPostgreSQL18 CI成功は実機受入の代用ではありません。**
