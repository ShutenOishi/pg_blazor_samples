# 学習順046：DapperとEF Coreの役割・選定を比較する

## 到達目標・前提知識
DapperのQuery/ExecuteとEF CoreのLINQ/SaveChangesを、読み書きの要件から比較します。DapperはSQLの構造と実行を明示しやすく、EF Coreはモデルのマッピング、変更追跡、SaveChanges、競合検出を一貫して扱えます。どちらもPostgreSQLがSQLを実行する点は同じです。EF Coreなら自動で速くなる、SQLの知識が要らなくなる、認可が自動で成立するといった考え方は正しくありません。
Webプロジェクト内のDataとServicesを使い、学習目的でDomain/Infrastructureなどを増設しません。学習画面では「一般的なCRUD」「既存の複雑なSQL」「大量一覧」を押すと、採用を検討する方式が文字で表示されます。この画面のボタンは実際のDB検索ではありません。選定の理由を言葉で説明する練習です。
前提は第4段階で学んだRazorイベント、DI、MudBlazorの日本語UIです。この章は既存80ステップのURLを維持したまま、全110本の学習順に従って読みます。必要に応じて開発用PostgreSQL 18を使用します。

## 仕組み・処理の流れ
一般的なCRUDでEntityの変更追跡とxminによる競合検出を使いたい場合はEF Coreが扱いやすい候補です。既存の複雑なSQLや特定のクエリプランを維持する場合はDapperとの併用、またはEF CoreのFromSqlを検討できます。大量一覧の場合は方式の名前より、検索条件、安定したORDER BY、ページング、必要列の射影、実行計画とインデックスが重要です。画面に10万件を一度に読み込む設計は、どちらを採用しても良い設計になりません。
既存SQLを残す場合も、利用者の入力をSQL本文へ直接連結しません。値はパラメーター化し、列名や並べ替え種別などパラメーター化できない識別子は許可した候補から選びます。DapperからEF Coreへ移行しただけではSQL注入、部署外参照、機密データ出力、監査失敗が解消するわけではありません。異なるORMを同じ更新に使うときはトランザクションと監査の原子性が成立するかを確認します。

## WinForms＋Dapperとの具体的な比較
| 比較の観点 | 従来の実装 | Web＋EF Core | 判断する点 |
| --- | --- | --- | --- |
| CRUDの変更 | SQL文とExecuteを自分で記述 | DbContextの追跡とSaveChanges | 競合・保守要件で選ぶ |
| 既存SQL | SQLを直接使用 | FromSql等も使用可能 | SQL保守と検証可能性で選ぶ |
| 大量一覧 | WHERE/ORDER BY/LIMITを記述 | LINQをDB側のSQLへ変換 | 実行計画・索引・件数制限で測定 |

## 実装に対応するコード例1
```csharp
// Dapperで書く場合の比較用参考例（この画面はDB実行しない）
const string sql = "SELECT id, title FROM learning_task WHERE title ILIKE @Term ORDER BY id LIMIT 20";
var records = await connection.QueryAsync<TaskSummary>(
    sql, new { Term = "%練習%" });
```
このコードは実装の一部分、または比較用と明記した参考例です。画面がどこまで動作するかを区別して確認します。

## 実装に対応するコード例2
```csharp
// LearningDbService.SearchAsyncにある実装の主要部分
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var query = db.Tasks.AsNoTracking();
if (!string.IsNullOrWhiteSpace(keyword)) {
    var term = keyword.Trim();
    query = query.Where(t => t.Title.Contains(term));
}
int count = await query.CountAsync(cancellationToken);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(
        t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
    .ToListAsync(cancellationToken);
```
表示用DTO、SQLの型、短命DbContext、DI登録などの役割は異なります。コードが示す操作の対象、寿命と副作用を説明してから次へ進んでください。

## Webに移ったときの基本的な境界
この教材ではBlazor Interactive Serverを使います。画面に表示されるHTMLやクリック操作の入口はブラウザーにありますが、RazorイベントのC#、DBサービス、EF Core、NpgsqlはWebサーバーで動きます。WinFormsではフォームのイベントが利用者PC上のプロセスで実行されていました。Webでは複数利用者の操作がサーバーに集まるため、画面に保持した状態とDB接続の寿命を同一視してはいけません。画面で固定の架空データが表示できたこと、DIの設定が読み取れたこと、SQLのテーブル定義が表示できたことは、それぞれ実DBへの接続が成功した証拠ではありません。
業務用のデータアクセスには別の条件があります。ADは本人認証とドメイン付きユーザー名の取得に用い、操作対象の部署・行範囲・権限は事前に整備されたDBまたは外部台帳を正本として毎操作サーバー側で判定します。データベース接続に付けるapplication_nameはアプリケーションの識別補助であって利用者別監査の代替ではありません。監査ができない更新、競合している更新、権限失効を確認できない操作は安全に拒否します。学習用の匿名データサービスを業務認可の済んだサービスとして流用しません。実AD、IIS、Kerberos、FDW、1万人以上の実機負荷は教材の模擬試験と分けて評価します。
開発環境ではVisual Studio 2026、.NET 10、EF Core 10とNpgsql、専用PostgreSQL 18を用います。DBが必要な操作だけ専用環境を構成し、業務DBや実在利用者データを練習対象にしないでください。接続情報にパスワードが含まれる場合はGitへ記録しません。コード例は既存実装の主要部分を抜粋したものと、比較のためだけの参考例を区別します。

## 実際の画面操作手順
1. /learn/db-bridge-01で一般的なCRUDを選択しEF Core候補という結果を読む。
2. 既存の複雑なSQLを選びDapperとFromSqlを比較する。
3. 大量一覧を選び、どちらもDB側で件数制限が必要と確認する。
4. 開発者ツールで画面操作がPostgreSQLへの直接接続ではないことを説明する。
正常系では説明用の結果と実DB操作の結果を分けて記録します。失敗例では設定未入力、サーバーからDBへの接続不能、DDL不一致、業務認可拒否、更新競合を同じエラーと見なさず、現象から原因を分類してください。

## 障害の切り分け表
| 現象 | 確認する場所 | 対処 |
| --- | --- | --- |
| ボタンは動くがDB行が出ない | 方式選定画面はDBへ接続しない | /learn/34で実DBを確認 |
| 一覧が20行で止まる | Take(20)が意図的に適用される | 総件数と表示件数を区別 |
| 結果順が安定しない | 一意のOrderBy指定を確認 | Id等を含めた安定順を使用 |

## 演習問題と詳しい解答
**問1：** 件数が百万件の一覧をEF Coreへ移すだけで高速化するか。**詳しい解答：** しません。DB側でWHEREとORDER BYとLIMITを実行し、取得列・索引・実行計画を実測します。**問2：** EF Coreなら利用者がSQLに文字列を連結しても安全か。**詳しい解答：** いいえ。生SQLに文字列を連結すれば危険です。値をパラメーター化します。**問3：** 方式選定ボタンの成功は実DB受入か。**詳しい解答：** いいえ。あくまでUI内の説明結果であり別の実接続試験が必要です。

## 次のステップへの接続
次の047では、EF CoreのEntityと画面に渡すDTOを区別します。DapperでSELECT結果をPOCOへ変換していた経験を使い、列の取得範囲と追跡対象を確認します。
