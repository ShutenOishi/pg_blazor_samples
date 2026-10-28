# 学習順051：LINQ検索とAsNoTracking

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L051EfSearchPage.razor)。画面の正規URLは `/learn/051` です。

## 到達目標と前提
LearningDbService.SearchAsyncは、Configuredを確認し、IDbContextFactoryでDbContextを作成します。AsNoTrackingで変更追跡を省き、空でない検索語をTrimしてWhere条件を付けます。CountAsyncで検索条件に一致する総件数を取得し、OrderBy(Id).Take(20).Selectで必要列のLearningTaskRowを作ってToListAsyncします。件数と20行の取得は別のSQLです。両者の処理中に他の更新があれば時点一致は保証されません。IQueryableのままRazorへ返さず、具体化したDTOを返すため、DbContextの寿命も明確です。Containsの大文字小文字の扱いはNpgsqlとDB照合規則を実DBで確認します。
学習順051は第5段階・第11組に含まれます。互換URLは変更せず、画面は `/learn/051` です。Visual Studio 2026と.NET 10、EF Core 10・Npgsql・PostgreSQL 18の構成を前提にします。前のページで学んだFactoryの役割を使い、処理がブラウザー、Webサーバー、DBのどこで行われるのかを説明してください。

## WinForms＋Dapperと実装を比較する
| 観点 | WinForms＋Dapper | Blazor Server＋EF Core | 注意すること |
| --- | --- | --- | --- |
| 検索 | SELECT ... WHERE / Dapper Query | Where＋ToListAsync | 利用者入力をSQLへ連結しない |
| 総件数 | COUNT(*)を別クエリ | CountAsync | 表示行数と異なる |
| 表示上限 | ORDER BY id LIMIT 20 | OrderBy(Id).Take(20) | 安定した順序が必要 |

## ソースコード例1：中心の処理
```csharp
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var query = db.Tasks.AsNoTracking();
if (!string.IsNullOrWhiteSpace(keyword)) {
    var term = keyword.Trim();
    query = query.Where(t => t.Title.Contains(term));
}
int count = await query.CountAsync(cancellationToken);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
        t.StartTime, t.IsDone, t.Version)).ToListAsync(cancellationToken);
```
上記は実装の主要部分を抜き出した例です。補助的なConfigured判定、入力検証や例外処理もソース全体で確認します。接続しているコードと、メモリ上の状態またはSQL文字列だけを扱うコードを分けてください。

## ソースコード例2：画面／対応機構とDapperの違い
```csharp
<MudTextField T="string" Label="課題名に含まれる文字" @bind-Value="_keyword" />
<MudButton OnClick="LoadAsync">DBを検索する</MudButton>
<p role="status" data-testid="step35-status">@_data.Message</p>
<p>総件数：@_data.TotalCount 件／画面取得：@_data.Rows.Count 件</p>
@code {
    private string _keyword = "";
    private LearningTaskResult _data = new("検索前です。", []);
    private async Task LoadAsync() => _data = await Database.SearchAsync(_keyword);
}
```
実際の構成ではRazor画面が入力とイベントを担当し、サービスが操作を実装します。比較用に示したコードが学習画面で実行されるとは限りません。どのメソッドを呼んだか、DBへの副作用があるかを必ず確認します。

## 実行場所・アプリケーション境界
WinFormsでボタンを押すとフォームイベントのC#が通常は利用者PC上で動き、Dapperに渡したSQLはアプリからDBへ送られます。Blazor Interactive ServerではRazorのC#イベントとEF Coreの処理はWebサーバーで動きます。ブラウザーはユーザー操作とHTML表示を受け持ち、直接PostgreSQLへ接続するわけではありません。NetworkタブにWebSocket通信が見えても、その通信がDB SQLを公開しているわけではありません。UIに表示された文字列はサービスが返した結果であり、成功の根拠をDB操作の有無までさかのぼって区別する必要があります。

この教材はWebプロジェクトのData/とServices/だけを使います。学習のためだけにDomain・Infrastructure・Repositoryの別プロジェクトを増設しません。IDbContextFactoryから必要な操作ごとにDbContextを作り、await using等で破棄します。DbContextをBlazorのcircuit全体で長期間共有しません。Npgsqlの物理接続プールとEF CoreのDbContextオブジェクトは寿命が別であることも意識してください。読み取りと変更追跡と保存はそれぞれ独立した概念です。SQLを生成しただけで検索実行を保証することも、追跡状態がAddedになっただけでINSERT完了と扱うこともできません。

## 安全条件と検証範囲
教材でDBを使うときは開発専用PostgreSQL 18、架空データ、専用DDLを使用します。ConnectionStrings__PostgreSQLの値や認証情報はGitに書かず、業務DB・実在利用者情報を演習へ持ち込みません。接続先の設定があることは実DB接続成功ではなく、SQLが生成できることもデータ取得成功ではありません。未設定と正常0件、入力不備、接続障害、更新競合、権限不足は別々の状況です。エラーの詳細はサーバーログから調べ、機密の接続文字列やスタックトレースを利用者へ返しません。

ADからは本人認証とドメイン付きユーザー名だけを取得する前提です。実業務の認可は、事前整備済みDB台帳または外部ビューの最新情報で、各業務操作・部署・対象行ごとにサーバー側で判定します。検索一覧とCount、詳細、CSV、更新のすべてで適用し、画面でボタンを隠すだけにしません。監査記録、権限行ロック、更新競合を同一トランザクションで保証できない操作は安全側に拒否します。FDWなどで原子性が証明できなければ更新を禁止します。application_nameは接続識別の補助であって利用者別の監査証跡ではありません。ここでのLearningDbServiceは認可を組み込んだ本番業務サービスではありません。実AD・IIS・Kerberos・FDW・1万人規模実機の成功を学習用CIから推測しないでください。

## 理解を確かめる確認の順番
最初に画面URLと初期状態を確認し、次に入力とイベント、続けて呼ばれるサービス、DbContextの生成・破棄、DBコマンドの有無、最終的な画面メッセージの順に調べます。問題が起きたら原因を飛ばして再実行するのではなく、未設定・入力検証・SQL生成・接続・DDL・権限・競合のどこまで進んだかを記録します。WinForms＋Dapperの経験を使い、SQLと処理場所を対比することで、同じC#でもWebサーバーと利用者端末では実行位置が違うことを確認してください。

## 実際の操作手順
1. /learn/051を開き『検索前です。』を確認する。
2. 検索語を入力して『DBを検索する』をクリックする。
3. 総件数と取得行数を比べ、最大20件という制約を説明する。
4. DB接続を外した学習用環境で『未設定』表示を確認する。
各手順の完了後には日本語の状態表示を読み、未接続・未設定の状態を成功として扱わないでください。WinFormsのダイアログに慣れていても、サーバーログやDB統合試験とブラウザー画面の証跡を混ぜないことが重要です。

## 障害の切り分け表
| 現象 | 調べる項目 | 判断と対応 |
| --- | --- | --- |
| 20件しか表示されない | Take(20)を確認 | 仕様どおりであり損失ではない |
| 『未設定』と表示 | ConnectionStrings__PostgreSQL | 実DB検索は未実行 |
| 0件で表示 | タイトルと照合条件 | 正常0件と通信失敗を区別 |

## 演習と詳しい解答
**問1：総件数が100で画面取得20は故障か**

**詳しい解答：** 違います。CountAsyncとTake(20)で目的が異なります。次頁は別の仕組みです。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問2：AsNoTrackingで業務認可できるか**

**詳しい解答：** できません。変更追跡の制御であり、所属や行権限の確認は別です。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問3：なぜDTOを返すのか**

**詳しい解答：** 必要な列を取得し、DbContextの寿命を越えて遅延実行しないためです。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

## 次へ進む前の確認
本教材だけを読んでも、入力、処理場所、戻り値、保存の有無、失敗時の対応を説明できるようにしてください。次は `/learn/052` に進みます。現在はPDFの生成・再生成・Preview/正式Release更新を停止しており、このMarkdownとRazor、索引ソースだけが更新対象です。
