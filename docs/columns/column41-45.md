# 学習コラム10：一覧のItemsとServerData、ソート・検索を要件から選ぶ

## 学習順061〜45の流れ

学習順061では「小さい・件数が決まっている一覧にItemsを使う」条件を復習します。学習順062ではMudTable ServerData、学習順063ではMudDataGrid ServerData、学習順064では複数列ソートの許可リスト、学習順065では文字・日付・完了状態の絞り込みを組み合わせます。

## 選択の目安

| 用途 | 第一候補 | 警戒すべき点 |
| --- | --- | --- |
| 8件の固定リスト | Items | DB全件取得と混同しない |
| 一般的な業務表・シンプルな列 | MudTable ServerData | TableStateのPage/SortLabelと取消 |
| 高機能・多列の業務一覧 | MudDataGrid ServerData | GridStateのSortDefinitionsと総件数 |
| 複数列ソート | 許可リスト＋ThenBy | 列名を生SQLにしない、主キーの一意順序 |
| 複合検索 | 共通のWhere→Count/Items | 列名や入力値の検証、認可条件の統一 |
| スクロールの可視範囲だけDBから読む | VirtualizeServerData | 学習順069で高さ/取消/遅延を実装 |

## DbContextFactoryのDIとキャンセル

MudTableとMudDataGridのServerDataは、**stateとCancellationTokenの2つ**を受けます。LearningPageServiceがIDbContextFactory<LearningDbContext>から1操作1DbContextで取得し、CountAsyncとToListAsyncの両方にトークンを渡します。長寿命のScoped DbContextをコンポーネントで持たず、独立したDomain/Infrastructureプロジェクトは追加しません。

## 競合と安全に関する重要な誤解

1. Virtualize=trueにしただけでDBからの全件読み込みを避けられるわけではありません。
2. ページング時に一意な並び順がなければ、ページ境界に重複・欠落が生じ得ます。
3. Countに検索条件を適用しないと、ページャーが誤った総件数を表示します。
4. 画面に権限フィルターをかけるだけでは安全になりません。サーバー側のWhereにも認可条件が必要です。
5. 文字列検索の大文字・小文字、部分一致、文字正規化と日本語の扱いはDBの照合順序・索引と関係します。

## 確認問題

**Q. MudTableとMudDataGridのどちらを優先する？**  
A. 一般的で単純な一覧はMudTable、列ごとの高度な操作が必要ならMudDataGridを優先します。

**Q. 件数と行を別クエリで取るとき注意することは？**  
A. 両方に同じ絞り込み・認可条件を適用します。厳密な同時整合性が必要ならトランザクション分離レベルや要件も検討します。

**Q. ユーザー指定の並び替え列を安全にSQLへ渡すには？**  
A. 許可済み列をEF Coreの静的な式木に変換します。最後は一意のIDで順序を安定させます。

## 次へ

学習順066〜50ではDTO射影と関連テーブル、キーセットページング、描画仮想化、VirtualizeServerData、検索UXと障害対応を学びます。

## 第13組の復習：学習順061〜065

この復習では、画面表示の成功とPostgreSQL 18の実データ取得を混同せず、一覧パターンの選定、ページング、ソート、検索、安全拒否を同じ業務要件で検討します。

| 学習順 | UI／入力 | サーバーで必ず行うこと |
| --- | --- | --- |
| 061 | 固定8件のItems | DBには接続せず、表示ページだけ変更 |
| 062 | MudTable／TableState | ページ・検索・単一列ソートの変換 |
| 063 | MudDataGrid／GridState | GridStateをDTOへ変換、短命Contextで検索 |
| 064 | 複数列ソート | 最大3列の許可リスト、末尾Idで安定化 |
| 065 | 検索語・完了・日付範囲 | 入力検査、同じWhereによるCountと行取得 |

| WinForms＋Dapper | Blazor＋EF Core 10 | 注意する失敗 |
| --- | --- | --- |
| DataGridViewへ全件代入 | 固定少数ならItems、業務表はServerData | 全件取得の重さを隠さない |
| QueryAsyncでOFFSET／LIMIT | Page・PageSizeをSkip／Takeへ変換 | 0始まりと上限50を守る |
| SQLのORDER BYを手書き | SortDefinitionsを静的式へ変換 | SQLへ利用者入力の列名を直結しない |
| WHEREとCOUNTを個別に記述 | 共通IQueryableのWhere→Count／行 | 部署認可や検索条件を片方で落とさない |

### 例1：TableStateと総件数の契約

```csharp
var result = await Pages.LoadAsync(
    new LearningPageQuery(Page: 2, PageSize: 5, Keyword: "点検"), ct);
// Rowsは最大5件、TotalCountは同じ検索条件での一致総数。
return new TableData<LearningTaskRow> {
    Items = result.Rows, TotalItems = result.TotalCount
};
```

### 例2：許可済みソートと検索範囲

```csharp
LearningPageService.Validate(new LearningPageQuery(
    Page: 0, PageSize: 5, IsDone: false,
    DueFrom: new DateOnly(2026, 10, 1),
    DueTo: new DateOnly(2026, 10, 31),
    Sorts: [new LearningSort("DueDate"), new LearningSort("Id")]));
```

### 実画面で操作する順序

1. `/learn/061`でItemsの8件、DB問い合わせ0件を確認します。2. `/learn/062`で検索語を入力し、TableStateの0始まりページ番号と件数を確かめます。3. `/learn/063`でGridStateが行と件数を返す流れを読み、列フィルターが現在無効であることを確認します。4. `/learn/064`で複数ソートのIndex順と最後のIdを確認します。5. `/learn/065`で日付逆転と検索語100文字超の拒否を確認します。6. 開発用DB未設定の環境では、未設定表示の確認を実DB結合成功とは扱いません。

### 障害切り分け表

| 症状 | 原因候補 | 確認箇所 |
| --- | --- | --- |
| 初期表示が未設定 | 接続文字列不足 | LearningDbService.Configured |
| 不正列で例外 | ソート列が許可外 | IsAllowedSortとValidate |
| 0件なのにページが残る | 検索前のページ番号を保持 | SearchAsyncとReloadServerData |
| 同順位が入れ替わる | ORDER BY末尾Id不足 | LearningPageService.AddOrder |
| 取消後に古い結果が戻る | CancellationTokenの伝達不足 | CountAsyncとToListAsync |

### 演習

架空の課題100万件から、自部署の未完了案件を対象にタイトルの部分一致、期限日範囲と期限日昇順の5件ページを表示する設計を説明してください。RowsPerPage=5の表で3ページ目を表示した際のSkip、Count、認可の検査位置を答えてください。

### 詳しい解答

膨大なDB行をItemsへ渡す方式は選びません。ServerDataが検索条件とページ情報をDTOへ変換し、Webサービスが既存DB台帳に基づく部署・対象行・操作権限を毎回検証します。サービス側のWhereへ検索と認可を組み込み、同じ条件のCountAsyncと並び替え後の行取得を実行します。3ページ目はPage=2なのでSkip(10)とTake(5)です。期限日が同じ案件があるので一意のIdを末尾に追加します。取消トークンをDB操作へ伝え、監査記録に失敗したときはデータを返さず拒否します。ADログインだけでは業務認可が成立したことになりません。既存の学習用画面は実AD/IISの検証ではなく、実機受入は別に行います。

次は第14組の学習順066〜070で、DTO射影・キーセットページング・仮想スクロール・検索UXへ進みます。
