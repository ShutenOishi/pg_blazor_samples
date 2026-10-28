# 学習順110：E2E・障害復旧・IIS受入表

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage09/L110AcceptanceLedgerPage.razor)。画面の正規URLは `/learn/110` です。

## 自動試験と現地受入
Capstone05Page.razorには教材・実装の一致、CI証跡、拒否の検証、実IIS/AD/Kerberos・負荷・復旧の四項目があります。加えてL110AcceptanceLedgerの読み取り専用未実施一覧を表示し、CIはNOT_VERIFIED、PRE-01〜07はNOT_RUN 7/7、SITE-01〜15はPASS 0/15・NOT_RUN 15/15と明示します。ここでいうNOT_VERIFIEDはGitHub CIが未成功という意味ではなく、画面が現在のHEADとRunを自動照合しないという意味です。正式な実証はGitHub上の同一HEAD CI、現地PRE承認、SITE実測と署名を独立に照合します。すべて選んでも「本番受入を宣言できません」と表示され、チェック欄だけで現地証拠を作らない仕様です。L110AcceptanceLedgerTestsではPRE/SITE件数と全NOT_RUN、すべての自己点検欄を選んでも判定が変わらないことを検証し、Chromeでも22行と独立区分、チェック操作後のPASS0を検査します。Windows Server 2025 Release、Linux Chrome、xUnit、PostgreSQL18のCIは自動検証です。CapstoneIntegrationTestsでは架空12行の検索、10行ページング、xmin競合とRollbackを試します。実IIS、Kerberos/SPN、HTTPS、Data Protection鍵、FDW、1万人超登録・同時circuit・DBプール、切戻しは未受入であり、実機と運用ログで検証します。WinForms＋Dapperのデスクトップ導入試験とWebサービスのサーバー側運用受入を同一視しません。

## 実コード例1
```csharp
private void Check()
{
    _status = L110AcceptanceLedger.AssessSelfCheck(
        _sourceVerified, _ciVerified, _denialsVerified, _serverVerified);
}
```

## 実コード例2
```csharp
var query = a.Tasks.AsNoTracking()
    .Where(t => t.UnitCode == unit && t.Title.StartsWith(prefix));
Assert.Equal(12, await query.CountAsync());
var rows = await query.OrderBy(t => t.Id).Take(10).ToListAsync();
Assert.Equal(10, rows.Count);
var x = await a.Tasks.SingleAsync(t => t.Id == ids[0]);
var y = await b.Tasks.SingleAsync(t => t.Id == ids[0]);
x.IsDone = !x.IsDone;
await a.SaveChangesAsync();
y.Title = prefix + "競合";
await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => b.SaveChangesAsync());
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
実AD、IIS、Kerberos/SPN、HTTPS証明書、Data Protection鍵・監査の保管と復旧、FDWの実際の原子性、1万人超登録・同時circuit・DBプールと切戻し負荷は現地の証跡が未完了です。画面のチェック欄やCIの架空Principalで実ADを受け入れたと扱わないでください。PDF CIの検証と正式Releaseへの反映は別工程です。正式PDF Release learning-textbook（Asset ID 683032765）は更新しません。

## 個別の操作手順
/learn/110で何も選ばず不足を確認します。その後全て選んで実行しても本番受入を宣言できないことを確認します。CIとPDFの結果を別々に記録し、実機項目には未試験でPASSを付けません。

## 演習と詳しい解答
**問題1：** 全CIがSUCCESSならIIS移行も認可できますか。

**詳しい解答：** できません。AD/Kerberos/SPN、鍵、FDW、監査、大量接続・復旧は実機証跡が必要です。

**問題2：** DB停止後、画面に古い行が残る場合は。

**詳しい解答：** 古いDTOの再送、二重実行、監査不能時の漏えいとコミット、circuit・鍵の復旧を個別に確認します。

## 完了条件
- [ ] 画面操作と安全拒否を確認する。
- [ ] 実コード2例とWinForms＋Dapperの違いを説明する。
- [ ] 実機未受入とPDF停止を区別する。
