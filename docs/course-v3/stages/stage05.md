# 第5段階の概要：DapperからEF Core 10・PostgreSQL 18へ

## この段階の到達目標
第5段階は学習順046〜060の15本です。WinForms＋DapperでSQLを扱ってきた経験を生かし、Blazor Interactive Serverのサーバー側でEF Core 10とNpgsqlを適切に利用します。第4段階で作ったMudBlazorの入力と一覧の仕組みに、DBから安全に取得した行を結び付けます。この段階の目的はすべてのDapperコードをEF Coreに置き換えることではありません。一般的なCRUD、既存SQL、一覧検索、大量データ、トランザクション、競合検出を要求に応じて選べることです。
WebプロジェクトのDataとServicesを中心に利用し、学習のためだけにDomainやInfrastructureなどの別プロジェクトを追加しません。ブラウザーの表示、Razorイベント、サービス、短命DbContext、Npgsql接続、PostgreSQL実行の境界を説明できることが第一の到達点です。

## 学習順と到達点
| 学習順 | テーマ | 身に付ける判断 |
| --- | --- | --- |
| 046 | DapperとEF Coreの役割・選定 | CRUD、既存SQL、大量一覧を比較 |
| 047 | モデルとDTO | Entityと表示列・追跡を区別 |
| 048 | PostgreSQLの学習テーブル | DDL、date/time/daterange、xmin |
| 049 | EF Core 10をWebに登録 | Configuration、DI、UseNpgsql |
| 050 | DbContextFactoryと接続プール | 操作単位の短命Context |
| 051 | LINQ検索とAsNoTracking | 読み取りクエリと件数上限 |
| 052 | SQLのWHERE／JOIN／ORDER BY | 生成SQLと実行計画 |
| 053 | DapperのExecuteとEF追跡 | SaveChangesまでの状態変化 |
| 054 | 登録・更新・削除 | 入力、保存、失敗の扱い |
| 055 | DateOnly・TimeOnly・Range | Npgsqlの型と範囲境界 |
| 056 | トランザクション | 原子性とコミット |
| 057 | xmin楽観的排他 | 競合時に上書きしない |
| 058 | 既存DB・ビュー・FDW | 既存台帳を読む境界 |
| 059 | FromSql／SqlQueryとDapper併用 | パラメーターを安全に渡す |
| 060 | PostgreSQL 18実接続確認 | 実DBでの動作と失敗を確認 |

## WinForms＋Dapperとの比較
| 従来の考え方 | Blazor＋EF Coreの形 | 注意点 |
| --- | --- | --- |
| フォームイベントでSQL | Razorイベント→Webサービス | C#はサーバーで動く |
| using NpgsqlConnection | FactoryでDbContextを生成しawait using | 物理接続プールとContextの寿命は別 |
| SQLのSELECTをDTOへ変換 | LINQのSelectでDTOへ射影 | 必要列だけ返す |
| WHEREとLIMITを手書き | Where、OrderBy、TakeをSQLに変換 | 実行計画を確認 |
| 条件付きUPDATEを記述 | xminとSaveChangesによる排他 | 業務認可は別に確認 |

## 実装から読むコード
```csharp
// Program.cs：Web内でFactoryをDI登録
builder.Services.AddDbContextFactory<LearningDbContext>(options =>
    options.UseNpgsql(pgApplicationConnection.ConnectionString));
builder.Services.AddScoped<LearningDbService>();
```
起動時のDI登録はDB接続が成功したことを示しません。接続文字列が未設定の場合も説明用の画面は表示できます。PostgreSQLへ実際に問い合わせたときの応答を別に確認してください。

```csharp
// LearningDbService.SearchAsyncの主要部分
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var rows = await db.Tasks.AsNoTracking()
    .OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(
        t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
    .ToListAsync(cancellationToken);
```
上記は検索語とCountAsyncを省略した抜粋です。実装では合計件数と最大20行の一覧を別クエリで取得します。大量一覧はDB側で件数・列数を制限し、後続でServerDataやページングを使う判断につなげます。

## 実際の画面操作
1. /learn/stage/db-bridgeで学習順046〜060の流れを確認します。
2. /learn/046でDapperとEF Coreの選定ボタンを試します。ここでは実DBの検索を行いません。
3. /learn/047で固定DTOとEntityの項目の違いを確認します。DB接続はまだ不要です。
4. /learn/048でDDLの列を見て、開発専用PostgreSQL 18へ必要な場合だけSQLを適用します。業務DBは操作しません。
5. /learn/049で設定の有無を確認し、設定済みが接続成功と同じ意味ではないことを説明します。
6. /learn/050で実DBへの検索を試し、未設定、接続障害、検索成功の違いを確認します。
7. /learn/column/db-bridge-50で第10組を復習し、051へ進みます。第11組は051〜055、第12組は056〜060を復習します。

## 障害の切り分け表
| 症状 | 確認する範囲 | 誤って判断してはいけないこと |
| --- | --- | --- |
| 画面だけ表示できる | DBなしで動くサンプルか | DB接続成功としない |
| ConfiguredはtrueだがDB接続失敗 | Host、Port、ロール、DDL | 環境変数だけで接続を保証しない |
| 日付列の型が一致しない | DDLとOnModelCreating | DateOnlyとdaterangeを混同しない |
| 50件中20件しか表示しない | Take(20) | データ消失とは限らない |
| 同時操作でDbContextが失敗 | circuit上の共有や並列実行 | Scopedを短命とは見なさない |
| 監査や台帳が利用できない | 業務サービスでの安全拒否 | 匿名教材サービスで代行しない |

## 演習と詳しい解答
**問題1** EF Coreを使えばSQLやインデックスを理解する必要はなくなりますか。**詳しい解答** なくなりません。LINQはSQLへ変換され、PostgreSQLで検索が行われます。大量一覧ではSQLのWHERE、ORDER BY、LIMIT、索引、実行計画を理解する必要があります。

**問題2** DbContextFactoryで短命Contextを作れば認可も成立しますか。**詳しい解答** いいえ。Factoryの責務はContextを生成することです。本人認証とは別に、既存台帳に基づく権限・部署・行範囲の都度判定が必要です。

**問題3** 学習用DBへDDLを適用したら、実ADとIISも動作検証済みですか。**詳しい解答** いいえ。実DB、Windows統合認証、Kerberos、IIS構成、FDW、1万人以上の負荷は異なる確認事項です。

ADは本人認証とユーザー名の取得に用い、部署・行・業務操作の権限は事前整備済みDBまたは外部台帳を正本にサーバー側で毎操作判定します。教材の匿名LearningDbServiceをそのまま業務データへ流用しません。更新の競合確認、権限行ロック、監査が同一トランザクションで保証できない場合は安全側に拒否します。設定画面やPDFの読了、CIでの模擬成功は実AD、IIS、Kerberos、FDW、1万人以上の負荷を実機受入したことにはなりません。

## 復習と後続への接続
第10組は046〜050、第11組は051〜055、第12組は056〜060で確認します。未改稿の学習順051以降まで完成したとは扱いません。まず第10組で「画面表示」「接続設定」「実DB取得」の違いを説明できるようになってから次に進みます。
