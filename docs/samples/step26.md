# 学習順36：MudDataGridで検索・列フィルター・ページングする

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step26/DataGridPage.razor)。画面の正規URLは `/learn/036` です。

**実習URL：`/learn/036`。** 第4段階・第8組の1本目です。ここで扱う架空データ・模擬処理は業務DBに保存されません。

## できるようになること
WinFormsのDataGridViewで検索・ソートしていた処理を、MudBlazorの`MudDataGrid`で再現します。学習順035の`MudTable`から進み、`PropertyColumn`・`QuickFilter`・列フィルター・複数列ソートを組み合わせます。**ここで扱う6件は既にサーバーのメモリにある固定データ**です。

## 前提と操作
学習順035の`Items`の意味が前提です。PostgreSQLとADは不要です。`/learn/036`を開いてください。

1. 「簡易検索の一致件数：6件」と表示される。
2. 「課題名を検索」に「Mud」を入力すると、`MudTable`と`MudDataGrid`の2件になる。
3. 入力を消し、課題名・分類・状態の列見出しでソートを試す。
4. 列フィルターメニューを開き、上部の簡易検索と別の条件を指定する。
5. ページャーで1ページ3件ずつ表示を切り替える。

## データと検索処理

```csharp
private sealed record LearningItem(
    int Id, string Title, string Category, string Status);

private readonly List<LearningItem> _rows =
[
    new(1, "Razor入門", "基礎", "完了"),
    new(2, "DIの利用", "基礎", "完了"),
    new(3, "日本語フォーム", "画面", "未完了"),
    new(4, "MudTable", "画面", "完了"),
    new(5, "MudDataGrid", "画面", "未完了"),
    new(6, "PostgreSQLの予習", "DB", "未完了")
];

private string _search = "";

private bool Matches(LearningItem item)
    => string.IsNullOrWhiteSpace(_search)
       || item.Title.Contains(_search.Trim(),
              StringComparison.OrdinalIgnoreCase);

private Func<LearningItem, bool> QuickFilter => Matches;
```

このコードは`DataGridPage.razor`の`@code`内の実装に対応します。`_search`が空なら全件一致、入力があれば課題名へ大文字小文字を区別しない部分一致検索を行います。`QuickFilter`の判定はサーバー側のC#で実行され、SQLの`WHERE`ではありません。

## DataGridの列とページャー

```razor
<MudTextField T="string" Label="課題名を検索"
              @bind-Value="_search" Immediate="true" />
<p>簡易検索の一致件数：@_rows.Count(Matches) 件</p>
<MudDataGrid T="LearningItem" Items="@_rows"
             QuickFilter="@QuickFilter"
             Filterable="true"
             FilterMode="DataGridFilterMode.ColumnFilterMenu"
             SortMode="SortMode.Multiple"
             RowsPerPage="3">
    <Columns>
        <PropertyColumn Property="x => x.Id" Title="番号" Filterable="false" />
        <PropertyColumn Property="x => x.Title" Title="課題名" />
        <PropertyColumn Property="x => x.Category" Title="分類" />
        <PropertyColumn Property="x => x.Status" Title="状態" />
    </Columns>
    <PagerContent>
        <MudDataGridPager T="LearningItem" />
    </PagerContent>
</MudDataGrid>
```

実装画面には検索アイコン、`Sortable`、見た目の設定もあります。`PropertyColumn`は表示列とその値の参照方法を定義します。列フィルターは`Filterable="true"`、複数列ソートは`SortMode.Multiple`です。

## 検索してから画面が更新されるまで
1. ブラウザーで検索文字列を入力する。
2. `Immediate="true"`の入力コンポーネントが`_search`を更新する。
3. `QuickFilter`が`Matches`を使って`_rows`の各行を判定する。
4. DataGridが条件とソートに合う行を表示する。
5. ページャーが表示上の件数を3件ずつ区切る。

**`_rows.Count(Matches)`は上部の簡易検索だけの件数**です。列フィルターも組み合わせた最終表示件数と常に同じとは限りません。画面に出ている数字が何の件数かを区別してください。

## 使い分け
`Items`は小規模で上限が明確な一覧向けです。DBに100万件ある場合、全件を`ToListAsync`して`Items`に渡してから3件ずつ表示しても、DB取得量は減りません。実DBの`WHERE`・`ORDER BY`・`Skip/Take`・`Count`をサーバー側で実行する`ServerData`を後続のDBと一覧設計の学習順で学びます。描画の仮想化だけでもDBの取得量は減りません。


## WinForms＋Dapperから考える一覧の処理単位

WinFormsのDataGridViewへDataTableを渡す場合、Dapperで取得した全件がアプリのメモリに入っているか、SQL側で絞った結果だけが入っているかを調べます。MudDataGridも同じように**取得した集合と表示中の集合を分けて考える**必要があります。ただしBlazor Interactive ServerではコンポーネントのC#状態はサーバー側の対話回線に置かれます。利用者が増えるほど、大量の行を各回線に保持する設計はメモリを消費します。

| 観点 | WinForms＋Dapper | このMudDataGrid | 後続のDB連携 |
| --- | --- | --- | --- |
| 取得元 | DapperのSQL結果 | 画面で宣言した固定6件 | 認可済みSQL検索 |
| 検索 | SQL WHEREまたはDataView | QuickFilterによるC#判定 | WHEREをDBで実行 |
| 複数列ソート | DataView.Sortなど | SortMode.Multiple | ORDER BYと安定したキー |
| ページング | 取得方法によって異なる | RowsPerPageは画面の区切り | Skip/Takeと総件数 |
| 利用者分離 | フォームインスタンス | circuitのコンポーネント状態 | サーバーで都度権限判定 |

### 条件の意味を追いかける
`_rows.Count(Matches)`は6件それぞれへ`Matches`を一度ずつ適用して算出します。`QuickFilter`にも同じメソッドを渡しますが、列メニューのフィルター・ソート・ページングはMudDataGrid自身が適用するため、上の一致件数には列メニューの条件を含みません。検索欄へ「Mud」と入力して2件になったあと、列「状態」を「完了」に絞ると表示中の行はMudTableだけになっても、上部は2件のままという状況が起こります。これは不具合ではなく**数値の定義が違う**ためです。将来の実務画面では「簡易検索に一致する件数」なのか「全フィルター適用後の総件数」なのか、ラベルを一致させます。

現在の列フィルターに設定した条件はDataGridコンポーネント内の状態です。検索欄を消しても列条件が残っていれば6行すべてが画面へ戻るとは限りません。切り分け時は検索語と列フィルターを両方確認し、ページャーを先頭へ戻して実験します。

## 操作が失敗したときの切り分け

| 症状 | 先に確認する場所 | 確認できる原因と対応 |
| --- | --- | --- |
| 「Mud」の検索結果が0件 | 検索欄の文字と全角・半角 | 半角Mudに戻して部分一致を確認 |
| 上部2件なのに表示が1件 | 列フィルターの状態 | QuickFilter以外の条件を解除する |
| ソート順が予想と異なる | 列見出し・複数列ソート | 既存のソート条件を解除して操作する |
| 次ページに期待した行がない | RowsPerPageと絞り込み | 3件単位で残りの行があるか確認 |
| 実DBへ検索が行かない | ItemsとServerData | この画面はDBに接続しない設計 |

### 比較演習の詳しい解答
**演習3：** 6件を1ページ3件にしてもDBへの問い合わせ数が減らない理由を説明してください。

**解答：** 画面の`_rows`は最初から6件全部を保持しています。`RowsPerPage="3"`はその後の表示を3件ごとに分割するだけです。Dapperで取得した100万件をDataGridViewで3件ずつ見せても初回のSQL取得量は100万件のままであることと同じです。DBに接続する版では権限付きWHERE、ORDER BY、Skip/Take、Countを取得操作単位で実行します。

**演習4：** なぜ検索欄の一致件数と列フィルター後の件数を同一視してはいけませんか。

**解答：** 上部のCountは`Matches`だけを評価し、DataGridの列フィルター内部状態を参照しないためです。意図が異なる数値ならラベルで違いを説明し、共通の総件数を必要とする実業務は検索条件をサービスに集約します。

## 演習と解答
**問1：** 「Mud」を入力したとき、どの2行が一致しますか。

**答え：** `MudTable`と`MudDataGrid`です。

**問2：** 検索後に列フィルターも指定した場合、上部の「簡易検索の一致件数」は最終表示件数を表しますか。

**答え：** 必ずしも表しません。`_rows.Count(Matches)`は列フィルターの条件を計算していません。

## 完了確認
- [ ] 検索・列フィルター・ソート・ページャーの違いを説明できる。
- [ ] `Items`と`ServerData`のデータ取得範囲の違いが分かる。
- [ ] PostgreSQLへ接続していないことを説明できる。

次は[学習順037：確認ダイアログ](step27.md)へ進みます。
