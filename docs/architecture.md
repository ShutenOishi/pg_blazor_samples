# アーキテクチャ方針と段階的なプロジェクト分割

## 方針

**単一のBlazor Webアプリを中心に、必要なタイミングでクラスライブラリへ責務を切り出す方式**を採用します。80個の独立アプリを複製しません。ただし学習用のコードと本番相当の業務機能は名前空間・ルーティングと権限で分離します。

DIを初めて学ぶ人にも適した順序にします。最初から4層構成を暗記させず、部品の入れ替えやテストを体験してから層を切り出します。

## ステップごとのプロジェクト導入

| 導入時点 | プロジェクト | 追加する理由 |
| --- | --- | --- |
| Step01 | `PgBlazorSamples.Web` | Razor、画面、イベントから始める |
| Step11〜12 | Web内の`Services/` | DIとインターフェイスを最小構成で体験する |
| Step18 | `PgBlazorSamples.UnitTests` | Fake実装を使い、画面なしのテストを学ぶ |
| Step19 | `PgBlazorSamples.Application` | 業務処理と抽象インターフェイスを画面から分離 |
| Step31 | `PgBlazorSamples.Domain` | Entity、値オブジェクト、業務ルールを隔離 |
| Step33 | `PgBlazorSamples.Infrastructure` | EF Core 10/Npgsqlなど外部環境の具象実装を隔離 |
| Step40 | `PgBlazorSamples.IntegrationTests` | PostgreSQL 18を使った再現可能な結合試験 |
| Step70以降 | `PgBlazorSamples.WebTests` | 認証、IIS、Web画面の総合検証 |

## 完成形のディレクトリ案

```text
PgBlazorSamples.slnx
src/
  PgBlazorSamples.Web/
    Components/Layout/
    Components/Samples/Step01/ ... Step80/
    Features/Users/
    Program.cs
  PgBlazorSamples.Application/
    Abstractions/
    Users/
  PgBlazorSamples.Domain/
    Users/
    Roles/
  PgBlazorSamples.Infrastructure/
    Persistence/
    Identity/
tests/
  PgBlazorSamples.UnitTests/
  PgBlazorSamples.IntegrationTests/
  PgBlazorSamples.WebTests/
database/
  schema/
  migrations/
  seed/
docs/
  samples/
```

この図は**完成時の予定構成**です。現在は `PgBlazorSamples.Web`、`PgBlazorSamples.Application`、`PgBlazorSamples.UnitTests` を実装済みです。Domain・Infrastructure、DB接続、AD認証は未実装です。

## 依存関係の約束

```text
Web ────────────────> Application ───────────────> Domain
 |                         ^                          ^
 |                         |                          |
 +──> Infrastructure ──────+──────────────────────────+
           |
     EF Core 10 / Npgsql
           |
     PostgreSQL 18
```

- WebがDIを登録する起動地点（Composition Root）。MudBlazor・Razor・IIS/ASP.NET Coreとの接点も担当。
- ApplicationはユースケースやRepositoryインターフェイスを置く。Infrastructureの実装に直接依存しない。
- Domainは業務ルール・値・不変条件を持ち、MudBlazorやNpgsqlに依存しない。
- InfrastructureがApplicationのRepositoryインターフェイスを実装し、DB/外部基盤を扱う。
- WebからInfrastructureの参照は**DI構成のためのみ**。ページ内から具象Repositoryを直接利用しない。
- DTOと画面モデルは目的に応じて置き、理由のない共有プロジェクトを増やさない。

## Blazor Interactive Server固有の注意点

- DIのScopedサービスは通常のHTTPリクエスト単位ではなく、対話型の**circuit**に紐づいて生存し得る。
- ユーザー固有情報をSingletonへ格納しない。
- NpgsqlDataSourceの安全な共有と、操作ごとに生成・破棄する接続/EF Core DbContextを区別する。
- 標準は`IDbContextFactory<AppDbContext>`による**操作ごとのDbContext生成・破棄**。DbContextをcircuitに長期間共有しない。
- Npgsqlの物理接続プールとEF CoreのDbContextプールは別物。初学者向けには前者のみを標準とする。
- MudDataGridはサービス経由でDTOを取得する。全件`Items`、`ServerData`、`VirtualizeServerData`の選択根拠は[一覧パターン](data-grid-patterns.md)で説明する。
- 同時イベント、切断・再接続、二重送信、描画時の二重初期化を教材に含める。
- 画面のAuthorizeViewは表示制御であり、業務サービスの認可の代用ではない。
- AD Windows統合認証はIISで実施。PostgreSQL側の利用者・権限確認はサーバー側で毎回適切に行う。

## 標準配備先

イントラネット、ADドメイン参加済み**Windows Server 2025 + IIS + .NET 10 Hosting Bundle**。TLS、Windows認証、内部DNS、SPN、Kerberos、SignalR/WebSocket、アプリプールID（必要に応じgMSA）とPostgreSQL 18接続を扱います。詳しくは[Windows Server 2025 + IIS](windows-server-2025-iis.md)。

## さらに読む

- [DI入門](di-for-beginners.md)
- [サンプル80ステップ](sample-catalog.md)
- [ADR-0001](decisions/0001-learning-app-structure.md)
