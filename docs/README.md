# ドキュメント目次

**全教材の再編計画（作業中）：** [新110ステップ・9段階の計画](course-v3/implementation-plan.md)／[新しい全学習順](course-v3/learning-sequence.md)／[改訂進捗](course-v3/progress.md)。現行の以下7段階は旧80ステップ版として公開中です。新規テーマの実装が完了するまで、計画段階のURLを実装済みとは表示しません。

すべての解説・コメント・画面表示を日本語で統一します。英語はC#識別子、API名、製品名、ファイル名など実装上必要なものに限ります。

## 段階ごとの概要ページ

ステップを始める前に、その段階で学ぶ理由・前提・順番・確認課題を読めます。

| 段階 | 概要ページ |
| --- | --- |
| 第1段階：Step01〜10 | [Razorと画面操作](stages/stage01.md) |
| 第2段階：Step11〜20 | [DIとサービス設計](stages/stage02.md) |
| 第3段階：Step21〜30 | [MudBlazorによる業務画面](stages/stage03.md) |
| 第4段階：Step31〜40 | [PostgreSQLとEF Core](stages/stage04.md) |
| 第5段階：Step41〜60 | [一覧データ連携と編集](stages/stage05.md) |
| 第6段階：Step61〜70 | [Windows認証とDBによる業務認可](stages/stage06.md) |
| 第7段階：Step71〜80 | [監査・競合制御・運用](stages/stage07.md) |



| 用途 | ドキュメント |
| --- | --- |
| DIを初めて学ぶ人へ | [DI入門・サービスライフタイム](di-for-beginners.md) |
| Windows Server 2025での構築 | [IIS・Windows認証・Kerberos・gMSA・PostgreSQL](windows-server-2025-iis.md) |
| PDFの生成・ソース整合性 | [PDFの生成と同期](pdf-generation.md) |
| 技術ハンドブック | [実装対応ハンドブック](implementation-handbook.md) |
| 実装済みの日本語教材 | [Step01](course-v3/lessons/Stage02/L006FirstBlazorPage.md)／[Step02](course-v3/lessons/Stage02/L007NavigationPage.md)／[Step03](course-v3/lessons/Stage02/L008RazorSyntaxPage.md)／[Step04](course-v3/lessons/Stage02/L010EventPage.md)／[Step05](course-v3/lessons/Stage02/L011TwoWayBindingPage.md)／[Step06](course-v3/lessons/Stage02/L012ConditionalRenderingPage.md)／[Step07](course-v3/lessons/Stage02/L013ComponentParametersPage.md)／[Step08](course-v3/lessons/Stage02/L014EventCallbackPage.md)／[Step09](course-v3/lessons/Stage02/L016ComponentLifecyclePage.md)／[Step10](course-v3/lessons/Stage02/L018ValidatedFormPage.md)／[Step11](course-v3/lessons/Stage03/L021DependencyInjectionPage.md)／[Step12](course-v3/lessons/Stage03/L022InterfaceClockPage.md)／[Step13](course-v3/lessons/Stage03/L023ServiceLifetimesPage.md)／[Step14](course-v3/lessons/Stage03/L024CircuitScopedPage.md)／[Step15](course-v3/lessons/Stage03/L025InjectionPatternsPage.md)／[Step16](course-v3/lessons/Stage03/L026OptionsPatternPage.md)／[Step17](course-v3/lessons/Stage03/L027AsyncLoggingPage.md)／[Step18](course-v3/lessons/Stage03/L028DependencyUnitTestPage.md)／[Step19](course-v3/lessons/Stage03/L029ApplicationLayerPage.md)／[Step20](course-v3/lessons/Stage03/L030TodoServicePage.md) |
| MudBlazor応用（26〜30） | [26：DataGrid](course-v3/lessons/Stage04/L036DataGridPage.md)／[27：Dialog](course-v3/lessons/Stage04/L037DialogPage.md)／[28：Snackbar](course-v3/lessons/Stage04/L038NotificationPage.md)／[29：テーマと操作性](course-v3/lessons/Stage04/L039AccessibilityPage.md)／[30：総合演習](course-v3/lessons/Stage04/L040UiPracticePage.md)／[コラム07](columns/column26-30.md) |
| MudBlazorのテンプレート・日本語化 | [ステップ21：導入](course-v3/lessons/Stage04/L031MudIntroductionPage.md)／[22：レイアウト](course-v3/lessons/Stage04/L032MudLayoutPage.md)／[23：入力](course-v3/lessons/Stage04/L033MudInputsPage.md)／[24：フォーム検証](course-v3/lessons/Stage04/L034MudValidationPage.md)／[25：一覧表](course-v3/lessons/Stage04/L035MudTablePage.md) |
| 5ステップごとの理解を深めるコラム | [全19本の教材・特別コラムを含む一覧](columns/README.md) |
| 一覧の実装済み教材（41〜50） | [41〜45：ServerDataと検索](columns/column41-45.md)／[46〜50：DTO・キーセット・仮想化](columns/column46-50.md) |
| 一覧の方式を要件から選ぶ | [MudTable/MudDataGrid・ServerData・Virtualize・検索・編集・CSVの選択ガイド](data-grid-patterns.md) |
| データアクセスの標準 | [EF Core 10・Npgsql・日付/Range・DbContextFactory](efcore10-data-access.md) |
| 次のチャットへの引き継ぎ | [引き継ぎプロンプト](next-chat-handoff.md) |
| 次の実装工程とPR単位 | [初回ソース実装計画](first-code-implementation-plan.md) |
| 学習順序と到達目標 | [学習ロードマップ](learning-path.md) |
| 80ステップの具体的な題材と進捗 | [サンプル一覧](sample-catalog.md) |
| 1サンプルの記述方法 | [サンプル作成テンプレート](sample-specification.md) |
| Visual Studioと実行環境 | [環境構築](environment.md) |
| システム構成と依存関係 | [アーキテクチャ](architecture.md) |
| AD認証・DB権限・安全設計 | [認証と認可](authentication.md) |
| PostgreSQLのテーブル案 | [データベース設計](database.md) |
| NuGetとサポート範囲 | [バージョン管理](versions.md) |
| コーディングと日本語方針 | [実装規約](conventions.md) |
| テスト・CI・完成条件 | [品質管理](quality.md) |
| 段階的な開発・マイルストーン | [実装計画](implementation-plan.md) |
| 設計の判断記録 | [ADR-0001：構成設計](decisions/0001-learning-app-structure.md)／[ADR-0002：EF Core 10と一覧方式](decisions/0002-efcore10-grid-patterns.md) |
| 公式情報と調査先 | [参考資料](references.md) |

まず[学習ロードマップ](learning-path.md)を読み、続いて[サンプル一覧](sample-catalog.md)を開いてください。

## DB編と用途別コンポーネント

[Web内で完結するEF Core構成](architecture.md)／[Step31〜40の学習](sample-catalog.md)／[MudBlazor用途別特別コラム](columns/column-mudblazor-by-purpose.md)。
