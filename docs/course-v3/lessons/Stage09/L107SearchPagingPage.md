# 学習順107：PostgreSQL検索とページング・生成SQL

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L107SearchPagingPage.razor)。画面の正規URLは `/learn/107` です。

## SQLの変換とDB実行を分ける
Capstone02Page.razorのSQL表示はFactory.CreateDbContextでLINQ式を組み立て、ToQueryStringで文字列を得る処理です。架空の部署DEMOはSQLの例示にだけ使い、本人の所属部署ではありません。「権限付き一覧を取得する」はBusiness.LoadPageAsyncを呼び、AuthorizeAsyncで実際の本人・台帳・監査を確認してからDBアクセスします。Visible(db, permit.Unit)のqueryへ検索条件を重ね、同じqueryでCountAsyncとID昇順のSkip/Takeを計算します。画面は1ページ最大10件で前後ページ操作ができ、サービスのMaxPageSizeは50件です。前後ページでは同じSecuredLearningService.LoadPageAsyncへ0始まりのページ番号を渡します。操作ごとに本人・台帳と部署・監査を再評価するため、画面に残るページ番号を認可の代用にしません。WinForms＋Dapperの場合もWHERE句の認可条件を外したCountは情報漏えいになります。EF Coreが自動SQLを生成しても認可が自動で付くわけではありません。

## 実コード例1
```csharp
using var db = Factory.CreateDbContext();
var keyword = _keyword.Trim();
_sql = db.Tasks.AsNoTracking()
    .Where(t => t.UnitCode == "DEMO" && t.Title.Contains(keyword))
    .OrderBy(t => t.Id).Take(10)
    .Select(t => new { t.Id, t.Title }).ToQueryString();
```

## 実装・試験の直接リンク
[認可付き一覧の本体](../../../../src/PgBlazorSamples.Web/Services/Learning/SecuredLearningService.cs)、[本人と台帳の照合](../../../../src/PgBlazorSamples.Web/Services/Learning/WindowsUserAccessService.cs)、[永続監査](../../../../src/PgBlazorSamples.Web/Services/Learning/LearningAuditService.cs)、[学習用DbContext](../../../../src/PgBlazorSamples.Web/Data/LearningDbContext.cs)、[自部署12件/2ページと実監査のPG18試験](../../../../tests/PgBlazorSamples.UnitTests/SecuredLearningIntegrationTests.cs)、[SQL生成とxminの別PG18試験](../../../../tests/PgBlazorSamples.UnitTests/CapstoneIntegrationTests.cs)、[匿名時Chrome拒否](../../../../scripts/browser-smoke.cjs)を参照します。

## 実コード例2
```csharp
var query = Visible(db, permit.Unit).AsNoTracking();
if (!string.IsNullOrWhiteSpace(request.Keyword))
    query = query.Where(t => t.Title.Contains(request.Keyword.Trim()));
var total = await query.CountAsync(ct);
var rows = await query.OrderBy(t => t.Id)
    .Skip(checked(request.Page * request.PageSize)).Take(request.PageSize)
    .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
        t.StartTime, t.IsDone, t.Version)).ToListAsync(ct);
```


## 全画面に適用する業務設計と安全条件
本教材はVisual Studio 2026、.NET 10、Blazor Interactive Server、EF Core 10、PostgreSQL 18による架空の課題管理業務を扱います。既存WinForms＋Dapperの画面イベントからSQLを実行していた処理をWeb化する際には、Razorの表示状態、サーバーの業務サービス、データベース、Windows認証と業務認可を別の責任として設計します。Razorの表示やボタン非表示は業務許可の根拠になりません。ブラウザーから入力された部署・ID・検索語・版番号を信頼してSQLのアクセス範囲を決めず、サーバーで都度確認します。

Active Directoryから利用するのは本人認証と信頼できるWindowsログイン名だけです。部署、操作権限、行単位の公開範囲は既存DB台帳を正本とします。WindowsUserAccessServiceで本人と既存台帳を対応付け、SecuredLearningServiceの業務入口で閲覧・編集権限を毎回確認します。認証済みでも権限台帳が無効・重複・部署不一致・操作権限不足であれば業務行や件数を返しません。ブラウザー側でユーザー名を入力させて本人認証とする設計も禁止します。

一覧では、許可された部署のqueryを基に同じWHERE条件でCountとItemsを求め、安定したID順でページを取得します。実DBの操作は画面が直接行わず、共通サービスが永続監査を保存できたときだけ応答します。監査DBが停止したときは読み取りも拒否してデータや件数を返しません。保存では画面を開いた時点の権限を信用せず、保存直前に最新の編集権限を照合します。DB台帳の権限行を同じトランザクションでFOR UPDATEロックし、xminに対応するVersionを比較し、課題更新と監査INSERTを原子的に確定します。正本がFDWや別DBでロック・原子性を保証できない場合は安全拒否します。

## WinForms＋Dapperとの比較
| 観点 | WinForms＋Dapper | Blazor Interactive Server |
| --- | --- | --- |
| イベント | フォームからSQLを呼び出す | Razorが認可済み業務サービスへ委譲 |
| 本人 | Windowsログオン情報を扱う | 信頼できるWindows認証済み主体を取得 |
| 業務権限 | DB台帳への照会を組み込む | 既存DB台帳の操作・行単位を毎操作照合 |
| 検索 | WHEREとCOUNTを組み立てる | 同じ認可queryからCountとItemsを計算 |
| 更新 | Dapper Executeとトランザクション | xmin、権限行ロック、監査INSERTを一体化 |
| 失敗 | SQL例外や競合を画面に示す | Forbidden・Conflict・Errorを安全に扱う |
| 運用 | デスクトップ配布とDB接続 | IIS、AD/Kerberos、鍵、circuitを受入 |

## 障害の切り分け
| 症状 | 確認先 | 対応 |
| --- | --- | --- |
| SQLが表示されたのに業務行が得られない | ToQueryStringとLoadPageAsync | SQL生成を業務認可の証拠としない |
| 認証済みでも拒否される | Windows設定、既存台帳、部署、監査 | 失効や不一致を許可に読み替えない |
| Countだけ多い | 部署・検索条件とqueryの再利用 | 件数も権限外データとして公開しない |
| 競合で保存できない | Detail DTOのVersionとxmin | 旧DTOを破棄して再取得 |
| 監査を書けない | WriteRequiredAsyncとトランザクション | 更新も読取結果も返さない |
| 実IISのみ失敗 | SPN、証明書、Data Protection鍵、接続プール | 自動CIの成功とは別に実機検証する |

## 検証手順と受入の区分
1. 第9段階の概要から学習順106〜110へ進み、画面の操作とサーバー処理の関係を確認します。
2. 匿名のChrome操作で一覧・詳細・集計が拒否されることを確認し、正常な検索0件と拒否の0件を区別します。
3. Razorの実イベントを読み、SecuredLearningServiceとWindowsUserAccessServiceを経由するか確認します。認可経路を省略するテスト用の例外を追加しません。
4. xUnitとPostgreSQL18実DBでは、架空台帳・架空行を使う検索、ページング、部署分離、xmin競合、取消と監査の安全条件を確認します。
5. Windows/Linux Release、xUnit、Chrome、PostgreSQL18の成功はそれぞれテストしたコミットの証拠として扱い、旧HEADのCIを別HEADへ代用しません。
6. 画面の結果、期待値、実際のメッセージ、対象のサービス、コミットSHA、CI Run IDを記録します。拒否条件を緩和して検査を通すことはしません。

## 実機では別途受入が必要
実AD、IIS、Kerberos/SPN、HTTPS証明書、Data Protection鍵・監査の保管と復旧、FDWの実際の原子性、1万人超登録・同時circuit・DBプールと切戻し負荷は現地の証跡が未完了です。画面のチェック欄やCIの架空Principalで実ADを受け入れたと扱わないでください。PDFの生成・再生成とPreview Release・正式Releaseの更新はユーザーが明示的に再開指示するまで禁止します。PDF目次のPythonソースは同期しても、PDF自体や旧Previewの生成元SHAは更新しません。

## 個別の操作手順
/learn/107で「生成SQLだけを確認する」を押し、SELECTを確認します。続いて認可付き一覧を押し、匿名なら拒否・総件数0・表示0で前後ページが押せないことを確認します。認可された架空利用者の実DB試験はSecuredLearningIntegrationTestsで自部署12件を10件と2件に分け、他部署の行を含まないこと、2ページのCount一致、一覧操作の永続監査記録と失効後の拒否を確認します。CapstoneIntegrationTestsの未認可なDbContext直接クエリはSQL/ページングそのものの別試験であり、権限付きサービスの証拠とは区別します。

## 演習と詳しい解答
**問題1：** SQLを表示できたら業務SELECTの許可が得られたことになりますか。

**詳しい解答：** なりません。ToQueryStringはSQL生成だけで、実DBの認可・監査の検査ではありません。

**問題2：** Countだけ部署範囲を外すとどうなりますか。

**詳しい解答：** 行が見えなくても他部署の件数が漏えいします。同じ認可queryからCountとItemsを取得します。

## 完了条件
- [ ] 画面操作と安全拒否を確認する。
- [ ] 実コード2例とWinForms＋Dapperの違いを説明する。
- [ ] 実機未受入とPDF停止を区別する。
