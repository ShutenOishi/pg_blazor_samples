# ステップ06：条件分岐と繰り返し表示

**学習テーマ：** `@if` / `@else`、`@foreach`、`@key`、条件付き一覧

**実装ソース：** [ConditionsPage.razor](../../src/PgBlazorSamples.Web/Components/Samples/Step06/ConditionsPage.razor)

## 前提

Step01〜05まで。アプリ内の架空データだけで動作するため、DB・ADは不要です。

## 動作手順

1. `/learn/06`を開きます。
2. 「すべての課題を表示しています。」の案内と4件の課題を確認します。
3. 「未完了の課題だけ表示する」にチェックを入れます。
4. 日本語案内が「未完了の課題に絞っています。」になり、2件の未完了課題だけが残ります。
5. チェックを外し、4件に戻ることを確認します。

## 実コードから学ぶ

```razor
@if (showOnlyIncomplete)
{
    <p class="hint" data-testid="filter-state" role="status">未完了の課題に絞っています。</p>
}
else
{
    <p class="hint" data-testid="filter-state" role="status">すべての課題を表示しています。</p>
}
```

```razor
@foreach (var task in VisibleTasks)
{
    <li @key="task.Id">
        <span>@task.Title</span>
        <span class="task-status">@(task.IsCompleted ? "完了" : "未完了")</span>
    </li>
}
```

`tasks`はソース内で定義した4件の架空の課題です。`VisibleTasks`は、チェックの有無に応じて未完了だけを抽出したり、全件を返したりします。

- `@if`は条件が真なら一方、偽なら`else`の内容を描画します。
- `@foreach`は配列の内容を順に出力します。
- `@key`には一意のIDを指定し、項目が切り替わった場合の描画を安定させます。
- `@bind`したチェックボックスによってフィルター条件を変更します。
- このサンプルはファイル内で`record`を定義し、複数プロジェクトへの分割はまだしません。

## 演習

- 架空の課題を1件追加し、合計件数が変わることを確認してください。
- 新たな課題を「完了」にすると、フィルター後の件数がどうなるか確認してください。
- 一覧が空の場合に日本語の「表示する課題はありません。」を出す条件を考えてください。

## 完了の判定

- [ ] 4件から2件に絞り込み、4件に戻せる
- [ ] `@if`、`@foreach`、`@key`の意味を説明できる
- [ ] 表示用配列と画面の関係を説明できる

前：[ステップ05](step05.md)

次は[Step07：親子コンポーネントと引数](step07.md)を学びます。
