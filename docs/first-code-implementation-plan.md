# ソースコード実装開始計画（最初の成果物から80ステップへ）

- 作成日：2026-10-26
- 対象：Visual Studio 2026 / .NET 10（C# 14）/ Blazor Web App（Interactive Server）
- 対象リポジトリ：[pg_blazor_samples](https://github.com/ShutenOishi/pg_blazor_samples)
- 標準配備先：Active Directoryドメイン内のWindows Server 2025 + IIS
- 現状：Step01〜10はWindows/Linuxビルド・ブラウザー操作・PDF生成を確認済み。Step11〜15とコラム03を追加し、Windows/Linux Releaseビルド、Ubuntu日本語HTTP・Chrome操作、PDF生成までSUCCESSを確認済み。Step26〜80および実AD・DB・IIS統合は今後の対象。
- この文書は実装の作業計画です。完了判定は各CI結果と[サンプル一覧](sample-catalog.md)に記録します。HTTPによる確認と実ブラウザーでの手動確認は区別します。

## 実装原則

1. **動くものを優先。** 最初のPRは、単独のWebプロジェクト、Step01〜03、動作確認手順、ビルドCIを一組として完成する。
2. **初心者の理解を優先。** 最初からApplication/Domain/Infrastructureを作らない。DIはStep11から実装・解説し、必要になった時点で分割する。
3. **80個の独立アプリを量産しない。** 主Webホスト内にURLと教材コンポーネントを追加し、各ステップを個別に学べるようにする。
4. **コードと資料を同時に更新。** 同じPRで実ソースと `docs/samples/stepNN.md` を追加する。PDFはGitHub Actionsで同じコミットから生成する。
5. **日本語が正本。** UI、利用者向けメッセージ、コメント、Markdownの解説は日本語。C#識別子や製品名等の技術的名称は英語を使う。
6. **基盤は本番運用を見据えるが先取りしない。** Step01〜30にAD/DB接続を不要とし、Step61以降で実際の認証・認可を検証する。公開リポジトリへ社内情報を持ち込まない。

## 実装するPR単位

| PR群 | 範囲 | 目標と技術ポイント | 完了条件 |
| --- | --- | --- | --- |
| A（最初） | Step01〜03 | .NET 10のWebホスト、URL、Razorの値表示、日本語ナビ、CI | Releaseビルド成功、3画面が開ける |
| B | Step04〜06 | `@onclick`、`@bind`、`@if`、`@foreach` | UIイベント・入力・条件描画をブラウザーで確認 |
| C | Step07〜10 | `[Parameter]`、`EventCallback`、ライフサイクル、フォーム | 入力検証・例外/キャンセルの初歩、演習 |
| D | Step11〜14 | DIの必要性、interface、3種のライフタイム、circuit | 状態共有と分離の違いを再現 |
| E | Step15〜20 | `@inject`、Options、ログ、UnitTests、Application分割 | 具象を交換でき、テスト合格 |
| F | Step21〜30 | MudBlazor 9系の実機動作、フォーム・一覧・ダイアログ | アクセシビリティ含むUI確認 |
| G | Step31〜40 | Domain・Infrastructure、EF Core 10/Npgsql、日付・Range、CRUD・排他制御 | PostgreSQL 18を使う結合テスト成功 |
| G2 | Step41〜60 | MudTable/MudDataGrid・ServerData・仮想化・カーソル・編集・CSVの選択と実装 | 10パターンの比較と実DBテストに合格 |
| H | Step61〜70 | IIS Windows認証、SID照合、業務ロール・操作単位の認可 | AD認証済みでもDB未登録/無効/権限なしを拒否 |
| I | Step71〜80 | IIS公開・gMSA検討・HTTPS・SignalR・監視・バックアップ | Windows Server 2025実機の受入項目を満たす |

PR Aの成功前にB以降を大量に追加しない。後続フェーズは、関連する2〜5ステップを1PRにまとめてもよいが、レビューできる大きさを維持する。

## PR A：実際に最初に作るファイル

```text
PgBlazorSamples.slnx
Directory.Build.props
.gitignore
.editorconfig
src/
  PgBlazorSamples.Web/
    PgBlazorSamples.Web.csproj
    Program.cs
    Components/
      App.razor
      Routes.razor
      _Imports.razor
      Layout/
        MainLayout.razor
        NavMenu.razor
      Pages/
        Home.razor
      Samples/
        Step01/FirstPage.razor
        Step02/NavigationPage.razor
        Step03/RazorSyntaxPage.razor
    wwwroot/
      app.css
docs/
  samples/
    step01.md
    step02.md
    step03.md
.github/workflows/
  build.yml
```

これは目標の構成であり、正確なテンプレート生成ファイルは.NET 10 SDKによる生成結果を正本とする。不要なテンプレートページ・サンプルデータを残さず、初期テンプレートの意味を日本語のドキュメントで説明する。

### Step01：はじめてのページ

- 学ぶこと：Razorコンポーネントの基本、`@page`、実行・再読み込み
- URL：`/learn/01`
- 完了：日本語の見出しを表示、ナビから到達、起動・停止の手順で再現できる

### Step02：ルーティング

- 学ぶこと：ルート、リンク、`NavLink`、選択状態
- URL：`/learn/02`
- 完了：トップページ・Step01・Step02を移動できる。不明なURLで適切な表示

### Step03：Razor構文

- 学ぶこと：`@code`、C#プロパティ、式の表示、HTMLエンコードの挙動
- URL：`/learn/03`
- 完了：データ表示と静的HTMLを区別できる。値を変更し、再実行で表示変化を確認

### CI / ビルド

- `actions/setup-dotnet@v4` + `10.0.x` + `dotnet restore` → `dotnet build -c Release --no-restore`。
- `ubuntu-24.04`でビルド、可能なら`windows-2025-vs2026`でも互換性を確認。
- `dotnet test -c Release`はテストプロジェクト追加後に有効化する。
- 簡易HTTPスモークテストで`/`、`/learn/01`〜`/learn/03`が適切なHTTP応答を返すことを検証。UIの操作性の判定は別途ブラウザーテストで行う。
- CI成功時だけ「ビルド確認済み」。IIS上のWindows認証が使えるかどうかは**このCIで確認できない**。

### 学習者が必要なもの

- Visual Studio 2026の「ASP.NETとWeb開発」ワークロード
- .NET 10 SDK。ターゲットフレームワークは`net10.0`
- Step01〜03はADとPostgreSQL不要。VSまたは`dotnet run --project src/PgBlazorSamples.Web`で実行
- 本番用の秘密・ドメイン名・個人データは一切不要

## プロジェクトを分割するタイミング

| 学習ポイント | その時点で追加 |
| --- | --- |
| Step01 | `PgBlazorSamples.Web` |
| Step11〜12 | Web内部の`Services`、インターフェイス |
| Step18 | `PgBlazorSamples.UnitTests` |
| Step19 | `PgBlazorSamples.Application` |
| Step31 | `PgBlazorSamples.Domain` |
| Step33 | `PgBlazorSamples.Infrastructure` |
| Step40 | `PgBlazorSamples.IntegrationTests` |
| Step70以降 | `PgBlazorSamples.WebTests`（必要な受入テストを追加） |

**依存関係：** `Web → Application → Domain`、`Infrastructure → Application/Domain`、`Web → Infrastructure`は起動時のDI登録に限る。ApplicationからInfrastructureを参照しない。

## Step31〜40の教材番号の整合性

旧カタログではStep31が「DB接続」、Step33が「検索」だった一方、構成設計ではStep31にDomain、Step33にInfrastructureを追加する計画だったため、矛盾していた。この計画に合わせて順序を改める。

- 31 Domainと業務モデル
- 32 PostgreSQL 18のDDL・初期スキーマ
- 33 InfrastructureとEF Core 10マッピング
- 34 DbContextFactoryとNpgsql接続プール
- 35 AsNoTracking・DTO射影・SELECT
- 36 INSERT/UPDATE/DELETE、変更追跡
- 37 DateOnly/TimeOnly・Range・画面型変換
- 38 トランザクションと大量更新
- 39 xmin競合・プーリング・キャンセル
- 40 PostgreSQL 18の結合テスト・性能確認

## 新設するStep41〜60：一覧・データ連携の20ステップ

[一覧パターンの比較と選択](data-grid-patterns.md)を教材の必須要件とする。初学者はStep26でDBなしのGridを学び、Step31〜40でEF Core 10を理解した後、Step41〜60でItems、MudTable ServerData、MudDataGrid ServerData、VirtualizeServerData、キーセット、編集・競合・集約・CSVまで試す。**番号41〜60は未実装**であり、従来計画していたAD認証の41〜50は61〜70、運用の51〜60は71〜80に移動する。正常/失敗ケース、実DB、PDF・コラムを同期する。

## 各PRでPDFとの整合性を守る仕組み

- 教材実装と同一PRでMarkdownの操作手順・実ファイルパス・バージョン・期待結果を記載。
- サンプルカタログの状態は「計画中 → 実装中 → 検証済み」の実態に合わせて更新。
- PDF生成は `scripts/build_docs_pdf.py` と `.github/workflows/build-docs-pdf.yml` を使用。対象コミットとファイルハッシュを付録へ掲載。
- PDFが成功しても、コードの意味との整合性やビルド成功を意味しない。**コードCIとPDF生成CIの両方**を確認する。
- コードで登場しない型名・APIを「実装済み」として教材に書かない。実装前の擬似コードには「計画例」と明記。

## 後続のドメイン運用で実証する条件

Windows Server 2025へ配備するPhase5/6では、Windows統合認証、SID照合、PostgreSQLユーザー照合、HTTPS、IIS、Kerberosの有無、SignalR切断復旧、DB失敗時の既定拒否、ログ・バックアップ・最小権限を実環境で検証。GitHubの公開Runnerは社内ADを利用できないため、ユニットテストでの疑似ユーザー成功を「実ADでの検証済み」としない。

## 最初のPRをマージしてよい条件

- [ ] `PgBlazorSamples.slnx`をVisual Studio 2026から開ける
- [ ] Webプロジェクトが`net10.0`でビルドできる
- [ ] Interactive Serverの構成が起動する
- [ ] 01〜03のページがリンクから開ける
- [ ] 各ステップの日本語解説と実コードが一致する
- [ ] 基本HTTPスモークテストとビルドCIが成功する
- [ ] GitHub ActionsでPDFが再生成できる
- [ ] 不要な認証・DB接続・資格情報を含まない
- [ ] カタログの状態が証拠に合っている

条件が満たされた段階で次のPR B（Step04〜06）へ進む。