# 開発環境と実行の前提

## 1. 必要なソフトウェア

- Windows、Visual Studio 2026（ASP.NETとWeb開発ワークロード）、.NET 10 SDK。
- PostgreSQL 18（31以降のDB連携教材）。
- Active Directoryの検証用ドメイン、Windows認証を利用できるブラウザー・開発端末（41以降）。
- PostgreSQL管理用のツールは任意。SQLを実行できる`psql`だけでもよい。

## 1.1 本番相当のサーバー環境

**Windows Server 2025（ADドメインに参加したメンバーサーバー）+ IIS + .NET 10 Hosting Bundle**を標準ターゲットとします。Webサーバー上へVisual Studioをインストールする必要はありません。内部DNS、HTTPS、Windows認証、Kerberos/SPN、WebSocket、PostgreSQL 18の疎通を専用の検証環境で確認します。詳細は[Windows Server 2025 + IIS構築計画](windows-server-2025-iis.md)。

## 2. 開発者が最初に確認すること

```powershell
dotnet --info
dotnet --list-sdks
```

出力から.NET 10 SDKを確認します。Visual Studioのターゲットに`.NET 10`が選べることを確認してください。

## 3. 実際のソリューションを開いて起動する

Step01〜03のソースが実装されているため、**新たに `dotnet new` を実行する必要はありません**。リポジトリにある `PgBlazorSamples.slnx` をVisual Studio 2026で開いてください。

```powershell
dotnet restore PgBlazorSamples.slnx
dotnet build PgBlazorSamples.slnx -c Release
dotnet run --project src/PgBlazorSamples.Web
```

表示されたアドレスにアクセスし、目次から `/learn/01`、`/learn/02`、`/learn/03` に移動します。対応コードは `src/PgBlazorSamples.Web/Components/Samples/` にあります。実装の説明は[ステップ01](course-v3/lessons/Stage02/L006FirstBlazorPage.md)、[ステップ02](course-v3/lessons/Stage02/L007NavigationPage.md)、[ステップ03](course-v3/lessons/Stage02/L008RazorSyntaxPage.md)を参照してください。

`Program.cs`ではサーバーとUIのカルチャーを`ja-JP`に設定します。`App.razor`ではHTMLの`lang="ja"`を使用し、接続エラーも日本語で表示します。

### ゼロから作成するときの参考

以下は学習用の**参考コマンド**であり、このリポジトリでは実行しません。既存ファイルを上書きすると教材が消える可能性があります。

```powershell
dotnet new sln -n PgBlazorSamples
dotnet new blazor -n PgBlazorSamples.Web -f net10.0 -int Server -ai
```



## 3.1 プロジェクトは必要に応じて追加する

最初はWeb単独でRazorを学びます。ステップ11でDI、18でUnitTests、19でApplication、31〜33でDomain/Infrastructure、40でIntegrationTestsを追加する構成です。初学者に最初から複数層を要求しません。詳細は[DI入門](di-for-beginners.md)と[設計](architecture.md)。

## 4. 実行環境の区分

| 区分 | 目的 | データ | 認証 |
| --- | --- | --- | --- |
| 基本学習 | 01～30のUI・C#概念 | 固定された架空データ | 公開してよい教材のみ匿名可能 |
| DB開発 | 31～40のSQL検証 | 専用DBと架空利用者 | 実業務利用者の資格情報を使わない |
| AD検証 | 61～70の本人確認・権限 | 専用DBとテスト用AD主体 | 実際のWindows認証が必須 |
| 統合検証 | 71～80の運用品質 | Windows Server 2025 + IIS / 分離したDB | AD認証とDB認可を両方確認 |

最初の段階ではADやPostgreSQLに接続せず、ダミーデータで学べる教材を優先します。認証不要ページは無害な教材のみに限定し、**本番業務画面への認証回避設定は一切用意しません**。

## 5. 接続情報

DB接続文字列や資格情報をGitへ保存しません。`appsettings.json`にはプレースホルダーまたは非秘密の構成例のみを置き、実値はユーザーシークレット、環境変数、運用側のシークレット管理を利用します。サンプル接続先は実業務DBと分離し、書き込み可能なアカウントに不要な管理者権限を与えません。

## 6. 日本語動作確認

- 画面表示と入力検証エラーは日本語。
- 日時は保存をUTC基準とし、画面表示では日本時間を明示して変換。
- DBエンコーディングはUTF-8。ソート・検索の日本語特性は実DBで確認する。
- コントラスト、文字サイズ、キーボード操作、スクリーンリーダーの確認を行う。

以後のバージョンの更新手順は[versions.md](versions.md)、構築後の検証は[quality.md](quality.md)を参照してください。
