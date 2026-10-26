# ステップ26：MudDataGridの検索・フィルター・ページング

## できるようになること
MudDataGridのItems、PropertyColumn、QuickFilter、列フィルター、複数列ソート、PagerContentを使い分けます。

## 前提
Step25でMudTableを操作済み。今回はDB/AD不要で、Webプロジェクト内の6件の架空データを使用します。

## 実際に操作する
1. 「/learn/26」を開き6件の課題を確認します。
2. 「課題名を検索」に「Mud」と入力すると2件に絞られます。
3. 検索を消し、列の見出しで並べ替えます。
4. ページャーで1ページ3件ずつ切り替えます。
5. 列フィルターの操作とQuickFilterの検索が独立していることを確認します。

## 重要なコード
\`\`\`razor
<MudDataGrid T="LearningItem" Items="@_rows"
             QuickFilter="@QuickFilter"
             Filterable="true"
             FilterMode="DataGridFilterMode.ColumnFilterMenu"
             RowsPerPage="3">
    <Columns>
        <PropertyColumn Property="x => x.Title" Title="課題名" />
    </Columns>
    <PagerContent><MudDataGridPager T="LearningItem" /></PagerContent>
</MudDataGrid>
\`\`\`
\`Items\`はすでにメモリにあるデータを渡すためのものです。画面側の検索・ページングを「PostgreSQLのサーバー側検索」と呼ばないでください。

## よくある間違いと比較
- 大量のDB行を\`ToListAsync\`で全件取得してItemsに渡すのは危険です。
- 描画仮想化だけでは取得済みデータ量は減りません。
- 並べ替え・絞り込みの条件をDBへ送る\`ServerData\`はStep43で扱います。
- 日本語のページャー共通文言はMudBlazor.Translationsの登録済み翻訳を利用します。

## 演習
分類に「研修」を追加し、QuickFilterに分類名も検索できるオプションを付けてください。

## 完了条件
- [ ] 6件表示・2件検索・ページャー操作を確認
- [ ] Itemsが小規模一覧に適する理由を説明できる
- [ ] ServerDataとの違いを説明できる
