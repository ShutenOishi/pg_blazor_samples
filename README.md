# pg_blazor_samples

**Blazorを初めて学ぶ人から、業務システムを設計・運用する開発者までを対象とした、日本語の段階式サンプル集です。**

Visual Studio 2026、.NET 10、Blazor Web App（Interactive Server）、MudBlazor、EF Core 10（Npgsqlプロバイダー）、PostgreSQL 18を使用します。認証はActive Directory（Windows統合認証）、アプリケーション利用者と権限はPostgreSQLで管理する構成を目指します。

> **現在の状態：Step01〜55の日本語Blazor画面・実装・教材を整備しました。** Step41〜50はMudTable/MudDataGridのServerData、JOIN・DTO、キーセット、仮想化、検索UXを実装しています。Windows/Linuxビルド、ChromeによるUI操作、PostgreSQL 18の実DB結合、PDF生成は[アプリCI](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build.yml)と[PDF生成CI](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build-docs-pdf.yml)で検証します。Active DirectoryとIISの実機認証検証はStep61以降の対象です。進捗は[サンプル一覧](docs/sample-catalog.md)を参照してください。

## 学習アプリを起動する

Visual Studio 2026で `PgBlazorSamples.slnx` を開き、`PgBlazorSamples.Web` を起動してください。コマンドラインでは次のとおりです。

```powershell
dotnet restore PgBlazorSamples.slnx
dotnet run --project src/PgBlazorSamples.Web
```

トップページから日本語の各ステップに移動できます。最初の20ステップはDB/ADなしで起動できる構成です。画面メニュー、再接続案内、エラー表示も日本語です。

実際のソースに対応する教材：[ステップ01](docs/samples/step01.md)／[ステップ02](docs/samples/step02.md)／[ステップ03](docs/samples/step03.md)／[ステップ04](docs/samples/step04.md)／[ステップ05](docs/samples/step05.md)／[ステップ06](docs/samples/step06.md)／[ステップ07](docs/samples/step07.md)／[ステップ08](docs/samples/step08.md)／[ステップ09](docs/samples/step09.md)／[ステップ10](docs/samples/step10.md)／[ステップ11](docs/samples/step11.md)／[ステップ12](docs/samples/step12.md)／[ステップ13](docs/samples/step13.md)／[ステップ14](docs/samples/step14.md)／[ステップ15](docs/samples/step15.md)／[ステップ16](docs/samples/step16.md)／[ステップ17](docs/samples/step17.md)／[ステップ18](docs/samples/step18.md)／[ステップ19](docs/samples/step19.md)／[ステップ20](docs/samples/step20.md)。ソースコード変更時は同じPRで教材を更新します。

## MudBlazor公式テンプレートと日本語ローカライズ

Step21〜25では、既存のBlazor Interactive ServerアプリへMudBlazor 9.11.0とMudBlazor.Translations 3.6.0を追加しました。公式テンプレートは別アプリを生成して仕組みを学び、既存のStep01〜20は残します。

```powershell
dotnet new install MudBlazor.Templates
dotnet new mudblazor -n MudBlazorPractice -o MudBlazorPractice -int Server -ai -e
```

翻訳には `builder.Services.AddMudTranslations()` と `ja-JP` 文化設定を使用します。翻訳パッケージが扱う共通UI文言と、教材側の日本語ラベル・入力検証を分離しています。[ステップ21の教材](docs/samples/step21.md)から進めてください。

## EF Core 10とMudBlazorのデータ連携（Step31〜60）

標準ORMを**EF Core 10**に統一し、Npgsql接続プールと`IDbContextFactory`の寿命管理、`DateOnly`/`TimeOnly`/`Range`、楽観的排他制御を必修とします。一覧表示については、[10種類の実務パターン比較と選択ガイド](docs/data-grid-patterns.md)、[EF Core 10とNpgsqlの基礎](docs/efcore10-data-access.md)から先に学べます。**Step31〜50には実DB用サンプルがあります。PostgreSQL 18の実接続テストはCI専用データベースで実施し、AD/IIS実機検証とは区別します。**

## ソースコード実装の次工程

Step01〜50の実装を土台として、次はStep56〜60の日付Range検索、利用者別キャッシュ、集計・CSV、性能測定、選定演習を追加します。EF CoreのためのDomain/Infrastructure等の層別プロジェクトは新設せず、Web内のDataとServicesを使います。

## 5ステップごとの学習コラム

Step05・10・15…80の節目ごとに、**通常16回と特別編2回、計18本**のコラムを用意します。現在は[コラム01：Razor・イベント・状態](docs/columns/column01-05.md)、[コラム02：親子コンポーネント・入力検証](docs/columns/column06-10.md)、[コラム03：DIとサービスの寿命](docs/columns/column11-15.md)、[コラム04：設定とApplication分離](docs/columns/column16-20.md)、[特別コラム05：ライブラリのDI](docs/columns/column-library-di.md)、[コラム06：MudBlazorのDIと日本語化](docs/columns/column21-25.md)、[コラム07：UIとデータ連携](docs/columns/column26-30.md)を掲載しています。コラム08〜11とMudBlazor用途別特別コラムも追加しました。後続のテーマは[全18本の一覧](docs/columns/README.md)を参照してください。

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
| UI | Blazor Web App / Interactive Server（Step01〜03で利用）／MudBlazor（Step21〜50のUIで活用） |
| DB | PostgreSQL 18 / EF Core 10（Npgsqlプロバイダー、Step31〜50で実装） |
| 認証 | Windows統合認証（Step61以降で導入予定） |
| 認可 | PostgreSQLの利用者・ロール・操作権限 |
| 運用言語 | 日本語のみ（画面、説明、注釈、検証観点） |

**NuGetの具体的な安定版は[バージョン管理方針](docs/versions.md)を参照してください。** 将来の最新版採用と、教材の再現性を両立するため、リポジトリの依存関係は検証済みのバージョンに固定します。

## 標準配備先と段階的なプロジェクト分割

本教材の最終的な配備先は**ドメイン参加済みWindows Server 2025上のIIS**です。アプリはBlazor Web App（Interactive Server）で動作し、Windows統合認証でAD利用者を特定した後、PostgreSQL 18の利用者・権限テーブルで認可します。

DIの学習用Applicationは維持しますが、**DBを学ぶための新しいDomain/Infrastructureプロジェクトは作りません。Web内のData・ServicesにEF Core 10を置きます。**

- [DIを初めて学ぶ人のためのガイド](docs/di-for-beginners.md)
- [Windows Server 2025 + IIS の構築計画](docs/windows-server-2025-iis.md)
- [Web内で完結するEF Coreの構成](docs/architecture.md)

## 全体像

1. **基礎（01～10）**：Razor、画面遷移、状態、フォーム。
2. **DIとサービス設計（11～20）**：インターフェイス、ライフタイム、非同期、テスト、Application分離。
3. **MudBlazor UI（21～30）**：表、ダイアログ、検証、アクセシビリティ。
4. **DB（31～40）**：PostgreSQLとEF Core 10、型変換、接続寿命、検索、CRUD、トランザクション。
5. **一覧・データ連携（41～60）**：Items、ServerData、仮想化、ページング、集約、編集・競合。
6. **認証・認可（61～70）**：AD、SID照合、業務ロール、監査。
7. **応用・運用（71～80）**：大量データ、競合、テスト、配布、監視。

各ステップは「前提 → 作るもの → 動かし方 → 仕組み → 演習 → 確認方法」で統一し、1ステップずつ動作可能な状態で追加します。設計上は単一の主力Webアプリに機能別教材を追加し、必要になるまでプロジェクトを増やしません。

## 学習者向けのPDFで読む

学習者向けの資料は **「Blazor 実践学習テキスト」** に統一しました。目次・見出しは日本語の学習テーマで表示し、開発管理用のレポジトリ情報・CI・PR・ハッシュなどはPDFへ載せません。ステップ01〜55・コラム01〜12・特別コラム2本を収録し、GitHub Actionsで生成・検査します。

[日本語学習PDFの生成ワークフロー](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build-docs-pdf.yml)の最新成功Runから、`blazor_learning_textbook_ja` を取得できます。コードとの照合情報は同一生成処理の別JSONへ出力し、学習本文に混ぜません。詳しくは[PDFの作成方法](docs/pdf-generation.md)をご覧ください。

## ドキュメント

新しいORM採用と学習構成の判断は[ADR-0002](docs/decisions/0002-efcore10-grid-patterns.md)で説明しています。

[日本語ドキュメント目次](docs/README.md) / [実装対応ハンドブック](docs/implementation-handbook.md)から、ロードマップ、構築方法、アーキテクチャ、AD、DB、テスト、貢献方法、実装計画へ移動できます。

## 重要事項

- ADとPostgreSQLには実運用の資格情報をコミットしません。パスワード・接続文字列の取り扱いは[SECURITY.md](SECURITY.md)を参照してください。
- ADがない人向けの単体学習と、実環境でのWindows認証の成功を混同しません。認証を偽装して「AD接続成功」とは記載しません。
- Windows統合認証で本人を確定してから、PostgreSQLの利用者テーブルで有効状態・権限を確認します。**認証成功と業務機能の利用許可は別です。**
- 教材は本番導入の完成品ではありません。実装後に必須テストと安全レビューを経て成果を記録します。

## 参加方法

[CONTRIBUTING.md](CONTRIBUTING.md)をご確認ください。提案や誤記修正も歓迎します。

## Step40と41の間の特別コラム

[用途から選ぶMudBlazorコンポーネント事典](docs/columns/column-mudblazor-by-purpose.md)で、MudSimpleTable/MudTable/MudDataGrid、日付・入力、階層、ダイアログ、通知、配置を比較します。Step41の前に読むことを推奨します。

## Step41〜45の日本語UIと実DBページング

Step41〜45の新しいソースはWeb内のData/とServices/のみで完結。MudTable/MudDataGridのServerData、CancellationToken、ソート許可リスト、日付と状態の検索を導入し、[コラム10](docs/columns/column41-45.md)で復習します。Step46以降は仮想化やキーセットページングを実装予定です。

## Step51〜55：編集・一括操作・競合・詳細

Web内のLearningEditServiceで、入力DTOとDbContextFactoryによる短命DbContext、MudDialog編集、最大50件の原子的な一括処理、xmin競合の通知、詳細DTOのJOIN射影を実装しました。[コラム12](docs/columns/column51-55.md)で5方式を復習できます。安全な業務認可はStep61〜70の対象であり、本サンプルは開発用DB限定です。
