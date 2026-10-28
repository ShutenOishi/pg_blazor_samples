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

**学習順052**：現在の[教材](lessons/db-bridge-02.md)から[DbBridge02Page.razor](../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge02Page.razor)へ進み、[LearningDbContext](../../src/PgBlazorSamples.Web/Data/LearningDbContext.cs)と[DB統合テスト群](../../tests/PgBlazorSamples.UnitTests/DbBridgeIntegrationTests.cs)を調べる。ページのToQueryStringはSQL生成であってDB実行ではない。テストがどの実処理を証明するかは検証して記録する。

**学習順107**：現在の[教材](lessons/capstone-02.md)、[Capstone02Page.razor](../../src/PgBlazorSamples.Web/Components/Samples/Capstone/Capstone02Page.razor)、[SecuredLearningService](../../src/PgBlazorSamples.Web/Services/Learning/SecuredLearningService.cs)、[認可付き結合テスト](../../tests/PgBlazorSamples.UnitTests/SecuredLearningIntegrationTests.cs)を開き、匿名拒否・操作/行認可・DB検索の各根拠を独立して確認する。**表示や模擬結果だけで実DB・本番認可をPASSにしない。**

## 段階的な移行記録

- **学習順001の第1段階への配置変更（正式受入済み）**：主Razorを`Components/Lessons/Stage01/L001WebExecutionPage.razor`へ移動。`/learn/001`および`/learn/web-01`を新しいファイルで保持し、教材・source-map・監査JSON・直接索引・静的検査の参照を同期した。共有サービスは変更していない。
- **学習順002の第1段階への配置変更（正式受入済み）**：主Razorを`Components/Lessons/Stage01/L002HttpRequestResponsePage.razor`へ物理移動し、正規`/learn/002`と互換`/learn/web-02`を保持。教材・直接索引・監査JSON・source-map・静的検査を同期し、Chrome試験に両URLのHTTP 200照合を追加。`@inject NavigationManager`はBlazor標準のDIであり、`App.razor`のInteractiveServerとフレームワークJSを利用する。学習用GET 200/404以外のDB・SQL・独自JS・業務サービスは直接呼ばない。ファイル配置と実動作を別に確認し、HEAD `42947aae612cc990f2718975fd65569197eaf1f4` のアプリRun40654626049・PDF Run40654626053の同一HEAD成功をPRコメント #6464044026 で正式受入済み。
- **学習順003の第1段階への配置変更（正式受入済み）**：主Razorと隣接CSS isolationを`Components/Lessons/Stage01/L003HtmlCssFormPage.razor`・`.razor.css`へ一緒に移動。正規`/learn/003`と互換`/learn/web-03`を維持する。`App.razor`で生成CSSバンドルを読み込み、Chromeで両URLのHTTP 200、CSS適用（枠線・入力欄・キーボードフォーカス）と従来の入力検証を個別確認する。教材リンク、source-index、source-map、監査JSON、静的検査を同一差分で同期する。Razorに独自DI、C#サービス/DTO、SQL/独自JS呼出しはなく、`App.razor`のInteractiveServerとBlazorのバインディング/C#イベントを使用。配置・参照先の静的検査とChrome動作を独立に確認し、HEAD `3cb5ee03f54c6009ecba9441d1ecfcfddebd57c7`・アプリRun40657167557・PDF40657167596の全成功をPRコメント #6464402707 で正式受入済み。
- **学習順004の第1段階への配置変更（正式受入済み、PRコメント #6464528813）**：主Razorを`Components/Lessons/Stage01/L004DomNetworkConsolePage.razor`へ移動。正規`/learn/004`と互換`/learn/web-04`を維持し、教材・索引・監査・静的検査を同期。Chromeで正規/互換のDocument HTTP 200および既存のDOM追加削除・対話イベントのDocument再取得なしを検証する。Razorは標準のInteractiveServerとC#イベントを使用し、個別のDIサービス・DTO・SQL・独自JSは直接呼ばない。静的参照存在、Chrome試験、現地受入を混同しない。
- **依存索引（今回の変更時点）**：001〜005は既存のChrome操作、CSS、App.razor等の実在する参照先を登録。041はJSモジュール・C#補助・xUnit・Chromeの参照を登録し直した。静的登録は個別試験の新しい合格を意味しない。
- 物理配置を変更した主Razorは学習順001〜010の10件、未移行100件。001〜004は正式受入済み、005〜010は新HEADの全必要CI判定待ち。教材Markdownの物理配置、残りの段階フォルダー、110件の意味的な依存追跡は引き続き未完了。`related_sources`は実際の参照だけを登録し、ファイル存在を実動作の受入と読み替えない。


### 2026-10-28 学習順005〜010バッチ移行（新HEAD CI判定待ち）

- 第1段階005・第2段階006〜010の主Razor計6件を一度に移動。ルート定義とC#イベントは維持、009のStatic SSR専用コンポーネント、Program.cs、共通サービスは移動しない。
- 教材6件・直接索引・source-map・implementation-auditの005/009・静的検査の配置規則を同期。正規URLと互換URLは両方保持する。
- Gitツリー上の配置・参照先実在は静的証拠。Windows/Ubuntu/PG18/Chrome/PDFは新HEADのCIで確認し、現地実機受入や手動デバッガー実施と区別する。
- 新HEADの必須CI全成功後、005〜010をまとめてPR #39へ正式受入コメントで記録する。011〜020も依存確認後5〜10件の単位で進める。成功済みCIの手動再実行はしない。

## 移行順と合格条件

- [ ] **監査**：110件すべての教材→Razor→C#/SQL/JS→テストの実際の参照関係を記録し、未検証を明示する。
- [ ] **設計**：段階/ファイル名の規則と共通コードの責務を確定し、不要な移動・分割を避ける。
- [ ] **小単位の移行**：学習順または段階単位で物理配置を変える。同じPR差分でRazorの参照、namespace、静的ファイル、教材、目次、全リンク、検査を修正する。
- [ ] **自動検査**：110件の入口の一意性、リンク先の実在、重複・旧番号の利用者向け表示、互換URL、テストの対応を検査する。
- [ ] **実動作受入**：各新HEADのWindows/Linuxビルド、xUnit、Chrome、PostgreSQL18学習用実DBの必要なCIがSUCCESSであることを確認する。成功済みCIは再実行しない。
- [ ] **安全**：ADは本人認証のみ。既存DB台帳の都度認可、権限失効、監査失敗、xmin競合、FDW原子性不明時の安全拒否は維持。実機SITE PASS 0/15・NOT_RUN 15/15は無許可で変更しない。正式PDF Releaseは全残作業の受入前に更新しない。

記録では **「画面が動く」「対応コードが読める」「構造が理解しやすい」「単体・実DB試験の根拠がある」「現地実機受入」** を別々の項目として判定し、完成率を混同しない。
