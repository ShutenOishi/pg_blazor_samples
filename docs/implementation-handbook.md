# 実装対応ハンドブック — 設計とソースの整合性

> 対象: Visual Studio 2026 / .NET 10 / Blazor Interactive Server / MudBlazor / linq2db / Npgsql / PostgreSQL 18 / Active Directory / Windows Server 2025。
>
> **2026-10-26 現在のリポジトリには `src/`、`tests/`、`.csproj`、`.razor`、`.cs` が存在しません。** 現在の説明は実装計画であり、動作検証済みの実アプリを表すものではありません。PDFには生成対象のGitコミットとソースファイル検出結果を自動掲載します。

## 1. 資料を読む順番と達成目標

| 段階 | Step | 新たなプロジェクト | 動作が成立する条件 |
| --- | --- | --- | --- |
| Blazor基礎 | 01–10 | Webのみ | .NET 10 / ブラウザー |
| DI基礎 | 11–20 | 18: UnitTests、19: Application | DIの理解・単体テスト |
| MudBlazor | 21–30 | WebとApplicationを活用 | UI検証・操作性確認 |
| PostgreSQL | 31–40 | 31: Domain、33: Infrastructure、40: IntegrationTests | PostgreSQL 18の独立DB |
| AD・認可 | 41–50 | 既存プロジェクトを拡張 | イントラネットのAD検証環境 |
| 運用 | 51–60 | 必要に応じWebTests | Windows Server 2025、IIS |

**60ステップの具体的な課題は [サンプル一覧](sample-catalog.md) が正本**です。この文書では、教材を実装するときに守る技術契約と受入条件を補足します。

## 2. プロジェクトと参照方向

最初のStep01では `PgBlazorSamples.Web` だけを用意し、`Program.cs`、`Components/`、`.razor` の読み方を学びます。インターフェイスとDI登録をWeb内で体験したあとで、Step19でApplicationを独立させます。DBを扱う時点で初めてDomainとInfrastructureを作ります。

```text
PgBlazorSamples.Web
  ├── PgBlazorSamples.Application ──> PgBlazorSamples.Domain
  └── PgBlazorSamples.Infrastructure
              ├── PgBlazorSamples.Application
              └── PgBlazorSamples.Domain
```

- Web：画面・ルーティング・MudBlazor・起動時のDI構成
- Application：ユースケース、サービス契約、操作権限のチェック
- Domain：業務規則、エンティティ、値オブジェクト
- Infrastructure：linq2db、Npgsql、PostgreSQLアクセス、認証情報との対応
- UnitTests：外部環境のない単体テスト
- IntegrationTests：隔離したPostgreSQL 18を用いた結合テスト
- WebTests：Windows認証、IIS、画面、接続切断の受入試験

**ApplicationからInfrastructureを参照しません。** サーバー上のユーザーごとの可変状態やDB接続をSingletonで共有しません。

## 3. DIを初めて学ぶ人の必修項目

DIは「必要な部品を自分で生成せず、外側で組み立てて渡してもらうこと」です。まず時計の実装を交換し、差し替え可能なテストから学びます。

```csharp
// 説明用の設計例。まだこのコードはリポジトリに実装していません。
public interface IClock
{
    DateTimeOffset GetNow();
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset GetNow() => DateTimeOffset.Now;
}

public sealed class GreetingService(IClock clock)
{
    public string GetMessage() =>
        clock.GetNow().Hour < 12 ? "おはようございます" : "こんにちは";
}
```

実装段階で `builder.Services.AddSingleton<IClock, SystemClock>()` と `@inject` を試し、固定時刻を返す偽時計に交換して結果を比較します。

| ライフタイム | 生存期間 | Blazor Interactive Serverでの注意 |
| --- | --- | --- |
| Singleton | アプリケーション全体 | 利用者ごとの状態・認証主体を保持しない |
| Scoped | スコープ内 | HTTP要求ではなくcircuitに長く関連し得る |
| Transient | 解決ごと | 短命な軽量サービス向け |

誤解しやすい点：**Interactive ServerではScopedの寿命が通常のHTTP要求とは異なります。** DB接続やlinq2dbの変更可能なコンテキストは、原則として操作単位で生成・破棄します。既存ガイド：[DI入門](di-for-beginners.md)。

## 4. PostgreSQL 18との役割分担

- ユーザー本人の認証：Active Directory / Windows統合認証
- アプリケーション内の利用可否・所属・ロール・権限：PostgreSQL 18
- データアクセス：Npgsql + linq2db
- AD側パスワード・チケット・NTLMハッシュ：**PostgreSQLに格納しない**

初期テーブル案：[データベース設計](database.md)。中心は `app_user`、`app_role`、`app_permission`、`app_user_role`、`app_role_permission`、`audit_event` です。Windows側の検証済みSIDを照合の主キーとして扱い、アカウント名は表示・補助情報に限定します。

業務データを読み書きする際には、認可をUIだけで完結させません。Applicationのユースケース入口でも操作権限を確認します。ユーザー未登録、無効、権限なし、DB照合失敗では**既定拒否**です。

## 5. AD・IIS・Windows Server 2025の構築観点

標準配備先はドメインに参加した **Windows Server 2025メンバーサーバー + IIS + .NET 10 Hosting Bundle** です。Visual Studioを本番サーバーへインストールすることは想定しません。

```text
社内Windows端末 + Edge
        │ HTTPS / Windows統合認証
        ▼
内部DNS → Windows Server 2025 / IIS
                         │
                         ├── ASP.NET Core / Blazor Interactive Server
                         ├── AD認証済み主体を取得
                         ├── Applicationの認可確認
                         └── PostgreSQL 18へTLS接続
```

構築と受入の具体的手順：[Windows Server 2025とIIS](windows-server-2025-iis.md)。

配備前に確認する項目：

1. IISのWebサーバー、Windows認証、WebSocket Protocolを導入。
2. .NET 10 Hosting Bundleを導入し、No Managed Codeのアプリプールを設定。
3. 内部DNS、社内CAのHTTPS証明書、証明書の更新運用を設定。
4. 業務サイトでは匿名認証を無効化し、Windows認証を有効化。
5. HTTP SPN、IISの実行ID、Kernel-mode認証設定を整合させ、KerberosとNTLMフォールバックを検証。
6. アプリプールIDは最小権限。gMSAが必要な場合だけ導入。
7. PostgreSQL接続には専用の最小権限IDを使い、スキーマ変更権限を与えない。
8. SignalR/WebSocketの再接続、IIS再起動、二重送信を検証。
9. アプリケーションログ、DB監査、DBバックアップと復元を検証。
10. 失敗経路の拒否、権限変更の反映、日本語表示とアクセシビリティを確認。

## 6. 実装ごとの受入基準

| 項目 | 正常系 | 否定系 |
| --- | --- | --- |
| Razor画面 | ページ遷移と操作 | 未入力・型変換エラー |
| DI | 差し替え可能 | Singletonに利用者状態が混入しない |
| MudBlazor | 入力・保存・通知 | 二重押下、検証不合格 |
| DB | パラメータ化されたCRUD | SQL注入、DB障害、トランザクション失敗 |
| Windows認証 | 有効なドメインユーザーで認証 | 未認証、SPN不整合 |
| DB認可 | 有効なユーザーの許可操作 | 未登録・無効・権限不足 |
| 運用 | HTTPSで接続、ログ取得 | IIS再起動、回線切断、DB停止 |
| 監査 | 操作主体・結果・時刻 | 機密情報をログへ書かない |

検証対象と結果の詳細は[品質管理](quality.md)に記録します。「ソースがある」と「ビルドできる」と「AD・IISで検証した」は別々の状態として扱います。

## 7. PDFとソースコードの同期ルール

このPDFは `docs/` と `README.md` を参照し、**PDF生成時のGitコミット**、利用したMarkdown、検出したソースファイルとSHA-256を自動的に付録へ含めます。 `src/`・`tests/`・`database/`に変更があればPDFを再生成します。

- コードが存在しない場合は**「未実装」**と明記する。
- コードがある場合は該当ファイルパス・内容ハッシュをPDFに載せる。
- 重要な実装解説は、実ファイル・型・メソッドの存在をレビューで照合する。
- ビルドや統合テストを通過していない機能を「検証済み」と表記しない。
- 将来コードと文章の乖離が判明した場合は、説明か実装を修正してからPDFを再生成する。

**ファイルとハッシュの自動照合は、説明文の意味まで機械的に保証するものではありません。** 実装とドキュメントの意味的な整合性はコードレビューとテストで担保します。

## 8. 仕様・参考情報への入口

- [60ステップの正本](sample-catalog.md)
- [学習ロードマップ](learning-path.md)
- [プロジェクトと依存関係](architecture.md)
- [DI入門](di-for-beginners.md)
- [DBテーブル設計](database.md)
- [AD認証と認可](authentication.md)
- [Windows Server 2025 / IIS](windows-server-2025-iis.md)
- [品質とCI](quality.md)
- [バージョン管理](versions.md)

この章のコード断片と構成図は**計画・説明用**です。実装ソースが追加された後は、出典のファイルパスを添えて追記します。
