# ステップ25：MudTableで一覧と並べ替えを作る

## できるようになること
MudTableで一覧を表示し、並べ替えとページャーを操作できます。

## 実際に操作する
1. /learn/25 を開き、課題が4件あることを確認します。
2. 「課題名」または「難易度」の見出しを押して並べ替えます。
3. ページャーを操作し、1ページ2件で次のページへ進みます。

## ソースとの対応
```razor
<MudTable T="LearningRow" Items="@_items" RowsPerPage="2">
    <HeaderContent>
        <MudTh><MudTableSortLabel T="LearningRow" SortBy="@_sortTitle">課題名</MudTableSortLabel></MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="課題名">@context.Title</MudTd>
    </RowTemplate>
    <PagerContent><MudTablePager /></PagerContent>
</MudTable>
```

MudTableSortLabelのSortByへ比較対象を返す関数を渡します。共通のページャー文言はMudBlazor.Translationsを利用し、行の業務データは架空の日本語データです。ここではDBへは接続しません。

## チェック
- [ ] 4件の課題を確認した
- [ ] ソートを切り替えた
- [ ] ページャーで別の行を表示した

次：学習コラム06
