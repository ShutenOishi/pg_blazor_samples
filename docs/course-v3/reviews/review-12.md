# 第12組の復習：トランザクション・xmin・安全SQL

## 対象と到達目標
学習順056〜060の5本は、一つのDB更新が正しく完了したか、ほかの利用者の変更を上書きしていないか、既存の台帳へ勝手にアクセスしていないか、SQLがパラメーター化されているか、実際のPostgreSQL18で確認できたかを連続した学習として扱います。第5段階で学んだEF Coreのモデル定義とCRUDを、WinForms＋Dapperで経験した接続・トランザクション・更新件数の考え方に対応付けてください。

## 5本の比較と実行場所
| 学習順 | 主題 | 画面が行うこと | DBへの接続 |
| --- | --- | --- | --- |
| 056 | トランザクション | 学習用2行をまとめて保存 | あり（開発専用DB） |
| 057 | xmin楽観的排他 | 行の版を照合して更新 | あり（開発専用DB） |
| 058 | 既存DB・キーなしビュー・FDW | EFモデルのHasNoKeyを確認 | なし |
| 059 | FromSql／SqlQuery／Dapper併用 | パラメーター化SQLを生成 | なし |
| 060 | PostgreSQL18実接続 | Countと上限20行を検索 | あり（開発専用DB） |

| 旧来のWinForms＋Dapper | 今回のBlazor＋EF Core | 共通する安全条件 |
| --- | --- | --- |
| SqlConnectionとBeginTransaction | DbContextFactoryとBeginTransactionAsync | 短命接続・まとめて確定 |
| WHERE id・xmin・影響行数 | Version照合とDbUpdateConcurrencyException | 競合時に自動上書きしない |
| SELECTからDTOへ対応付け | HasNoKey／ToView | 元台帳を正本として都度認可 |
| QueryAsync(sql, parameters) | FromSqlInterpolated／SqlQuery | SQLの構文と入力値を分離 |
| OpenAsync／QueryAsyncで接続試験 | CountAsync／ToListAsyncとSQL試験 | モデル成功と実接続成功を区別 |

## ケース1：2件の登録のうち2件目で失敗する
学習順056のAddTwoInTransactionAsyncは、同じDbContextとトランザクションで2件を追加してからSaveChangesAsyncとCommitAsyncを呼びます。一部だけ残ることを許さないために、Commitの前にエラーが起きたら未確定の取引を破棄します。画面のボタンは何度でも押せるので、業務では二重送信への対策と冪等性の要件も別途設計します。

```csharp
await using var db = await factory.CreateDbContextAsync(ct);
await using var transaction = await db.Database.BeginTransactionAsync(ct);
db.Tasks.Add(new LearningTask { Title = "取引の練習A" });
db.Tasks.Add(new LearningTask { Title = "取引の練習B" });
await db.SaveChangesAsync(ct);
await transaction.CommitAsync(ct);
```

この実装とDapperのconnection.BeginTransactionを比較してください。SQLコマンドを複数実行しただけでは、一つの取引として確定したことにはなりません。更新と監査ログが分離したトランザクションの場合、更新だけが成功する可能性もあります。監査の原子性を証明できない状態では本番の承認・編集処理は拒否します。

## ケース2：古いタブで同じ行を更新する
学習順057では、古い画面が保持したVersionとDBのxminを対応付け、変更済みの行を更新しない仕組みを観察します。画面でバージョンが違うと判定される場合と、再読込からSaveChangesAsyncまでの間に競合する場合の両方を検討します。

```csharp
var task = await db.Tasks.SingleOrDefaultAsync(x => x.Id == id, ct);
if (task is null) return "対象が削除されています。再読み込みしてください。";
if (task.Version != version)
    return "別の利用者が更新しました。再読み込みしてください。";
task.IsDone = !task.IsDone;
await db.SaveChangesAsync(ct);
```

古いVersionのまま再試行を重ねることは正しい復旧手段ではありません。元の内容が今も適切かを読み直して判断する必要があります。データが削除された場合、内容だけが変更された場合、本人の権限が失効した場合の三つは異なる拒否理由です。

## ケース3：既存台帳の定義を見たらログインを許可してよいか
学習順058のモデル確認は、DBを問い合わせないローカルのメタデータ操作です。HasNoKeyが取得できたことから、特定ユーザーが有効である、部署へアクセスしてよい、という結論は出せません。ADで取得するのは本人とユーザー名であり、業務認可は既存台帳をサーバーで都度照合します。

```csharp
var access = modelBuilder.Entity<ExistingUserAccessRow>();
access.HasNoKey();
access.ToView("learning_user_access_v");
access.Property(x => x.LoginName).HasColumnName("login_name");
access.Property(x => x.UnitCode).HasColumnName("unit_code");
access.Property(x => x.AuthorizationVersion).HasColumnName("authorization_version");
```

FDW越しの参照は遠隔側の障害・権限・通信を考慮します。外部更新と内部監査が一つの原子的な取引として成立する保証がなければ、監査要件のある更新を許可しません。

## ケース4：SQL注入に似た入力を入れる
学習順059では初期入力が疑わしい文字列でも、それをSQL構文へ連結しないことを確認します。ToQueryStringはSQLを生成するだけで実DB通信を行いません。FromSqlでEntityを復元する場合はPostgreSQLのシステム列xminが必要な点にも注意します。

```csharp
var keyword = term; // 入力欄の文字列
fromSql = db.Tasks.FromSqlInterpolated(
    $"SELECT xmin, * FROM learning_task WHERE title = {keyword}")
    .AsNoTracking().ToQueryString();
sqlQuery = db.Database.SqlQuery<int>(
    $"SELECT COUNT(*)::int AS \"Value\" FROM learning_task WHERE title = {keyword}")
    .ToQueryString();
```

Dapperでも、匿名オブジェクトでパラメーターを指定すればSQL構文と値を分離できます。ただしEFとDapperの両方に未認可のSQL実行経路を作ると、別の場所から業務認可を回避できてしまいます。業務用の一覧、集計、CSVなどにも同じ台帳認可を適用することが必要です。

## ケース5：画面表示だけで実DB受入としない
学習順060では検索ボタンで専用PostgreSQL18のCountAsyncと上限20件のToListAsyncを実行します。database/tests/001_learning_task_check.sqlによるdate・time・daterange・datemultirange・xminの確認と、DB停止時の拒否は別の証拠として残してください。実AD、IIS、Kerberos、FDW、1万人超負荷は現地での独立した受入が必要です。

## 復習操作
1. 学習順056から順に画面を開き、まずDBへ接続する画面かどうかを判定する。
2. 学習順056で専用DBへの2行追加を1回だけ実施し、成功・未設定・失敗を区別する。
3. 学習順057で2タブを使い、古いVersionの更新拒否と再読込の案内を確認する。
4. 学習順058でモデル定義だけを確認し、台帳データを取得していないことを説明する。
5. 学習順059で疑わしい値を含む入力をパラメーター化し、SQL本文の構文が変わらないことを確認する。
6. 学習順060で実DB接続と専用SQL試験を実施し、確認した型・件数・競合の結果を記録する。

## 障害の切り分け
| 現象 | 誤解しやすい判断 | 正しい対応 |
| --- | --- | --- |
| SQLが生成された | DB接続も成功した | ToQueryStringと実Queryを区別 |
| 競合を検知 | 自動再送すればよい | 再読込し利用者に判断を求める |
| モデルにHasNoKeyあり | 台帳の認可済み | 本人・業務・行の権限を都度評価 |
| FDW通信不可 | 前回の許可で続行 | 安全側に拒否し診断ログを確認 |
| 実DBのCount成功 | AD/IIS/負荷も合格 | 別環境・別試験の証拠が必要 |

## 演習
設問1：学習用2行追加の途中で例外が発生した。トランザクションの破棄と、Dapperで同等の処理を書くときの注意点を述べる。
設問2：タブAが保存後、古いタブBから更新した場合の拒否とUI復旧案内を書く。
設問3：ADのユーザー名が確認できても、FDW先の既存台帳が取得できない場合に業務処理を進めてよいか答える。
設問4：SqlQueryを使った一覧件数とCSV出力で別の認可条件を使ってよいか検討する。
設問5：Chrome操作の成功とPostgreSQL18の実DB統合試験成功の違いを説明する。

## 詳しい解答
設問1：Commit前にエラーが出た未確定取引をロールバックし、1件だけ確定させません。Dapperなら全Executeへ同じIDbTransactionを渡し、成功したときだけCommitを呼びます。画面の二重送信・認可・監査は別途保証します。

設問2：タブBのVersionがDBのxminと異なる場合、保存せず競合を知らせます。SaveChanges中に競合した場合もDbUpdateConcurrencyExceptionを捕捉します。削除済みか別の更新かを判定し、最新行の再読込を案内します。値を強制上書きしません。

設問3：進めてはいけません。認証で得た名前は本人識別の材料であり、業務許可の確定ではありません。既存台帳を確認できなければ権限の有効性を検証できないため、外部接続や監査が回復するまで拒否します。

設問4：同じ本人・部署・業務・行条件を全出口に適用します。件数から他部署情報が推測される場合もあり、Count、CSV、検索候補などを抜け道にしません。SqlQueryかDapperかは権限判定の省略理由になりません。

設問5：Chrome成功は指定UIの表示と操作を証明しますが、別のテストDBのDDL・型・トランザクションを検証したことにはなりません。Windows/Linuxのビルド、xUnit、Chrome、専用PostgreSQL18実DB、安全拒否それぞれに対応するCIジョブの成功証拠を確認します。

## 完了チェック
- [ ] 5ステップの処理場所と実DB接続の有無を説明できる。
- [ ] トランザクションとxminの違いを比較できる。
- [ ] 既存台帳の認可とモデル定義を同一視しない。
- [ ] EF CoreとDapperの両方でパラメーター化を維持する。
- [ ] 実DB、監査、現地AD/IIS/FDW・負荷の受入範囲を区別できる。
