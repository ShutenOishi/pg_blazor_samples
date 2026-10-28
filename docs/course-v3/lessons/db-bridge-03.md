# 学習順053：Dapperの接続／ExecuteとDbContext・追跡・SaveChanges

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge03Page.razor)。画面の正規URLは `/learn/053` です。

## 到達目標と前提
DbBridge03Page.razorはDB非接続で状態の変化を再現します。new LearningTaskはDetachedです。db.Tasks.Add(entity)後はAddedです。ChangeTracker.Clearで追跡が解除されDetachedへ戻ります。SaveChangesAsyncは呼びません。DapperのExecuteはSQLを実行する操作ですが、EF CoreのAddはDbContextのメモリ上で『追加する予定』を記録するだけです。実際の登録・更新・削除は次の学習順054でSaveChangesAsyncを呼んで初めてDBコマンドを送ります。Blazorの複数イベントでDbContextを共有しないこと、状態を持つEntityと表示DTOを区別すること、追跡解除とDELETEを混同しないことが重要です。
学習順053は第5段階・第11組に含まれます。旧ステップURLは変更せず、画面は `/learn/053` です。Visual Studio 2026と.NET 10、EF Core 10・Npgsql・PostgreSQL 18の構成を前提にします。前のページで学んだFactoryの役割を使い、処理がブラウザー、Webサーバー、DBのどこで行われるのかを説明してください。

## WinForms＋Dapperと実装を比較する
| 観点 | WinForms＋Dapper | Blazor Server＋EF Core | 注意すること |
| --- | --- | --- | --- |
| 新規Entity | POCOをnew | Detached | オブジェクト生成は未保存 |
| 追加予定 | INSERTの準備 | Added | SaveChanges前は未実行 |
| DB確定 | ExecuteAsyncでSQL実行 | SaveChangesAsync | DB結果を確認 |
| 追跡解除 | DapperのSQLとは無関係 | ChangeTracker.Clear | DELETEではない |

## ソースコード例1：中心の処理
```csharp
using var db = Factory.CreateDbContext();
var entity = new LearningTask { Title = "追跡の練習" };
var before = db.Entry(entity).State; // Detached
db.Tasks.Add(entity);
var added = db.Entry(entity).State; // Added
db.ChangeTracker.Clear();
var after = db.Entry(entity).State; // Detached
state = $"生成：{before}／Add後：{added}／Clear後：{after}（保存なし）";
```
上記は実装の主要部分を抜き出した例です。補助的なConfigured判定、入力検証や例外処理もソース全体で確認します。接続しているコードと、メモリ上の状態またはSQL文字列だけを扱うコードを分けてください。

## ソースコード例2：画面／対応機構とDapperの違い
```csharp
// LearningDbService.AddAsyncでは実際に次を実行する（開発DBのみ）
await using var db = await factory.CreateDbContextAsync(ct);
db.Tasks.Add(new LearningTask { Title = title,
    DueDate = DateOnly.FromDateTime(DateTime.Today),
    StartTime = TimeOnly.FromDateTime(DateTime.Now) });
await db.SaveChangesAsync(ct);
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
1. /learn/053を開いて未実行表示を読む。
2. 『追跡状態を再現する』をクリックする。
3. Detached→Added→Detached（保存なし）を確認する。
4. DbContext追跡とPostgreSQLの永続化を区別する。
各手順の完了後には日本語の状態表示を読み、未接続・未設定の状態を成功として扱わないでください。WinFormsのダイアログに慣れていても、サーバーログやDB統合試験とブラウザー画面の証跡を混ぜないことが重要です。

## 障害の切り分け表
| 現象 | 調べる項目 | 判断と対応 |
| --- | --- | --- |
| AddedなのにDB行がない | SaveChanges呼び出し | この画面は呼ばない |
| Clear後も保存されたと思う | 追跡の意味 | Detachedとなるだけ |
| 接続設定がなくても動く | Factoryの追跡実験 | SQL未実行で正常 |

## 演習と詳しい解答
**問1：Addedなら登録済みか**

**詳しい解答：** いいえ。追跡上の予定です。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問2：ClearはDBのDELETEか**

**詳しい解答：** いいえ。メモリ中の追跡を解除するだけです。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問3：circuitとDbContextを同じ寿命にしてよいか**

**詳しい解答：** 避けます。並列実行で同じコンテキストを共有しないためです。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

## 次へ進む前の確認
本教材だけを読んでも、入力、処理場所、戻り値、保存の有無、失敗時の対応を説明できるようにしてください。次は `/learn/054` に進みます。現在はPDFの生成・再生成・Preview/正式Release更新を停止しており、このMarkdownとRazor、索引ソースだけが更新対象です。

## 実DBでのDapper ExecuteとEF Core SaveChanges比較

[比較サービス](../../../src/PgBlazorSamples.Web/Services/Learning/L053PersistenceComparisonService.cs)を追加しました。元のDB未接続の追跡観察とは別に、専用PostgreSQL18でDapperのパラメーター化INSERT（Execute）、取引内SELECT、EF CoreのAdd→SaveChangesとAdded→Unchangedを実行します。架空のタイトルは内部で生成し、どちらの取引もロールバックします。新しいDbContextでレコードが残っていないことを確認します。これは書込み方法の学習用試験であり、永続化や業務認可を確認するものではありません。

[単体・結合テスト](../../../tests/PgBlazorSamples.UnitTests/L053PersistenceComparisonTests.cs)では未設定・Windows認証有効時の拒否と、PostgreSQL18専用ジョブでの取引内件数・変更追跡・ロールバックを検証します。通常のChromeではDB未設定時の拒否のみを確認します。学習画面での成功表示から実AD/IIS/FDWや業務台帳の認可・監査が成立したと推定しません。
