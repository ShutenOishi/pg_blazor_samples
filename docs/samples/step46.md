# ステップ46：DTO射影と関連テーブル

## できるようになること

EF Coreのナビゲーションを利用したJOINとDTO射影を理解する。Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor 9、EF Core 10を使用します。

## 前提と実行環境

Step41〜45のServerDataとEF Coreの基礎を終えてから進みます。この画面のDB結果を確認するにはPostgreSQL 18の開発用DBを設定します。

## 画面で操作する
1. 開発用PostgreSQL 18と学習用DDL・架空データを用意します。
2. ステップ46の画面を開き、「関連表を読み込む」を選びます。
3. 課題名と分類の組合せを確認します。分類のない行は「未分類」と表示されます。

## ソースの読み方
LearningDbContextはlearning_categoryを学習用モデルとして定義し、LearningTask.CategoryIdを外部キーにします。LearningAdvancedPageService.LoadCategoriesAsyncはDB側で20件に絞ります。

~~~csharp
var rows = await db.Tasks.AsNoTracking()
    .OrderBy(t => t.Id).Take(20)
    .Select(t => new CategoryTaskRow(t.Id, t.Title,
        t.Category == null ? "未分類" : t.Category.Name))
    .ToListAsync(token);
~~~

このSelectは関連列だけを取り出し、EF CoreがSQLのJOINへ変換します。各行に対して分類を別問い合わせするN+1とは異なります。DTOは表示専用でありEntityを画面へ渡しません。

## 実務で判断する
| 方針 | 選ぶ条件 | 注意 |
| --- | --- | --- |
| SelectでDTO | 一覧の必要列が決まっている | 射影式がSQLへ翻訳可能か |
| Include | 後で関連Entityを操作する必要がある | 不要列や多重JOINの負荷 |
| 明示的な詳細取得 | 詳細を開いた時だけ読みたい | 一覧で行単位のDB問合せを発生させない |

## 確認問題と答え
**Q. 関連データが必要なら必ずIncludeする？** いいえ。表示専用一覧ならSelectのDTO射影を第一候補にします。

## 完了条件
- [ ] 分類あり・なしの日本語表示を確認した
- [ ] SQLに変換されるJOINとN+1の違いを説明できる


## 次に学ぶ内容

Step47で新しい一覧設計を学びます。
