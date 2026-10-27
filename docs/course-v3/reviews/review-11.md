# 第11組の復習：LINQ・追跡・CRUD・型

## 学習順051〜055を横断する到達目標
第11組は実DBの読取り・SQL生成・メモリの変更追跡・実DBの保存・日付型の値変換を区別する復習です。WinForms＋DapperではQueryAsyncとExecuteAsyncを呼ぶ位置を追っていました。Blazor Interactive ServerではRazorイベントからサービスへ進み、EF Coreの操作とPostgreSQLへのSQL実行の有無を分けて確認します。SQLが表示されただけでSELECT成功と扱わず、Added状態だけで登録済みと判断しません。学習用DBと業務認可付きDBを混同しないことも到達条件です。

| 学習順 | 主題 | DBへの効果 | 見るべき点 |
| --- | --- | --- | --- |
| 051 | AsNoTrackingと検索 | SELECTを実行 | CountAsyncと最大20件は別 |
| 052 | ToQueryString | SQL生成のみ | 実データ取得なし |
| 053 | ChangeTracker | 追跡状態のみ | Detached→Added→Detached、保存なし |
| 054 | CRUD | SaveChangesで書込み | Version/xmin競合と入力検証 |
| 055 | DateOnly/Range | 型変換のみ | [当日,翌日)と最大日付 |

## WinForms＋Dapperとの横断比較
| 観点 | WinForms＋Dapper | EF Core 10 | 確認方法 |
| --- | --- | --- | --- |
| 検索 | SELECT＋Query | LINQ＋ToListAsync | 実DBの行 |
| SQL確認 | SQL文を書く | ToQueryString | 生成SQLだけ |
| 追加予定 | SQLパラメーターの準備 | Added | DB保存ではない |
| 更新確定 | Execute | SaveChangesAsync | xminと例外 |
| 期間 | daterangeリテラル | NpgsqlRange<DateTime> | 境界inclusive/exclusive |

## 検索のコードを読む
```csharp
// LearningDbService.SearchAsyncの主要部分
var query = db.Tasks.AsNoTracking();
int count = await query.CountAsync(ct);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
        t.StartTime, t.IsDone, t.Version)).ToListAsync(ct);
```
実際のメソッドはキーワードのTrimとWhereも行い、Configuredが偽なら未設定と返します。CountとToListは別のSQLなので、Rows.Countの最大20とTotalCountを区別します。検索条件はDB側へ送り、全件をアプリへ移してから絞り込まないようにします。

## 変更追跡と保存の違いを読む
```csharp
// 学習順053の実装：保存しない追跡状態の確認
using var db = Factory.CreateDbContext();
var task = new LearningTask { Title = "追跡の練習" };
var before = db.Entry(task).State; // Detached
db.Tasks.Add(task);
var added = db.Entry(task).State; // Added
db.ChangeTracker.Clear(); // Detached、SaveChangesなし
```
一方の学習順054は実際にSaveChangesAsyncします。現在のxminと画面が持つVersionが一致しなければ拒否し、保存直前の競合もDbUpdateConcurrencyExceptionで区別します。自動再試行で他人の変更を上書きしません。

## 実際の操作と復習の順序
1. /learn/35で架空データを検索し、TotalCountと最大20件の違いを見る。設定不足なら未接続のまま。
2. /learn/db-bridge-02で架空キーワードからSQLを生成し、WHERE・JOIN・ORDER BYを読み取る。DBの結果を取得したとは記録しない。
3. /learn/db-bridge-03で追跡状態を実演し、Detached→Added→Detached（保存なし）を読む。
4. 専用PostgreSQL18を準備して/learn/36で登録・切替・削除する。競合時は再読み込みし、無条件上書きを避ける。
5. /learn/37でDateOnlyの値とNpgsqlRange<DateTime>の半開期間を表示する。ここはDB非接続。
6. 第11組の画面で5件の差と、次の/learn/38で扱うトランザクションを説明する。

## 障害の切り分け
| 現象 | 確認先 | 結論 |
| --- | --- | --- |
| 051で20件のみ | Take(20) | 件数上限 |
| 051と054で未設定 | 接続環境変数 | DB処理していない |
| 052でSELECT文字列だけ | ToQueryString | DB非接続 |
| 053のAdded | ChangeTracker | 保存なし |
| 054で競合 | xmin/Version | 更新拒否が正常 |
| 055で翌日を作れない | DateOnly.MaxValue | 境界の拒否 |

## 横断演習と詳しい解答
**問1：** SQLを生成しただけでPostgreSQLの接続成功を証明できるか。

**詳しい解答：** できません。ToQueryStringはSQL文字列の生成であり、ToListAsyncでの取得もSaveChangesでの保存もしていません。PostgreSQL18の実DB試験を別途確認します。

**問2：** 53のAddedはDBへのINSERTを表すか。

**詳しい解答：** 表しません。メモリ上の追跡状態であり、ClearするとDetachedに戻ります。サンプルはSaveChangesを呼ばず、DBへの副作用はありません。

**問3：** 51のCountAsyncで100件、一覧20件なら欠落したか。

**詳しい解答：** 欠落ではありません。全件数とTake(20)による画面の取得件数は異なります。ページングや画面の取得上限を明示してください。

**問4：** 54のVersionが古い場合、再実行で上書きしてよいか。

**詳しい解答：** いけません。最新の対象行と競合した更新内容を利用者が確認する必要があります。xminは認可や監査の仕組みとは独立しています。

**問5：** 55で当日だけを含めたいとき、範囲の上限へ同じ日を設定するか。

**詳しい解答：** いいえ。翌日の始まりを含まない上限として設定します。NpgsqlRangeの4引数を用い、上限exclusiveを明示します。DateOnly.MaxValueはAddDays(1)の前に拒否します。

**問6：** AD認証済みの利用者には匿名のLearningDbServiceを本番利用させてよいか。

**詳しい解答：** いけません。ADから得るのは本人認証とユーザー名で、対象部署・行範囲・業務権限は既存のDB台帳を正本にサーバー側で毎操作判定します。変更は監査と権限行ロックを原子的に保証し、失敗時は拒否します。application_nameは個人監査の代替になりません。

## 実行場所・アプリケーション境界
WinFormsでボタンを押すとフォームイベントのC#が通常は利用者PC上で動き、Dapperに渡したSQLはアプリからDBへ送られます。Blazor Interactive ServerではRazorのC#イベントとEF Coreの処理はWebサーバーで動きます。ブラウザーはユーザー操作とHTML表示を受け持ち、直接PostgreSQLへ接続するわけではありません。NetworkタブにWebSocket通信が見えても、その通信がDB SQLを公開しているわけではありません。UIに表示された文字列はサービスが返した結果であり、成功の根拠をDB操作の有無までさかのぼって区別する必要があります。

この教材はWebプロジェクトのData/とServices/だけを使います。学習のためだけにDomain・Infrastructure・Repositoryの別プロジェクトを増設しません。IDbContextFactoryから必要な操作ごとにDbContextを作り、await using等で破棄します。DbContextをBlazorのcircuit全体で長期間共有しません。Npgsqlの物理接続プールとEF CoreのDbContextオブジェクトは寿命が別であることも意識してください。読み取りと変更追跡と保存はそれぞれ独立した概念です。SQLを生成しただけで検索実行を保証することも、追跡状態がAddedになっただけでINSERT完了と扱うこともできません。

## 安全条件と検証範囲
教材でDBを使うときは開発専用PostgreSQL 18、架空データ、専用DDLを使用します。ConnectionStrings__PostgreSQLの値や認証情報はGitに書かず、業務DB・実在利用者情報を演習へ持ち込みません。接続先の設定があることは実DB接続成功ではなく、SQLが生成できることもデータ取得成功ではありません。未設定と正常0件、入力不備、接続障害、更新競合、権限不足は別々の状況です。エラーの詳細はサーバーログから調べ、機密の接続文字列やスタックトレースを利用者へ返しません。

ADからは本人認証とドメイン付きユーザー名だけを取得する前提です。実業務の認可は、事前整備済みDB台帳または外部ビューの最新情報で、各業務操作・部署・対象行ごとにサーバー側で判定します。検索一覧とCount、詳細、CSV、更新のすべてで適用し、画面でボタンを隠すだけにしません。監査記録、権限行ロック、更新競合を同一トランザクションで保証できない操作は安全側に拒否します。FDWなどで原子性が証明できなければ更新を禁止します。application_nameは接続識別の補助であって利用者別の監査証跡ではありません。ここでのLearningDbServiceは認可を組み込んだ本番業務サービスではありません。実AD・IIS・Kerberos・FDW・1万人規模実機の成功を学習用CIから推測しないでください。

## 理解を確かめる確認の順番
最初に画面URLと初期状態を確認し、次に入力とイベント、続けて呼ばれるサービス、DbContextの生成・破棄、DBコマンドの有無、最終的な画面メッセージの順に調べます。問題が起きたら原因を飛ばして再実行するのではなく、未設定・入力検証・SQL生成・接続・DDL・権限・競合のどこまで進んだかを記録します。WinForms＋Dapperの経験を使い、SQLと処理場所を対比することで、同じC#でもWebサーバーと利用者端末では実行位置が違うことを確認してください。

## 次の工程
学習順056〜060と第12組復習に進みます。旧80ステップのURLとDraft PR #36を維持します。Windows/Linuxビルド、xUnit、Chrome、PostgreSQL18実DB、安全拒否と静的教材検査は同一HEADで受入を確認します。PDFの生成・再生成、Preview/正式Release更新は明示的な再開指示まで保留します。
