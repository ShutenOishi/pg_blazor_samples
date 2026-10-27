# 学習順40（旧Step30）：検索・入力・確認・通知の総合演習

**実習URL：`/learn/30`。** 第4段階・第8組の5本目です。ここで扱う架空データ・模擬処理は業務DBに保存されません。

## このステップでできるようになること
Step26〜29のMudDataGrid、入力欄、確認ダイアログ、Snackbar、状態表示を組み合わせます。WinFormsの簡単な一覧管理画面をWebに移したような操作を練習しますが、**この画面の利用者はすべて架空で、AD認証・既存台帳への登録・PostgreSQL保存はありません**。実際の業務利用者管理を実装する例ではありません。

## 前提と画面操作
Step26〜29を学んでから`/learn/30`を開きます。DB・ADは不要です。

1. 最初は架空の3名「架空 太郎」「練習 花子」「見本 一郎」が表示される。
2. 「利用者名を検索」に「練習」を入力すると1件に絞られる。
3. 名前を空で「確認して追加する」を押すと「利用者名は1〜30文字で入力してください。」が表示される。
4. 「テスト 四郎」を入力して確認ダイアログを開き、「中止」すると行は追加されない。
5. もう一度試して「追加する」を選ぶと、部署「研修」の架空行が増え、本文に結果が表示されSnackbarに「一覧を更新しました。」と出る。
6. 検索を解除すれば追加した行も確認できる。

## 固定データと検索のコード

```csharp
private sealed record PracticeUser(int Id, string Name, string Department);
private readonly List<PracticeUser> _users =
[
    new(1, "架空 太郎", "総務"),
    new(2, "練習 花子", "企画"),
    new(3, "見本 一郎", "情報")
];
private string _keyword = "";
private string _newName = "";
private string _state = "入力を待っています。";
private bool _saving;

private bool Matches(PracticeUser user)
    => string.IsNullOrWhiteSpace(_keyword)
       || user.Name.Contains(_keyword.Trim(),
              StringComparison.OrdinalIgnoreCase);
private Func<PracticeUser, bool> Filter => Matches;
```

一覧側は次のように`Items`と`QuickFilter`を使用します。

```razor
<MudTextField T="string" Label="利用者名を検索"
              @bind-Value="_keyword" Immediate="true" />
<p>検索結果：@_users.Count(Matches) 件</p>
<MudDataGrid T="PracticeUser" Items="@_users"
             QuickFilter="@Filter" RowsPerPage="3"
             SortMode="SortMode.Single" Filterable="false">
    <Columns>
        <PropertyColumn Property="x => x.Id" Title="番号" />
        <PropertyColumn Property="x => x.Name" Title="氏名（架空）" />
        <PropertyColumn Property="x => x.Department" Title="部署（架空）" />
    </Columns>
    <PagerContent><MudDataGridPager T="PracticeUser" /></PagerContent>
</MudDataGrid>
```

ここでデータ検索は`_users`という画面メモリ上の一覧に対する検索であり、SQLの`WHERE`やAD照会ではありません。

## 確認後の追加処理

```csharp
private async Task AddAsync()
{
    if (_saving) return;
    var name = _newName.Trim();
    if (name.Length == 0 || name.Length > 30)
    {
        _state = "利用者名は1〜30文字で入力してください。";
        return;
    }

    _saving = true;
    try
    {
        var answer = await Dialogs.ShowMessageBoxAsync(
            "追加内容の確認", $"架空の利用者「{name}」を追加しますか？",
            yesText: "追加する", cancelText: "中止");

        if (answer != true)
        {
            _state = "追加を中止しました。";
            return;
        }

        _users.Add(new PracticeUser(_users.Max(x => x.Id) + 1, name, "研修"));
        _newName = "";
        _state = $"架空の利用者「{name}」を追加しました。";
        Snackbar.Add("一覧を更新しました。", Severity.Success);
    }
    finally
    {
        _saving = false;
    }
}
```

実際の`UiPracticePage.razor`は`IDialogService`と`ISnackbar`をDIで受け取り、ボタンの`Disabled="@_saving"`で操作中の重複を抑制します。上記は主な業務操作部分を実装と同じ動作で示しています。

## イベントから一覧更新まで
1. ブラウザーで名前を入力する。
2. 追加ボタンからサーバーの`AddAsync`を実行する。
3. 空白除去と1〜30文字の条件を検査する。
4. ダイアログを表示し、中止なら状態メッセージだけを変えて終了する。
5. 明示的に追加を選んだ場合に`_users`へ新しい架空行を加える。
6. 一覧と本文が再描画され、Snackbarに通知を表示する。

## 実務に置き換えるときの注意
- **この画面の「架空利用者の追加」を実業務の登録UIの設計と混同しないでください。** 後続の業務認可編では、既存DB/外部テーブルに事前登録された利用者情報を参照する前提です。
- データ量が大きい場合、`Items`の全件取得を避けて`ServerData`とEF Core/Npgsqlで必要分だけ取得します。
- 画面の確認だけでサーバー側の認可・監査・競合検出にはなりません。
- 実業務の更新は権限と対象データを安全に確認した上で、適切なトランザクションを使用します。


## 画面の状態遷移とWinForms＋Dapper比較

| 動作 | WinForms＋Dapperでの考え方 | 学習順040の実装 | 実業務へ移すとき |
| --- | --- | --- | --- |
| 一覧初期値 | SQLでDataGridViewへ取得 | 固定3名をListに保持 | 認可付き検索結果だけ取得 |
| 絞り込み | SQL WHEREまたはBindingSource | QuickFilterをC#で評価 | サーバーの検索条件へ移す |
| 追加確認 | MessageBox | IDialogServiceのbool? | 確認後に権限を再評価 |
| 処理中 | Enabled=false | _savingとDisabled | サーバー側の冪等性も必要 |
| 結果通知 | LabelやMessageBox | _stateとSnackbar | 監査・保存の成功を別に判定 |

この画面は`_users`というメモリ内リストへ、名前と部署「研修」の架空データを追加します。ユーザー名をDBへ登録する機能ではありません。特にこの教材の認証・認可編は、利用者が事前に整備済みのDBまたは外部台帳に存在する前提です。画面上の練習用追加を、本番台帳へ登録する機能だと考えないでください。

## 実装の読み方と手順

1. `/learn/30`で3名の架空利用者が表示される。
2. 「練習」を検索すると1件になる。検索語は`_keyword`に入り、`Matches`は大文字小文字を区別しない部分一致で判定する。
3. 検索語を戻し、空の入力で追加を押す。1〜30文字という制約の拒否文言が出るがリストは変わらない。
4. 「テスト 四郎」を入力し、確認で「中止」を選ぶ。`answer != true`の分岐でリストは変わらない。
5. 同じ入力で「追加する」を選ぶ。リスト末尾へIDを増やした架空行が入り、入力欄は空に戻る。
6. 検索欄に「練習」が残っていれば追加した行は一覧に見えない。検索解除と全件の比較を行う。

| 症状 | 見る値 | 切り分け結果 |
| --- | --- | --- |
| 追加した行が見えない | _keywordとQuickFilter | 検索条件から外れている |
| 中止後に行が増えない | answer != true | 仕様どおりの不実行 |
| 名前が空で追加できない | TrimとLength | 検証による拒否 |
| 二度押しで何度も開く | _savingとDisabled | 同一画面の抑止を確認 |
| DBに保存されない | _usersのList.Add | 永続化を実装していない |

## 総合演習と詳しい解答

**演習3：** 検索欄に「練習」を残したまま「テスト 四郎」を追加した場合、一覧上の検索結果と保持データはそれぞれ何件ですか。

**詳しい解答：** 初期の「練習 花子」だけが検索語に一致するので検索結果は1件、固定3名に1名追加した`_users`は4件です。検索結果の件数が増えないだけで追加が失敗したわけではありません。

**演習4：** このUIへ実DBの利用者情報を表示する際、検索欄の条件だけで部署外の行を隠せば十分ですか。

**詳しい解答：** 不十分です。QuickFilterは取得後の表示を絞るだけで、サーバーから取得してはいけない行を保護できません。まずADで本人を確かめ、DBまたは外部台帳を正本にした部署・権限判定をサーバー側の一覧取得、総件数、詳細、CSV出力、更新の各操作で行います。権限が失効したときや監査保証が成立しない更新は拒否し、実行可否を確認ダイアログへ委ねません。

**演習5：** `_users.Max(x => x.Id)+1`は実業務の同時追加に安全ですか。

**詳しい解答：** 安全とは限りません。ここには画面一つの固定データしかありませんが、別回線の同時更新があるDBではIDの重複が起こり得ます。永続化が必要な業務テーブルはDB側の一意制約や生成方式と更新トランザクションを使用し、競合を正常終了として扱わないようにします。

## 演習と解答
**問1：** 検索欄に「練習」を入力したまま「テスト 四郎」を追加すると、検索結果の件数はどうなりますか。

**答え：** 新しい名前は「練習」を含まないため、検索結果は1件のままです。全件リスト自体は4件になります。

**問2：** この画面の`_users`をそのまま実際の既存利用者台帳に置き換えてよいですか。

**答え：** いけません。実業務ではADによる本人認証、既存台帳に基づく認可、保存方法、同時更新、監査などが必要です。ここはUIの練習専用です。

## 完了確認
- [ ] 検索・未入力拒否・中止・確認後の追加を確認した。
- [ ] メモリ一覧とDBの権限付き一覧の違いを説明できる。
- [ ] DI、Dialog、Snackbar、Razorの再描画のつながりを説明できる。

[コラム07：MudBlazor実務画面の使い分け](../columns/column26-30.md)で第4段階の第8組を復習します。
