# 学習順058：既存DB・ビュー・キーなしモデル・FDWを扱う

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Samples/DbBridge/DbBridge04Page.razor)。画面の正規URLは `/learn/058` です。

## 到達目標と実装の位置付け
学習順058は、既存の台帳を業務アプリが登録し直さず参照するためのモデリングを理解する回です。新規ユーザーテーブルを作らず、事前に整備されたDBテーブルまたは外部テーブルを正本とします。画面の「キーなしモデル定義を確認する」はIDbContextFactoryからDbContextを一時的に作り、モデルにPrimaryKeyがないことだけを確認します。別のボタンから、固定の架空値2件だけを返す学習専用ビューを実際にSELECTします。既存利用者台帳へのSELECTやFDW通信は実行しません。匿名画面の結果を認可済みと判断してはいけません。

## 実装を理解する
既存のWinForms＋Dapperではビュー名と列名をSQLに直書きしてDTOへ読み出すことが多いでしょう。EF Core 10でもDB Firstのモデル定義をWebプロジェクトのData内に置けます。既存テーブルにEF専用のIdを追加する必要はなく、HasNoKeyで読み取り用ビューを定義できます。ただしキーなしモデルは変更追跡・通常の更新対象にできません。FDWはPostgreSQLから別DBのデータを見る仕組みであり、接続先の権限、通信経路、外部側のトランザクション境界を設計から排除できません。

### 実コード例1：教材の実装
```csharp
var access = modelBuilder.Entity<ExistingUserAccessRow>();
access.HasNoKey();
access.ToView("learning_user_access_v");
access.Property(x => x.LoginName).HasColumnName("login_name");
access.Property(x => x.UnitCode).HasColumnName("unit_code");
access.Property(x => x.AuthorizationVersion).HasColumnName("authorization_version");
```

この定義はLearningDbContext.OnModelCreatingの要点です。ExistingUserAccessRowのlogin_nameなどの列は、既存台帳の構造と一致させます。HasNoKeyは「権限を確認しなくてよい」という意味ではありません。匿名利用者が見られるのはモデル定義と、Windows認証が無効かつ専用学習DB接続が設定済みの場合の固定架空値だけです。実在する利用者のログイン名・所属・権限は表示しません。ToViewは読み取り専用の意図をモデル上で示すもので、DBロールの権限やビュー定義の検証を代行しません。

### 実コード例2：比較または補助検証
```csharp
// 認証・認可済みサーバー処理でのみ使う考え方
await using var db = await factory.CreateDbContextAsync(ct);
var row = await db.ExistingUserAccess.AsNoTracking()
    .SingleOrDefaultAsync(x => x.LoginName == loginName, ct);
if (row is null || !row.IsActive || !row.CanRead)
    throw new UnauthorizedAccessException("台帳で認可されていません");
```

後者は設計を説明する例であり、学習順058の匿名画面で実行するコードではありません。実業務ではログイン名を外部から直接渡さず、認証済みPrincipalから取得し、対象業務・部署・行条件・AuthorizationVersion・失効状態を操作ごとに確認します。利用者台帳が外部テーブルの場合でも、「1万人いるのでブラウザー側に全員分をキャッシュする」としてはなりません。サーバーで認可し、一覧Count、各行、CSV、集計など出口を個別に検査します。外部側の障害で有効権限を判断できない場合は許可せず拒否します。

## 実行場所・安全な前提
ブラウザーはボタンと表示を扱い、C#の処理とDB接続はBlazor Interactive Serverのサーバー側で実行します。学習用PostgreSQL 18とdatabase/schemaのDDLを用意してください。Visual Studio 2026と.NET 10、EF Core 10、Npgsqlを前提とし、専用DB以外の既存業務データや実利用者の氏名・所属は使いません。接続文字列はサーバー環境変数ConnectionStrings__PostgreSQLなどで渡し、Gitへ書き込みません。

この画面の確認範囲は**既存台帳のEFモデル定義検査と、別の固定架空データ用ビューの実SELECT**です。後者の実接続は専用PostgreSQL18の結合テストだけで確認し、通常のChromeはDB未設定時の拒否を確認します。Razorが表示されたこと、EF Coreのモデル定義が取得できたこと、生成SQLが作れたこと、実SQLがDBで成功したことはそれぞれ異なります。実DBへのアクセスが必要な確認は学習専用環境で別に実施します。障害の種類を混同すると、安全拒否が成功表示へ紛れ込みます。

## WinForms＋Dapperとの比較
| 観点 | Blazor＋EF Core 10 | WinForms＋Dapper | 判断の要点 |
| --- | --- | --- | --- |
| モデル定義 | HasNoKey／ToView | SELECTの結果をDTOへ対応付け | 新規台帳の作成は不要 |
| 実データ取得 | 固定の架空値ビューのみReadAsyncでSELECT | DapperのQueryAsync | 既存の利用者台帳は認可済みサービス以外から参照しない |
| 認可基準 | 既存DB/外部ビューの都度確認 | SQL実行直前の台帳照合 | ADは本人とユーザー名のみ |
| 更新 | Keylessは通常更新対象でない | 明示SQLでも更新を許可する根拠が必要 | 監査の原子性未確認なら拒否 |
| FDW障害 | 遠隔ネットワーク／権限／接続先エラーを分離 | NpgsqlException等で原因分類 | 認可不明のまま許可に倒さない |

## 画面の操作手順
1. /learn/058を開き、表示が第5段階・学習順058であることを確認する。
2. 「キーなしモデル定義を確認する」を押して、「データは取得していません」という表示を確かめる。
3. 「架空データのビューを読み取る」を押し、未設定時は拒否されることを確認する。専用PostgreSQL18ジョブでは固定架空値2件の読取を検証する。
4. LearningDbContext.OnModelCreatingのExistingUserAccessRowにHasNoKeyとToViewが設定されている箇所を読む。
5. 既存のdatabase/schema/002_app_access.sqlを調べ、教育用の既存台帳ビューと実運用の外部台帳の違いを説明する。
6. 匿名画面から実ユーザーデータを読む機能を追加しない。FDW更新が必要な業務要件があれば先に原子的監査の保証を設計する。

操作前後のデータを混同しないよう、ボタンを押した回数、エラー表示、DBの検索条件と確認時刻を記録します。画面で処理を開始したユーザーとDBへ接続した主体は同じとは限りません。業務アプリではサーバーに認証済み本人がいても、業務データの参照・更新には別途その時点の権限照合が必要です。

## 障害の切り分け
| 表示・症状 | 疑う原因 | 検証・対処 |
| --- | --- | --- |
| モデルが見つからない | DbSetかOnModelCreatingの定義不足 | DbContextと型の登録を照合 |
| DBと接続できない | 学習専用ビューの照会時だけ接続する | 未設定/Windows認証時は拒否し、PostgreSQL18ジョブで別途実証 |
| 台帳を参照できない | ビュー権限やFDW接続など | 本人・業務認可を満たす別の専用検証で調査 |
| 認可が確認できない | 失効・外部台帳の未取得・監査不可 | 必ず処理拒否し調査ログを保全 |

## AD・既存台帳・監査との境界
ADは本人認証とユーザー名の取得にのみ使います。権限の登録画面や自動同期テーブルを新しく作る前提にはしません。事前に整備されたDBの利用者台帳または外部台帳を参照し、ログイン名、所属、業務権限、対象行との対応、有効・失効状態を**要求のたびにサーバー側で**確認します。一覧・Count・集計・CSV・更新・削除はどれも別の出口であり、どれか一つの画面で認可済みでも他の操作を無条件に許可しません。

この教材の学習順047〜40のLearningDbServiceは、学習専用DBを扱うための簡素なサービスです。WindowsAuthentication:Enabledの場合には無認可サービスの実DB操作を止める安全策があります。実業務の権限済み処理はSecuredLearningServiceなどの安全な入口で設計・検証します。監査の失敗や権限失効、更新競合、FDWの原子性が不明な状態は成功として扱わず、必要な操作を拒否します。

## 演習
演習1：キーなしモデルを定義しただけで認可済みと表示してはいけない理由を述べてください。演習2：ADからユーザー名だけを受け取る構成で、操作対象の部署が違う行の更新を拒否するために必要な情報を整理してください。演習3：外部DBのFDWにある台帳を読み取れないとき、前回の許可結果を無条件に再利用してよいでしょうか。

## 詳しい解答
詳しい解答：1ではモデルの列名が合っている事実と利用者に権限がある事実は独立です。画面は定義だけを検査し、実台帳を読まないことを明示します。2では認証済みユーザー名に対して、既存台帳の有効状態、業務ごとの読み書き権限、対象部署・行との対応、権限の版と有効期間などをサーバー操作の直前に照合します。3では権限失効を検知できないため許可しません。外部台帳への接続や原子的な監査が成立しない更新はフェイルクローズします。

## 完了条件
- [ ] 学習順058のRazor画面と対応するServiceまたはDataコードを読み、処理場所を説明できる。
- [ ] WinForms＋Dapperの実装と比較し、SQL実行・変更追跡・接続の寿命の違いを説明できる。
- [ ] 正常表示と失敗表示、DB非接続と実DB操作を混同しない。
- [ ] 認可・監査・安全拒否を本番受入から省略しない。

次：学習順059

## 学習順058の学習用ビュー実読取（追加検証）

[キーなしDTO](../../../src/PgBlazorSamples.Web/Data/LearningKeylessDemoRow.cs)を`HasNoKey`と`ToView("learning_keyless_demo_v")`で対応付けます。[DDL](../../../database/schema/001_learning_task.sql)の`learning_keyless_demo_v`は`VALUES`から構成する固定架空値2件のみで、実ユーザーのログイン名、部署、権限台帳やFDWを参照しません。`HasNoKey`はDB権限を与える設定ではありません。

[サービス](../../../src/PgBlazorSamples.Web/Services/Learning/L058KeylessViewService.cs)は、接続文字列が設定されWindows認証が無効な学習環境だけで、短命DbContextの`AsNoTracking`、並び順、10件上限付き`ToListAsync`を実行します。読み取りに失敗したときは内部例外を画面へ出さず、行を返しません。

[試験](../../../tests/PgBlazorSamples.UnitTests/L058KeylessViewTests.cs)は未設定拒否、Windows認証有効時の拒否、専用PostgreSQL18上のビューの実在・キーなし・固定架空値2件のSELECTを別々に確認します。通常のChromeでは未設定時の拒否だけを試験します。画面の成功を実AD、既存台帳の都度認可、実FDW、SITE受入の成功と混同しません。
