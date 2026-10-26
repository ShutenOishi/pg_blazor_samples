# MudBlazor一覧・DataGrid・EF Core 10連携：実務パターンの選択ガイド

> **学習計画（未実装）：** Step26はDBなしのDataGrid基本操作、Step31〜40はEF Core・PostgreSQLの基礎、Step41〜50は実際のDB接続を使った一覧・検索・仮想化を実装しています。Step51〜60の編集・集約は今後の計画です。本ガイドは正しい選択方法と、実装時に検証すべき契約を定義します。

## 目標：画面要件から最適な方式を自分で選べる

「MudTableとMudDataGridのどちらか」だけではありません。**データ量（表示対象・検索対象）・操作（参照/編集）・一覧のUX・更新頻度・権限・運用負荷**を考えて、クライアント全件取得、サーバーページング、仮想化、カーソルページング、集約・検索専用画面を選びます。行数だけでは決めません。

## 1. パターン比較・選択表

| パターン | 向く状況 | MudBlazor | DB取得方法 | 注意点 |
| --- | --- | --- | --- | --- |
| A 固定・小規模一覧 | 件数に明確な上限がある選択肢/マスタ | `MudTable Items` / `MudDataGrid Items` | 一度取得してDTO化 | 全件取得の上限を定義。ブラウザー内フィルターの意味を説明 |
| B 通常の業務検索一覧 | 検索・ページ指定・総件数が必要 | `MudDataGrid ServerData` | DB側`Where/OrderBy/Skip/Take` | 絞り込み条件に一致する総件数・安定ソートが必要 |
| C 単純なページ付き表 | Gridの高度な列機能が不要 | `MudTable ServerData` | Bと同様 | Gridより概念が少ない。機能要件で選ぶ |
| D スクロール中心の大量一覧 | 明確なページ移動不要・連続閲覧 | `MudDataGrid VirtualizeServerData` | 表示範囲をDBから段階取得 | `Height`設定、スクロール更新、リクエスト中断、総件数 |
| E 既に読み込んだ多数の行 | データはメモリにあり描画だけ高コスト | `MudDataGrid Virtualize` | 既存`Items` | 仮想化はDB取得量を削減しない。高さ・行高の安定性に注意 |
| F 次へ・前への連続探索 | 超深いページ・取得順が安定 | キーセット用の独自ページナビ/表 | `WHERE (sort,id) > cursor`相当 | 任意のページ番号へ直接ジャンプ不可。Bの`ServerData`をそのまま置換しない |
| G 編集・承認一覧 | 複数人が同じ行を変更し得る | Grid表示＋Dialog/Form/編集用DTO | 再取得→認可→競合確認→保存 | 編集中のDbContextを保持しない。Conflict UXが必要 |
| H 集約・ダッシュボード | グループ別集計/統計/件数 | Table/Grid/Chart | DBで`GroupBy/Select`、集計DTO | 明細を全件取得してメモリで集計しない |
| I 大量エクスポート | 画面の1ページでなく検索結果全体を出力 | 実行ボタン/進捗 | 別の権限付きサーバー処理で逐次出力 | 表示中の`Items`をそのまま全件出力と誤認しない |
| J 階層・詳細展開 | 親子情報や1:N詳細がある | Expand/Detail/別ページ | 親ページを先に取得、必要時に子を別取得 | N+1と巨大JOIN、権限漏れを防ぐ |

**標準の第一候補はB（MudDataGrid ServerData）**。小規模のAやCのほうが明快な場合は無理にGridを使いません。D/F/H/Iは要件があるときに選びます。

## 2. 判断フローチャート（文章版）

1. データの最大件数が小さく保証されるか？ → はい：Aを候補にする。**最大件数が不明ならAにしない**。
2. DBに問い合わせる必要があるか？ → はい：B/C/D/F/H/Iを選び、クエリをサービス層へ集約。
3. ソート/フィルター/列設定/行選択が必要か？ → はい：通常はB。単純な表でよいならC。
4. ページ番号への移動が重要か？ → はい：B（オフセット）。連続スクロール主体ならD。
5. ページが数十万行先まで進みオフセットが重いか？ → はい：Fを評価。任意ジャンプとの要件差を説明。
6. 同時編集、集計、エクスポートがあるか？ → それぞれG/H/Iを**B等の上に組み合わせる**。
7. 運用計測でP95/P99、クエリプラン、DB負荷、転送サイズが許容内か？ → 超過なら索引・射影・検索UX・方式を見直す。

**行数に固定的な「1,000行を超えたら仮想化」等の万能閾値は設けません。** 列数・行テンプレート・通信・変更頻度・端末負荷が異なるため、検証データで判断します。

## 3. MudDataGridのデータ取得API

| 機能 | パラメーター | 処理 |
| --- | --- | --- |
| 全件を渡す | `Items` | 既取得データの表示。小規模向け |
| ページごとに取得 | `ServerData` | `Func<GridState<T>, CancellationToken, Task<GridData<T>>>` |
| スクロール範囲ごとに取得 | `VirtualizeServerData` | `Func<GridStateVirtualize<T>, CancellationToken, Task<GridData<T>>>` |
| 描画を仮想化 | `Virtualize=true` | `Height`指定が必要。サーバー取得とは別 |

MudDataGridで`ServerData`を指定したら`Items`を同時指定しません。画面側で任意のLINQを`Items`へ適用することを「サーバー検索」と呼ばないでください。VirtualizeServerDataの引数はGridStateとは**別型**です。利用中のMudBlazor 9系のAPIを実装時に再確認します。

### 標準の画面例（説明用・未実装）

```razor
<MudDataGrid T="EmployeeRow"
             ServerData="LoadAsync"
             RowsPerPage="20"
             Loading="@_loading">
    <Columns>
        <PropertyColumn Property="x => x.Id" Title="番号" />
        <PropertyColumn Property="x => x.Name" Title="氏名" />
    </Columns>
    <PagerContent>
        <MudDataGridPager T="EmployeeRow" />
    </PagerContent>
</MudDataGrid>
```

```csharp
// 画面のLoadAsyncはサービスだけを呼ぶ（計画例）。
private async Task<GridData<EmployeeRow>> LoadAsync(
    GridState<EmployeeRow> state, CancellationToken ct)
{
    var page = await EmployeeQuery.SearchAsync(
        new EmployeeSearchRequest(
            Page: state.Page,
            PageSize: state.PageSize),
        ct);

    return new GridData<EmployeeRow>
    {
        Items = page.Items,
        TotalItems = page.TotalCount // 未ページングの全検索一致件数
    };
}
```

ここでは意図的にソート/フィルターを省略しています。Step44でGridStateの**SortDefinitions / FilterDefinitionsを安全な検索条件へ変換**し、フィルターの種類、NULL、空文字、日本語、数値・日付・範囲を網羅します。検索仕様が成立するまでは任意のフィルターUIを公開しません。

### EF Coreサービス内のクエリ例（計画例）

```csharp
await using var db = await factory.CreateDbContextAsync(ct);
IQueryable<Employee> query = db.Employees.AsNoTracking();

if (!string.IsNullOrWhiteSpace(request.Keyword))
    query = query.Where(x => x.Name.Contains(request.Keyword));

var total = await query.CountAsync(ct);
var items = await query
    .OrderBy(x => x.Name)
    .ThenBy(x => x.Id)              // 同名の行も安定した順序に
    .Skip(request.Page * request.PageSize)
    .Take(request.PageSize)
    .Select(x => new EmployeeRow(x.Id, x.Name))
    .ToListAsync(ct);
```

ページ番号/件数はサービス側で**0以上・上限あり**に検証し、乗算のオーバーフローにも注意します。検索条件は`CountAsync`とページ取得で同じものを使います。件数と結果は別クエリなので、更新の多いテーブルで「完全に同一時点のスナップショット」を保証するものではありません。

## 4. ソートとフィルターの契約

- **許可リスト方式**：画面列（例：番号、氏名、登録日）をサービスが理解する固定のソートキーに翻訳する。任意の式・SQL・テーブル名を引数として受け取らない。
- 複数列ソート時は順番と昇順/降順を保持し、最後に必ず主キー等の一意キーを追加する。
- 検索キーワードは空白正規化、最大長、単語分割の仕様を先に決める。`Contains`、`Like`、`ILike`、全文検索（必要時）を混同しない。
- 日付は`DateOnly`、時間帯はUTC、Rangeは排他的上限を意識。時刻帯の文化依存変換を画面へ漏らさない。
- 複雑な`IN`、NULL、空欄、大小比較、複数条件、結合先の列は**条件ごとにSQLとテスト**を用意する。
- 認可による行制限を**CountとItemsの両方より前**に適用し、総件数で他部署データの存在を漏らさない。
- 検索条件/ページサイズ/ソート変更時はページを先頭に戻し、古い応答による上書きを防ぐ。

## 5. 初回ロードとUX

- **初回表示**：勝手に全件検索するか、検索ボタン押下まで0件とするかを業務要件で決める。高負荷なデータなら後者も有効。
- **デバウンス**：入力ごとにDBへ問い合わせる代わりに、短い遅延・明示的検索ボタン・最小文字数を検討する。
- **古いリクエスト**：CancellationTokenをサービス・EF Coreまで伝播し、戻っても古い結果を画面へ適用しない。
- **ローディング**：Spinnerだけでなく対象件数、空状態、ネットワーク/DB失敗、再試行を日本語で提示する。
- **ページサイズ**：利用者に選ばせても、サーバー上限を強制。極端な値を拒否する。
- **選択行**：行オブジェクトを保持するより主キーを保持し、検索条件変更・削除・権限変更で選択を再検証。
- **障害時**：例外詳細・接続文字列を画面へ出さず、画面には安全な日本語案内、ログには相関IDと必要な文脈。

## 6. ページング方式の違い

| 項目 | オフセット（Skip/Take） | キーセット（カーソル） |
| --- | --- | --- |
| 任意のページ番号にジャンプ | 可能 | 基本的に不可 |
| 深いページの性能 | 大きなOFFSETは不利 | 適切な索引があれば有利 |
| 更新中の行の移動/重複 | 発生し得る | 条件付きで緩和 |
| MudDataGridとの統合 | `ServerData`に自然 | 専用ナビとカーソル管理が必要 |
| 必須 | 安定ソート・索引・境界検証 | 一意な順序・カーソルの署名/検証・索引 |

**重要**：`Skip/Take`をキーセットと呼ばない。キーセットで同一条件の全ページを通すには、ソートとフィルターにカーソルの生成時条件を結び付ける。

## 7. 仮想化の誤解

- `Virtualize=true`は**描画する行数の削減**であり、既に`Items`へ渡した全データの取得コストを消しません。
- `VirtualizeServerData`は**可視範囲に応じたサーバー取得**に使用します。`GridStateVirtualize`の開始位置・件数・検索状態を検証して処理します。
- Scroll時は短時間に複数の要求が起こり得ます。取消し・再入・最後の結果の採用を確認します。
- 行の高さを変動させる長文・展開行・複数段折り返しは位置計算に影響し得るため、レイアウトとキーボード操作を評価します。
- スクリーンリーダー、フォーカス保持、再接続・画面破棄、全件選択の意味もテスト対象です。

## 8. 編集・承認・競合（Gridを入力フォームの代わりにしない）

1. 一覧は`AsNoTracking`のDTOを表示し、編集は専用のForm/Dialog（または要件が合えばセル編集）を使う。
2. 編集開始時に対象IDと表示版の`xmin`等を保存。DBのConnection/DbContextを開き続けない。
3. 保存要求時に**再認可**とサーバー側検証を行う。入力エラーは日本語で返す。
4. 別ユーザーによる更新があれば競合を検知し、**再読み込み/編集し直し/中止**を提示する。黙って上書きしない。
5. 保存後は対象行だけ反映/ページ再取得のどちらが適切か判断。ソート順が変わる更新なら再読み込みを優先。
6. 一括承認は部分成功/全部失敗、監査、冪等性の仕様を決めてテストする。

## 9. 集約・CSV・大量更新

- **集約**：`GroupBy`と`Count/Sum`等をDBへ送る。DBの`COUNT(*)`コストを計測し、総件数表示を必須にしない選択も検討。
- **CSV**：Gridに表示されたページではなく、権限をかけた検索条件の結果をサーバーから安全に出力。CSV式インジェクション、エンコーディング、最大件数、タイムアウト、キャンセルをテスト。
- **大量更新**：`ExecuteUpdateAsync`/`ExecuteDeleteAsync`は通常の追跡エンティティ更新とは異なる。更新件数、監査・競合、トランザクションを明示し、バルク処理と通常CRUDを比較する。
- **キャッシュ**：利用者や権限ごとに混線しないキー設計が必要。認可条件が変わった際には結果の再利用を拒否/無効化する。

## 10. 実データを用いた検証・合格条件

| ケース | 合格条件 |
| --- | --- |
| 0件 / 1件 / ページ境界前後 | 空表示、次ページ、総件数が正しい |
| 同名レコードや同日レコードが大量 | 主キーで順序が安定し、重複/取りこぼしがない |
| ページ数が多く深いページへ移動 | OFFSET方式の限界を測定して代替判断できる |
| 文字種・NULL・期間境界 | 日本語・空文字・DateOnly・範囲条件が正しい |
| 検索条件を連続変更 | 古い結果で新しい状態を上書きしない |
| DB停止・キャンセル・タイムアウト | 接続を返却し安全なエラー表示に戻る |
| 行が更新/削除される | `xmin`競合・選択解除・総件数再計算 |
| 他部署の行を同時に保持 | ItemsもTotalItemsも閲覧権限を超えない |
| 表示/出力件数が大きい | DTO射影、実行計画、メモリ、通信、CPU、P95/P99を計測 |
| 読み上げ・キーボード操作 | 日本語表示・焦点移動・仮想化の操作が成立する |
| 長時間のフォーム編集 | DBトランザクション・DbContextを保持しない |
| 多ユーザー同時アクセス | プール上限・DB接続数を守り、二重更新を検出 |

**この章のStep41〜60は計画段階です。** 今後コードを実装したときは、同じPRで`docs/samples/stepNN.md`、日本語のUI、ソースのコメント、DB統合テスト、コラム、PDF目次・本文、学習カタログを同期します。

## 公式参考

- [MudDataGrid API](https://mudblazor.com/api/MudDataGrid)
- [MudTable API](https://mudblazor.com/api/MudTable)
- [Blazorの仮想化](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/virtualization?view=aspnetcore-10.0)
- [EF Coreのページング](https://learn.microsoft.com/ja-jp/ef/core/querying/pagination)
- [BlazorとEF Core](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0)
- [EF Core 10・Npgsql学習ガイド](efcore10-data-access.md)

## Step41〜45の実装に対応する補足

Web/Services/Learning/LearningPageService.csが、一覧のサーバー側検索・ページング・Count・Sort許可リストを実装します。最大ページサイズ50件、複数ソート3列、最後に一意の主キーIdを追加。MudTableとMudDataGridのServerDataはCancellationTokenを受けてこのサービスを呼び出し、IQueryableはコンポーネントへ渡しません。

## Step46〜50の実装済み例

関連テーブルの射影はStep46、IDキーセットはStep47、描画だけの仮想化はStep48、DBへの範囲要求はStep49、検索待機・再試行はStep50に対応します。Step47のカーソルはId専用、Step49はId固定順であり、複数列ソートまで実装したわけではありません。
