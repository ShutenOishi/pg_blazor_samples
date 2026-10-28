# 学習順001〜110：教材とサンプルコードの直接索引

この索引は教材v3の**現在の学習順**を唯一の番号として使用します。学習者は番号を読み替える必要がありません。各行から教材本文とアプリのRazorソースを直接開けます。リンク先の内部ファイル名は現行リポジトリの構造を参照しています。

画面と本文が存在することは、その学習項目のすべてが実際に動くサンプルで説明できること、単体・結合試験が足りていること、実機受入を完了したことを意味しません。実装充実・テストの進捗は [Issue #38](https://github.com/ShutenOishi/pg_blazor_samples/issues/38) に従って別途確認します。

| 状態 | 件数 | 意味 |
| --- | ---: | --- |
| 学習項目 | 110 | curriculum.json にある現在の順序 |
| 教材リンク | 110 | Markdownファイルの存在を確認 |
| Razorソースリンク | 110 | ファイルの存在を確認 |
| 追加項目の動作監査 | 30件を継続監査 | 画面存在とテーマの完備は別の判定 |

## 第1段階：Web開発の前提（学習順001〜005）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 001 | WinFormsとWebの処理場所を比較する | [本文](../course-v3/lessons/Stage01/L001WebExecutionPage.md) | `/learn/001` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L001WebExecutionPage.razor) |
| 002 | URL・HTTPと要求／応答を理解する | [本文](../course-v3/lessons/Stage01/L002HttpRequestResponsePage.md) | `/learn/002` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L002HttpRequestResponsePage.razor) |
| 003 | HTML・CSS・フォームの基礎 | [本文](../course-v3/lessons/Stage01/L003HtmlCssFormPage.md) | `/learn/003` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L003HtmlCssFormPage.razor) |
| 004 | 開発者ツールでDOM・Network・Consoleを見る | [本文](../course-v3/lessons/Stage01/L004DomNetworkConsolePage.md) | `/learn/004` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L004DomNetworkConsolePage.razor) |
| 005 | Visual Studio 2026でWebアプリを起動・デバッグする | [本文](../course-v3/lessons/Stage01/L005VisualStudioDebugPage.md) | `/learn/005` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L005VisualStudioDebugPage.razor) |

## 第2段階：Blazorの基本と実行原理（学習順006〜020）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 006 | 最初のBlazor画面を表示する | [本文](lessons/Stage02/L006FirstBlazorPage.md) | `/learn/006` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L006FirstBlazorPage.razor) |
| 007 | URLと画面遷移を理解する | [本文](lessons/Stage02/L007NavigationPage.md) | `/learn/007` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L007NavigationPage.razor) |
| 008 | RazorでC#の値を表示する | [本文](lessons/Stage02/L008RazorSyntaxPage.md) | `/learn/008` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L008RazorSyntaxPage.razor) |
| 009 | Static SSRとInteractive Serverの違い | [本文](lessons/Stage02/L009StaticSsrInteractivePage.md) | `/learn/009` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L009StaticSsrInteractivePage.razor) |
| 010 | ボタンのイベントを扱う | [本文](lessons/Stage02/L010EventPage.md) | `/learn/010` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L010EventPage.razor) |
| 011 | 入力値を双方向に結び付ける | [本文](lessons/Stage02/L011TwoWayBindingPage.md) | `/learn/011` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L011TwoWayBindingPage.razor) |
| 012 | 条件分岐と繰り返しで表示する | [本文](lessons/Stage02/L012ConditionalRenderingPage.md) | `/learn/012` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L012ConditionalRenderingPage.razor) |
| 013 | 親から子へ値を渡す | [本文](lessons/Stage02/L013ComponentParametersPage.md) | `/learn/013` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L013ComponentParametersPage.razor) |
| 014 | 子から親へ通知する | [本文](lessons/Stage02/L014EventCallbackPage.md) | `/learn/014` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L014EventCallbackPage.razor) |
| 015 | SignalR・circuit・再接続 | [本文](lessons/Stage02/L015CircuitReconnectPage.md) | `/learn/015` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L015CircuitReconnectPage.razor) |
| 016 | 初期化・更新・破棄を理解する | [本文](lessons/Stage02/L016ComponentLifecyclePage.md) | `/learn/016` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L016ComponentLifecyclePage.razor) |
| 017 | 事前レンダリングと初期化の実行回数 | [本文](lessons/Stage02/L017PrerenderInitializationPage.md) | `/learn/017` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L017PrerenderInitializationPage.razor) |
| 018 | 入力フォームを日本語で検証する | [本文](lessons/Stage02/L018ValidatedFormPage.md) | `/learn/018` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L018ValidatedFormPage.razor) |
| 019 | URL・履歴・再読込と画面状態 | [本文](lessons/Stage02/L019UrlHistoryStatePage.md) | `/learn/019` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L019UrlHistoryStatePage.razor) |
| 020 | circuit・ブラウザー・DBの状態保存の選択 | [本文](lessons/Stage02/L020StatePersistenceChoicePage.md) | `/learn/020` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L020StatePersistenceChoicePage.razor) |

## 第3段階：DI・サービスと単体テスト（学習順021〜030）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 021 | DI（依存性注入）の基本 | [本文](lessons/Stage03/L021DependencyInjectionPage.md) | `/learn/021` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L021DependencyInjectionPage.razor) |
| 022 | インターフェイスで部品を交換する | [本文](lessons/Stage03/L022InterfaceClockPage.md) | `/learn/022` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L022InterfaceClockPage.razor) |
| 023 | Singleton・Scoped・Transient | [本文](lessons/Stage03/L023ServiceLifetimesPage.md) | `/learn/023` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L023ServiceLifetimesPage.razor) |
| 024 | BlazorのScopedと対話回線 | [本文](lessons/Stage03/L024CircuitScopedPage.md) | `/learn/024` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L024CircuitScopedPage.razor) |
| 025 | injectとコンストラクター注入 | [本文](lessons/Stage03/L025InjectionPatternsPage.md) | `/learn/025` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L025InjectionPatternsPage.razor) |
| 026 | Optionsで設定を受け取る | [本文](lessons/Stage03/L026OptionsPatternPage.md) | `/learn/026` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L026OptionsPatternPage.razor) |
| 027 | 非同期処理とログ | [本文](lessons/Stage03/L027AsyncLoggingPage.md) | `/learn/027` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L027AsyncLoggingPage.razor) |
| 028 | DIによる単体テスト | [本文](lessons/Stage03/L028DependencyUnitTestPage.md) | `/learn/028` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L028DependencyUnitTestPage.razor) |
| 029 | Applicationへ処理を分離 | [本文](lessons/Stage03/L029ApplicationLayerPage.md) | `/learn/029` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L029ApplicationLayerPage.razor) |
| 030 | DIで課題管理を作る | [本文](lessons/Stage03/L030TodoServicePage.md) | `/learn/030` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L030TodoServicePage.razor) |

## 第4段階：MudBlazorとブラウザーの業務UI（学習順031〜045）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 031 | MudBlazorと日本語翻訳を導入する | [本文](lessons/Stage04/L031MudIntroductionPage.md) | `/learn/031` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L031MudIntroductionPage.razor) |
| 032 | MudBlazorのレイアウトを学ぶ | [本文](lessons/Stage04/L032MudLayoutPage.md) | `/learn/032` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L032MudLayoutPage.razor) |
| 033 | 日本語で入力部品を使う | [本文](lessons/Stage04/L033MudInputsPage.md) | `/learn/033` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L033MudInputsPage.razor) |
| 034 | MudFormの入力検証を学ぶ | [本文](lessons/Stage04/L034MudValidationPage.md) | `/learn/034` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L034MudValidationPage.razor) |
| 035 | MudTableで一覧とソート | [本文](lessons/Stage04/L035MudTablePage.md) | `/learn/035` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L035MudTablePage.razor) |
| 036 | MudDataGridで検索・並べ替えする | [本文](lessons/Stage04/L036DataGridPage.md) | `/learn/036` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L036DataGridPage.razor) |
| 037 | 確認ダイアログで実行と中止を選ぶ | [本文](lessons/Stage04/L037DialogPage.md) | `/learn/037` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L037DialogPage.razor) |
| 038 | Snackbarと処理中の状態を表示する | [本文](lessons/Stage04/L038NotificationPage.md) | `/learn/038` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L038NotificationPage.razor) |
| 039 | ダークモードとキーボード操作を学ぶ | [本文](lessons/Stage04/L039AccessibilityPage.md) | `/learn/039` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L039AccessibilityPage.razor) |
| 040 | MudBlazor日本語UIを総合演習する | [本文](lessons/Stage04/L040UiPracticePage.md) | `/learn/040` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L040UiPracticePage.razor) |
| 041 | JavaScript interopの利用判断 | [本文](lessons/Stage04/L041JsInteropPage.md) | `/learn/041` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L041JsInteropPage.razor) |
| 042 | ファイルアップロードと入力制約 | [本文](lessons/Stage04/L042FileUploadPage.md) | `/learn/042` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L042FileUploadPage.razor) |
| 043 | CSV・PDFファイルのダウンロード | [本文](lessons/Stage04/L043DownloadPage.md) | `/learn/043` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L043DownloadPage.razor) |
| 044 | ErrorBoundary・取消・再試行 | [本文](lessons/Stage04/L044ErrorRecoveryPage.md) | `/learn/044` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L044ErrorRecoveryPage.razor) |
| 045 | HTMLの意味・フォーカス・拡大表示の最終確認 | [本文](lessons/Stage04/L045AccessibilityReviewPage.md) | `/learn/045` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L045AccessibilityReviewPage.razor) |

## 第5段階：DapperからEF Core 10・PostgreSQL 18へ（学習順046〜060）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 046 | DapperとEF Coreの役割・選定を比較する | [本文](lessons/Stage05/L046OrmComparisonPage.md) | `/learn/046` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L046OrmComparisonPage.razor) |
| 047 | モデルとDTOを理解する | [本文](lessons/Stage05/L047EfModelPage.md) | `/learn/047` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L047EfModelPage.razor) |
| 048 | PostgreSQLで学習テーブルを定義する | [本文](lessons/Stage05/L048SqlSchemaPage.md) | `/learn/048` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L048SqlSchemaPage.razor) |
| 049 | EF Core 10をWebへ登録する | [本文](lessons/Stage05/L049EfSetupPage.md) | `/learn/049` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L049EfSetupPage.razor) |
| 050 | DbContextFactoryと接続プール | [本文](lessons/Stage05/L050DbContextFactoryPage.md) | `/learn/050` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L050DbContextFactoryPage.razor) |
| 051 | LINQ検索とAsNoTracking | [本文](lessons/Stage05/L051EfSearchPage.md) | `/learn/051` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L051EfSearchPage.razor) |
| 052 | SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL | [本文](lessons/Stage05/L052SqlAndLinqPage.md) | `/learn/052` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L052SqlAndLinqPage.razor) |
| 053 | Dapperの接続／ExecuteとDbContext・追跡・SaveChanges | [本文](lessons/Stage05/L053PersistenceComparisonPage.md) | `/learn/053` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L053PersistenceComparisonPage.razor) |
| 054 | EF Coreの登録・更新・削除 | [本文](lessons/Stage05/L054EfCrudPage.md) | `/learn/054` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L054EfCrudPage.razor) |
| 055 | DateOnly・TimeOnly・Rangeの型変換 | [本文](lessons/Stage05/L055EfDateRangePage.md) | `/learn/055` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L055EfDateRangePage.razor) |
| 056 | トランザクションを理解する | [本文](lessons/Stage05/L056EfTransactionPage.md) | `/learn/056` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L056EfTransactionPage.razor) |
| 057 | xmin楽観的排他を確認する | [本文](lessons/Stage05/L057EfConcurrencyPage.md) | `/learn/057` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L057EfConcurrencyPage.razor) |
| 058 | 既存DB・ビュー・キーなしモデル・FDWを扱う | [本文](lessons/Stage05/L058KeylessViewPage.md) | `/learn/058` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L058KeylessViewPage.razor) |
| 059 | FromSql／SqlQueryのパラメーターとDapper併用 | [本文](lessons/Stage05/L059ParameterizedSqlPage.md) | `/learn/059` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L059ParameterizedSqlPage.razor) |
| 060 | PostgreSQL 18の実接続確認 | [本文](lessons/Stage05/L060EfDbCheckPage.md) | `/learn/060` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L060EfDbCheckPage.razor) |

## 第6段階：一覧データ連携・検索・編集（学習順061〜080）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 061 | Itemsを使う一覧の条件 | [本文](lessons/Stage06/L061ItemsBoundaryPage.md) | `/learn/061` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L061ItemsBoundaryPage.razor) |
| 062 | MudTableのServerDataでDB検索 | [本文](lessons/Stage06/L062TableServerDataPage.md) | `/learn/062` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L062TableServerDataPage.razor) |
| 063 | MudDataGridのServerDataとEF Core | [本文](lessons/Stage06/L063GridServerDataPage.md) | `/learn/063` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L063GridServerDataPage.razor) |
| 064 | 複数列ソートを安全に組み立てる | [本文](lessons/Stage06/L064SafeSortPage.md) | `/learn/064` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L064SafeSortPage.razor) |
| 065 | 日付と完了状態で絞り込む | [本文](lessons/Stage06/L065FilterSearchPage.md) | `/learn/065` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L065FilterSearchPage.razor) |
| 066 | 関連テーブルをDTOへ射影する | [本文](lessons/Stage06/L066RelatedDtoPage.md) | `/learn/066` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L066RelatedDtoPage.razor) |
| 067 | キーセットページングを理解する | [本文](lessons/Stage06/L067KeysetPage.md) | `/learn/067` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L067KeysetPage.razor) |
| 068 | 取得済み一覧を部分描画する | [本文](lessons/Stage06/L068VirtualizeItemsPage.md) | `/learn/068` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L068VirtualizeItemsPage.razor) |
| 069 | DB取得と仮想スクロールを連携する | [本文](lessons/Stage06/L069VirtualizeDatabasePage.md) | `/learn/069` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L069VirtualizeDatabasePage.razor) |
| 070 | デバウンスと障害時の再試行 | [本文](lessons/Stage06/L070SearchUxPage.md) | `/learn/070` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L070SearchUxPage.razor) |
| 071 | 編集DTOをフォームで検証して保存する | [本文](lessons/Stage06/L071EditDtoPage.md) | `/learn/071` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L071EditDtoPage.razor) |
| 072 | ダイアログで保存と取消を分ける | [本文](lessons/Stage06/L072EditDialogPage.md) | `/learn/072` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L072EditDialogPage.razor) |
| 073 | 複数選択と原子的な一括処理 | [本文](lessons/Stage06/L073BulkEditPage.md) | `/learn/073` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L073BulkEditPage.razor) |
| 074 | xmin競合と最新値の確認 | [本文](lessons/Stage06/L074ConflictEditorPage.md) | `/learn/074` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L074ConflictEditorPage.razor) |
| 075 | 詳細DTOとN+1の防止 | [本文](lessons/Stage06/L075DetailProjectionPage.md) | `/learn/075` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L075DetailProjectionPage.razor) |
| 076 | 日本語日付範囲とDB検索 | [本文](lessons/Stage06/L076RangeSearchPage.md) | `/learn/076` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L076RangeSearchPage.razor) |
| 077 | キャッシュと権限境界 | [本文](lessons/Stage06/L077CacheBoundaryPage.md) | `/learn/077` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L077CacheBoundaryPage.razor) |
| 078 | DB側集計とCSV出力 | [本文](lessons/Stage06/L078StatisticsExportPage.md) | `/learn/078` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L078StatisticsExportPage.razor) |
| 079 | EXPLAINと応答時間計測 | [本文](lessons/Stage06/L079PerformancePage.md) | `/learn/079` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L079PerformancePage.razor) |
| 080 | 一覧パターンを要件から選ぶ | [本文](lessons/Stage06/L080PatternChoicePage.md) | `/learn/080` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L080PatternChoicePage.razor) |

## 第7段階：Webセキュリティ・AD認証とDB認可（学習順081〜095）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 081 | Windows認証・Cookie・HTTP・circuit | [本文](lessons/Stage07/L081WindowsCookieCircuitPage.md) | `/learn/081` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L081WindowsCookieCircuitPage.razor) |
| 082 | Middlewareの順序・HTTPS・IISの経路 | [本文](lessons/Stage07/L082MiddlewareHttpsPage.md) | `/learn/082` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L082MiddlewareHttpsPage.razor) |
| 083 | XSS・CSRF・SQL注入の防御境界 | [本文](lessons/Stage07/L083XssCsrfSqlPage.md) | `/learn/083` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L083XssCsrfSqlPage.razor) |
| 084 | 環境別設定・Secret・Data Protection鍵 | [本文](lessons/Stage07/L084SecretsKeysPage.md) | `/learn/084` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L084SecretsKeysPage.razor) |
| 085 | 接続切断・セッション失効・権限再検証 | [本文](lessons/Stage07/L085ReauthorizationPage.md) | `/learn/085` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L085ReauthorizationPage.razor) |
| 086 | Windows統合認証を構成する | [本文](lessons/Stage07/L086NegotiatePage.md) | `/learn/086` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L086NegotiatePage.razor) |
| 087 | 認証済みドメインユーザー名を扱う | [本文](lessons/Stage07/L087SidPage.md) | `/learn/087` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L087SidPage.razor) |
| 088 | 認証済みユーザー名と既存台帳を結ぶ | [本文](lessons/Stage07/L088UserMappingPage.md) | `/learn/088` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L088UserMappingPage.razor) |
| 089 | 未登録・無効利用者を拒否する | [本文](lessons/Stage07/L089UserRefusalPage.md) | `/learn/089` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L089UserRefusalPage.razor) |
| 090 | 既存台帳の業務権限を確認する | [本文](lessons/Stage07/L090RolePermissionPage.md) | `/learn/090` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L090RolePermissionPage.razor) |
| 091 | サービス側で認可を強制する | [本文](lessons/Stage07/L091AuthorizedEntryPage.md) | `/learn/091` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L091AuthorizedEntryPage.razor) |
| 092 | 部署ごとのCountとItems | [本文](lessons/Stage07/L092UnitScopedPage.md) | `/learn/092` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L092UnitScopedPage.razor) |
| 093 | 集計とCSVにも認可を適用 | [本文](lessons/Stage07/L093ScopedExportPage.md) | `/learn/093` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L093ScopedExportPage.razor) |
| 094 | 保存直前の権限とxminを再確認 | [本文](lessons/Stage07/L094SecuredEditPage.md) | `/learn/094` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L094SecuredEditPage.razor) |
| 095 | Windows Server 2025の実機受入を分ける | [本文](lessons/Stage07/L095DeploymentGatePage.md) | `/learn/095` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L095DeploymentGatePage.razor) |

## 第8段階：監査・競合制御・Windows Server運用（学習順096〜105）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 096 | 既存台帳のキー正規化・重複拒否と索引 | [本文](lessons/Stage08/L096DirectoryNormalizationPage.md) | `/learn/096` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L096DirectoryNormalizationPage.razor) |
| 097 | 明示Windowsログインと短命Cookie | [本文](lessons/Stage08/L097ExplicitWindowsLoginPage.md) | `/learn/097` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L097ExplicitWindowsLoginPage.razor) |
| 098 | 権限行ロックと更新の原子性 | [本文](lessons/Stage08/L098TransactionalAuthorizationPage.md) | `/learn/098` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L098TransactionalAuthorizationPage.razor) |
| 099 | 本人・操作・結果を永続監査する | [本文](lessons/Stage08/L099DurableAuditPage.md) | `/learn/099` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L099DurableAuditPage.razor) |
| 100 | IIS・Kerberos・負荷・障害の実機受入 | [本文](lessons/Stage08/L100IisAcceptancePage.md) | `/learn/100` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L100IisAcceptancePage.razor) |
| 101 | 二人の編集競合とxmin | [本文](lessons/Stage08/L101ConcurrentEditPage.md) | `/learn/101` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L101ConcurrentEditPage.razor) |
| 102 | キャッシュの有効期限と権限失効 | [本文](lessons/Stage08/L102AuthorizedCachePage.md) | `/learn/102` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L102AuthorizedCachePage.razor) |
| 103 | ヘルスチェック・構造化ログ・運用監視 | [本文](lessons/Stage08/L103DiagnosticsPage.md) | `/learn/103` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L103DiagnosticsPage.razor) |
| 104 | 実DB・ブラウザー・認可の結合/E2E試験 | [本文](lessons/Stage08/L104EndToEndAcceptancePage.md) | `/learn/104` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L104EndToEndAcceptancePage.razor) |
| 105 | Windows Server 2025/IISの配布・切替・復旧 | [本文](lessons/Stage08/L105DeploymentPage.md) | `/learn/105` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L105DeploymentPage.razor) |

## 第9段階：業務アプリの通し演習（学習順106〜110）

| 学習順 | テーマ | 教材 | 正規URL | 画面ソース |
| --- | --- | --- | --- | --- |
| 106 | 業務要件・画面遷移・WinForms移行の設計 | [本文](lessons/Stage09/L106RequirementFlowPage.md) | `/learn/106` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L106RequirementFlowPage.razor) |
| 107 | PostgreSQL検索とページング・生成SQL | [本文](lessons/Stage09/L107SearchPagingPage.md) | `/learn/107` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L107SearchPagingPage.razor) |
| 108 | DTO編集・競合・取消・例外の統合 | [本文](lessons/Stage09/L108DtoEditPage.md) | `/learn/108` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L108DtoEditPage.razor) |
| 109 | 認証済み本人と既存台帳による権限・監査 | [本文](lessons/Stage09/L109PermissionAuditPage.md) | `/learn/109` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L109PermissionAuditPage.razor) |
| 110 | E2E・障害復旧・IIS受入表 | [本文](lessons/Stage09/L110AcceptanceLedgerPage.md) | `/learn/110` | [Razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L110AcceptanceLedgerPage.razor) |

## 詳細照合

この索引と同じ110件の構造化データは [学習項目と画面ソースの対応表](source-map.json) にあります。追加30テーマについては、実画面の有無に加え、実行できる処理、模擬・説明だけの処理、関連するサービスとテスト、負例を調べ、必要な教材だけ更新します。データの `audit_state` は進捗管理の暫定区分であり、合格認定ではありません。

実AD/IIS/Kerberos/FDW、鍵、性能、切戻しの現地試験は [現地実機受入計画](on-site-acceptance-plan.md) の許可と証跡による別の受入です。学習用の操作やCIを実機PASSに換算しません。
