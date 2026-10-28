# 学習順059：FromSql／SqlQueryのパラメーターとDapper併用

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L059ParameterizedSqlPage.razor)。画面の正規URLは `/learn/059` です。

## 到達目標と実装の位置付け
学習順059では、SQLの形を自分で書きたい場面でも利用者入力を構文へ混ぜないことを確認します。Web画面の入力欄は、あえてSQL注入を想定した値で初期化されています。「安全なSQLを生成する」を押すと、EF CoreのFromSqlInterpolatedとSqlQueryのToQueryStringがSQLとパラメーターを生成します。実データを取得するToListAsyncやCountAsyncは実行しません。SQLが画面に表示されることは、PostgreSQLへの接続が成功したことを意味しません。

## 実装を理解する
DapperでQueryAsyncを使っていた開発者は、EF Coreでも全ての複雑なSQLをLINQへ直す必要はありません。EntityへマッピングするSQLはFromSqlInterpolated、スカラーやDTO相当の値はSqlQueryの用途を考えられます。Dapperを併用する場合も、安全なパラメーターと一元化された認可条件、接続の寿命、監査範囲を保ちます。SQL自体を文字列結合することと、補間文字列をパラメーターとして渡すことは違います。テーブル名やORDER BYの列名など識別子は値のパラメーター化とは別に固定選択肢で制御します。

### 実コード例1：教材の実装
```csharp
var keyword = term; // 入力欄の文字列
fromSql = db.Tasks.FromSqlInterpolated(
    $"SELECT xmin, * FROM learning_task WHERE title = {keyword}")
    .AsNoTracking().ToQueryString();
sqlQuery = db.Database.SqlQuery<int>(
    $"SELECT COUNT(*)::int AS \"Value\" FROM learning_task WHERE title = {keyword}")
    .ToQueryString();
```

既存のDbBridge05Page.Generateの要点です。PostgreSQLのxminはシステム列のため通常のSELECT *には含まれません。学習用LearningTaskはIsRowVersionでxminをマッピングしているので、FromSqlでEntityを構築するときはxminも返す必要があります。SqlQuery<int>では単一値の列の別名Valueを付け、後続のクエリ操作との整合を取りやすくしています。ToQueryStringは生成されたパラメーター表現を確かめるための教材機能で、本番サービスでは生の入力値や認証情報を詳細ログに残さない方針も必要です。

### 実コード例2：比較または補助検証
```csharp
// 比較：WinForms＋Dapperの検索例（認可済みサーバー処理で実行）
const string sql = "SELECT id, title FROM learning_task WHERE title = @Term ORDER BY id LIMIT 20";
var rows = await connection.QueryAsync<LearningTaskDto>(
    new CommandDefinition(sql, new { Term = term }, cancellationToken: ct));
// 悪い例："... WHERE title = '" + term + "'" は使わない。
```

Dapper比較ではCommandDefinitionの引数としてTermを渡すので、値をSQL構文に連結しません。上のLearningTaskDtoは実システム側で定義する説明用DTOです。DapperのQueryAsyncを呼ぶ場合はDB接続が必要ですが、従来のSQL生成ボタンはQueryAsyncを呼びません。追加した実検索ボタンは専用学習DBだけでDapperのQueryAsyncとEF CoreのFromSqlInterpolated・SqlQuery<int>を実行します。FromSqlInterpolatedでは補間文字列を適切なAPIへそのまま渡すとパラメーター化されます。一度通常のstringへ組み立てたSQLを危険なRaw系APIへ渡しても安全にはなりません。EFとDapperの混在であっても、認可抜けの別経路を増やしてはいけません。

## 実行場所・安全な前提
ブラウザーはボタンと表示を扱い、C#の処理とDB接続はBlazor Interactive Serverのサーバー側で実行します。学習用PostgreSQL 18とdatabase/schemaのDDLを用意してください。Visual Studio 2026と.NET 10、EF Core 10、Npgsqlを前提とし、専用DB以外の既存業務データや実利用者の氏名・所属は使いません。接続文字列はサーバー環境変数ConnectionStrings__PostgreSQLなどで渡し、Gitへ書き込みません。

この画面では**生成SQLのみを検査するボタン（DB接続なし）と、学習用DBを実検索するボタン**を分けています。Razorが表示されたこと、EF Coreのモデル定義が取得できたこと、生成SQLが作れたこと、実SQLがDBで成功したことはそれぞれ異なります。実DBへのアクセスが必要な確認は学習専用環境で別に実施します。障害の種類を混同すると、安全拒否が成功表示へ紛れ込みます。

## WinForms＋Dapperとの比較
| 観点 | Blazor＋EF Core 10 | WinForms＋Dapper | 判断の要点 |
| --- | --- | --- | --- |
| 一覧の単純検索 | LINQ＋AsNoTracking | パラメーター付きQueryAsync | コードと保守性で選ぶ |
| 複雑な既存SQL | FromSqlInterpolated | SQLを維持してQueryAsync | 戻り列とEntity型の整合を確認 |
| 集計スカラー | SqlQuery<int> | ExecuteScalarAsync等 | 列名Valueと型を照合 |
| SQL注入対策 | Interpolated APIへ値を渡す | @Termの匿名引数 | 識別子は許可リストで制限 |
| 実DBへの接続 | ToListAsync等の実行時 | QueryAsyncの実行時 | ToQueryStringだけでは接続しない |

## 画面の操作手順
1. /learn/059を開き、学習順059とDB非接続の説明を確認する。
2. 入力欄の初期値を変えずに「安全なSQLを生成する」を押す。
3. FromSqlとSqlQueryの2つの生成SQLを見比べ、値がSQL本文に単純に埋め込まれていないことを確認する。
4. 通常の日本語検索語でも再実行し、パラメーターの表現とSQLの構文が変わらないか調べる。
5. 「学習DBで3方式を実検索する」を押し、未設定環境では安全拒否を確認する。専用PostgreSQL18ジョブでは実検索とSQL注入を模した値の検証を行う。

操作前後のデータを混同しないよう、ボタンを押した回数、エラー表示、DBの検索条件と確認時刻を記録します。画面で処理を開始したユーザーとDBへ接続した主体は同じとは限りません。業務アプリではサーバーに認証済み本人がいても、業務データの参照・更新には別途その時点の権限照合が必要です。

## 障害の切り分け
| 表示・症状 | 疑う原因 | 検証・対処 |
| --- | --- | --- |
| クエリ文字列だけ出た | ToQueryStringだけ実行された正常な挙動 | 実検索ボタンの成功・拒否と区別する |
| xmin列不足の例外 | Entityの要求列をSELECTしなかった | SELECT xmin, *の整合を確認 |
| 値が構文に直結している | Unsafeな文字列連結やRaw系APIの誤使用 | 補間APIとパラメーターを確認 |
| Dapperで権限漏れ | 別SQLで既存認可条件を忘れた | サーバーの業務認可サービスを共有 |

## AD・既存台帳・監査との境界
ADは本人認証とユーザー名の取得にのみ使います。権限の登録画面や自動同期テーブルを新しく作る前提にはしません。事前に整備されたDBの利用者台帳または外部台帳を参照し、ログイン名、所属、業務権限、対象行との対応、有効・失効状態を**要求のたびにサーバー側で**確認します。一覧・Count・集計・CSV・更新・削除はどれも別の出口であり、どれか一つの画面で認可済みでも他の操作を無条件に許可しません。

この教材の学習順047〜40のLearningDbServiceは、学習専用DBを扱うための簡素なサービスです。WindowsAuthentication:Enabledの場合には無認可サービスの実DB操作を止める安全策があります。実業務の権限済み処理はSecuredLearningServiceなどの安全な入口で設計・検証します。監査の失敗や権限失効、更新競合、FDWの原子性が不明な状態は成功として扱わず、必要な操作を拒否します。

## 演習
演習1：termに引用符やSQL構文に似た文字列を入れても、パラメーター化していれば何が変わらないでしょうか。演習2：FromSqlでLearningTaskのxminを省略すると何が起きるでしょうか。演習3：ソート列名を利用者入力から選ぶ必要がある場合、安全に実装する方法を説明してください。

## 詳しい解答
詳しい解答：1ではWHERE以降のSQL構文が変わらず、term全体が検索値として扱われます。2ではEF CoreがEntityのモデルに必要な列をマッピングできず、実DBからの結果取得時に例外となります。システム列xminを含めたSQLを作り、専用のPostgreSQL18で検証します。3では列名を値パラメーターとして使えないので、選択可能な列と昇降順をサーバー側の固定列挙やswitchへ対応付けます。DapperでもEF Coreでも、本人・部署・行単位の認可はSQL選択の前提条件です。

## 完了条件
- [ ] 学習順059のRazor画面と対応するServiceまたはDataコードを読み、処理場所を説明できる。
- [ ] WinForms＋Dapperの実装と比較し、SQL実行・変更追跡・接続の寿命の違いを説明できる。
- [ ] 正常表示と失敗表示、DB非接続と実DB操作を混同しない。
- [ ] 認可・監査・安全拒否を本番受入から省略しない。

次：学習順060

## 学習順059の学習用PostgreSQL実検索（追加検証）

[比較サービス](../../../../src/PgBlazorSamples.Web/Services/Learning/L059ParameterizedSqlService.cs)は、同じ検索値をDapperの`@Term`、EF Coreの`FromSqlInterpolated`、`SqlQuery<int>`に渡します。Entity読取ではPostgreSQLの`xmin`を明示し、Dapper/EFのID・タイトル、SqlQueryのCOUNT値を照合します。テーブル名は固定で、値をSQL文に連結しません。検索値は1〜60文字、一覧は最大10件とし、接続設定がない場合またはWindows認証が有効な場合、匿名の学習用DB読取を拒否します。

[単体・結合テスト](../../../../tests/PgBlazorSamples.UnitTests/L059ParameterizedSqlTests.cs)では未設定/Windows認証/空欄/61文字の拒否、専用PostgreSQL18での固有架空行の実検索、`' OR 1=1 --`をSQL構文ではなく検索値として扱い0件となることを独立検証します。架空の検査行はテスト終了時に削除します。

通常のChromeジョブはDB未設定時の拒否だけを確認し、実DB接続成功の証拠として扱いません。これは学習用データのSQL検証であり、実在の利用者台帳やFDWの認可・監査、SITE受入の実証ではありません。
