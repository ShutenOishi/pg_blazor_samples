# 第10組の復習：DapperとEF Coreの入口

## 学習順046〜050をつないで理解する
第10組はDapperとEF Coreの使い分け、EntityとDTO、PostgreSQLのDDL、DI登録、DbContextFactoryとNpgsql接続プールを扱いました。個別のAPI名を暗記するだけでは、実際の画面で発生した障害を原因別に分類できません。Razorからサービスへ呼び出す順番、DBが不要な操作と必要な操作、戻り値を表示する場所を一続きで説明できることを目標とします。

WinFormsではフォームイベントからDapperでQueryやExecuteを呼び出していたかもしれません。Interactive Serverではブラウザーでの操作を受け、サーバー側のコンポーネントがサービスを呼び、Factoryが操作用DbContextを生成します。サービスは入力の検証や件数上限を担い、PostgreSQLへの接続はNpgsqlを経由します。さらに本番業務では、DBアクセスの前に本人認証とは別の業務認可と監査が必要です。

## 5テーマの比較表
| 学習順 | 実装上の主題 | WinForms＋Dapperの経験と結ぶ点 | 間違えやすいこと |
| --- | --- | --- | --- |
| 046 | 方式の選定ボタン | SQLとQuery/Execute、LINQとSaveChanges | ボタンは実DBへ接続しない |
| 047 | LearningTaskとLearningTaskRow | SELECT結果のPOCOと表示用DTO | 固定DTOの表示はDBからの取得ではない |
| 048 | learning_taskのDDL | SQLの列・キー・索引・NULL | xminをCREATE TABLEで追加しない |
| 049 | AddDbContextFactoryとUseNpgsql | 接続文字列の設定と利用 | Configured=trueは接続成功ではない |
| 050 | SearchAsyncとawait using | 操作単位の接続と破棄 | DbContext再利用と物理接続プールは別 |

## コードで処理境界を確認する
最初のコードは学習順050の画面にある実装の主要部分です。入力を受けたイベントでDB接続文字列を組み立てていないことに注目してください。返された文字列を画面へ表示するのみです。

```razor
@inject PgBlazorSamples.Web.Services.Learning.LearningDbService Database
<MudButton OnClick="LoadAsync">接続して一覧を取得</MudButton>
<p role="status" data-testid="step34-result">@_message</p>
@code {
    private string _message = "まだ実行していません。";
    private async Task LoadAsync() =>
        _message = (await Database.SearchAsync()).Message;
}
```

サービス側は設定不足ならNotConfiguredを返し、DBへ接続しません。設定済みでもネットワーク、ロール、DB名、DDLの状態によって失敗する可能性があります。画面は接続失敗を日本語で案内し、接続文字列や詳細な例外を利用者へ出力しません。

```csharp
if (!Configured) return new(NotConfigured, []);
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var query = db.Tasks.AsNoTracking();
int count = await query.CountAsync(cancellationToken);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(
        t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
    .ToListAsync(cancellationToken);
```

この検索の主要部分ではCountAsyncとToListAsyncが別のDBコマンドであり、TotalCountと行の最大数20は異なります。更新の多い業務で件数と明細の時点一致を要求する場合、トランザクションの分離レベルを検討します。SQLがPostgreSQLでどのように実行されるかは次の組で詳しく確認します。

## 画面を連続操作する
1. /learn/046を開き「一般的なCRUD」「既存の複雑なSQL」「大量一覧」を順に押します。DapperとEF Coreの選択理由が切り替わりますが、DBアクセスはありません。
2. /learn/047へ移動し、固定DTOのId、課題名、日付、完了状態を確認します。EntityにはUnitCodeやAvailableDatesなど別の列があることを確認します。
3. /learn/048でDDLの型を確認し、開発専用DBでのみSQLを適用します。SELECT current_database(), current_user;で接続先を確認します。画面を開いただけではDBのテーブルを作りません。
4. /learn/049でDB設定の有無を読み、設定済み表示は接続成功ではないと説明します。接続文字列をGitへ書き込みません。
5. /learn/050で「接続して一覧を取得」を押します。未設定なら未設定案内、設定済みで実DBが利用できれば検索成功の案内を確認します。
6. /learn/column/db-bridge-50へ戻って処理の順番と失敗の分類を説明し、次の学習順051へ進みます。

## 障害の切り分け表
| 現象 | 最初に調べる項目 | 正しい判断 |
| --- | --- | --- |
| モデルが表示されたがDBの行がない | 固定サンプルと実DBの操作の違い | モデル表示だけではDB接続成功ではない |
| 接続先が未設定 | 環境変数ConnectionStrings__PostgreSQL | 未設定ならDB操作を開始しない |
| 接続先は設定済みだが失敗する | Host・Port・ロール・DB名・DDLとサーバーログ | 設定済みと接続成功を分ける |
| 一覧は最大20件 | Take(20)とCountAsync | 上限は仕様、総件数は別 |
| DbContextの同時利用で例外 | circuit内の共有や並列利用 | Factoryから操作ごとに生成する |
| 更新が競合した | xminと現在の対象行 | 無条件に上書きしない |

## 横断演習と詳しい解答
**問題1** DTOから部署列を省けば、部署外の行も安全に扱えますか。**詳しい解答** 省いた列が表示されないだけです。サーバー側で既存DB台帳に基づき行範囲をWHEREに適用し、一覧・件数・詳細・CSV・更新ごとに認可します。単なる画面の制御やDTOの省略では認可を保証できません。

**問題2** DbContextをawait usingで破棄したら毎回TCP接続が切れますか。**詳しい解答** 必ずしも切れません。Npgsqlの物理接続プールはDbContextインスタンスの寿命とは別です。AddDbContextFactoryを使うこととAddPooledDbContextFactoryでContext自体をプールすることも違います。

**問題3** EF Coreのモデルを読み終えたので、PostgreSQLのテーブル定義も正しいと判断できますか。**詳しい解答** 判断できません。DBに存在する列の型・制約・索引とOnModelCreatingを比較し、開発DBでSQLを実行して検証する必要があります。IF NOT EXISTSでSQLが通っても同名の既存表の構成が正しいとは限りません。

**問題4** 50件のDB行から20件返ってきた場合、データ損失ですか。**詳しい解答** Take(20)による件数制限なので、それだけではデータ損失ではありません。TotalCountとRowsの件数を別に表示し、業務画面では適切なページングを選びます。

## 運用に進む前の境界
ADは本人認証とユーザー名の取得に用い、部署・行・業務操作の権限は事前整備済みDBまたは外部台帳を正本にサーバー側で毎操作判定します。教材の匿名LearningDbServiceをそのまま業務データへ流用しません。更新の競合確認、権限行ロック、監査が同一トランザクションで保証できない場合は安全側に拒否します。設定画面やPDFの読了、CIでの模擬成功は実AD、IIS、Kerberos、FDW、1万人以上の負荷を実機受入したことにはなりません。

## 次の学習順
051ではLINQとAsNoTrackingを使った検索の実装を読み、052ではWHERE、JOIN、ORDER BYと生成SQLを比較します。第10組で学んだ短命DbContextの基本を維持して進んでください。
