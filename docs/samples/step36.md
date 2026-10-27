# 学習順054：EF Coreの登録・更新・削除

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step36/EfCrudPage.razor)。画面の正規URLは `/learn/054` です。

## 到達目標と前提
学習順054はLearningDbServiceのAddAsync、ToggleAsync、DeleteAsyncを画面から呼び出します。AddAsyncではConfiguredを検査し、Title.Trim後に1〜100文字を検証してからTasks.AddとSaveChangesAsyncを行います。ToggleAsyncとDeleteAsyncは画面のLearningTaskRowからIdとVersionを渡し、対象行を再取得してxminのVersionが一致するか確認します。更新直前の競合はDbUpdateConcurrencyExceptionで拒否し、最新データを無条件で上書きしません。操作後にはSearchAsyncを使い、画面表示を再読み込みします。実際の削除ボタンには確認ダイアログがなく、本番の業務アプリへそのまま転用しないでください。学習用の専用DBだけで操作します。
学習順054は第5段階・第11組に含まれます。旧ステップURLは変更せず、画面は `/learn/054` です。Visual Studio 2026と.NET 10、EF Core 10・Npgsql・PostgreSQL 18の構成を前提にします。前のページで学んだFactoryの役割を使い、処理がブラウザー、Webサーバー、DBのどこで行われるのかを説明してください。

## WinForms＋Dapperと実装を比較する
| 観点 | WinForms＋Dapper | Blazor Server＋EF Core | 注意すること |
| --- | --- | --- | --- |
| 登録 | INSERTとExecute | Add＋SaveChangesAsync | 入力検証 |
| 更新 | UPDATEと旧版条件 | Toggle＋xmin | 競合は拒否 |
| 削除 | DELETEと旧版条件 | Remove＋SaveChangesAsync | 既削除を区別 |
| 一覧更新 | 再Query | SearchAsyncで再取得 | 古いVersionを保持しない |

## ソースコード例1：中心の処理
```csharp
if (!Configured) return NotConfigured;
title = title.Trim();
if (title.Length is < 1 or > 100)
    return "課題名は1〜100文字で入力してください。";
await using var db = await factory.CreateDbContextAsync(ct);
db.Tasks.Add(new LearningTask {
    Title = title, DueDate = DateOnly.FromDateTime(DateTime.Today),
    StartTime = TimeOnly.FromDateTime(DateTime.Now) });
await db.SaveChangesAsync(ct);
```
上記は実装の主要部分を抜き出した例です。補助的なConfigured判定、入力検証や例外処理もソース全体で確認します。接続しているコードと、メモリ上の状態またはSQL文字列だけを扱うコードを分けてください。

## ソースコード例2：画面／対応機構とDapperの違い
```csharp
var task = await db.Tasks.SingleOrDefaultAsync(x => x.Id == id, ct);
if (task is null) return "対象が削除されています。再読み込みしてください。";
if (task.Version != version)
    return "別の利用者が更新しました。再読み込みしてください。";
task.IsDone = !task.IsDone;
await db.SaveChangesAsync(ct);
// DbUpdateConcurrencyExceptionは競合として捕捉する。
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
1. 専用PostgreSQL18に架空のlearning_taskを用意する。
2. /learn/054で課題名を入力して登録する。
3. 一覧再読み込み後、切り替えと削除を操作する。
4. 設定不足・入力不正・競合のメッセージを区別する。
各手順の完了後には日本語の状態表示を読み、未接続・未設定の状態を成功として扱わないでください。WinFormsのダイアログに慣れていても、サーバーログやDB統合試験とブラウザー画面の証跡を混ぜないことが重要です。

## 障害の切り分け表
| 現象 | 調べる項目 | 判断と対応 |
| --- | --- | --- |
| 登録で未設定 | 接続環境変数 | DBへ保存していない |
| 空白入力 | Trimと長さ | 1〜100字に制限 |
| 古いVersion | xminと競合 | 無条件上書きしない |
| DB障害 | PostgreSQLのログとDDL | 内部例外を利用者へ出さない |

## 演習と詳しい解答
**問1：Version不一致でも更新するか**

**詳しい解答：** いいえ。最新状態を取得し確認します。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問2：削除ボタンがあれば本番権限は足りるか**

**詳しい解答：** 足りません。既存台帳による毎操作の認可と監査が必要です。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問3：保存と一覧更新は同じ処理か**

**詳しい解答：** 違います。SaveChangesの後、SearchAsyncで再取得します。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

## 次へ進む前の確認
本教材だけを読んでも、入力、処理場所、戻り値、保存の有無、失敗時の対応を説明できるようにしてください。次は `/learn/055` に進みます。現在はPDFの生成・再生成・Preview/正式Release更新を停止しており、このMarkdownとRazor、索引ソースだけが更新対象です。
