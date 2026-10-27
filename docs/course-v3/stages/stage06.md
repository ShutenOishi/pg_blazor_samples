# 第6段階の概要：一覧データ連携・検索・編集

> 第6段階は全20件のうち学習順061〜065を改稿中です。後続066〜080の最終改稿・実機受入は別途確認します。

## この段階でできるようになること
Items・ServerData・仮想化・キーセットを条件で使い分け、認可の前段階として安全なデータ操作を理解する。

## この段階の前提
第5段階のDbContextFactory/SQL。PostgreSQL 18。

## WinForms・Dapperとのつながり
DataGridViewへの全件一括読み込みから、DB検索と画面描画を分離した方式へ移る。

## 学習の順番と到達点
- 学習順061：Itemsを使う一覧の条件（旧Step41に対応）
- 学習順062：MudTableのServerDataでDB検索（旧Step42に対応）
- 学習順063：MudDataGridのServerDataとEF Core（旧Step43に対応）
- 学習順064：複数列ソートを安全に組み立てる（旧Step44に対応）
- 学習順065：日付と完了状態で絞り込む（旧Step45に対応）
- 学習順066：関連テーブルをDTOへ射影する（旧Step46に対応）
- 学習順067：キーセットページングを理解する（旧Step47に対応）
- 学習順068：取得済み一覧を部分描画する（旧Step48に対応）
- 学習順069：DB取得と仮想スクロールを連携する（旧Step49に対応）
- 学習順070：デバウンスと障害時の再試行（旧Step50に対応）
- 学習順071：編集DTOをフォームで検証して保存する（旧Step51に対応）
- 学習順072：ダイアログで保存と取消を分ける（旧Step52に対応）
- 学習順073：複数選択と原子的な一括処理（旧Step53に対応）
- 学習順074：xmin競合と最新値の確認（旧Step54に対応）
- 学習順075：詳細DTOとN+1の防止（旧Step55に対応）
- 学習順076：日本語日付範囲とDB検索（旧Step56に対応）
- 学習順077：キャッシュと権限境界（旧Step57に対応）
- 学習順078：DB側集計とCSV出力（旧Step58に対応）
- 学習順079：EXPLAINと応答時間計測（旧Step59に対応）
- 学習順080：一覧パターンを要件から選ぶ（旧Step60に対応）

## 5ステップごとに確認すること
- 学習順065の後：第13組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順070の後：第14組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順075の後：第15組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順080の後：第16組の復習。直前の5テーマの関係を説明し、動作を再確認する。

## 練習課題と解答の確認点
CountとItemsの一致、安定ソート、キャンセル、xmin競合、CSV上限とEXPLAINを観察する。

**確認の基準：** 画面操作の結果だけでなく、処理がブラウザー・サーバー・DBのどこで動くか、例外や拒否をどう確認するかを説明できること。未実装の教材では学習結果の達成を宣言しません。

## 次の段階への準備
データ取得に本人・部署・操作単位の認可を適用する。

全体の順番は[新学習順表](../learning-sequence.md)を参照してください。

## 第6段階の最初の5件を学ぶ順序

第6段階は学習順061〜080の20件を扱います。このうち最初の5件で一覧取得の基本契約を確かめ、次の5件でDTO射影・キーセットと仮想化を扱い、その後に更新競合と統計・性能測定へ進みます。学習者はWinForms＋DapperのDataGridViewを使っていてもWeb開発は初めてという前提です。ブラウザーでページを移動したとき、Razorイベント、WebのLearningPageService、EF Core、Npgsql、PostgreSQL18のどこでどの処理が動くかを順に追ってください。

| 順番 | 実習画面 | 到達する判断 |
| --- | --- | --- |
| 061 | /learn/41 | DBに接続しない固定8件のItemsと表示行数を区別する |
| 062 | /learn/42 | MudTableのTableStateを検索DTOに変換する |
| 063 | /learn/43 | MudDataGridのGridStateとDTO・TotalCountの契約を確認する |
| 064 | /learn/44 | 最大3列の許可済み複数ソートとIdによる一意順序を理解する |
| 065 | /learn/45 | 日付・完了状態・キーワードを共通Whereへ適用する |

| WinForms＋Dapperでの経験 | Blazor＋EF Coreの置換先 | 確認すべき失敗 |
| --- | --- | --- |
| DataGridView.DataSourceに配列を設定 | 小さな固定配列だけItemsへ渡す | 件数が多いのに全件取得しない |
| ボタンでSQLを実行 | ServerDataからサービスを非同期呼び出し | CancellationTokenを伝達する |
| OFFSETとLIMITを手書き | Page×PageSizeをSkip、Takeへ変換 | ページ0始まりと上限50を守る |
| WHERE付きCOUNTを別途実行 | 共通queryのCountAsyncを使用 | 表示と件数の検索・認可条件を揃える |

### 処理を追うための実コード

```csharp
// LearningPageQueryはMudBlazorに依存しないWeb内のDTO
var request = new LearningPageQuery(
    Page: 0, PageSize: 5, Keyword: "点検",
    IsDone: false, DueFrom: new DateOnly(2026, 10, 1),
    DueTo: new DateOnly(2026, 10, 31));
LearningPageService.Validate(request);
```

```csharp
// サービスの処理は画面の寿命と分離する
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var query = db.Tasks.AsNoTracking();
var total = await query.CountAsync(cancellationToken);
var rows = await query.OrderBy(t => t.Id)
    .Skip(0).Take(5).ToListAsync(cancellationToken);
```

この抜粋のCountと行取得は、全条件を適用する前の仕組みを示す例です。実装では検索・ソートの条件を組み立てた後のIQueryableを共用し、必要列をLearningTaskRowへ射影しています。画面のGridStateなどをDbContextへ直接渡したり、Contextをコンポーネントへ長期間保持したりしません。

### 操作と観察

1. 先に第5段階のトランザクション、xmin、EF Coreの短命DbContextを確認します。2. 学習順061の8件固定を表示し、DB操作0件を観察します。3. 062と063でTableStateとGridStateの違いを確認します。4. 064で許可された列、最大3列、一意のIdを確認します。5. 065で日付逆転・キーワード100文字超を拒否し、正常0件と混同しないことを確かめます。6. 第13組復習で自分の判断を説明します。

### 典型的な障害の切り分け

| 症状 | 調べる順序 | 誤った対処 |
| --- | --- | --- |
| DB未設定 | 設定→専用DDL→サーバーログ | 固定Itemsが表示されたのでDB成功と宣言する |
| ページ重複 | ORDER BYの一意性→データ変化→OFFSET | 画面表示だけを並び替え直す |
| 件数の誤り | 共通Where→Count→行取得 | 行の長さだけをTotalItemsへ返す |
| 不許可行が見える | 既存台帳に基づく業務サービスの行単位認可 | Razorで非表示にするだけで済ませる |
| 検索が遅い | SQLとEXPLAIN、索引、キャンセル、負荷 | 無条件に全件を一度に取得する |

### 演習と詳しい解答

演習：固定8件の選択肢と、100万件の業務台帳から所属別に検索する一覧を設計し、どちらをItemsとServerDataに割り当てるか、TotalCount、認可、ソートの考え方を説明してください。

**詳しい解答：** 固定8件の上限が既知ならItemsで充分です。100万件の一覧ではServerDataを選び、入力条件とページ情報をサービスへ渡します。サービスは既存台帳を正本に所属と操作権限を都度判定し、同じ権限・検索条件のWhereからCountAsyncと行取得を行います。ページ指定の前に一意なIdを最後の並び順へ加えます。DBエラー、権限失効や監査の失敗は安全に拒否し、成功表示に置き換えません。実AD/IIS/Kerberos/FDW、1万人超負荷の試験はこの学習用CIの範囲外です。
