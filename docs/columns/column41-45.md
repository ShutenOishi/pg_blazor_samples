# 学習コラム10：一覧のItemsとServerData、ソート・検索を要件から選ぶ

## Step41〜45の流れ

Step41では「小さい・件数が決まっている一覧にItemsを使う」条件を復習します。Step42ではMudTable ServerData、Step43ではMudDataGrid ServerData、Step44では複数列ソートの許可リスト、Step45では文字・日付・完了状態の絞り込みを組み合わせます。

## 選択の目安

| 用途 | 第一候補 | 警戒すべき点 |
| --- | --- | --- |
| 8件の固定リスト | Items | DB全件取得と混同しない |
| 一般的な業務表・シンプルな列 | MudTable ServerData | TableStateのPage/SortLabelと取消 |
| 高機能・多列の業務一覧 | MudDataGrid ServerData | GridStateのSortDefinitionsと総件数 |
| 複数列ソート | 許可リスト＋ThenBy | 列名を生SQLにしない、主キーの一意順序 |
| 複合検索 | 共通のWhere→Count/Items | 列名や入力値の検証、認可条件の統一 |
| スクロールの可視範囲だけDBから読む | VirtualizeServerData | Step49で高さ/取消/遅延を実装 |

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

Step46〜50ではDTO射影と関連テーブル、キーセットページング、描画仮想化、VirtualizeServerData、検索UXと障害対応を学びます。
