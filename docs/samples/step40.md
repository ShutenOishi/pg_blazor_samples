# 学習順060：PostgreSQL 18の実接続確認

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step40/EfDbCheckPage.razor)。画面の正規URLは `/learn/060` です。

## 到達目標と実装の位置付け
学習順060は、ここまでのモデル定義、SQL生成、追跡、CRUD、日付・Range、トランザクション、競合を、開発専用のPostgreSQL 18に対して整合確認する回です。画面の「開発用DBへ接続して検索」はLearningDbService.SearchAsyncを呼び、検索のメッセージと総件数を表示します。画面の表示成功、SQLのToQueryString成功、実DBから結果取得できたことはそれぞれ別の確認項目です。実AD、IIS、Kerberos、FDW、1万人超負荷はこのステップだけで検証済みになりません。

## 実装を理解する
WinForms＋Dapperの開発では、接続文字列を設定しOpenAsync→QueryAsyncを試せば接続確認に進めました。Blazor Interactive Serverでもサーバー側でDBを開きますが、利用者のブラウザーには資格情報を渡しません。LearningDbServiceは構成の有無を先に確認し、Factoryから1操作につきDbContextを生成してCountAsyncと最大20件のToListAsyncを実行します。接続プールはNpgsql側が管理し、DbContextをcircuit全体で保持しません。接続確認にはテーブルの存在・権限・型・実際のSQLを含めて観察します。

### 実コード例1：教材の実装
```csharp
await using var db = await factory.CreateDbContextAsync(ct);
var query = db.Tasks.AsNoTracking();
var count = await query.CountAsync(ct);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(
        t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
    .ToListAsync(ct);
```

LearningDbService.SearchAsyncから抽出した説明用の形です。Countと行取得は別SQLであり、データの更新が頻繁な環境では全く同じ時点の結果とは限りません。画面では検索条件を絞り、OrderBy(Id)で安定順序にし、Take(20)で表示件数を制限しています。実業務の一覧ではCountにも同じ認可条件を適用し、ページ境界・同時更新・索引を考慮します。総件数が20件を超えることと行の取得失敗は同じ意味ではありません。

### 実コード例2：比較または補助検証
```sql
-- database/tests/001_learning_task_check.sqlを専用DBで実行
SELECT current_setting('server_version') AS server_version;
SELECT id, title, xmin FROM learning_task ORDER BY id LIMIT 20;
EXPLAIN (ANALYZE, BUFFERS)
SELECT id, title FROM learning_task WHERE is_done = false ORDER BY id LIMIT 20;
```

このSQLは接続の説明用の例であり、実際の受入ではdatabase/tests/001_learning_task_check.sqlに記載されたdate、time、daterange、datemultirange、xminのSQL試験を実施します。EXPLAIN ANALYZEは実際にクエリを実行するため、本番の更新文へ安易に適用しません。索引や実行計画を確認する際は開発データの行数、統計情報、絞り込み条件の分布を記録してください。環境変数ConnectionStrings__PostgreSQLはサーバー上で管理し、Git、ブラウザー、教材のスクリーンショットへ値を出しません。

## 実行場所・安全な前提
ブラウザーはボタンと表示を扱い、C#の処理とDB接続はBlazor Interactive Serverのサーバー側で実行します。学習用PostgreSQL 18とdatabase/schemaのDDLを用意してください。Visual Studio 2026と.NET 10、EF Core 10、Npgsqlを前提とし、専用DB以外の既存業務データや実利用者の氏名・所属は使いません。接続文字列はサーバー環境変数ConnectionStrings__PostgreSQLなどで渡し、Gitへ書き込みません。

この画面の確認範囲は**開発用DBへSELECT**です。Razorが表示されたこと、EF Coreのモデル定義が取得できたこと、生成SQLが作れたこと、実SQLがDBで成功したことはそれぞれ異なります。実DBへのアクセスが必要な確認は学習専用環境で別に実施します。障害の種類を混同すると、安全拒否が成功表示へ紛れ込みます。

## WinForms＋Dapperとの比較
| 観点 | Blazor＋EF Core 10 | WinForms＋Dapper | 判断の要点 |
| --- | --- | --- | --- |
| 画面の表示 | Razorレンダリング成功 | フォームを表示できた状態 | DB接続の証拠にはならない |
| モデルの確認 | DbContext.Model / ToQueryString | SQL組み立て成功 | 通信前の検証に限る |
| 実DBのSELECT | CountAsync / ToListAsync | QueryAsync / ExecuteScalarAsync | 接続・権限・DDLを検証 |
| DB型の確認 | DateOnly/TimeOnly/NpgsqlRange | NpgsqlのDbType等を明示 | 型変換をSQL試験で確認 |
| 本番受入 | AD/IIS/FDW/負荷は別工程 | 同じく別環境で確認 | CIと現地試験を区別 |

## 画面の操作手順
1. PostgreSQL 18の学習専用DBを用意し、database/schema/001_learning_task.sqlなど必要なDDLを適用する。
2. サーバーのConnectionStrings__PostgreSQLを設定する。Windows認証を有効化した実業務モードで学習用無認可サービスを動かさない。
3. /learn/060を開き「開発用DBへ接続して検索」を押し、成功メッセージと件数を確認する。
4. psqlからdatabase/tests/001_learning_task_check.sqlを実行し、日付・期間・xminの各結果を記録する。
5. 接続設定を外した場合、DB停止の場合を別々に検証し、未設定と接続失敗と認可拒否を区別する。

操作前後のデータを混同しないよう、ボタンを押した回数、エラー表示、DBの検索条件と確認時刻を記録します。画面で処理を開始したユーザーとDBへ接続した主体は同じとは限りません。業務アプリではサーバーに認証済み本人がいても、業務データの参照・更新には別途その時点の権限照合が必要です。

## 障害の切り分け
| 表示・症状 | 疑う原因 | 検証・対処 |
| --- | --- | --- |
| PostgreSQLは未設定 | 接続文字列がない／無認可サービスを抑止 | サーバー設定とモードを確認 |
| DBへ接続できません | サーバー停止・通信・DDL・認証情報の問題 | DBログとアプリログを確認 |
| 件数は出るが型試験が失敗 | date/time/range定義とマッピングが不一致 | DDL・C#プロパティ・試験SQLを比較 |
| 競合試験が失敗 | xminの版比較や更新競合の設定に差 | 学習順057と統合試験を再検証 |

## AD・既存台帳・監査との境界
ADは本人認証とユーザー名の取得にのみ使います。権限の登録画面や自動同期テーブルを新しく作る前提にはしません。事前に整備されたDBの利用者台帳または外部台帳を参照し、ログイン名、所属、業務権限、対象行との対応、有効・失効状態を**要求のたびにサーバー側で**確認します。一覧・Count・集計・CSV・更新・削除はどれも別の出口であり、どれか一つの画面で認可済みでも他の操作を無条件に許可しません。

この教材の学習順047〜40のLearningDbServiceは、学習専用DBを扱うための簡素なサービスです。WindowsAuthentication:Enabledの場合には無認可サービスの実DB操作を止める安全策があります。実業務の権限済み処理はSecuredLearningServiceなどの安全な入口で設計・検証します。監査の失敗や権限失効、更新競合、FDWの原子性が不明な状態は成功として扱わず、必要な操作を拒否します。

## 演習
演習1：モデルのSQL生成が成功しただけでDB結合の受入を完了してはいけない理由を述べてください。演習2：Countが100で実際の表示が20件のときに疑うべき問題と正常な理由を分けてください。演習3：CIでPostgreSQL18の結合試験に合格しても、実AD・IIS・FDW・1万人負荷の受入を完了扱いできるでしょうか。

## 詳しい解答
詳しい解答：1ではSQL生成は通信、接続先の権限、実DDL、DB型の返却を検証していません。2ではTake(20)による正常な上限がまず考えられます。失敗ではない一方、Countと一覧の検索条件・認可条件・ソート順が違えば欠陥です。3ではできません。CIは指定のテスト環境だけを証明します。実際のAD認証、既存台帳の部署別認可、IIS配備、FDWの通信経路、監査の原子性、1万人超の同時操作に関する実機確認は別の受入計画と証跡が必要です。

## 完了条件
- [ ] 学習順060のRazor画面と対応するServiceまたはDataコードを読み、処理場所を説明できる。
- [ ] WinForms＋Dapperの実装と比較し、SQL実行・変更追跡・接続の寿命の違いを説明できる。
- [ ] 正常表示と失敗表示、DB非接続と実DB操作を混同しない。
- [ ] 認可・監査・安全拒否を本番受入から省略しない。

次：第12組の復習
