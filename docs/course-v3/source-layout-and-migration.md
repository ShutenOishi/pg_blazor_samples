# 学習順001〜110：ソースコード構造の教材化と配置改善計画

## 対象と受入の区別

この教材はWeb画面の操作だけでなく、GitHubで実際のソースを開き、処理の入口からテストまで追うための教材でもある。ソースの構造・命名・依存関係自体が分かりにくければ改善する。

**本資料は構造監査・段階移行の台帳。全110件の依存追跡・物理移行完了や現地実機受入を示すものではない。** 正本は[Issue #38](https://github.com/ShutenOishi/pg_blazor_samples/issues/38)と[カリキュラム](curriculum.json)。学習順001〜110を唯一の学習番号として使う。

## 現在確認できる構造上の課題

| 現行の場所 | 内容 | 学習者にとっての課題と改善候補 |
| --- | --- | --- |
| [Samples/Step01](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L006FirstBlazorPage.razor) から Samples/Step80 | 未移行の既存画面の格納 | 物理ディレクトリの番号が現在の学習順と一致しない。例：L006FirstBlazorPage.razorは学習順006。学習順のファイル名・段階分類へ段階的に移行する |
| [Samples/WebFoundation](../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L005VisualStudioDebugPage.razor)、Runtime、Browser、DbBridge、Security、Capstone | 追加画面のうち未移行分 | 学習番号ではなくテーマ内番号で名前が始まる。9段階・学習順でソースを直接探せる一貫した命名へ統一する |
| [docs/samples](../samples/step01.md) と [course-v3/lessons](lessons/web-01.md) | 80件と30件で分かれた本文 | 入口が二つに分かれている。学習順ごとの本文パスへ整理する。移動時はPDF目次、相対リンクと静的検査も同期する |
| [Services/Learning](../../src/PgBlazorSamples.Web/Services/Learning/SecuredLearningService.cs)、[Data](../../src/PgBlazorSamples.Web/Data/LearningDbContext.cs) | 複数教材で共用する処理 | 何を共用し、どの画面から呼ぶかが教材の索引だけでは分からない。依存先を追加し、必要な責務分離を検討する |
| [UnitTests](../../tests/PgBlazorSamples.UnitTests/DbBridgeIntegrationTests.cs)、[database](../../database/schema/001_learning_task.sql)、[JS](../../src/PgBlazorSamples.Web/wwwroot/js/browser-learning.js) | 単体・結合・SQL・ブラウザー処理 | 画面から正常系・拒否系の試験と実装を一緒に見つけられるようにする |

以上は**確認された配置と監査対象**を示す。個々のサービス・テストがどの学習順と直接対応するかは、呼出しとテストコードを調べて確定する。見た目の近さだけで対応を推測しない。

## 目標とするファイル構成（未実装の例）

次は候補であり、まだ存在するとは限らない。

```text
src/PgBlazorSamples.Web/
  Components/Lessons/
    Stage01/
      L001WebExecutionPage.razor
      L002HttpRequestResponsePage.razor
      L003HtmlCssFormPage.razor
      L003HtmlCssFormPage.razor.css
      L004DomNetworkConsolePage.razor
    Stage02/
      L006FirstBlazorPage.razor
    Stage05/
      L052SqlAndLinqPage.razor
  Services/Learning/
    （責務ごとに整理した共通の業務・学習サービス）
  Data/
    （複数の教材で共用するDbContext・モデル）
  wwwroot/js/
    （明示的にJSを必要とする学習用モジュール）
docs/course-v3/
  lessons/
    001-web-execution.md
    006-first-blazor.md
    052-sql-and-linq.md
  reviews/  （復習22組）
  stages/   （段階概要9件）
  source-map.json
tests/
  （実装単位・機能単位の単体/結合テスト）
database/
  schema/
  seed/
  tests/
```

### 配置規則

1. **学習入口は学習順**：Razor/教材の主ファイル名には3桁の学習順を置き、9段階のフォルダーから探せるようにする。旧番号の読み替えを要求しない。
2. **共有コードは機能名で命名**：複数の学習項目が使う認可・DBアクセス・DTO・監査を学習順ごとに複製しない。共有サービスに不用意な学習番号を付けず、教材から実際の呼出箇所へリンクする。
3. **イベント→サービス→DB/JS→テストをたどる**：Razorのコードから、必要な依存登録、DTO、EF Core/SQL、JavaScript、テストに進める。DBや認可を使わない学習項目に架空の依存を増やさない。
4. **実例と説明用コードを区別**：実装抜粋は実際の署名・戻り値・失敗時動作と一致させる。模擬判定、SQL生成のみ、実DB接続、実AD/IIS/FDWは別扱いにする。
5. **短い学習単位の維持**：内容に関係のない巨大Razorや、意味のない多層抽象化は避ける。UIとビジネスルールの分離は必要な範囲で行う。共通化のための重複も避ける。
6. **移行と互換性を区別**：内部ファイル名やnamespaceは改善可能だが、既存の公開URLの別名は維持する。移動したパスを参照するリンク、CSS、JS、DI、テスト、PDF目次ソースを同期し、監査証跡は書き換えない。

## 学習者がソースを追うための確認例

**学習順006**：[教材](../samples/step01.md)から[L006FirstBlazorPage.razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L006FirstBlazorPage.razor)へ移動し、ページのルート、表示のHTML、学習順とファイル名の不一致を観察する。学習順006の名前と実ファイルが一致することを確認する。

**学習順052**：現在の[教材](lessons/db-bridge-02.md)から[L052SqlAndLinqPage.razor](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L052SqlAndLinqPage.razor)へ進み、[LearningDbContext](../../src/PgBlazorSamples.Web/Data/LearningDbContext.cs)と[DB統合テスト群](../../tests/PgBlazorSamples.UnitTests/DbBridgeIntegrationTests.cs)を調べる。ページのToQueryStringはSQL生成であってDB実行ではない。テストがどの実処理を証明するかは検証して記録する。

**学習順107**：現在の[教材](lessons/capstone-02.md)、[Capstone02Page.razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone02Page.razor)、[SecuredLearningService](../../src/PgBlazorSamples.Web/Services/Learning/SecuredLearningService.cs)、[認可付き結合テスト](../../tests/PgBlazorSamples.UnitTests/SecuredLearningIntegrationTests.cs)を開き、匿名拒否・操作/行認可・DB検索の各根拠を独立して確認する。**表示や模擬結果だけで実DB・本番認可をPASSにしない。**

## 段階的な移行記録

- **学習順001の第1段階への配置変更（正式受入済み）**：主Razorを`Components/Lessons/Stage01/L001WebExecutionPage.razor`へ移動。`/learn/001`および`/learn/web-01`を新しいファイルで保持し、教材・source-map・監査JSON・直接索引・静的検査の参照を同期した。共有サービスは変更していない。
- **学習順002の第1段階への配置変更（正式受入済み）**：主Razorを`Components/Lessons/Stage01/L002HttpRequestResponsePage.razor`へ物理移動し、正規`/learn/002`と互換`/learn/web-02`を保持。教材・直接索引・監査JSON・source-map・静的検査を同期し、Chrome試験に両URLのHTTP 200照合を追加。`@inject NavigationManager`はBlazor標準のDIであり、`App.razor`のInteractiveServerとフレームワークJSを利用する。学習用GET 200/404以外のDB・SQL・独自JS・業務サービスは直接呼ばない。ファイル配置と実動作を別に確認し、HEAD `42947aae612cc990f2718975fd65569197eaf1f4` のアプリRun40654626049・PDF Run40654626053の同一HEAD成功をPRコメント #6464044026 で正式受入済み。
- **学習順003の第1段階への配置変更（正式受入済み）**：主Razorと隣接CSS isolationを`Components/Lessons/Stage01/L003HtmlCssFormPage.razor`・`.razor.css`へ一緒に移動。正規`/learn/003`と互換`/learn/web-03`を維持する。`App.razor`で生成CSSバンドルを読み込み、Chromeで両URLのHTTP 200、CSS適用（枠線・入力欄・キーボードフォーカス）と従来の入力検証を個別確認する。教材リンク、source-index、source-map、監査JSON、静的検査を同一差分で同期する。Razorに独自DI、C#サービス/DTO、SQL/独自JS呼出しはなく、`App.razor`のInteractiveServerとBlazorのバインディング/C#イベントを使用。配置・参照先の静的検査とChrome動作を独立に確認し、HEAD `3cb5ee03f54c6009ecba9441d1ecfcfddebd57c7`・アプリRun40657167557・PDF40657167596の全成功をPRコメント #6464402707 で正式受入済み。
- **学習順004の第1段階への配置変更（正式受入済み、PRコメント #6464528813）**：主Razorを`Components/Lessons/Stage01/L004DomNetworkConsolePage.razor`へ移動。正規`/learn/004`と互換`/learn/web-04`を維持し、教材・索引・監査・静的検査を同期。Chromeで正規/互換のDocument HTTP 200および既存のDOM追加削除・対話イベントのDocument再取得なしを検証する。Razorは標準のInteractiveServerとC#イベントを使用し、個別のDIサービス・DTO・SQL・独自JSは直接呼ばない。静的参照存在、Chrome試験、現地受入を混同しない。
- **依存索引（今回の変更時点）**：001〜005は既存のChrome操作、CSS、App.razor等の実在する参照先を登録。041はJSモジュール・C#補助・xUnit・Chromeの参照を登録し直した。静的登録は個別試験の新しい合格を意味しない。
- 物理移行001〜030の30件は正式受入済み（021〜030はHEAD `e8f53fc0f699f6726349e9ac86edf685c6e5cd4d`、PRコメント #6465382248）。031〜040の第4段階一括移行は正式受入済み（HEAD 5f637d8e5ae731885fadc118ab39769bac414345、PRコメント #6465703916）。教材Markdownの物理配置、残りの段階フォルダー、110件の意味的な依存追跡は引き続き未完了。`related_sources`は実際の参照だけを登録し、ファイル存在を実動作の受入と読み替えない。


### 2026-10-28 学習順005〜010バッチ移行（正式受入済み）

- 第1段階005・第2段階006〜010の主Razor計6件を一度に移動。ルート定義とC#イベントは維持、009のStatic SSR専用コンポーネント、Program.cs、共通サービスは移動しない。
- 教材6件・直接索引・source-map・implementation-auditの005/009・静的検査の配置規則を同期。正規URLと互換URLは両方保持する。
- Gitツリー上の配置・参照先実在は静的証拠。Windows/Ubuntu/PG18/Chrome/PDFは新HEADのCIで確認し、現地実機受入や手動デバッガー実施と区別する。
- 学習順005〜010はHEAD `dc02bd231fdd94b3256c9fd0b88ef796d667c28d` のアプリRun40661222283（Windows/Ubuntu/PG18）・PDF Run40661222230の全SUCCESSを確認し、PRコメント #6465003839 で6件を一括正式受入済み。既存成功CIは再実行しない。

### 2026-10-28 学習順011〜020の10件一括物理移行（新HEAD CI判定待ち）

- Stage02へ次の10主Razorを配置し、旧主Razorを削除した。011 L011TwoWayBindingPage、012 L012ConditionalRenderingPage、013 L013ComponentParametersPage、014 L014EventCallbackPage、015 L015CircuitReconnectPage、016 L016ComponentLifecyclePage、017 L017PrerenderInitializationPage、018 L018ValidatedFormPage、019 L019UrlHistoryStatePage、020 L020StatePersistenceChoicePage。すべて3桁学習順で命名する。
- 013はStep07/GreetingCard、014はStep08/TopicPicker、016はStep09/LifecycleTracker、018はStep10/PracticeEntryを実際に参照する。子部品と入力モデルは元の場所に残し、主画面の移行だけを理由に複製しない。
- 015は既存L015CircuitCounter・xUnit・隔離Chromeの依存を維持。017は標準描画ライフサイクルとChrome試験、019はNavigationManagerと履歴試験、020は標準IJSRuntimeによる非機密localStorage保存とChrome試験の既存動作を変更しない。実AD/実Cookie/業務DB受入は示さない。
- 教材10件、source-index、source-map、implementation-auditの対象4テーマ、静的検査のStage02共通配置規則、Chromeでの正規/互換Document GETを同じ変更単位で同期。静的参照存在と実挙動、実DB/現地受入は別証拠。
- 新HEADのWindows/Ubuntu Chrome/PG18/PDFの全必要CIがcompleted/SUCCESSとなった場合のみPR #39に正式受入を記録する。既存成功CIの手動再実行、正式PDF Release更新を行わない。

## 移行順と合格条件

- [ ] **監査**：110件すべての教材→Razor→C#/SQL/JS→テストの実際の参照関係を記録し、未検証を明示する。
- [ ] **設計**：段階/ファイル名の規則と共通コードの責務を確定し、不要な移動・分割を避ける。
- [ ] **小単位の移行**：学習順または段階単位で物理配置を変える。同じPR差分でRazorの参照、namespace、静的ファイル、教材、目次、全リンク、検査を修正する。
- [ ] **自動検査**：110件の入口の一意性、リンク先の実在、重複・旧番号の利用者向け表示、互換URL、テストの対応を検査する。
- [ ] **実動作受入**：各新HEADのWindows/Linuxビルド、xUnit、Chrome、PostgreSQL18学習用実DBの必要なCIがSUCCESSであることを確認する。成功済みCIは再実行しない。
- [ ] **安全**：ADは本人認証のみ。既存DB台帳の都度認可、権限失効、監査失敗、xmin競合、FDW原子性不明時の安全拒否は維持。実機SITE PASS 0/15・NOT_RUN 15/15は無許可で変更しない。正式PDF Releaseは全残作業の受入前に更新しない。

記録では **「画面が動く」「対応コードが読める」「構造が理解しやすい」「単体・実DB試験の根拠がある」「現地実機受入」** を別々の項目として判定し、完成率を混同しない。

### 2026-10-28 学習順021〜030の第3段階10件一括物理移行（正式受入済み）

- 主Razor10件を`Components/Lessons/Stage03/L021〜L030*.razor`へ移行し、旧主ファイルを削除。正規`/learn/021〜030`と互換`/learn/11〜20`を維持した。
- 10件の教材直接リンク・source-index・source-map・静的検査・Chromeの両URL GETを同期。027はRazorクラス名変更に合わせ`ILogger<L027AsyncLoggingPage>`へ変更した。
- `Step11/DiOverviewPage`、`Step14/SharedCounter`等の共有子コンポーネント、DI登録、サービス、Application契約、xUnit試験は移動・複製しない。source-mapの関連ソース登録は静的な参照であり、全件の意味的依存追跡や実動作受入の完了を示さない。
- HEAD `e8f53fc0f699f6726349e9ac86edf685c6e5cd4d` のアプリRun40663589348とPDF Run40663589353の全必要CI成功をPRコメント #6465382248 で正式受入。実機PRE/SITE未実施、SITE PASS 0/15、PR Draft/未マージ、正式PDF Release変更なし。
- 次の候補は031〜040の第4段階。主Razor・子Razor・MudBlazor/DIと各教材を確認し、一括移行できる範囲を決める。

### 2026-10-28 学習順031〜040の第4段階10件一括物理移行（新HEAD CI判定待ち）

- 主Razor10件を`Components/Lessons/Stage04/L031〜L040*.razor`へ移動し、旧`Samples/Step21〜Step30`の主Razor10件を削除。正規`/learn/031〜040`と互換`/learn/21〜30`は維持。
- 教材10件・source-index・source-mapの依存参照・静的検査・Chromeの正規/互換Document GETを同期。既存MudBlazor日本語翻訳、共通DI・Provider、ThemePreferenceServiceの実装や共有業務処理を変更・複製しない。
- 既存Chrome操作（通知、Drawer、入力、検証、Table、DataGrid、ダイアログ、Snackbar、テーマ、架空一覧）と今回追加したGET10件を、新HEAD Windows/Linux/xUnit/PG18/PDFの全必要CI completed/SUCCESS後のみ受入。成功CIは手動再実行しない。
- 追加30テーマの実装受入30/30と構造移行は別指標。SITE PASS 0/15・NOT_RUN 15/15、実AD/IIS/FDWと10,001人負荷は未受入、PR Draft/未マージ、正式PDF Release未変更。次は041〜050のJSとDB依存を先に監査する。

### 2026-10-28 学習順041〜050の主Razor10件一括構造移行（同一HEADのCI判定待ち）

- 第4段階041〜045は`Components/Lessons/Stage04/L041〜L045*.razor`、第5段階046〜050は`Components/Lessons/Stage05/L046〜L050*.razor`へ主Razorを物理移動。Browser/DbBridge/Step31〜34の旧主Razorのみ削除し、既存`@page`の正規/互換ルートは保持。
- 041の`ILogger<L041JsInteropPage>`を改名に同期。044は既存共有子`BrowserFaultDemo`のnamespaceを`@using`で明示。045のCSS isolationは新Razorと同名で隣接配置。元のJSモジュール、共用C#サービス/DTO/テスト、学習用PostgreSQL・DDL、Program.cs/DI、認証・認可・監査の安全拒否は変更も複製もしない。
- 10教材の直接リンク、source-index、source-mapと明示依存、追加30テーマ監査台帳の041〜046、静的検査、Chrome正規/互換10組のGETを同一バッチで同期。静的パス存在と実DB・Chrome・現地受入は別の評価。
- 現時点は**新コミットの必要CI判定待ち**であり、構造移行50/110の正式受入は未宣言。全必要CIと全ジョブcompleted/SUCCESSを独立GETで確認した場合のみ受入する。
- 041の実切断後InvokeAsync自身、043のExcel/LibreOffice、044の実離脱取消、実AD/IIS/FDW、10,001人超の負荷・復旧、PRE/SITEは未実証。SITE PASS0/15・NOT_RUN15/15、Draft/未マージ、正式PDF Release未変更、旧成功CIの再実行禁止を維持。


### 2026-10-28 学習順051〜060の第5段階主Razor10件一括移行（同一HEAD CI受入待ち）

- 学習順051〜060の10主Razorを`Components/Lessons/Stage05/L051〜L060*.razor`に配置し、旧`Samples/Step35〜Step40`・`Samples/DbBridge`内の対応する主画面10件を削除。正規URLと互換URLは保持する。
- 教材10件・source-index・source-map・追加30テーマ監査台帳の4件・静的検査・Chrome正規/互換10組を同期。共通LearningDbService・DbContext、学習専用PostgreSQL/SQL、L053/L058/L059サービス、DI、xUnit、安全拒否は変更・複製しない。
- `related_sources`は実在する関連ファイルの静的な手掛かりであり、意味的依存をすべて実証したことや実DB・本番SITEの受入を意味しない。
- **新HEADのWindows/Linux/Chrome/PG18およびPDFソース検証CIがすべてcompleted/SUCCESSになるまでは正式受入しない**。成功済みCI再実行、PDF生成・正式Release更新、PR Draft解除・マージ、現地無許可PRE/SITE実施は禁止。SITE PASS0/15・NOT_RUN15/15を維持。


### 2026-10-28 学習順061〜070の第6段階主Razor10件一括移行（同一HEAD CI受入待ち）

- 学習順061〜070の10主Razorを`Components/Lessons/Stage06/L061〜L070*.razor`へ配置し、旧`Samples/Step41〜Step50`の該当する主Razorのみ削除。第6段階概要`Samples/Step41/Stage06OverviewPage.razor`は移動しない。正規`/learn/061〜070`・互換`/learn/41〜50`の双方を保持。
- Items固定一覧、MudTable/MudDataGrid ServerData、ホワイトリストによるソート、複合フィルター、DTO、キーセット、Virtualize、デバウンスは既存LearningPageService・LearningAdvancedPageService、LearningDbService、DbContext、DTO、DB統合試験とブラウザー試験を共用する。共有サービス・認証/認可/監査・SQL・DI登録・テストは変更も複製もしない。
- 教材10件の直接リンク、source-index、source-mapの関連実在ファイル、静的検査の旧主Razor固定参照、Chromeの正規/互換10組のDocument GETを同一変更で同期。関連ファイルの静的実在は意味的な依存・DB実操作・行認可・本番SITEの受入を保証しない。
- **新HEADのWindows/Linux/Chrome/PG18とPDFソース検証の全必要CIがcompleted/SUCCESSになるまでは正式受入せず、60/110件を正式受入値に維持する。** 成功済みCIを再実行せず、Draft解除・mainマージ・正式PDF Release更新・未承認のPRE/SITE試験を禁止する。SITE PASS 0/15・NOT_RUN 15/15を維持。

### 2026-10-28 学習順071〜080の第6段階主Razor10件一括移行（新HEAD CI判定待ち）

- 編集フォーム・ダイアログ・一括処理・xmin競合・詳細DTO・期間Range・架空権限キャッシュ・DB集計/CSV・性能観察・方式選択の主Razor10件を `Components/Lessons/Stage06/L071〜L080*.razor` に配置し、旧主Razor10件を削除した。正規 `/learn/071〜080` と互換 `/learn/51〜60` は維持。
- 学習順072の子 `Components/Samples/Step52/EditDialog.razor` は元の位置に維持し、新主ページに子コンポーネントの `@using` を追加。既存子Razorを複製・改名しない。
- `LearningEditService`、`LearningRangeExportService`、`LearningPerformanceService`、`LearningCacheLessonService`、`LearningDbService`、`LearningDbContext`、DTO/SQL/DI/Program.cs/xUnit/安全拒否を変更しない。DB統合試験・Chromeの既存動作根拠を配置試験や現地受入と混同しない。
- 教材10件の直接リンク、source-index、source-mapのRazorパス・関連実在ソース、静的検査の第15/16組、Chrome正規・互換URLのDocument GET 20経路を同一コミットで同期。
- 全必要CI（Windows/Ubuntu Chrome/PostgreSQL18/PDFソース）が新HEADで completed/SUCCESS となるまでは構造移行の正式受入を70/110に維持する。成功済みCIの再実行、Draft解除、mainマージ、正式PDF Release更新、実AD/IIS/FDW・未承認PRE/SITE試験は禁止。SITE PASS 0/15・NOT_RUN 15/15。

### 2026-10-28 学習順081〜090の第7段階主Razor10件一括物理移行（新HEAD CI判定待ち）

- 学習順081〜085のセキュリティ演習主Razorと086〜090のWindows本人認証・既存台帳照合主Razorを `Components/Lessons/Stage07/L081〜L090*.razor` に配置し、旧主10件を削除する。元の `Security` 段階概要・第17組復習や `Step61〜65` の共用処理、`@page` の正規URL・互換URLは維持する。
- 081〜085は固定の架空Principal/隔離HTTP・安全拒否、086〜090は共通の `WindowsUserAccessService` による認証済みユーザーと既存台帳の照合を再利用する。共通DI/Program.cs・サービス・DB/SQL・監査・xUnit・安全拒否は一切変更・複製しない。
- 教材10件の直接Razorリンク、source-index、source-map、監査台帳の対象5件、Python静的検査の旧物理パス、Chrome正規・互換のDocument GET検証を同一変更で同期する。静的参照の存在と実AD/IIS/Kerberos/FDW、DB実操作、監査・現地SITEの受入は混同しない。
- 新HEADでWindows/Ubuntu実Chrome/PostgreSQL18/PDFソース検証の**全必要CI completed/SUCCESS**がそろうまで構造移行の正式受入は80/110のままとする。既存成功CI再実行・Draft解除・マージ・正式PDF Release更新・無許可PRE/SITE試験は禁止。SITE PASS **0/15**・NOT_RUN **15/15** を維持。
