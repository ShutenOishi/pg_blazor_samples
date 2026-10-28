# 学習順057：xmin楽観的排他を確認する

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L057EfConcurrencyPage.razor)。画面の正規URLは `/learn/057` です。

## 到達目標と実装の位置付け
学習順057では、画面に表示した行と、更新する瞬間のDB行が同じ版かを確認します。PostgreSQLのシステム列xminをuintのVersionへ割り当て、DbContextでIsRowVersionとして扱うことで、後から保存した操作が他人の変更を無言で上書きしない設計を学びます。現行画面は「DBの状態を読み直す」で最大20件を取得し、行ごとの「状態を更新」で事前に取得したVersionを渡します。操作後には一覧を再取得します。

## 実装を理解する
WinForms＋DapperではWHERE id = @Id AND xmin = @Versionとし、影響行数が0なら競合と判断する方法が分かりやすいです。EF Core 10では追跡するEntityにIsRowVersionを設定し、SaveChangesAsync時に楽観的排他を行います。ただし画面で渡した古い版は、取得直後にDbContextを閉じる構成のため、サーバー側で対象行を再読込して最初にVersionを照合します。再読込とUPDATEの間にさらに別の更新が入った場合もDbUpdateConcurrencyExceptionを捕捉します。

### 実コード例1：教材の実装
```csharp
var task = await db.Tasks.SingleOrDefaultAsync(x => x.Id == id, ct);
if (task is null) return "対象が削除されています。再読み込みしてください。";
if (task.Version != version)
    return "別の利用者が更新しました。再読み込みしてください。";
task.IsDone = !task.IsDone;
await db.SaveChangesAsync(ct);
```

既存のLearningDbService.ToggleAsyncでは、見つからない行と古いVersionの行を区別しており、いずれも保存しません。Version比較だけで全競合を防げるわけではありません。比較した直後の競合は、EF Coreが生成する更新SQLの一致行数により検出されます。正常更新の場合はPostgreSQLのxminが変わり、古いタブからの次の操作は拒否されます。DbUpdateConcurrencyExceptionを無視して即再試行すると相手の変更を失うため、最新表示と操作判断を改めて求めます。

### 実コード例2：比較または補助検証
```csharp
// Dapperで同じ条件を明示する場合の考え方
var rows = await connection.ExecuteAsync(
    "UPDATE learning_task SET is_done = @Done " +
    "WHERE id = @Id AND xmin = @Version::xid",
    new { Id = id, Done = nextValue, Version = oldVersion });
if (rows != 1) throw new InvalidOperationException("競合または削除");
```

Dapperの断片は比較用の概念例です。実際のPostgreSQLパラメーターのxid変換は型・ドライバーの動作を専用DBで検証する必要があります。既存実装はEF CoreのIsRowVersionによる更新競合を正本としており、SQL例を貼り付けて無検証で置き換えません。後から読んだ側が最新の版を保持していても、認可が失効した場合は更新してはいけません。業務処理では本人確認、既存台帳の部署・業務・行単位の認可、競合トークン、監査成功を別々に照合します。

## 実行場所・安全な前提
ブラウザーはボタンと表示を扱い、C#の処理とDB接続はBlazor Interactive Serverのサーバー側で実行します。学習用PostgreSQL 18とdatabase/schemaのDDLを用意してください。Visual Studio 2026と.NET 10、EF Core 10、Npgsqlを前提とし、専用DB以外の既存業務データや実利用者の氏名・所属は使いません。接続文字列はサーバー環境変数ConnectionStrings__PostgreSQLなどで渡し、Gitへ書き込みません。

この画面の確認範囲は**専用DBの行を読み更新**です。Razorが表示されたこと、EF Coreのモデル定義が取得できたこと、生成SQLが作れたこと、実SQLがDBで成功したことはそれぞれ異なります。実DBへのアクセスが必要な確認は学習専用環境で別に実施します。障害の種類を混同すると、安全拒否が成功表示へ紛れ込みます。

## WinForms＋Dapperとの比較
| 観点 | Blazor＋EF Core 10 | WinForms＋Dapper | 判断の要点 |
| --- | --- | --- | --- |
| 旧版の保持 | RazorのLearningTaskRow.Version | DataGridViewのDTOにxminを持つ | 画面が版を自動更新したと誤解しない |
| 更新時の検査 | Version比較とSaveChangesの競合検知 | WHERE xmin条件と影響行数 | 値が合わなければ拒否 |
| 削除の検知 | SingleOrDefaultがnull | UPDATEの0件 | 競合と不存在を調べる |
| UI復旧 | 再読込を促し、一覧を再取得 | DBから再検索して再編集 | 上書きを自動継続しない |
| 認可・監査 | SecuredLearningService等で別途実行 | 業務SQL前に毎回確認 | xminだけでは認可にならない |

## 画面の操作手順
1. /learn/057を表示し、先に学習順056のトランザクションと確定タイミングを確認する。
2. 学習専用DBを用意したうえで2つのタブに同じ学習行を表示し、両方で「DBの状態を読み直す」を押す。
3. タブAで「状態を更新」を実行し、成功と一覧のVersion変化を確認する。
4. 再読込前のタブBで同じ行の更新を試し、「別の利用者が更新しました」等の拒否を確認する。
5. タブBを再読込し直し、最新Versionで自分の操作を判断し直す。認可のない実データで試験しない。

操作前後のデータを混同しないよう、ボタンを押した回数、エラー表示、DBの検索条件と確認時刻を記録します。画面で処理を開始したユーザーとDBへ接続した主体は同じとは限りません。業務アプリではサーバーに認証済み本人がいても、業務データの参照・更新には別途その時点の権限照合が必要です。

## 障害の切り分け
| 表示・症状 | 疑う原因 | 検証・対処 |
| --- | --- | --- |
| 対象なし | 別の操作で削除済み | サーバーから再読込し対象の有無を確認 |
| 競合メッセージ | 画面のVersionが古い、またはSaveChanges中に変更された | 最新行を再読込して判断し直す |
| Versionが見えない | 画面を読込んでいないかDB未設定 | 専用DB設定と再読込ボタンを確認 |
| 誰でも更新できそうに見える | 教材用サービスは本番認可の例ではない | 本番処理はSecuredLearningServiceの都度認可を確認 |

## AD・既存台帳・監査との境界
ADは本人認証とユーザー名の取得にのみ使います。権限の登録画面や自動同期テーブルを新しく作る前提にはしません。事前に整備されたDBの利用者台帳または外部台帳を参照し、ログイン名、所属、業務権限、対象行との対応、有効・失効状態を**要求のたびにサーバー側で**確認します。一覧・Count・集計・CSV・更新・削除はどれも別の出口であり、どれか一つの画面で認可済みでも他の操作を無条件に許可しません。

この教材の学習順047〜40のLearningDbServiceは、学習専用DBを扱うための簡素なサービスです。WindowsAuthentication:Enabledの場合には無認可サービスの実DB操作を止める安全策があります。実業務の権限済み処理はSecuredLearningServiceなどの安全な入口で設計・検証します。監査の失敗や権限失効、更新競合、FDWの原子性が不明な状態は成功として扱わず、必要な操作を拒否します。

## 演習
演習1：タブAとタブBが同じVersionを表示した後、Aが先に更新した場合、Bを黙って成功させると何が問題でしょうか。演習2：読み出したVersionが最新なら部署権限の再確認を省いてよいでしょうか。演習3：行が削除されたときの案内と、他者が変更したときの案内を設計してください。

## 詳しい解答
詳しい解答：1ではBが古い表示を根拠に操作するため、Aの結果を意図せず戻したり別の状態へ変えたりする恐れがあります。行の版を比較し、不一致なら最新行を読み直す案内にします。2では版の一致と認可の有効性は無関係なので省略できません。ADによる本人認証だけでなく、各操作時点のDB台帳における本人・所属・権限・対象行の判定を再実施します。3では対象が消えた場合は削除を伝え、別更新の場合は競合を伝えます。いずれも旧画面の値を使って自動再送しないことがポイントです。

## 完了条件
- [ ] 学習順057のRazor画面と対応するServiceまたはDataコードを読み、処理場所を説明できる。
- [ ] WinForms＋Dapperの実装と比較し、SQL実行・変更追跡・接続の寿命の違いを説明できる。
- [ ] 正常表示と失敗表示、DB非接続と実DB操作を混同しない。
- [ ] 認可・監査・安全拒否を本番受入から省略しない。

次：学習順058
