# 学習順109：認証済み本人と既存台帳による権限・監査

## ADと業務権限の担当
Active Directoryは本人認証とログイン名の取得までです。WindowsUserAccessServiceが既存PostgreSQL台帳の有効性、所属部署、閲覧・編集の権限を確認します。Capstone04Page.razorでは認可前提の確認と集計を別々のボタンで行います。集計はBusiness.CountByDoneAsyncを通して部署の完了・未完了件数を取得します。監査の永続化が失敗したら結果を返しません。匿名のChromeでは両ボタンとも拒否されることを確認します。WinForms＋Dapperでログオン名を取得しただけで業務を許可してはいけないのと同様、Blazorでも既存台帳の照合が必要です。ログイン名を画面に表示してデバッグしません。実IIS/NegotiateとKerberos/SPNの本人認証は現地受入に残ります。

## 実コード例1
```csharp
var permit = await Business.AuthorizeAsync();
_status = permit.Unit is null
    ? permit.Message
    : "業務の閲覧権限を確認しました。業務データと本人の名前は表示していません。";
```

## 実コード例2
```csharp
var permit = await AuthorizeAsync(ct: ct);
if (permit.Unit is null) return new([], permit.Message);
await using var db = await factory.CreateDbContextAsync(ct);
var groups = await Visible(db, permit.Unit).AsNoTracking()
    .GroupBy(t => t.IsDone)
    .Select(g => new LearningCountRow(g.Key, g.Count())).ToListAsync(ct);
await audit.WriteRequiredAsync(db, "task.summary", permit.Unit, 0,
    groups.Sum(g => g.Count), "success", ct);
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
/learn/capstone-04で権限前提を確認し、続いて集計を押します。匿名時には双方が拒否され、集計行や本人名が表示されないことを確認します。サービスのAuthorizeAsyncとCountByDoneAsyncの条件を照合します。

## 演習と詳しい解答
**問題1：** AD認証が成功したが台帳が無効な場合は。

**詳しい解答：** 業務を拒否します。本人確認と操作・行単位認可は別です。

**問題2：** 読取りなら監査停止中でも返せますか。

**詳しい解答：** この業務契約では返せません。監査の永続化が成功したときだけ取得結果を返します。

## 完了条件
- [ ] 画面操作と安全拒否を確認する。
- [ ] 実コード2例とWinForms＋Dapperの違いを説明する。
- [ ] 実機未受入とPDF停止を区別する。
