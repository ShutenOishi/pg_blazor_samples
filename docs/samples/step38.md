# 学習順056：トランザクションを理解する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step38/EfTransactionPage.razor)。画面の正規URLは `/learn/056` です。

## 到達目標と実装の位置付け
学習順056は、1回のボタン操作で複数レコードを登録し、途中で失敗した場合に一部だけ残る状態を防ぐ回です。既存のLearningDbService.AddTwoInTransactionAsyncが、DbContextFactoryで短命なDbContextを作り、BeginTransactionAsync、SaveChangesAsync、CommitAsyncを順に呼びます。実行すると学習専用のPostgreSQL 18に「取引の練習A」「取引の練習B」が追加されます。画面が表示されただけでは登録は発生しません。

## 実装を理解する
WinFormsでSqlConnection.BeginTransaction()を呼んで複数のDapper Executeを同じトランザクションへ渡す設計を、EF CoreではDbContext.Database.BeginTransactionAsyncへ対応付けます。SaveChangesAsyncは追跡中の変更をSQLとして送信しますが、明示したトランザクション内ではCommitAsyncまで確定しません。画面のボタンイベントから直接DbContextを保持せずサービスへ依頼するのは、Blazor Interactive Serverで利用者ごとの回線・circuitの寿命とDB操作の寿命を分離するためです。

### 実コード例1：教材の実装
```csharp
await using var db = await factory.CreateDbContextAsync(ct);
await using var transaction = await db.Database.BeginTransactionAsync(ct);
db.Tasks.Add(new LearningTask { Title = "取引の練習A" });
db.Tasks.Add(new LearningTask { Title = "取引の練習B" });
await db.SaveChangesAsync(ct);
await transaction.CommitAsync(ct);
```

この抜粋は既存のLearningDbService.AddTwoInTransactionAsyncと同じ順序です。await usingでトランザクションを破棄する際、Commit前に例外が生じた未確定トランザクションは破棄時にロールバックされます。ここでの二重追加はデモ操作であるため、ボタンを2回押せばさらに2件が作られます。業務処理なら二重送信対策、排他制御、入力と認可の再確認が必要です。テーブルの既定値とnull許容制約はDDLの定義と照合してください。

### 実コード例2：比較または補助検証
```sql
// 実行内容を確かめるための開発専用SQL
BEGIN;
INSERT INTO learning_task (title) VALUES ('検証A');
INSERT INTO learning_task (title) VALUES ('検証B');
SELECT title FROM learning_task WHERE title IN ('検証A','検証B');
ROLLBACK;
```

後者のSQLはロールバックを観察する補助演習であり、画面のAddTwoInTransactionAsyncは成功時にCommitします。BEGIN〜ROLLBACKを同じpsqlセッションで実行すると、そのセッションでは一時的に行が見えても、終了後の別セッションの検索結果には残りません。開発DBでも作業前の件数を記録し、確認用の行と既存の学習用行を区別します。ExecuteUpdateAsyncやExecuteDeleteAsyncは変更追跡とは独立して即SQLを実行するため、一連の操作で原子的な更新が必要なら明示的トランザクションと確認手順が必要です。

## 実行場所・安全な前提
ブラウザーはボタンと表示を扱い、C#の処理とDB接続はBlazor Interactive Serverのサーバー側で実行します。学習用PostgreSQL 18とdatabase/schemaのDDLを用意してください。Visual Studio 2026と.NET 10、EF Core 10、Npgsqlを前提とし、専用DB以外の既存業務データや実利用者の氏名・所属は使いません。接続文字列はサーバー環境変数ConnectionStrings__PostgreSQLなどで渡し、Gitへ書き込みません。

この画面の確認範囲は**開発用DBへ2件登録**です。Razorが表示されたこと、EF Coreのモデル定義が取得できたこと、生成SQLが作れたこと、実SQLがDBで成功したことはそれぞれ異なります。実DBへのアクセスが必要な確認は学習専用環境で別に実施します。障害の種類を混同すると、安全拒否が成功表示へ紛れ込みます。

## WinForms＋Dapperとの比較
| 観点 | Blazor＋EF Core 10 | WinForms＋Dapper | 判断の要点 |
| --- | --- | --- | --- |
| 画面の起点 | ボタンからServiceを呼ぶ | イベントハンドラーからDapperを呼ぶ | ブラウザーに接続文字列を渡さない |
| 接続の寿命 | Factoryから毎操作DbContext生成 | SqlConnectionをusingで生成 | circuit全体で共有しない |
| まとめて確定 | BeginTransactionAsync→CommitAsync | BeginTransaction→Commit | 失敗時に一部だけ確定させない |
| 保存単位 | 追跡後SaveChangesAsync | Executeをトランザクション指定で複数回 | 追跡の有無と確定の違い |
| 監査 | 実業務では同じ原子性を検証 | 業務更新と監査INSERTを同じ取引に | FDW越しの原子性未確認なら拒否 |

## 画面の操作手順
1. 専用DBのDDLと接続文字列を確認し、業務用の資格情報やデータは使わない。
2. /learn/056を表示し、学習順056の比較表と説明を読んで、初期表示が実DB保存ではないと確かめる。
3. 「開発用DBに2件登録する」を一度だけ押し、ステータスが成功か未設定・失敗かを読む。
4. 成功時は別セッションで登録された2件を確認する。作業前後の件数を比較し、後片付けは学習専用データに限定する。
5. 開発DBを停止する、接続文字列を空にするなどの安全な異常系を試し、成功表示と混同しない。

操作前後のデータを混同しないよう、ボタンを押した回数、エラー表示、DBの検索条件と確認時刻を記録します。画面で処理を開始したユーザーとDBへ接続した主体は同じとは限りません。業務アプリではサーバーに認証済み本人がいても、業務データの参照・更新には別途その時点の権限照合が必要です。

## 障害の切り分け
| 表示・症状 | 疑う原因 | 検証・対処 |
| --- | --- | --- |
| 未設定と表示 | ConnectionStrings__PostgreSQLが未設定、または安全設定が無認可アクセスを抑止 | 専用接続設定とWindowsAuthentication:Enabledを確認 |
| 失敗のみ表示 | DDL不一致、DB停止、権限不足、接続タイムアウト | サーバーログの例外とDB側ログを対照 |
| 片方だけ残ったように見える | 別操作の実行結果または検証方法の取り違え | 実行前後の対象行とトランザクション境界を分けて照合 |
| 2件以上増えた | ボタンの二重押下や過去の実験 | 操作回数を記録し、業務では冪等化を検討 |

## AD・既存台帳・監査との境界
ADは本人認証とユーザー名の取得にのみ使います。権限の登録画面や自動同期テーブルを新しく作る前提にはしません。事前に整備されたDBの利用者台帳または外部台帳を参照し、ログイン名、所属、業務権限、対象行との対応、有効・失効状態を**要求のたびにサーバー側で**確認します。一覧・Count・集計・CSV・更新・削除はどれも別の出口であり、どれか一つの画面で認可済みでも他の操作を無条件に許可しません。

この教材の学習順047〜40のLearningDbServiceは、学習専用DBを扱うための簡素なサービスです。WindowsAuthentication:Enabledの場合には無認可サービスの実DB操作を止める安全策があります。実業務の権限済み処理はSecuredLearningServiceなどの安全な入口で設計・検証します。監査の失敗や権限失効、更新競合、FDWの原子性が不明な状態は成功として扱わず、必要な操作を拒否します。

## 演習
演習1：取引の練習Aだけ確定し、Bが失敗したときにAだけ残る仕様は妥当か説明してください。演習2：SaveChangesAsyncの直後とCommitAsyncの直後を別の接続から見た場合の違いを整理してください。演習3：監査INSERTの成否が業務の承認操作に直結する場合、更新だけを先に確定させてよいか検討してください。

## 詳しい解答
詳しい解答：1は不適切です。複数行を一つの業務上の確定単位としたなら、失敗時には取引全体をロールバックします。2では、通常の分離レベルならCommit前の未確定行は別セッションから見えず、Commit後に初めて確定済みの行として読み取れます。3では監査が必要条件なら監査失敗は業務処理失敗です。単一PostgreSQL上で同じトランザクションへ更新と監査を収める実装を確認し、FDW越しなど原子性が保証できない経路は更新を拒否します。学習用の2行追加をそのまま業務処理の認可済み実装と考えてはいけません。

## 完了条件
- [ ] 学習順056のRazor画面と対応するServiceまたはDataコードを読み、処理場所を説明できる。
- [ ] WinForms＋Dapperの実装と比較し、SQL実行・変更追跡・接続の寿命の違いを説明できる。
- [ ] 正常表示と失敗表示、DB非接続と実DB操作を混同しない。
- [ ] 認可・監査・安全拒否を本番受入から省略しない。

次：学習順057
