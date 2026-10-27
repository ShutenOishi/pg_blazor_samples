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

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 001 | WinFormsとWebの処理場所を比較する | [本文](../course-v3/lessons/web-01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/WebFoundation/Web01Page.razor) |
| 002 | URL・HTTPと要求／応答を理解する | [本文](../course-v3/lessons/web-02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/WebFoundation/Web02Page.razor) |
| 003 | HTML・CSS・フォームの基礎 | [本文](../course-v3/lessons/web-03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/WebFoundation/Web03Page.razor) |
| 004 | 開発者ツールでDOM・Network・Consoleを見る | [本文](../course-v3/lessons/web-04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/WebFoundation/Web04Page.razor) |
| 005 | Visual Studio 2026でWebアプリを起動・デバッグする | [本文](../course-v3/lessons/web-05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/WebFoundation/Web05Page.razor) |

## 第2段階：Blazorの基本と実行原理（学習順006〜020）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 006 | 最初のBlazor画面を表示する | [本文](../samples/step01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step01/FirstPage.razor) |
| 007 | URLと画面遷移を理解する | [本文](../samples/step02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step02/NavigationPage.razor) |
| 008 | RazorでC#の値を表示する | [本文](../samples/step03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step03/RazorSyntaxPage.razor) |
| 009 | Static SSRとInteractive Serverの違い | [本文](../course-v3/lessons/runtime-01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Runtime/Runtime01Page.razor) |
| 010 | ボタンのイベントを扱う | [本文](../samples/step04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step04/EventPage.razor) |
| 011 | 入力値を双方向に結び付ける | [本文](../samples/step05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step05/BindingPage.razor) |
| 012 | 条件分岐と繰り返しで表示する | [本文](../samples/step06.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step06/ConditionsPage.razor) |
| 013 | 親から子へ値を渡す | [本文](../samples/step07.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step07/ParametersPage.razor) |
| 014 | 子から親へ通知する | [本文](../samples/step08.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step08/CallbackPage.razor) |
| 015 | SignalR・circuit・再接続 | [本文](../course-v3/lessons/runtime-02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Runtime/Runtime02Page.razor) |
| 016 | 初期化・更新・破棄を理解する | [本文](../samples/step09.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step09/LifecyclePage.razor) |
| 017 | 事前レンダリングと初期化の実行回数 | [本文](../course-v3/lessons/runtime-03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Runtime/Runtime03Page.razor) |
| 018 | 入力フォームを日本語で検証する | [本文](../samples/step10.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step10/FormPage.razor) |
| 019 | URL・履歴・再読込と画面状態 | [本文](../course-v3/lessons/runtime-04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Runtime/Runtime04Page.razor) |
| 020 | circuit・ブラウザー・DBの状態保存の選択 | [本文](../course-v3/lessons/runtime-05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Runtime/Runtime05Page.razor) |

## 第3段階：DI・サービスと単体テスト（学習順021〜030）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 021 | DI（依存性注入）の基本 | [本文](../samples/step11.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step11/DiIntroductionPage.razor) |
| 022 | インターフェイスで部品を交換する | [本文](../samples/step12.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step12/InterfacePage.razor) |
| 023 | Singleton・Scoped・Transient | [本文](../samples/step13.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step13/LifetimePage.razor) |
| 024 | BlazorのScopedと対話回線 | [本文](../samples/step14.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step14/CircuitPage.razor) |
| 025 | injectとコンストラクター注入 | [本文](../samples/step15.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step15/InjectionPage.razor) |
| 026 | Optionsで設定を受け取る | [本文](../samples/step16.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step16/OptionsPage.razor) |
| 027 | 非同期処理とログ | [本文](../samples/step17.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step17/AsyncLoggingPage.razor) |
| 028 | DIによる単体テスト | [本文](../samples/step18.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step18/UnitTestPage.razor) |
| 029 | Applicationへ処理を分離 | [本文](../samples/step19.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step19/ApplicationPage.razor) |
| 030 | DIで課題管理を作る | [本文](../samples/step20.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step20/TodoPage.razor) |

## 第4段階：MudBlazorとブラウザーの業務UI（学習順031〜045）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 031 | MudBlazorと日本語翻訳を導入する | [本文](../samples/step21.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step21/MudIntroductionPage.razor) |
| 032 | MudBlazorのレイアウトを学ぶ | [本文](../samples/step22.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step22/MudLayoutPage.razor) |
| 033 | 日本語で入力部品を使う | [本文](../samples/step23.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step23/MudInputsPage.razor) |
| 034 | MudFormの入力検証を学ぶ | [本文](../samples/step24.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step24/MudValidationPage.razor) |
| 035 | MudTableで一覧とソート | [本文](../samples/step25.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step25/MudTablePage.razor) |
| 036 | MudDataGridで検索・並べ替えする | [本文](../samples/step26.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step26/DataGridPage.razor) |
| 037 | 確認ダイアログで実行と中止を選ぶ | [本文](../samples/step27.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step27/DialogPage.razor) |
| 038 | Snackbarと処理中の状態を表示する | [本文](../samples/step28.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step28/NotificationPage.razor) |
| 039 | ダークモードとキーボード操作を学ぶ | [本文](../samples/step29.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step29/AccessibilityPage.razor) |
| 040 | MudBlazor日本語UIを総合演習する | [本文](../samples/step30.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step30/UiPracticePage.razor) |
| 041 | JavaScript interopの利用判断 | [本文](../course-v3/lessons/browser-01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser01Page.razor) |
| 042 | ファイルアップロードと入力制約 | [本文](../course-v3/lessons/browser-02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser02Page.razor) |
| 043 | CSV・PDFファイルのダウンロード | [本文](../course-v3/lessons/browser-03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser03Page.razor) |
| 044 | ErrorBoundary・取消・再試行 | [本文](../course-v3/lessons/browser-04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser04Page.razor) |
| 045 | HTMLの意味・フォーカス・拡大表示の最終確認 | [本文](../course-v3/lessons/browser-05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser05Page.razor) |

## 第5段階：DapperからEF Core 10・PostgreSQL 18へ（学習順046〜060）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 046 | DapperとEF Coreの役割・選定を比較する | [本文](../course-v3/lessons/db-bridge-01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge01Page.razor) |
| 047 | モデルとDTOを理解する | [本文](../samples/step31.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step31/EfModelPage.razor) |
| 048 | PostgreSQLで学習テーブルを定義する | [本文](../samples/step32.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step32/SqlSchemaPage.razor) |
| 049 | EF Core 10をWebへ登録する | [本文](../samples/step33.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step33/EfSetupPage.razor) |
| 050 | DbContextFactoryと接続プール | [本文](../samples/step34.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step34/DbFactoryPage.razor) |
| 051 | LINQ検索とAsNoTracking | [本文](../samples/step35.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step35/EfSearchPage.razor) |
| 052 | SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL | [本文](../course-v3/lessons/db-bridge-02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge02Page.razor) |
| 053 | Dapperの接続／ExecuteとDbContext・追跡・SaveChanges | [本文](../course-v3/lessons/db-bridge-03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge03Page.razor) |
| 054 | EF Coreの登録・更新・削除 | [本文](../samples/step36.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step36/EfCrudPage.razor) |
| 055 | DateOnly・TimeOnly・Rangeの型変換 | [本文](../samples/step37.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step37/EfDateRangePage.razor) |
| 056 | トランザクションを理解する | [本文](../samples/step38.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step38/EfTransactionPage.razor) |
| 057 | xmin楽観的排他を確認する | [本文](../samples/step39.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step39/EfConcurrencyPage.razor) |
| 058 | 既存DB・ビュー・キーなしモデル・FDWを扱う | [本文](../course-v3/lessons/db-bridge-04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge04Page.razor) |
| 059 | FromSql／SqlQueryのパラメーターとDapper併用 | [本文](../course-v3/lessons/db-bridge-05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge05Page.razor) |
| 060 | PostgreSQL 18の実接続確認 | [本文](../samples/step40.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step40/EfDbCheckPage.razor) |

## 第6段階：一覧データ連携・検索・編集（学習順061〜080）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 061 | Itemsを使う一覧の条件 | [本文](../samples/step41.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step41/ItemsBoundaryPage.razor) |
| 062 | MudTableのServerDataでDB検索 | [本文](../samples/step42.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step42/TableServerDataPage.razor) |
| 063 | MudDataGridのServerDataとEF Core | [本文](../samples/step43.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step43/GridServerDataPage.razor) |
| 064 | 複数列ソートを安全に組み立てる | [本文](../samples/step44.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step44/SafeSortPage.razor) |
| 065 | 日付と完了状態で絞り込む | [本文](../samples/step45.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step45/FilterSearchPage.razor) |
| 066 | 関連テーブルをDTOへ射影する | [本文](../samples/step46.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step46/RelatedDtoPage.razor) |
| 067 | キーセットページングを理解する | [本文](../samples/step47.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step47/KeysetPage.razor) |
| 068 | 取得済み一覧を部分描画する | [本文](../samples/step48.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step48/VirtualizeItemsPage.razor) |
| 069 | DB取得と仮想スクロールを連携する | [本文](../samples/step49.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step49/VirtualizeDatabasePage.razor) |
| 070 | デバウンスと障害時の再試行 | [本文](../samples/step50.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step50/SearchUxPage.razor) |
| 071 | 編集DTOをフォームで検証して保存する | [本文](../samples/step51.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step51/EditDtoPage.razor) |
| 072 | ダイアログで保存と取消を分ける | [本文](../samples/step52.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step52/EditDialogPage.razor) |
| 073 | 複数選択と原子的な一括処理 | [本文](../samples/step53.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step53/BulkEditPage.razor) |
| 074 | xmin競合と最新値の確認 | [本文](../samples/step54.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step54/ConflictEditorPage.razor) |
| 075 | 詳細DTOとN+1の防止 | [本文](../samples/step55.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step55/DetailProjectionPage.razor) |
| 076 | 日本語日付範囲とDB検索 | [本文](../samples/step56.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step56/RangeSearchPage.razor) |
| 077 | キャッシュと権限境界 | [本文](../samples/step57.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step57/CacheBoundaryPage.razor) |
| 078 | DB側集計とCSV出力 | [本文](../samples/step58.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step58/StatisticsExportPage.razor) |
| 079 | EXPLAINと応答時間計測 | [本文](../samples/step59.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step59/PerformancePage.razor) |
| 080 | 一覧パターンを要件から選ぶ | [本文](../samples/step60.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step60/PatternChoicePage.razor) |

## 第7段階：Webセキュリティ・AD認証とDB認可（学習順081〜095）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 081 | Windows認証・Cookie・HTTP・circuit | [本文](../course-v3/lessons/security-01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Security/Security01Page.razor) |
| 082 | Middlewareの順序・HTTPS・IISの経路 | [本文](../course-v3/lessons/security-02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Security/Security02Page.razor) |
| 083 | XSS・CSRF・SQL注入の防御境界 | [本文](../course-v3/lessons/security-03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Security/Security03Page.razor) |
| 084 | 環境別設定・Secret・Data Protection鍵 | [本文](../course-v3/lessons/security-04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Security/Security04Page.razor) |
| 085 | 接続切断・セッション失効・権限再検証 | [本文](../course-v3/lessons/security-05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Security/Security05Page.razor) |
| 086 | Windows統合認証を構成する | [本文](../samples/step61.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step61/NegotiatePage.razor) |
| 087 | 認証済みドメインユーザー名を扱う | [本文](../samples/step62.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step62/SidPage.razor) |
| 088 | 認証済みユーザー名と既存台帳を結ぶ | [本文](../samples/step63.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step63/UserMappingPage.razor) |
| 089 | 未登録・無効利用者を拒否する | [本文](../samples/step64.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step64/UserRefusalPage.razor) |
| 090 | 既存台帳の業務権限を確認する | [本文](../samples/step65.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step65/RolePermissionPage.razor) |
| 091 | サービス側で認可を強制する | [本文](../samples/step66.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step66/AuthorizedEntryPage.razor) |
| 092 | 部署ごとのCountとItems | [本文](../samples/step67.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step67/UnitScopedPage.razor) |
| 093 | 集計とCSVにも認可を適用 | [本文](../samples/step68.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step68/ScopedExportPage.razor) |
| 094 | 保存直前の権限とxminを再確認 | [本文](../samples/step69.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step69/SecuredEditPage.razor) |
| 095 | Windows Server 2025の実機受入を分ける | [本文](../samples/step70.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step70/DeploymentGatePage.razor) |

## 第8段階：監査・競合制御・Windows Server運用（学習順096〜105）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 096 | 既存台帳のキー正規化・重複拒否と索引 | [本文](../samples/step71.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step71/DirectoryNormalizationPage.razor) |
| 097 | 明示Windowsログインと短命Cookie | [本文](../samples/step72.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step72/ExplicitWindowsLoginPage.razor) |
| 098 | 権限行ロックと更新の原子性 | [本文](../samples/step73.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step73/TransactionalAuthorizationPage.razor) |
| 099 | 本人・操作・結果を永続監査する | [本文](../samples/step74.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step74/DurableAuditPage.razor) |
| 100 | IIS・Kerberos・負荷・障害の実機受入 | [本文](../samples/step75.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step75/IisAcceptancePage.razor) |
| 101 | 二人の編集競合とxmin | [本文](../samples/step76.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step76/ConcurrentEditPage.razor) |
| 102 | キャッシュの有効期限と権限失効 | [本文](../samples/step77.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step77/AuthorizedCachePage.razor) |
| 103 | ヘルスチェック・構造化ログ・運用監視 | [本文](../samples/step78.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step78/DiagnosticsPage.razor) |
| 104 | 実DB・ブラウザー・認可の結合/E2E試験 | [本文](../samples/step79.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step79/EndToEndAcceptancePage.razor) |
| 105 | Windows Server 2025/IISの配布・切替・復旧 | [本文](../samples/step80.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Step80/DeploymentPage.razor) |

## 第9段階：業務アプリの通し演習（学習順106〜110）

| 学習順 | テーマ | 教材 | 画面ソース |
| --- | --- | --- | --- |
| 106 | 業務要件・画面遷移・WinForms移行の設計 | [本文](../course-v3/lessons/capstone-01.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone01Page.razor) |
| 107 | PostgreSQL検索とページング・生成SQL | [本文](../course-v3/lessons/capstone-02.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone02Page.razor) |
| 108 | DTO編集・競合・取消・例外の統合 | [本文](../course-v3/lessons/capstone-03.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone03Page.razor) |
| 109 | 認証済み本人と既存台帳による権限・監査 | [本文](../course-v3/lessons/capstone-04.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone04Page.razor) |
| 110 | E2E・障害復旧・IIS受入表 | [本文](../course-v3/lessons/capstone-05.md) | [Razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone05Page.razor) |

## 詳細照合

この索引と同じ110件の構造化データは [学習項目と画面ソースの対応表](source-map.json) にあります。追加30テーマについては、実画面の有無に加え、実行できる処理、模擬・説明だけの処理、関連するサービスとテスト、負例を調べ、必要な教材だけ更新します。データの `audit_state` は進捗管理の暫定区分であり、合格認定ではありません。

実AD/IIS/Kerberos/FDW、鍵、性能、切戻しの現地試験は [現地実機受入計画](on-site-acceptance-plan.md) の許可と証跡による別の受入です。学習用の操作やCIを実機PASSに換算しません。
