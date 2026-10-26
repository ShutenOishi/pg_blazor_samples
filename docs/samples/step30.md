# ステップ30：検索・入力・確認・通知の総合演習

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

## 演習と解答
**問1：** 検索欄に「練習」を入力したまま「テスト 四郎」を追加すると、検索結果の件数はどうなりますか。

**答え：** 新しい名前は「練習」を含まないため、検索結果は1件のままです。全件リスト自体は4件になります。

**問2：** この画面の`_users`をそのまま実際の既存利用者台帳に置き換えてよいですか。

**答え：** いけません。実業務ではADによる本人認証、既存台帳に基づく認可、保存方法、同時更新、監査などが必要です。ここはUIの練習専用です。

## 完了確認
- [ ] 検索・未入力拒否・中止・確認後の追加を確認した。
- [ ] メモリ一覧とDBの権限付き一覧の違いを説明できる。
- [ ] DI、Dialog、Snackbar、Razorの再描画のつながりを説明できる。

[コラム07：MudBlazor実務画面の使い分け](../columns/column26-30.md)で第3段階を復習します。
