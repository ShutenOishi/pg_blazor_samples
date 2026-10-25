# pg_blazor_samples

**Blazorを初めて学ぶ人から、業務システムを設計・運用する開発者までを対象とした、日本語の段階式サンプル集です。**

Visual Studio 2026、.NET 10、Blazor Web App（Interactive Server）、MudBlazor、linq2db、Npgsql、PostgreSQL 18を使用します。認証はActive Directory（Windows統合認証）、アプリケーション利用者と権限はPostgreSQLで管理する構成を目指します。

> **現在の状態：企画・設計・学習計画を整備した段階です。** サンプルの実装、ビルド、AD接続、PostgreSQL接続はまだ実証していません。実装状況は[サンプル一覧](docs/sample-catalog.md)で更新します。

## どこから始める？

| あなたの状況 | 読む場所 |
| --- | --- |
| Blazorが初めて | [学習ロードマップ](docs/learning-path.md) → ステップ01 |
| C#とWeb開発は経験済み | [サンプル一覧](docs/sample-catalog.md)からテーマを選ぶ |
| 社内システムの設計を学びたい | [構成設計](docs/architecture.md)と[認証・認可](docs/authentication.md) |
| 開発環境を準備したい | [開発環境](docs/environment.md) |
| サンプルを追加・改善したい | [開発・レビュー規約](docs/conventions.md)と[CONTRIBUTING.md](CONTRIBUTING.md) |

## 技術スタック

| 項目 | 基準 |
| --- | --- |
| IDE | Visual Studio 2026 |
| 言語 | C# 14 |
| ランタイム | .NET 10（`net10.0`） |
| UI | Blazor Web App / Interactive Server / MudBlazor |
| DB | PostgreSQL 18 / linq2db / Npgsql |
| 認証 | Windows統合認証（Negotiate。IIS利用時はWindows認証も検証） |
| 認可 | PostgreSQLの利用者・ロール・操作権限 |
| 運用言語 | 日本語のみ（画面、説明、注釈、検証観点） |

**NuGetの具体的な安定版は[バージョン管理方針](docs/versions.md)を参照してください。** 将来の最新版採用と、教材の再現性を両立するため、リポジトリの依存関係は検証済みのバージョンに固定します。

## 全体像

1. **基礎（01～10）**：Razor、画面遷移、状態、フォーム。
2. **UI（11～20）**：MudBlazor、表、ダイアログ、検証、アクセシビリティ。
3. **設計（21～30）**：DI、非同期、状態管理、例外、単体テスト。
4. **DB（31～40）**：PostgreSQLとlinq2db、検索、CRUD、トランザクション。
5. **認証・認可（41～50）**：AD、SID照合、業務ロール、監査。
6. **応用・運用（51～60）**：大量データ、競合、テスト、配布、監視。

各ステップは「前提 → 作るもの → 動かし方 → 仕組み → 演習 → 確認方法」で統一し、1ステップずつ動作可能な状態で追加します。設計上は単一の主力Webアプリに機能別教材を追加し、必要になるまでプロジェクトを増やしません。

## ドキュメント

[日本語ドキュメント目次](docs/README.md)から、ロードマップ、構築方法、アーキテクチャ、AD、DB、テスト、貢献方法、実装計画へ移動できます。

## 重要事項

- ADとPostgreSQLには実運用の資格情報をコミットしません。パスワード・接続文字列の取り扱いは[SECURITY.md](SECURITY.md)を参照してください。
- ADがない人向けの単体学習と、実環境でのWindows認証の成功を混同しません。認証を偽装して「AD接続成功」とは記載しません。
- Windows統合認証で本人を確定してから、PostgreSQLの利用者テーブルで有効状態・権限を確認します。**認証成功と業務機能の利用許可は別です。**
- 教材は本番導入の完成品ではありません。実装後に必須テストと安全レビューを経て成果を記録します。

## 参加方法

[CONTRIBUTING.md](CONTRIBUTING.md)をご確認ください。提案や誤記修正も歓迎します。
