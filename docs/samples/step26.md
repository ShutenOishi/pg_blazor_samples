# ステップ26：MudDataGridで検索・列フィルター・ページングする

## できるようになること
WinFormsのDataGridViewで検索・ソートしていた処理を、MudBlazorの`MudDataGrid`で再現します。Step25の`MudTable`から進み、`PropertyColumn`・`QuickFilter`・列フィルター・複数列ソートを組み合わせます。**ここで扱う6件は既にサーバーのメモリにある固定データ**です。

## 前提と操作
Step25の`Items`の意味が前提です。PostgreSQLとADは不要です。`/learn/26`を開いてください。

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
`Items`は小規模で上限が明確な一覧向けです。DBに100万件ある場合、全件を`ToListAsync`して`Items`に渡してから3件ずつ表示しても、DB取得量は減りません。実DBの`WHERE`・`ORDER BY`・`Skip/Take`・`Count`をサーバー側で実行する`ServerData`をStep41〜60で学びます。描画の仮想化だけでもDBの取得量は減りません。

## 演習と解答
**問1：** 「Mud」を入力したとき、どの2行が一致しますか。

**答え：** `MudTable`と`MudDataGrid`です。

**問2：** 検索後に列フィルターも指定した場合、上部の「簡易検索の一致件数」は最終表示件数を表しますか。

**答え：** 必ずしも表しません。`_rows.Count(Matches)`は列フィルターの条件を計算していません。

## 完了確認
- [ ] 検索・列フィルター・ソート・ページャーの違いを説明できる。
- [ ] `Items`と`ServerData`のデータ取得範囲の違いが分かる。
- [ ] PostgreSQLへ接続していないことを説明できる。

次は[ステップ27：確認ダイアログ](step27.md)へ進みます。
