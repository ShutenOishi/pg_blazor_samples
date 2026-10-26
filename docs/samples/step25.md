# ステップ25：MudTableで一覧と並べ替えを作る

## このステップでできるようになること
WinFormsのDataGridViewに相当する一覧表示を、MudBlazorの`MudTable`で作ります。今回は**4件の固定データ**をローカルなリストから表示し、ソートとページャーを試します。DBから必要なページだけを読み込む`ServerData`は後続のデータ連携編で学びます。

## 前提と操作
Step21〜24のMudBlazorコンポーネントを使った画面に慣れたら`/learn/25`を開きます。DB・ADは不要です。

1. 「練習用の課題：4件」と表示されることを確認する。
2. 表には「課題名」「難易度」「状態」が並ぶ。
3. 「課題名」の見出しを押してソート、もう一度押して順序の変化を確認する。
4. 「難易度」でも並べ替え、ページャーを使って1ページ2件の別の行を見る。

## 使うデータと型

```csharp
private sealed record LearningRow(string Title, int Level, bool Done);

private readonly List<LearningRow> _items =
[
    new("Razorの基本", 1, true),
    new("DIの復習", 2, true),
    new("日本語フォーム", 3, false),
    new("一覧表の操作", 4, false)
];

private readonly Func<LearningRow, object> _sortTitle =
    item => item.Title;
private readonly Func<LearningRow, object> _sortLevel =
    item => item.Level;
```

`LearningRow`の`Done`は真偽値です。画面では`true`を「完了」、`false`を「未完了」と表示します。

## 一覧コンポーネントの構成

```razor
<MudTable T="LearningRow" Items="@_items" RowsPerPage="2">
  <HeaderContent>
    <MudTh>
      <MudTableSortLabel T="LearningRow" SortBy="@_sortTitle">
        課題名
      </MudTableSortLabel>
    </MudTh>
    <MudTh>
      <MudTableSortLabel T="LearningRow" SortBy="@_sortLevel">
        難易度
      </MudTableSortLabel>
    </MudTh>
    <MudTh>状態</MudTh>
  </HeaderContent>
  <RowTemplate>
    <MudTd DataLabel="課題名">@context.Title</MudTd>
    <MudTd DataLabel="難易度">@context.Level</MudTd>
    <MudTd DataLabel="状態">@(context.Done ? "完了" : "未完了")</MudTd>
  </RowTemplate>
  <PagerContent>
    <MudTablePager />
  </PagerContent>
</MudTable>
```

実装では`Hover`・`Striped`・`MudPaper`などの装飾も設定しています。`T="LearningRow"`は行の型です。`HeaderContent`は見出し、`RowTemplate`は1行分、`PagerContent`はページャーを配置する場所です。

## 画面操作から表示まで
1. Razorがサーバー上の`_items`の4件を`Items`に渡す。
2. `MudTable`が`RowsPerPage=2`に従って表示する行を決める。
3. 見出しを押すと`SortBy`の式で並べ替える。
4. ページャーを操作すると別の2件が表示される。

ここで**画面のページを切り替えてもDBへの検索は発生しません**。`Items`として既に渡した4件を使う例です。

## ItemsとServerDataを使い分ける
`Items`は少量の取得済みデータを扱うときに分かりやすい方式です。ただし大量のレコードを全件取得して`Items`へ渡すと、DB通信量・サーバーメモリ・応答性に問題が出ます。

後続のStep42では`MudTable ServerData`、Step43では`MudDataGrid ServerData`、Step48〜49では仮想化を学びます。**ページャーが見えることは、DBでページングしていることの証明ではありません**。件数と使い方に応じて方式を選びます。

## 日本語化の分担
業務固有の列名「課題名」「難易度」「状態」や行データはこの教材が日本語で定義しています。ページャーなどMudBlazorが備える共通表示には`MudBlazor.Translations`の翻訳リソースを使用します。

## よくある問題
- `T`と`SortBy`の対象型を一致させる。
- 状態表示の式は`Done`の真偽値から日本語へ変換する。
- `RowsPerPage`は表に表示する件数です。DBアクセスを制限する機能ではありません。
- ソート条件や検索条件をそのままSQL文字列へ連結しない。実DB編で安全なソートの組み立てを扱います。

## 演習と解答
**問：** `RowsPerPage="2"`なのに`_items`には4件入っています。2件しかサーバーへ届いていないという意味ですか。

**答え：** 違います。既に4件を`Items`へ渡し、そのうち1ページ2件を表示しています。サーバー側DBでの`Skip/Take`とは異なります。

## 完了確認
- [ ] ソートとページャーの操作を確認した。
- [ ] `Items`が全件取得済みであることを説明できる。
- [ ] `MudTable`と将来学ぶ`ServerData`の違いを説明できる。

[コラム06：MudBlazorの使い方](../columns/column21-25.md)で5ステップをまとめます。
