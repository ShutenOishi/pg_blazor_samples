# 第14組の復習：関連DTO・キーセット・仮想化・検索UX

**学習順066〜070・第6段階／第14組**

## 五つの技術の目的を分ける
学習順066では関連Categoryから表示に必要な列だけ取得しました。067はIdを使って次の10件を続けて読みます。068はすでにメモリにある500件のDOM描画数を減らし、069はスクロール位置に応じてDBから取得する範囲自体を減らします。070は入力が続く間の不要な検索要求を抑えます。どれも一覧画面ですが、解決する課題は違います。WinForms＋DapperのSQLやDataGridViewで行ってきた工夫が、BlazorとEF Core 10ではどの層にあるか整理します。

| 学習順 | 方式 | 得意な用途 | 誤解しやすい点 |
| --- | --- | --- | --- |
| 066 | CategoryTaskRowのSelect射影 | JOINした分類名の表示 | Include必須でも各行N+1でもない |
| 067 | Idキーセット | 続きから10件 | 任意ページへ直接飛ぶのは苦手 |
| 068 | Items＋Virtualize | 取得済み500件の画面描画 | 500件のメモリは減らない |
| 069 | VirtualizeServerData | DB取得の表示範囲指定 | 深いOFFSETは残る |
| 070 | 400ミリ秒のデバウンス | 連続入力による検索削減 | DB処理の取消や認可とは別 |

## 実コード例1：関連列をDTOへ射影する

```csharp
var rows = await db.Tasks.AsNoTracking().OrderBy(t => t.Id).Take(20)
    .Select(t => new CategoryTaskRow(t.Id, t.Title,
        t.Category == null ? "未分類" : t.Category.Name))
    .ToListAsync(token);
```

分類なしの場合の表示と、先頭20件だけという制約を確認してください。この取得とCountAsyncは別のSQLです。後から分類名を各行ごとに読み直す方法ではN+1となり、行数が多くなるほどSQLの往復が増えます。

## 実コード例2：DB範囲を仮想化へ渡す

```csharp
private async Task<GridData<LearningTaskRow>> LoadAsync(
    GridStateVirtualize<LearningTaskRow> state, CancellationToken token)
{
    var count = Math.Min(state.Count, LearningAdvancedPageService.MaxWindowSize);
    if (count < 1) return new() { Items = [], TotalItems = 0 };
    var result = await Pages.LoadWindowAsync(state.StartIndex, count, token);
    return new() { Items = result.Rows, TotalItems = result.TotalCount };
}
```

この方式では開始位置をSkip、取得数をTakeへ渡します。最大件数は200です。TotalItemsは総件数であり、Itemsは今回の範囲だけです。DB処理中にスクロールが変わった場合の取消も考慮します。

## WinForms＋Dapperとの比較
| 観点 | WinForms＋Dapper | Blazor＋EF Core 10 |
| --- | --- | --- |
| 関連表 | Dapper用のJOIN SQLを書く | Select射影をSQLへ変換 |
| 次のページ | WHERE id > @cursor とLIMIT | Where、OrderBy、Take(size+1) |
| 既存行の仮想化 | DataGridView.VirtualMode等 | Items＋Virtualize |
| DB範囲取得 | スクロール位置からOFFSET/LIMIT発行 | VirtualizeServerDataのStartIndex/Count |
| 検索入力 | Timer等による待機 | Task.Delay(400, token)とServerData |
| 認可 | SQL実行ごとに既存台帳を照合 | 同じくDB側で操作・行単位の都度認可 |

## 画面で操作する順序
1. /learn/46で「関連表を読み込む」を押し、分類名・未分類・表示20件の上限と総件数を確認します。
2. /learn/47で「次の10件」と「先頭から読み直す」を使い、最後のIdとHasMoreを確認します。
3. /learn/48で500件の保持とDB問い合わせ0件を確認してスクロールします。
4. /learn/49でDB未設定・スクロール範囲・TotalItemsとItemsの違いを確認します。
5. /learn/50で連続入力、400ミリ秒待機、100文字超の拒否、再読み込みを確認します。
6. PostgreSQL18の学習用DBがある場合のみ実際の件数と生成SQLを確認し、未設定時の成功表示と混同しません。

## 障害の切り分け
| 状態 | 可能な原因 | 確認と復旧 |
| --- | --- | --- |
| 20件だけ表示される | Step46がTake(20)を使用 | TotalCountとRowsを区別 |
| ページに行が重複する | カーソルや並び順が不一致 | Idの一意順を確認 |
| 仮想化でもメモリが減らない | Itemsの全500件を保持 | DOM描画数と保持数を区別 |
| 深いスクロールが遅い | OFFSETの走査負荷 | 実行計画とキーセット採用を検討 |
| 旧検索結果が表示される | 待機やDBアクセスの取消漏れ | CancellationTokenをサービスまで伝達 |
| DBが未設定 | 開発DBの接続文字列がない | 未設定メッセージ、学習用DDLを確認 |
| 検索0件と障害を区別できない | statusを確認していない | 画面とサーバーログを両方読む |

## 演習1：方式の選択
分類付き先頭20件、SNS風の「続きを読む10件」、取得済み500件をスムーズにスクロールする画面、DB内の大量行を範囲取得する仮想スクロール、連続した検索入力には、どの方式が向くでしょうか。何を減らすのかも答えてください。

### 詳しい解答1
分類付き先頭20件はDTO射影でN+1と不要列の転送を抑えます。続きを読む10件はIdキーセットで深いOFFSETを避けますが、任意ページジャンプには向きません。取得済み500件はItemsとVirtualizeでDOMの描画量を減らしますがデータの保持量は変わりません。DB内の大量行の範囲取得はVirtualizeServerDataを使いますが深いOFFSETには注意します。連続検索は400ミリ秒待機と実DBへの取消トークンの伝達を組み合わせます。

## 演習2：件数を計算する
Id1〜26までの課題を10件ずつキーセットで読み込む場合、それぞれ何件を表示し、HasMoreはどうなりますか。Step49で開始位置120、Count30、総件数250のときItemsとTotalItemsはどうなりますか。

### 詳しい解答2
キーセットは最初の10件と次の10件では11件目が取得できるためHasMore=true、最後の6件には次がないためfalseです。カーソルは10、20、26です。Step49のItemsは30行、TotalItemsは250です。これを250行全部をブラウザーへ転送する処理と混同してはいけません。

## 演習3：安全拒否
AD認証後に権限のない行をブラウザーのフィルターで隠した場合、DB取得の認可は完了していますか。監査記録ができなかったら保存を成功としてよいですか。

### 詳しい解答3
いずれもいいえ。ADは本人確認とユーザー名の取得だけです。既存DB/外部台帳で操作と対象行を都度確認し、Count、Rows、詳細、更新、ファイル出力に反映します。監査失敗や権限失効、xmin競合、DB間の原子性不明は安全に拒否してください。利用者をアプリへ登録する画面は設けません。

## 安全性と実機受入の確認
この五つのステップは学習用の架空データです。検索結果が少なく見えることは業務認可ではありません。サーバーへ未許可行を取得してからUI上で隠す方法は避けます。PostgreSQL 18のSQL結果を確認できても、実AD、Windows Server 2025/IIS、Kerberos、FDW、1万人超の実機負荷試験を済ませたことにはなりません。CountとRowsの両方へ権限条件を掛け、監査を確実に残し、拒否時の画面・ログも検証します。認可条件の変更や停止は今回の学習UI改稿の対象外です。

## 次の学習
学習順071 /learn/51 では編集DTOと保存処理を学びます。閲覧・編集で必要な権限、取消、更新競合、監査が異なることを意識してください。
