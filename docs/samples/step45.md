# ステップ45：複合条件の検索・Count・日付フィルター

## できるようになること

文字、完了状態、期限日の開始/終了を複合絞り込み。今回のEF Core 10連携は、**既存のWebプロジェクト内**のData/LearningPageQuery.cs、Services/Learning/LearningPageService.csで実装します。Domain・Infrastructure・Repositoryの別プロジェクトは作りません。

## 前提

Step31〜40でIDbContextFactory、DTO、日付型を学び、[MudBlazor用途別特別コラム](../columns/column-mudblazor-by-purpose.md)で表・DataGridを使い分けます。DBへ問い合わせる画面には**開発専用PostgreSQL 18**、学習用のDDLと架空データ、環境変数ConnectionStrings__PostgreSQLが必要です。資格情報はソースに記載しません。

## 実際に操作する

1. /learn/45 を開き、日本語ラベルと表示件数を確認します。
2. 検索語、完了のみ/未完了のみ、MudDatePickerの日付範囲を選び検索する。
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

Countとページに同じWhereを適用する。SQLのパラメーター化、DB文字列照合、検索入力の長さ・NULL・開始が終了より後の場合を検証する。

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

## 完了条件

- [ ] 検索語、完了のみ/未完了のみ、MudDatePickerの日付範囲を選び検索する。
- [ ] DB未設定と正常なDB取得の違いを理解
- [ ] ServiceがAsNoTracking/Where/OrderBy/Skip/Take/DTOのどれを担当するか説明
