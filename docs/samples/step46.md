# 学習順066：関連テーブルとDTO射影

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L066RelatedDtoPage.razor)。画面の正規URLは `/learn/066` です。

**学習順066・第6段階／第14組**

## 学習目標と前提
WinForms＋Dapperに慣れた学習者が、Blazor Interactive ServerとEF Core 10で同じ処理をどこに記述するか理解することが目標です。Visual Studio 2026、.NET 10、MudBlazor 9、PostgreSQL 18を前提とします。学習順061〜45で学んだItems、ServerData、並び順と検索を振り返ってからこの画面へ進んでください。例で扱うのは学習用の架空の課題データです。実ADや業務台帳を使った実機受入ではありません。

## 処理の順序と現行実装
LoadCategoriesAsyncはDBのTasksにAsNoTrackingを指定し、CountAsyncで総件数を計算します。一覧はId昇順の先頭20行から、CategoryTaskRow(Id, Title, CategoryName)だけをSelectで射影します。関連CategoryがないときCategoryNameは未分類です。EF Coreがナビゲーション参照をSQLのJOINへ変換し、行ごとに分類を再取得するN+1を防ぎます。

Razorの表示と入力はWeb画面の担当で、DB接続や検索・件数の決定はWebサーバー上のサービスが担当します。WinFormsではフォームのイベントから直接DapperのSQLを呼んだ経験があるかもしれませんが、Interactive Serverではブラウザーとサーバーを対話回線で結び、C#のイベント処理はサーバー側で実行されます。接続が切れる可能性や、短時間に複数の要求が重なることを考慮し、例外処理、CancellationToken、DbContextの寿命を分けて設計します。

## 実コード例1：サービス側

```csharp
var query = db.Tasks.AsNoTracking();
var total = await query.CountAsync(token);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new CategoryTaskRow(t.Id, t.Title,
        t.Category == null ? "未分類" : t.Category.Name))
    .ToListAsync(token);
```

このコードは実装の中核を示す抜粋です。入力の上限検査、DB未設定の確認、例外ログ等は同じサービスの別の処理も読み、成功時と失敗時の戻り値を区別します。EF Coreの生成SQLでPostgreSQLへ渡す条件・列・件数を確認します。画面で表示される状態メッセージとサービスのMessageは合わせて読みます。DBへクエリを送る時点で、実運用では毎回の台帳認可が必要です。

## 実コード例2：Razor又はイベント処理

```razor
<MudTable T="CategoryTaskRow" Items="@_rows" Dense="true">
    <HeaderContent><MudTh>番号</MudTh><MudTh>課題名</MudTh><MudTh>分類</MudTh></HeaderContent>
    <RowTemplate>
        <MudTd>@context.Id</MudTd><MudTd>@context.Title</MudTd>
        <MudTd>@context.CategoryName</MudTd>
    </RowTemplate>
</MudTable>
```

画面に渡すものはDTO、件数、状態メッセージであり、DbContextやIQueryableを利用者の対話状態へ持ち越さない構造です。DB読み取りの時点でCancellationTokenを使用できるように、Razorのコールバックからサービスまで引き継ぎます。表示だけを切り替える条件と、DBで実際に返す行を絞る条件は同じものではありません。

## WinForms＋Dapperとの比較
| 観点 | WinForms＋Dapper | Blazor＋EF Core 10 |
| --- | --- | --- |
| この節の中心 | DapperのSELECT LEFT JOINで必要な列を取得し、手動でDTOへ変換 | Selectが関連Categoryの列だけをDTOへ射影し、MudTableへ渡す |
| UIイベント | フォームのイベントハンドラー | RazorのC#イベントと対話回線 |
| データアクセス | using NpgsqlConnectionとSQLパラメーター | IDbContextFactoryで操作ごとのDbContext |
| 表の描画 | DataGridViewに行を渡す | MudTable又はMudDataGridにDTOを渡す |
| 検索条件 | SQLのWHERE/ORDER BY/LIMITを明示 | LINQのWhere/OrderBy/TakeからSQL生成 |
| 安全性 | 入力値の検証、業務台帳の都度認可と監査 | 同じ条件をサービスのDB取得前に実施 |

## 実装を選ぶ理由と適用範囲
Includeは関連Entity自体が必要な場合に選ぶ処理です。単に分類名を一覧へ表示するだけならDTO射影を優先します。Take(20)は画面の上限で、TotalCountは検索対象の全件数です。DB未設定時は未設定の案内、DB取得時の失敗は読込失敗の案内として区別します。

特に件数は四つの意味に分けて考えます。DBの総件数、検索条件に一致する件数、今回DBから返す行数、画面が実際に描画する行数です。目の前の表に10件しか見えないことから、DBが10件だけ返したとは推測できません。CountとRowsの条件が異なればページャーも正しく動作しません。必要ならPostgreSQL 18の学習用DBでToQueryString、EXPLAIN (ANALYZE, BUFFERS)を確認します。結果は索引、統計、データ量で変わるので一律の速さを断言しません。

## 操作手順
1. ブラウザーから /learn/066 を開き、学習順066と説明を確認します。
2. DB未設定の環境では未設定の案内を確認し、実DBの取得成功と区別します。
3. 「関連表を読み込む」を押して総件数と先頭20件、分類の表示を比較します。
4. 開発用PostgreSQL 18と架空データを用意できる場合は、同じ操作でDBの結果と画面を照合します。
5. Razor→LearningAdvancedPageService又はLearningPageService→DbContext→SQL→DTO→表示の順に呼出しを追います。
6. 学習順067へ進み、正規URL /learn/066 が維持されていることを確認します。

## 障害の切り分け
| 症状 | 考えられる原因 | 確認と対処 |
| --- | --- | --- |
| DB未設定と表示 | 開発用接続が未設定 | ConnectionStrings__PostgreSQLを確認し秘密情報は表示しない |
| 空の表なのに正常と見える | 検索不一致かDB障害かが不明 | statusとログを読み、通常0件と区別する |
| 表示行数と総件数が違う | CountとTake、描画件数の違い | サービスの戻り値を追い、何件を数えたか確認 |
| 同じ行が繰り返される | ソート順又はカーソルが不安定 | Id等の一意順序とフィルターの適用順を確認 |
| 操作を重ねると旧結果が出る | 非同期処理の取消が伝わっていない | CancellationTokenの経路と例外処理を確認 |
| DB検索が遅い | 深いOFFSETや索引の不足 | 学習DBでSQLの実行計画を確認 |
| 期待したUIにならない | ブラウザー又はサーバー側のエラー | ChromeのConsole/Networkとサーバーログを分けて確認 |

障害を切り分ける際は、最初に再現した操作を記録し、表示のstatusを見て、次にサーバーログ、最後にDBへ届いたSQLを確認します。クライアント上のデータの絞り込みは業務のデータアクセス拒否ではありません。また、想定外の入力を正常0件として握りつぶすと、設定失敗や取消と区別しにくくなります。

## 演習
学習DBに25件あり、そのうち3件が未分類です。画面が表示する行数とTotalCount、分類のない行の表示を答え、N+1を避ける取得処理を説明してください。

### 詳しい解答
先頭の最大20行が表示されTotalCountは25です。表示対象に含まれる分類なしの行は「未分類」となります。分類名の取得はSelectの中で関連Categoryを参照し、EF Coreに単一一覧クエリのJOINを組み立てさせます。全25行が画面にあるという意味ではありません。

## 実務へ適用する前の安全確認
この教材は学習データを用いています。AD本人認証に成功しユーザー名を取得しても、業務の閲覧・更新を許可する根拠にはなりません。事前に登録済みの既存DB台帳または外部台帳から、本人・対象業務・操作・対象行を都度判定します。一覧のCountとRows、詳細、編集、CSV出力にも同じ認可条件を適用してください。権限失効、監査記録失敗、xmin競合、FDWを含む別DB間の原子性不明は安全に拒否します。application_nameは個人監査の代用にはなりません。実AD、Windows Server 2025/IIS、Kerberos、FDW、1万人超負荷は現地実機で別途受入が必要です。

## よくある誤解を確認する
- WinFormsで動いていたDapperの接続をそのままRazorのフィールドに保持してよいでしょうか。いいえ。複数の要求や対話回線の寿命に対応し、操作ごとにDbContextを生成・破棄します。
- DBから取り出したレコードを画面で非表示にすれば認可できますか。いいえ。サーバーでDBへ問い合わせる前に権限を判定し、許可された行だけを取得します。
- 一覧表示が速いことはDB検索が速いことと同じですか。いいえ。キャッシュ、転送、描画、SQLは別の時間を消費します。段階ごとに測定します。
- 取消例外が起きたら0件正常と返してよいですか。いいえ。古い要求の結果が新しい画面を上書きしないように、取消を通常0件と区別します。

## 完了条件
- [ ] 学習順066の実画面と二つのコード例を対応付けた
- [ ] WinForms＋Dapperとの差とサービスの責任を説明できる
- [ ] 件数・描画・DB処理とPostgreSQL18の実接続を区別できる
- [ ] 業務台帳による認可と安全拒否が別途必要と説明できる

## 次に学ぶ内容
学習順067（/learn/067）へ進みます。
