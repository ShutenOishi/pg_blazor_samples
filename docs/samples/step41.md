# ステップ41：Itemsを使用する条件と取得済み一覧


**学習順061・第6段階／第13組**

## できるようになること

すでにメモリ内にある8件の固定リスト。今回のEF Core 10連携は、**既存のWebプロジェクト内**のData/LearningPageQuery.cs、Services/Learning/LearningPageService.csで実装します。Domain・Infrastructure・Repositoryの別プロジェクトは作りません。

## 前提

Step31〜40でIDbContextFactory、DTO、日付型を学び、[MudBlazor用途別特別コラム](../columns/column-mudblazor-by-purpose.md)で表・DataGridを使い分けます。DBへ問い合わせる画面には**開発専用PostgreSQL 18**、学習用のDDLと架空データ、環境変数ConnectionStrings__PostgreSQLが必要です。資格情報はソースに記載しません。

## 実際に操作する

1. /learn/41 を開き、日本語ラベルと表示件数を確認します。
2. MudTableのItemsに固定数のDTOを渡し、RowsPerPage=3のページ送りを確認。DBの問い合わせはありません。
3. DBが設定されない環境で、未接続を成功と誤認しないことを確認します。
4. 小規模ItemsとServerDataを比較し、どの時点でDB検索が行われるか説明します。

## 重要なコードと役割

~~~csharp
// Webのサービスで結果を作り、UIには行と総件数だけ返す。
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var query = db.Tasks.AsNoTracking();
// Where(条件)の後に同一条件のCountを取得する。
var total = await query.CountAsync(cancellationToken);
// ソートは許可リストから式木で選ぶ。
var rows = await query.OrderBy(t => t.Id).Skip(page * pageSize)
    .Take(pageSize).Select(t => new LearningTaskRow(
        t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
    .ToListAsync(cancellationToken);
~~~

上の抜粋は仕組みを示す最小例です。検索条件・複数列ソート・ページサイズと入力の検証を含む完全な実装はLearningPageService.LoadAsyncを参照します。MudTableはTableState、MudDataGridはGridStateと、**どちらもCancellationTokenを受け取るServerData**を使用します。

## 誤解・安全上の注意

大量の業務テーブルをToListAsyncで全件ロードしてからItemsへ渡すとメモリ・ネットワーク・初回応答の問題が起きる。

- UIのページ表示範囲、DB側の取得行数、絞り込み前のテーブル全件数、絞り込み後のCountは別の概念です。
- 画面のフィルターは認可ではありません。本番業務では業務サービスのWhereに権限条件を追加し、Countにも必ず適用します。
- PostgreSQLのILIKE/LIKEの挙動や照合順序は日本語検索に影響するため、性能・索引を後続のStep50・59で確認します。
- 大量行の実測ではEXPLAIN（ANALYZE, BUFFERS）、DbContext/接続プール、CancellationTokenを合わせて検証します。

## 確認問題

**Q. Itemsが表示できるのでDB100万件を最初に全部取得してよい？**  
A. いいえ。必要なページ・絞り込みをDBに渡します。

**Q. ServerDataで表示する20件だけCountすればよい？**  
A. いいえ。ページャーの総件数には、同じ検索・認可条件を適用した全一致件数を返します。

**Q. ソート列名をそのままSQLに連結してよい？**  
A. いいえ。許可リストに変換してEF Coreの式でソートします。


## 学習順061：実装に即した詳しい解説

このテーマの中心は固定8件のItemsです。MudTableはRowsPerPage=3で画面の描画範囲だけを切り替える。DBへの問い合わせは0件であり、学習用の固定配列を作っている。 Itemsを与える場合はあらかじめ件数上限が明確で、全件を一度に保持して差し支えないことが必要である。既存業務台帳の行が数十万件あり得る場合、全件取得後にUIのページャーで3行だけ見せても負荷は減らない。

### 実コード例1：画面と検索条件

```csharp
// ItemsBoundaryPage.razor の固定データ
var items = Enumerable.Range(1, 8).Select(i => new LearningTaskRow(
    i, $"架空の課題{i:00}", new DateOnly(2026, 10, 26),
    new TimeOnly(9, 0), i % 2 == 0, 0)).ToList();
```

### 実コード例2：境界と戻り値

```csharp
// Step41ではDB接続を行わない。表示する件数を固定する。
var visible = items.Skip(3).Take(3).ToArray();
// 2ページ目は3件、総件数は8件のまま。
```

### WinForms＋Dapperとの比較

| 観点 | WinForms＋Dapper | Blazor＋EF Core 10 |
| --- | --- | --- |
| 表示イベント | フォームでデータを読みDataGridViewへ設定 | Razorのイベント・ServerDataコールバックを使用 |
| データ取得 | SQLとパラメーターを手動構築 | LearningPageServiceが型付きの検索条件を受ける |
| 取得範囲 | LIMITやOFFSETを自分でSQLへ書く | Where、OrderBy、Skip、TakeをDBへ変換 |
| 状態管理 | 画面がDataTableなどを保持 | DTOと総件数だけを画面へ返す |
| 接続管理 | using NpgsqlConnectionを短時間開く | IDbContextFactoryから操作ごとにContextを作る |
| 安全条件 | SQLパラメーター・台帳認可を呼び出し側で守る | 入力検証と台帳認可をサービス側で必ず行う |

この表は実装方法の比較であり、EF Coreを導入するだけで認可や監査が完成することは意味しません。Itemsはこの8件を全部保持してMudTableへ渡す。 このプロジェクトはWeb内のDataとServicesで完結し、学習のためにDomainやInfrastructureなどの別プロジェクトを作りません。ServerDataを使う画面のCancellationTokenはDbContext生成、CountAsync、ToListAsyncまで伝達します。取消を通常0件へ握りつぶすと古い検索の結果が画面へ戻るおそれがあるため、OperationCanceledExceptionと接続失敗の扱いを区別します。

### 操作手順

1. `/learn/41`へアクセスして日本語の表示と入力欄を確認します。2. 表の表示行数を確認してからページ送りと列見出しの並び替えを試します。3. DB未設定なら専用の未設定メッセージを確認し、正常な取得0件と区別します。4. PostgreSQL18の学習用DDLと架空データを用意できる場合だけDB検索を実行します。5. 検索条件の変更、再検索、ページ位置の変化を記録します。6. ソースのRazor→LearningPageQuery→LearningPageService→EF Core→Npgsqlの順に処理の実行場所を追跡します。7. Countと表示行を同じ条件で取得することを説明します。

### 障害の切り分け表

| 観察した症状 | 考える原因 | 確認・復旧方法 |
| --- | --- | --- |
| 一覧が空で「未設定」と出る | 開発DBの接続設定がない | ConnectionStrings__PostgreSQLと学習用DDLを確認する |
| 条件を変えると空ページになる | 以前のPageが残っている | 検索開始時に先頭ページへ戻す動作を確認する |
| ページの行数と総件数が違う | TotalCountは一致した全行数 | CountとTakeの意味を分けて確認する |
| 同じ値の行が不安定に並ぶ | 一意な並び順がない | ソートの末尾にIdがあるかを確認する |
| 読み込みに時間がかかる | SQLの計画、索引、ネットワーク等 | 専用DBで実行計画とサーバーログを確認する |
| 不許可の列・範囲で失敗 | Validateの入力拒否 | サービスの許可リストと上限を確認する |

### 演習と詳しい解答

課題：固定8件の一覧と部署別に変化する100万件の一覧について、ItemsかServerDataを選んで理由を述べてください。

**詳しい解答：** 解答：固定8件は上限と更新頻度が明確なのでItemsが適しています。100万件のDB行は全部読み込まず、既存DB台帳で認可された条件をサービスで適用してServerDataから必要なページだけ取り出します。RowsPerPage=3なら3ページ目は2件ですが、全件数が2になるわけではありません。

### 業務データへ適用する前の安全確認

このサンプルが操作するのは学習用の架空データです。実際の業務ではADで本人認証しユーザー名を取得しても、その事実だけでは対象レコードの閲覧・更新を許可できません。既存DBの利用者台帳または外部台帳を正本として、本人、所属、対象行、操作の条件を毎回確認します。画面に表示しないだけではDB取得の拒否にならず、Count、一覧、集計、CSVなどすべてのサービス経路に同じ認可条件を掛けます。権限失効、監査への記録失敗、不明な利用者などの失敗を成功として扱わず安全に拒否します。実AD、IIS、Kerberos、FDW、1万人を超える負荷の現地受入は別途必要です。

## 完了条件

- [ ] MudTableのItemsに固定数のDTOを渡し、RowsPerPage=3のページ送りを確認。DBの問い合わせはありません。
- [ ] DB未設定と正常なDB取得の違いを理解
- [ ] ServiceがAsNoTracking/Where/OrderBy/Skip/Take/DTOのどれを担当するか説明
