# 学習順048：PostgreSQLで学習テーブルを定義する

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L048SqlSchemaPage.razor)。画面の正規URLは `/learn/048` です。

## 到達目標・前提知識
PostgreSQL 18の開発専用テーブルlearning_taskを定義し、EF CoreのLearningDbContextと一致させます。学習順048のRazorは代表的な列を表で表示する説明画面で、開いただけでDDLを実行するものではありません。WinForms＋DapperでもDBの物理列を把握しましたが、EF CoreではOnModelCreatingで指定した名前・長さ・型とDDLの対応も確認します。
学習用DDLにはid、title、due_date、start_time、available_dates、is_doneがあり、後続でcategory_idとunit_codeを追加します。idはIDENTITY、xminは手動作成しないシステム列です。実業務DBにこのSQLを適用してはいけません。
前提は第4段階で学んだRazorイベント、DI、MudBlazorの日本語UIです。この章は既存80ステップのURLを維持したまま、全110本の学習順に従って読みます。必要に応じて開発用PostgreSQL 18を使用します。

## 仕組み・処理の流れ
bigint GENERATED ALWAYS AS IDENTITYはDB側が主キーを生成します。title varchar(100) NOT NULLは100文字以内の入力と非NULLを要求し、Web側でも入力を検証します。dateは日付、time without time zoneは時刻、daterangeは日付範囲です。Range型を画面の終了日と変換する場合、PostgreSQLで一般的な半開区間[)と、利用者が考える終了日を混同しないでください。
due_dateには通常のB-tree索引、available_datesにはGiST索引があります。列に索引を作ったという事実だけで、すべての検索が高速になるわけではありません。WHERE句と実行計画を調べ、実データの件数や分布が少ない場合にはSeq Scanが合理的なことも理解します。category_idはlearning_categoryへの外部キーで、unit_codeは部署別参照の学習で使います。
CREATE TABLE IF NOT EXISTSを使っているからといって、既存テーブルの列型が期待どおりである保証はありません。カラム一覧やテーブル定義を確認し、すでに別のスキーマで同名表がある場合のsearch_pathにも注意します。

## WinForms＋Dapperとの具体的な比較
| 比較の観点 | 従来の実装 | Web＋EF Core | 判断する点 |
| --- | --- | --- | --- |
| 主キー | SQLのidを明示的に使う | HasKey/ValueGeneratedOnAddと対応 | IDENTITYをDBが生成 |
| 日付列 | SQLのdate/timeを読む | DateOnly?/TimeOnly?へ変換 | NULLと型の対応 |
| 索引 | WHEREと実行計画で確認 | LINQ生成SQLにも影響 | インデックスだけで性能を保証しない |

## 実装に対応するコード例1
```sql
-- database/schema/001_learning_task.sqlの主要部分
CREATE TABLE IF NOT EXISTS learning_task (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title varchar(100) NOT NULL,
    due_date date NULL,
    start_time time without time zone NULL,
    available_dates daterange NULL,
    is_done boolean NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_learning_task_due_date
    ON learning_task (due_date);
```
このコードは実装の一部分、または比較用と明記した参考例です。画面がどこまで動作するかを区別して確認します。

## 実装に対応するコード例2
```sql
-- DDL後半：別テーブルと後続学習用列
CREATE TABLE IF NOT EXISTS learning_category (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(60) NOT NULL UNIQUE
);
ALTER TABLE learning_task ADD COLUMN IF NOT EXISTS
    category_id integer REFERENCES learning_category(id);
ALTER TABLE learning_task ADD COLUMN IF NOT EXISTS
    unit_code varchar(32) NOT NULL DEFAULT 'DEMO';
-- xminはシステム列なので作成しない
```
表示用DTO、SQLの型、短命DbContext、DI登録などの役割は異なります。コードが示す操作の対象、寿命と副作用を説明してから次へ進んでください。

## Webに移ったときの基本的な境界
この教材ではBlazor Interactive Serverを使います。画面に表示されるHTMLやクリック操作の入口はブラウザーにありますが、RazorイベントのC#、DBサービス、EF Core、NpgsqlはWebサーバーで動きます。WinFormsではフォームのイベントが利用者PC上のプロセスで実行されていました。Webでは複数利用者の操作がサーバーに集まるため、画面に保持した状態とDB接続の寿命を同一視してはいけません。画面で固定の架空データが表示できたこと、DIの設定が読み取れたこと、SQLのテーブル定義が表示できたことは、それぞれ実DBへの接続が成功した証拠ではありません。
業務用のデータアクセスには別の条件があります。ADは本人認証とドメイン付きユーザー名の取得に用い、操作対象の部署・行範囲・権限は事前に整備されたDBまたは外部台帳を正本として毎操作サーバー側で判定します。データベース接続に付けるapplication_nameはアプリケーションの識別補助であって利用者別監査の代替ではありません。監査ができない更新、競合している更新、権限失効を確認できない操作は安全に拒否します。学習用の匿名データサービスを業務認可の済んだサービスとして流用しません。実AD、IIS、Kerberos、FDW、1万人以上の実機負荷は教材の模擬試験と分けて評価します。
開発環境ではVisual Studio 2026、.NET 10、EF Core 10とNpgsql、専用PostgreSQL 18を用います。DBが必要な操作だけ専用環境を構成し、業務DBや実在利用者データを練習対象にしないでください。接続情報にパスワードが含まれる場合はGitへ記録しません。コード例は既存実装の主要部分を抜粋したものと、比較のためだけの参考例を区別します。

## 実際の画面操作手順
1. 開発専用DBでSELECT current_database(), current_user;を実行して接続先を記録する。
2. /learn/048で代表列の型を読み、まだテーブルは作られないと確認する。
3. 開発用DBにdatabase/schema/001_learning_task.sqlを適用する。
4. information_schema.columnsで列を照合し、SELECT id,title FROM learning_task ORDER BY id LIMIT 5を試す。
正常系では説明用の結果と実DB操作の結果を分けて記録します。失敗例では設定未入力、サーバーからDBへの接続不能、DDL不一致、業務認可拒否、更新競合を同じエラーと見なさず、現象から原因を分類してください。

## 障害の切り分け表
| 現象 | 確認する場所 | 対処 |
| --- | --- | --- |
| relation does not exist | DB名とスキーマを確認 | 開発用DBへDDL適用 |
| column does not exist | category_id、unit_codeの追加状況 | DDLとマッピングを照合 |
| permission denied | 接続ロールの権限 | 業務用ロールを緩めない |

## 演習問題と詳しい解答
**問1：** xmin列をDDLに作らない理由は。**詳しい解答：** PostgreSQLが用意するシステム列で、競合判定に使います。**問2：** IF NOT EXISTSで実行できれば列型の一致も保証されるか。**詳しい解答：** されません。既存同名表があれば作成を省略するため、定義を別途確認します。**問3：** daterangeにGiST索引があるなら必ず使われるか。**詳しい解答：** 必ずではありません。実行計画は統計・行数・検索条件で決まります。

## 次のステップへの接続
次の049ではこのDDLとDbContextのマッピングをProgram.csへ登録し、接続文字列を安全に渡します。画面とDDLを比べた段階を実DB接続成功と取り違えないようにします。
