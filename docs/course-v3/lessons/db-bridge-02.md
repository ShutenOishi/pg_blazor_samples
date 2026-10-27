# 学習順052：SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge02Page.razor)。画面の正規URLは `/learn/052` です。

## 到達目標と前提
DbBridge02Page.razorは入力した架空キーワードをTrimしてLINQを作り、ToQueryStringでSQLを生成する画面です。「LINQからSQLを生成する」の操作ではDBへの問い合わせを行いません。「学習DBで実際に検索する」の操作では隔離した学習用PostgreSQL 18を検索し、最大10件を表示します。WhereはSQLのWHERE、Categoryナビゲーションの参照は関連テーブルとの結合、OrderByとThenByはORDER BY、Take(10)は上限の指定につながります。Idで並び順を確定させ、ページングの行順が変動しないようにします。CategoryIdは任意なので、Category!というC#記号でDBのNULLが消えるわけではありません。生成されたJOINの形とNULLの扱いは確認が必要です。ContainsとDapperのILIKEは必ずしも同等条件になりません。SQL生成結果はSQLが実行された証拠でも実行計画でもありません。
学習順052は第5段階・第11組に含まれます。旧ステップURLは変更せず、画面は `/learn/052` です。Visual Studio 2026と.NET 10、EF Core 10・Npgsql・PostgreSQL 18の構成を前提にします。前のページで学んだFactoryの役割を使い、処理がブラウザー、Webサーバー、DBのどこで行われるのかを説明してください。

## WinForms＋Dapperと実装を比較する
| 観点 | WinForms＋Dapper | Blazor Server＋EF Core | 注意すること |
| --- | --- | --- | --- |
| WHERE | SQLのWHEREと値パラメーター | WhereとContains | 条件の大小文字差を確認 |
| JOIN | LEFT JOINとON | Categoryナビゲーション | NULLを検証 |
| ORDER BY | c.name,id | OrderByとThenBy | Idで同順位を解消 |
| 実行 | Dapper QueryAsync | ToQueryString／ToListAsync | 生成SQL表示と実DB検索を区別 |

## ソースコード例1：中心の処理
```csharp
using var db = Factory.CreateDbContext();
var term = keyword.Trim();
sql = db.Tasks.AsNoTracking().Where(task => task.Title.Contains(term))
    .OrderBy(task => task.Category!.Name).ThenBy(task => task.Id)
    .Select(task => new { task.Id, task.Title, Category = task.Category!.Name })
    .Take(10).ToQueryString();
state = "SQL生成のみ完了しました。DBには接続していません。";
```
上記は実装の主要部分を抜き出した例です。補助的なConfigured判定、入力検証や例外処理もソース全体で確認します。接続しているコードと、メモリ上の状態またはSQL文字列だけを扱うコードを分けてください。

## ソースコード例2：画面／対応機構とDapperの違い
```csharp
// Dapperの比較用参考例。学習画面では実行しない。
const string sql = """
SELECT t.id, t.title, c.name
FROM learning_task t
LEFT JOIN learning_category c ON c.id = t.category_id
WHERE t.title ILIKE @Keyword
ORDER BY c.name, t.id
LIMIT 10
""";
// connection.QueryAsync<Row>(sql, new { Keyword = "%架空%" });
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
1. /learn/052を開く。
2. 架空の検索語を入力し「LINQからSQLを生成する」を押す。
3. WHERE・結合・並べ替えとパラメーターを確認する。
4. SQL文字列だけでは実行結果を検証できないことを説明する。
5. 隔離した学習DBを設定した場合に限り「学習DBで実際に検索する」を押す。専用DB未設定なら拒否メッセージを確認する。
6. DB検索時は件数と表の表示を確認し、正常0件・入力不備・接続障害を区別する。
各手順の完了後には日本語の状態表示を読み、未接続・未設定の状態を成功として扱わないでください。WinFormsのダイアログに慣れていても、サーバーログやDB統合試験とブラウザー画面の証跡を混ぜないことが重要です。

## 学習DBを使った実検索

`LearningDbService.Configured`による学習DB設定とWindows認証無効の確認を経て、Razorイベントで次の検索を実行します。

```csharp
await using var db = await Factory.CreateDbContextAsync();
var rows = await db.Tasks.AsNoTracking()
    .Where(task => task.Title.Contains(term))
    .OrderBy(task => task.Category!.Name).ThenBy(task => task.Id)
    .Select(task => new { task.Id, task.Title,
        Category = task.Category == null ? "未分類" : task.Category.Name })
    .Take(10).ToListAsync();
```

この`ToListAsync`はDBへ実際のSQLを送る点が`ToQueryString`と異なります。実装では検索語1〜100文字を検証し、例外の詳細や接続情報を利用者へ出さず、失敗と0件を別に表示します。Windows本人認証を有効にした環境では匿名の学習用DB検索を拒否します。統合試験は専用DBの架空データでWHERE・任意カテゴリのJOIN・件数・重複の有無を検査します。

## 障害の切り分け表
| 現象 | 調べる項目 | 判断と対応 |
| --- | --- | --- |
| SELECTが出ても行が表示されない | ToQueryStringの役割 | 正常、SQL生成のみ |
| Categoryがnull | 任意のCategoryId | !ではDB NULLを解消しない |
| 検索語の扱いが異なる | ContainsとILIKE | 生成SQLで比較する |

## 演習と詳しい解答
**問1：ToQueryStringだけで実DB結果を確認できるか**

**詳しい解答：** できません。SQLの構築を確認するだけです。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問2：C#の!はDB NULL制約か**

**詳しい解答：** 違います。C#の静的解析の扱いで、DB制約ではありません。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問3：IdのThenByは何のためか**

**詳しい解答：** 同じカテゴリ名が複数あるときの安定順を確保するためです。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

## 次へ進む前の確認
本教材だけを読んでも、入力、処理場所、戻り値、保存の有無、失敗時の対応を説明できるようにしてください。次は `/learn/053` に進みます。正式PDF Releaseは全残工程を受入後に更新します。
