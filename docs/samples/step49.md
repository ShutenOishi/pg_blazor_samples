# ステップ49：VirtualizeServerData

## できるようになること

スクロールに応じてPostgreSQL 18から必要な範囲だけ取得する。Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor 9、EF Core 10を使用します。

## 前提と実行環境

Step41〜45のServerDataとEF Coreの基礎を終えてから進みます。この画面のDB結果を確認するにはPostgreSQL 18の開発用DBを設定します。

## 画面で操作する
1. 開発用PostgreSQL 18でステップ49を開きます。
2. 表をスクロールします。画面の総件数と表示範囲を確認します。
3. 実DBなしの場合は未設定が表示され、成功扱いされないことを確認します。

## ソースの読み方
MudDataGridのVirtualizeServerDataは、GridStateVirtualizeとCancellationTokenを受け取ります。

~~~csharp
private async Task<GridData<LearningTaskRow>> LoadAsync(
    GridStateVirtualize<LearningTaskRow> state, CancellationToken token)
{
    var result = await Pages.LoadWindowAsync(state.StartIndex, state.Count, token);
    return new GridData<LearningTaskRow>
    {
        Items = result.Rows, TotalItems = result.TotalCount
    };
}
~~~

教材画面の実装ではCountに上限を設定し、状態メッセージを更新しています。サービス側はCountAsyncとToListAsyncに同じ取消トークンを渡し、Idの一意順序でSkip/Takeします。

## 設計時の注意
VirtualizeServerDataとItems・ServerDataを同時に指定しません。ページャーは使わず、固定高さのスクロール領域を使います。任意列ソートを許可すると、表示範囲の整合を保つ追加実装が必要です。今回はID順で固定します。

## 確認問題と答え
**Q. Virtualize=trueだけでDB取得が範囲指定になる？** いいえ。VirtualizeServerDataのコールバックでDB側のSkip/Takeを行います。

## 完了条件
- [ ] StartIndexとCountを説明できる
- [ ] CountとRowsと取消トークンの関係を説明できる


## 次に学ぶ内容

Step50で新しい一覧設計を学びます。
