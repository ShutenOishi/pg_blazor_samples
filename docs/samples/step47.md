# ステップ47：キーセットページング

## できるようになること

OFFSETとキーセットの得失を理解し、IDカーソルで連続取得する。Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor 9、EF Core 10を使用します。

## 前提と実行環境

Step41〜45のServerDataとEF Coreの基礎を終えてから進みます。この画面のDB結果を確認するにはPostgreSQL 18の開発用DBを設定します。

## 画面で操作する
1. 開発用PostgreSQL 18へ接続しステップ47を開きます。
2. 「次の10件」で最後の番号の次から取得することを確認します。
3. 「先頭から読み直す」でカーソルが初期化されることを確認します。

## ソースの読み方
LearningAdvancedPageService.LoadAfterIdAsyncは一意なIdの昇順に固定します。

~~~csharp
if (afterId is { } cursor)
    query = query.Where(t => t.Id > cursor);
var fetched = await query.OrderBy(t => t.Id)
    .Take(size + 1).ToListAsync(token);
~~~

実際の実装はToListAsyncの前にDTOへSelectしています。11件目があれば続きありと判定し、表示は10件にします。全件Countを取らないため、ページ番号や総件数を必須としない連続探索に向きます。

## OFFSETとの比較
| 方式 | 向く操作 | 注意 |
| --- | --- | --- |
| Skip/Take | 5ページ目へ移動、総件数表示 | 深いページほど並び順・走査負荷を意識 |
| IDキーセット | 前から次へ順に閲覧 | 任意ページへジャンプしにくい |
| 複合キーセット | 日付・状態等の複数ソート | NULL、昇降順、同値時のIDもカーソルへ含める |

データ追加・削除中の一貫性は業務要件で判断します。今回の教材はIdの単一キーに限定します。

## 確認問題と答え
**Q. タイトル順でもIdだけをカーソルにしてよい？** いいえ。全ての並び替えキーが必要です。

## 完了条件
- [ ] 続きを押して重複のない結果を確認した
- [ ] OFFSETとキーセットの選択理由を説明できる


## 次に学ぶ内容

Step48で新しい一覧設計を学びます。
