# Blazor教材改訂計画 v2：WinForms＋Dapper経験者がWeb開発を初めて学ぶために

状態：**計画案。既存の実装済み80ステップは変更しない。** 対象：Visual Studio 2026、.NET 10、Blazor Web App（Interactive Server）、MudBlazor＋MudBlazor.Translations、EF Core 10/Npgsql、PostgreSQL 18、Windows Server 2025/IIS。

## 1. 読者像と到達目標

- C#・WinForms・イベント・DataGridView・Dapper・SQL・業務DB操作を理解しているが、HTML/CSS、HTTP、ブラウザー、ASP.NET Core、Blazorの経験がない。
- 最終目標：社内ドメインでの業務Webアプリを、画面→サービス→PostgreSQL→AD本人認証・既存DB認可→監査・運用まで理解し、自分で小規模なものを作成・デバッグできる。
- DIとEF Coreは初学者にも分かるように説明する。EF Coreを採用するがDapperを全面禁止とはしない。必要なSQLはEF CoreのSQL APIまたは既存Dapperと比較し、理由を示して選択する。
- 大規模な層別プロジェクト増設はしない。従来どおりWeb内のData/Servicesを主とし、学習用ApplicationとUnitTestsは維持する。

## 2. 現状の確認（2026-10-26、mainを再確認してから作業）

- 公開済み：既存Step01〜80と5ステップごとの復習コラム、日本語PDF単体のRelease。
- 80個のdocs/samples/stepNN.mdのファイルサイズ中央値は約2.25KB、29個は2KB未満。例えばStep11・17・19・22・24・25の説明は相対的に短い。
- Step31〜40の教材Markdownにコードフェンスがなく、画面・サービスと生成SQLをPDFだけで追うのは難しい。逆にStep41〜50は主題ごとのコード例があるため、この水準を基準に改善する。
- HTML/CSS/HTTP/Networkタブ、SSRとInteractive Serverの違い、SignalR、事前レンダリング、ブラウザー側の保存・JS相互運用などを、原理から順序立てて教える章が不足。
- DapperとEF Coreの対応付けが不足。従来のSQL・DTO・接続/トランザクション・パラメーターがEF Coreでどう置き換わるかが不明瞭。
- 内容整合性の即時修正対象：PDF生成ソースの表紙にStep75と残っている一方、実際の収録は80まで。docs/learner-guide.md本文に「01〜50」と残っている。docs/sample-catalog.mdの66〜70に「計画中」と残っているなど、実装済み状態と表示が食い違う。ドキュメントの一律置換だけでなく実際の受入範囲も検証する。

## 3. 改訂方針

1. **先に現行80ステップを読める教材へ拡充**する。番号追加より優先し、既存のサンプル実コードと学習者向け日本語PDFを一致させる。
2. **新規30テーマを6組×5テーマ**で追加し、最終的に110個の学習ステップと、5ステップごとの通常コラム22本＋従来の特別コラム3本を目安とする。重複が判明したテーマは統合・変更し、110を絶対条件にしない。
3. **現在の /learn/01〜/learn/80 は維持**する。新教材は意味のあるID（web-01など）を付け、学習順番号は別データのカリキュラム表から表示する。既存リンクを一括変更しない。完全移行時は旧ステップとの対応表を用意する。
4. **同じ業務テーマを段階的に拡張**する。架空の「課題・申請・承認」サンプルを用い、DBなし→画面→実DB→検索/編集→認可/監査→配布へ成長させる。デモが複数システムに散らばらないようにする。
5. 理解に必要な**最小限のHTML/CSSとブラウザーDevToolsを、MudBlazorの前に教える**。JavaScriptの大規模フロントエンド開発は要求しない。必要な場合だけJS interopへ進む。

## 4. 新しく追加する30テーマ（提案）

追加は合計30個。教材へ入れる位置は現行番号の直後ではなく、以下の推奨学習順を採用する。各組の5番目に復習コラムを追加する。

| グループ | 提案ID | 追加テーマ | 推奨位置と確認する成果 |
| --- | --- | --- | --- |
| A Web基礎 | web-01 | WinFormsイベントとWebのブラウザー/サーバーの実行場所 | 現行Step01の前。処理場所を図で説明 |
| A | web-02 | URL、HTTP GET/POST、要求/応答、ステータスコード | 同上。Networkで操作を確認 |
| A | web-03 | HTMLの意味・フォーム・CSSの最小基礎とレスポンシブ | 同上。HTMLとCSSを変更 |
| A | web-04 | ブラウザー開発者ツール、DOM、Network、Console | 同上。通信と描画を観察 |
| A | web-05 | Visual StudioのWeb起動、HTTPS、Kestrel、ブレークポイント | 同上。Webをデバッグ |
| B Blazor実行原理 | runtime-01 | Static SSRとInteractive Serverを比較する | 現行Step03〜04の間。静的なボタンと対話可能なボタンを比較 |
| B | runtime-02 | SignalRとcircuit、再接続、サーバーで動くイベント | 現行Step05以降。通信を切断して観察 |
| B | runtime-03 | 事前レンダリング、二度の初期化と安全なデータ取得 | 現行Step09と隣接。実行回数を比較 |
| B | runtime-04 | URL・履歴・画面状態・ブラウザー更新時の消失 | 現行Step10〜14付近。検索条件の維持を練習 |
| B | runtime-05 | 保存すべき状態と保存しない状態（circuit/ブラウザー/DB） | 現行Step14の後。回線断・再読込 |
| C ブラウザーを使う実務UI | browser-01 | JS interopを使う場面と使わない場面 | 現行Step29〜30後。必要最小限のJS |
| C | browser-02 | ファイルアップロード、容量・形式・検証・一時保存 | 同上。架空の添付ファイル |
| C | browser-03 | CSV/PDFダウンロードとブラウザーへの引渡し | 同上。架空の出力 |
| C | browser-04 | ErrorBoundary、例外、取消、再試行とユーザー表示 | 現行Step17/28/50とつないで学ぶ |
| C | browser-05 | HTMLセマンティクス、CSS分離、フォーカスと拡大表示試験 | MudBlazor UI総合の直後に確認 |
| D DapperからEF Core | db-bridge-01 | DapperとEF Coreの責任・向き不向き・同じ検索の実装比較 | 現行Step31直前 |
| D | db-bridge-02 | SQLのWHERE/JOIN/ORDER BYとLINQ・生成SQLの対応 | Step35/46の前後 |
| D | db-bridge-03 | Dapperの接続/ExecuteとEFのDbContext・追跡・SaveChanges | Step34〜38の間 |
| D | db-bridge-04 | 既存DB、読み取り専用ビュー、キーなしモデル、外部テーブル | Step40/63の前後 |
| D | db-bridge-05 | FromSql/SqlQuery、パラメーター、Dapper併用・使い分け | Step40の直後 |
| E Webセキュリティと配備基礎 | security-web-01 | Windows認証・Cookie・HTTP・circuitの関係 | 現行Step61より前 |
| E | security-web-02 | ASP.NET Core Middlewareの順序、HTTPS・IIS経路 | 同上。Program.csのリクエスト経路を追う |
| E | security-web-03 | XSS・CSRF・SQL注入とフォーム/DBでの対策の境界 | 同上。否定試験 |
| E | security-web-04 | 開発/検証/本番の設定、シークレット、Data Protection鍵 | 同上。環境別起動 |
| E | security-web-05 | ブラウザーのセッション消失・再接続と認可の再確認 | Step61〜65への橋渡し |
| F 業務アプリの通し演習 | capstone-01 | 要件と画面遷移、WinFormsからの移行設計 | 既存Step80の後。小規模な業務画面 |
| F | capstone-02 | PostgreSQLの検索・絞り込み・ページングと生成SQL | 同上。実DB一覧 |
| F | capstone-03 | DTO入力・更新・競合・取消・例外を一画面にまとめる | 同上。完全な編集動作 |
| F | capstone-04 | AD認証済み本人と既存DB権限、部署・操作・永続監査 | 同上。成功/拒否の試験 |
| F | capstone-05 | 結合/E2E・ログ・障害復旧・IIS配布の受入票 | 同上。CIと現地受入を分離 |

## 5. 既存80ステップの配置見直し

- 元のStep01〜10は基本操作の題材として残すが、web基礎の後に学ぶ。Step09のライフサイクルをruntime-03の前後に置き、事前レンダリングを誤解しないようにする。
- 元のStep11〜20ではDI全般をすべて必修で深掘りするのでなく、まずライブラリ登録→注入→Scoped/circuit→テストの順で示す。元Step19のApplication分離は「学習のための1回限りの分離」と位置付け、EFのための新しい層別プロジェクトを必須にしない。
- 元のStep21〜30より前にHTML/CSS入門を置く。Step10のEditFormとStep24のMudFormは比較表を追加し、役割の重複を活用する。
- db-bridge教材を元Step31〜40に差し込み、Dapperの接続、SQL、DTO、トランザクションを対応させる。元Step37のRange/Multirangeなど高度な型は基本CRUD/日付の後に移す。
- 一覧編は「小さいItems→標準MudTable ServerData→標準MudDataGrid ServerData→検索ソート→編集→例外/競合→仮想化/キーセット/高度な集計」という順に改善。高度なページングを標準と誤解させない。
- 元Step57と77のキャッシュ、54と76の排他、70・75・80のIISは、各ステップの役割を「入門」「認可を伴う実務」「運用受入」に分け、同じ説明を繰り返さない。
- 元Step61〜80のAD認証、既存DBの正本、登録UI不要、権限剥奪、監査の同一トランザクション、安全な拒否は必ず維持する。FDWで更新の原子性を保証できないときは更新しない。

## 6. 全既存ステップ共通の文章拡充仕様

各ステップはPDFだけでも概念・操作・コードの流れが理解できるよう、以下を**実際のサンプルと同じ動作**で記述する。

1. このステップが必要な理由。WinFormsの既知の考え方との対比（対応する点・対応しない点）。
2. 新用語の短い定義。ブラウザー、C#、サーバー、DBのうち**どこで**動くかを明記。
3. 完成画面と成功/失敗の期待結果、具体的な入力値、実行前提と環境構築方法。
4. 自己完結した**実行可能な最小コード**。Razorの表示箇所、必要な型・DI登録・サービス・SQLなど抜粋の不足を防ぐ。省略がある場合は明示し、参照ファイル名に依存しない解説にする。
5. 処理の順序（ブラウザー→イベント→サービス→DbContext→SQL→結果→UI）を番号付きで解説。WinForms版/Dapper版と比較するテーマでは横並びのコード例。
6. 書き換えて学ぶ練習課題、答え、誤り例、頻出例外と確認するログ・DevTools。
7. 単元終了時に「どういう場面で使う／使わない／次に何を学ぶ」を確認。5ステップごとに小さな統合演習と解答。
8. PDFに開発管理用のGitHubやPR等を混ぜない。ただし**実行に必要なコードとファイル構造の説明は削らない**。ソース全文の機械的転記ではなく、学習に必要な最小の一式を揃える。

字数で合否を決めず、読者がPDFだけで処理の動作を説明し、開発環境で復元できるかを受入条件にする。まず説明の薄いDI 11〜20、MudBlazor 21〜30、EF Core 31〜40を優先して書き直す。

## 7. 修正工程（実装順）

| PR群 | 最優先の作業 | 完了条件 |
| --- | --- | --- |
| 0. 事実整合 | PDF表紙（75→実際の80）、学習者ガイドの50記載、一覧の66〜70等の状態を事実に合わせる。CIと実機受入の区別を残す | PDFとWeb目次、進捗表の表示内容が一致 |
| 1. Web橋渡し | web-01〜05＋5ステップの復習コラムを作り、現行Step01へ自然につなぐ | Network/DevToolsを読者が操作・説明できる |
| 2. 最も薄い教材拡充 | Step11〜20、21〜30、31〜40の既存教材を5本単位で改稿（順次全80本へ展開） | 実コード・操作・エラー・解答をPDFで読める |
| 3. 描画とWeb UI | runtime-01〜05とbrowser-01〜05を適切な位置へ挿入 | prerender・circuit・失効/再接続・JS/ファイルが検証可能 |
| 4. Dapper橋渡し | db-bridge-01〜05と、既存EF 31〜60のSQLとの対比を補強 | 同一SQLをDapper/EFで比較し方式を説明できる |
| 5. セキュリティの段差 | security-web-01〜05を61〜80の前に挿入。既存認可ゲートを緩めない | HTTPS/CSRF/設定/認証/認可の違いを説明できる |
| 6. 総合演習 | capstone-01〜05と復習コラムを追加 | 架空の一業務アプリで画面・EF・認可・監査・テストを一貫して示す |
| 7. 運用品質 | 全110項目の相互参照・IDと表示順・PDF目次・コード・ブラウザー/E2E整合を監査。PDF単体配布 | Win/Linux・Chrome・PostgreSQL18・日本語PDFが保護PR/mian postmergeで成功。実AD/IIS受入は別途 |

各PRは**5ステップ＋対応するコラム＋ソース・日本語教材・PDF・テスト**を単位とし、大規模な一括改番・大量の空ステップ・テキストだけの「実装済み」は禁止する。既存教材だけの文章改善PRは関連テストとPDF整合チェックを必須にする。

## 8. 読者視点での受入指標

- 新しく登場した要素を、定義・処理場所・使う理由・WinForms/Dapperとの相違の四つで説明できる。
- すべての主要ステップで、PDFに記載したコードだけから小さな実例を再現でき、実装ソースとの差がない。
- セットアップ（Webのみ/DBあり/ADあり）を区別し、DB/ADがない場合も理解できる範囲を明記する。
- 確認問題は「成功操作」だけでなく、再接続・競合・権限剥奪・監査失敗・SQLの実行結果を含む。
- 原則、画面は日本語。翻訳はMudBlazor.Translationsと日本語カルチャを使用し、コードコメント・エラーも日本語。
- 実ブラウザーの操作・単体・PostgreSQL18の否定試験を継続。CIの認証偽装を実ADの受入と呼ばない。
- Windows Server 2025/IIS、Kerberos、FDWの現地受入をCIに混ぜて「成功」にしない。
- PDFは単体のblazor_learning_textbook_ja.pdfとして公開しZIP化しない。

## 9. 計画の参照元

- 現在の実装一覧：docs/sample-catalog.md
- 現在の学習順：docs/learning-path.md
- 現在の教材作成契約：docs/sample-specification.md
- PDF生成：scripts/build_docs_pdf.py
- Microsoft Learn .NET10 Render Modes: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0
- Microsoft Learn Server state: https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-10.0
- Microsoft Learn EF Core SQL: https://learn.microsoft.com/en-us/ef/core/querying/sql-queries
