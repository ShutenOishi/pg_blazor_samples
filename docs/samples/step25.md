# 学習順35：MudTableで一覧とソート

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step25/MudTablePage.razor)。画面の正規URLは `/learn/035` です。

## 学ぶ前に
この教材はWinForms＋Dapperを使ってきた開発者が、WebのUIとサーバー側C#の境界を理解するためのものです。学習用の架空データで試し、認証・認可や実業務の保存が成立したとは見なしません。対象画面は /learn/035 です。

## WinForms＋Dapperとの比較
| 観点 | デスクトップでの経験 | MudBlazorでの対応 |
| --- | --- | --- |
| 小規模一覧 | DataGridView.DataSource | MudTable Items |
| 並べ替え | DataGridView.Sort | MudTableSortLabel SortBy |
| ページング | UI表示の切り替え | RowsPerPageとMudTablePager |
| 大量データ | Dapperで必要行だけ取得 | 後続のServerDataを選択 |

## 不具合時の切り分け
| 症状 | 確認する場所 |
| --- | --- |
| 行が表示されない | Itemsの件数とバインド |
| ソートが効かない | SortBy式とLearningRowの型 |
| ページャーがない | PagerContentとMudTablePager |
| 大量データで遅い | Items全件取得とServerDataを比較 |

## 一覧の取得件数と画面の表示件数
MudTableのItemsへ渡すのは架空の4行です。RowsPerPage=2は表示する行数を2にするだけで、PostgreSQLから検索した結果が2行になるという意味ではありません。WinFormsのDataGridViewへDapperで全件取得したDataTableを渡してから画面をスクロールする方式と似ています。DB側の検索件数を本当に制限するには、後続のServerData方式で条件・並べ替え・ページングをDBへ渡す設計が必要です。

```csharp
private sealed record LearningRow(string Title, int Level, bool Done);
private readonly List<LearningRow> _items =
[
    new("Razorの基本", 1, true), new("DIの復習", 2, true),
    new("日本語フォーム", 3, false), new("一覧表の操作", 4, false)
];
```

```razor
<MudTable T="LearningRow" Items="@_items" RowsPerPage="2">
    <HeaderContent><MudTh><MudTableSortLabel T="LearningRow" SortBy="@_sortTitle">課題名</MudTableSortLabel></MudTh></HeaderContent>
    <RowTemplate><MudTd>@context.Title</MudTd></RowTemplate>
    <PagerContent><MudTablePager /></PagerContent>
</MudTable>
```

SortByは並べ替えに利用するプロパティを指定します。実DBのソート列をユーザー入力のSQL断片から直接作ってはいけません。サービス側で許可列を明示して組み立てます。ユーザーごとの閲覧権限は表示済み行を隠すだけでは不十分で、取得する段階で認可を強制します。

**演習と詳しい解答：** 初期表示の2行と次ページの2行を比較し、データが4行のままであることを説明します。列見出しの課題名と難易度をクリックするとSortByの式で表示順が変わります。10,001行をそのままItemsへ投入するのは検索コストとサーバーメモリを増大させるため不適切です。後続でServerData、DB側絞り込みとページ取得を学びます。

## 実装に対応した操作・コード・詳細解答

## このステップでできるようになること
WinFormsのDataGridViewに相当する一覧表示を、MudBlazorの`MudTable`で作ります。今回は**4件の固定データ**をローカルなリストから表示し、ソートとページャーを試します。DBから必要なページだけを読み込む`ServerData`は後続のデータ連携編で学びます。

## 前提と操作
学習順031〜24のMudBlazorコンポーネントを使った画面に慣れたら`/learn/035`を開きます。DB・ADは不要です。

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

後続の学習順062では`MudTable ServerData`、学習順063では`MudDataGrid ServerData`、学習順068〜49では仮想化を学びます。**ページャーが見えることは、DBでページングしていることの証明ではありません**。件数と使い方に応じて方式を選びます。

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


## 学習順35の追加確認
- [ ] 画面で操作を実際に行い、状態の変化を確認した。
- [ ] 比較表と切り分け表に沿って、WinFormsとの相違点を説明できる。
- [ ] DapperによるDB検索と画面表示の処理を区別できる。
- [ ] 教材の演習に回答し、失敗時の復旧手順を説明できる。
