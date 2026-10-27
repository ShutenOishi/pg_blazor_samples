# 学習順30：DIで学習課題管理を作る

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step20/TodoPage.razor)。画面の正規URLは `/learn/030` です。

対象画面は`/learn/030`です。WinForms＋Dapperの経験と結び付けて理解します。

## このステップでできるようになること
入力画面、業務サービスの契約、メモリ上の保存実装、設定のOptionsを一つの機能にまとめます。WinForms＋Dapper経験者にとっては、フォームから`ITodoService`のメソッドを呼ぶ形と似ていますが、今回はSQLやDB接続の代わりに**短時間だけ保持するメモリ上の一覧**を使います。

## 前提と完成画面
学習順021〜19のDI・Options・Scopedを学び、`/learn/030`を開いてください。DB・ADは不要です。

1. 初期課題「DIの登録を確認する」「ライブラリの利用手順を読む」の2件を確認する。
2. 空欄で「課題を追加する」を押すと「課題名は1〜40文字で入力してください。」と表示される。
3. 「DIを復習する」と入力して追加すると3件になり、入力欄が空になる。
4. チェックボックスを操作すると完了状態が反転する。
5. さらに追加して5件になった後、6件目を試みると「課題は最大5件までです。」と表示される。

## Applicationの契約とDTO
このステップでは既存の`Application`に次の型を置き、Web側が具体的な保存方法を知らずに操作できるようにします。

```csharp
public sealed record TodoItem(Guid Id, string Title, bool IsComplete);

public interface ITodoService
{
    IReadOnlyList<TodoItem> GetAll();
    bool TryAdd(string? title, out string message);
    bool Toggle(Guid id);
}
```

`TodoItem`は1件の課題を表す不変の`record`です。`Guid`で行を区別し、表示に使います。

## メモリ実装の要点
`InMemoryTodoService`はWebプロジェクトの`Services/Learning`内にあります。以下は実コードの主要なメソッドです。

```csharp
private readonly List<TodoItem> _items =
[
    new(Guid.NewGuid(), "DIの登録を確認する", false),
    new(Guid.NewGuid(), "ライブラリの利用手順を読む", false)
];
private readonly int _limit;

public InMemoryTodoService(IOptions<LearningOptions> options)
{
    _limit = Math.Clamp(options.Value.MaxTasks, 1, 20);
}

public IReadOnlyList<TodoItem> GetAll() => _items.AsReadOnly();

public bool TryAdd(string? title, out string message)
{
    var trimmed = title?.Trim() ?? "";
    if (trimmed.Length is < 1 or > 40)
    {
        message = "課題名は1〜40文字で入力してください。";
        return false;
    }
    if (_items.Count >= _limit)
    {
        message = $"課題は最大{_limit}件までです。";
        return false;
    }
    _items.Add(new TodoItem(Guid.NewGuid(), trimmed, false));
    message = "課題を追加しました。";
    return true;
}

public bool Toggle(Guid id)
{
    var index = _items.FindIndex(item => item.Id == id);
    if (index < 0) return false;
    _items[index] = _items[index] with
    {
        IsComplete = !_items[index].IsComplete
    };
    return true;
}
```

このコードは`InMemoryTodoService : ITodoService`のクラス内に含まれる部分です。`Math.Clamp`で設定上限を1〜20の範囲に収めます。初期設定は5です。サービスの中で課題名の文字数・件数を検査するため、画面表示だけの制限ではありません。

## Program.csと画面をつなぐ
`Program.cs`ではOptionsとScopedの保存実装を登録します。

```csharp
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
builder.Services.AddScoped<ITodoService, InMemoryTodoService>();
```

画面は`ITodoService`を受け取るだけです。

```razor
@page "/learn/030"
@inject ITodoService Tasks

<input maxlength="40" @bind="newTitle" @bind:event="oninput" />
<button @onclick="Add">課題を追加する</button>
<p role="status">@message</p>
<p>登録件数：@Tasks.GetAll().Count 件</p>
@foreach (var item in Tasks.GetAll())
{
    <label @key="item.Id">
        <input type="checkbox" checked="@item.IsComplete"
               @onchange="() => Toggle(item.Id)" />
        @item.Title
    </label>
}

@code {
    private string newTitle = "";
    private string message = "";

    private void Add()
    {
        var success = Tasks.TryAdd(newTitle, out message);
        if (success) newTitle = "";
    }

    private void Toggle(Guid id) => Tasks.Toggle(id);
}
```

実際の`TodoPage.razor`では日本語ラベルや`ul/li`による一覧構造、前後の移動リンクを設けています。上記は操作の中心部分です。

## 追加ボタンを押してから表示されるまで
1. ブラウザーで課題名を入力すると`@bind:event="oninput"`がC#の`newTitle`を更新する。
2. 「課題を追加する」を押すと`Add`が`Tasks.TryAdd`を呼ぶ。
3. `InMemoryTodoService`は前後の空白、文字数、件数上限を検査する。
4. 成功時は`Guid`付きの`TodoItem`を追加し、メッセージを返す。失敗時は件数を変えない。
5. Razorがメッセージと`Tasks.GetAll()`の最新一覧を再描画する。
6. チェックボックスの操作では`Toggle(id)`が対象の課題を新しい`record`へ差し替える。

## 誤解しやすい点と安全上の注意
- `AddScoped`の保存先はメモリです。**PostgreSQLに永続保存されません。** 接続終了やサーバー再起動で失われることがあります。
- 利用者全員の課題をSingletonに置いてはいけません。画面ごとやcircuitごとの状態の寿命を理解してください。
- 設定値の表示（学習順026）と実際のサービス側制限は別です。
- `maxlength="40"`はブラウザー入力の補助です。サービスの`TryAdd`が40文字超を拒否する処理は省略できません。
- 将来DBへ移す場合でも、認可や同時編集、トランザクションを追加せずに保存先を変えるだけでは本番用になりません。

## 実際の単体テスト
`TodoServiceTests`では、名前の前後空白を除くこと、未入力・41文字の拒否、2件上限、見つからない`Guid`を指定した`Toggle`の拒否、サービスを二つ作った場合の状態の独立を確認しています。ここでは実DBの試験は行いません。

## 演習と解答
**演習1：** 「  DIの確認  」を入力した場合、登録される名前は何ですか。

**解答：** `Trim()`により「DIの確認」になります。

**演習2：** 設定上限を2件にした場合、初期状態から追加できますか。

**解答：** できません。既に2件あるため`TryAdd`は`false`と上限メッセージを返します。

**演習3：** 保存先をPostgreSQLへ移すとき、画面が参照する型を変えないで済む条件は何ですか。

**解答：** `ITodoService`の契約と画面が期待する振る舞いを維持できることです。具体的な実装を交換してDI登録を変更します。認証・認可・監査など、実務に必要な条件は別途追加します。

## 学習順030：DIの総合演習をWinForms＋Dapperに対応させる
`/learn/030`で利用する`ITodoService`は課題一覧の取得・追加・完了反転を約束する型で、`InMemoryTodoService`が実際の処理を担当します。WinFormsでフォームからDapper経由のDB処理を呼ぶような責任分担を、DBを用意せずに練習する例です。ただし保存先はListであり、永続化・同時編集・監査を備えた業務アプリではありません。Scopedサービスの状態は対話回線に依存することがあり、サーバー再起動後まで残る保証はありません。

| 観点 | WinForms＋Dapperの例 | Blazorの学習用実装 | 本番で必要なこと |
| --- | --- | --- | --- |
| フォーム入力 | TextBoxの値 | `newTitle`の`oninput` | 保存前の再検証 |
| 追加 | Executeやサービス呼出 | `TryAdd` | 必要ならDBトランザクション |
| 完了 | UPDATEと再描画 | `Toggle(Guid)` | 楽観的競合と監査 |
| 保存先 | PostgreSQL等 | メモリ`List<TodoItem>` | 既存台帳による認可 |

`LearningOptions.MaxTasks`の初期値は5で、`InMemoryTodoService`が上限を1〜20へ補正します。初期課題が2件あるので、追加可能なのは3件です。`maxlength="40"`は入力補助であり、`TryAdd`の`Trim`と文字数判定が実際の拒否です。`Toggle`は`Guid`で行を特定し、`record`の`with`で完了フラグを反転した新しい値へ差し替えます。`ITodoService`の契約を残して保存実装を差し替えられても、それだけでAD本人認証とDBの都度認可・監査が満たされたことにはなりません。

| 症状 | 切り分けの場所 | 必要な確認 |
| --- | --- | --- |
| 空白だけで追加できる | `Trim`と文字数判定 | 件数が増えないこと |
| 6件目が追加できる | `_items.Count >= _limit` | 最大5件で拒否 |
| 完了状態が変わらない | `Toggle`と対象Guid | 対象項目の状態 |
| 別の接続に同じ状態が現れる | `AddScoped`の寿命 | 共有状態の有無 |
| 再読込で消える | メモリサービス | DBへ保存していない |

### 追加演習と詳しい解答
**演習4：** 上限を2に変更した場合、新しい課題を追加できますか。

**解答：** 初期課題が2件あるため追加できません。`TryAdd`は`false`と上限メッセージを返し、一覧は変化しません。

**演習5：** Dapperを使う実装へ交換すれば、権限や監査は自動的に追加されますか。

**解答：** されません。本人認証、事前に整備されたDB/外部台帳を使う操作ごとの認可、更新競合・原子的監査などを別に実装・試験します。次の第6組復習とライブラリDI特別コラムで、標準サービスと自作サービスの役割を整理してください。

## 完了確認
- [ ] 空欄・6件目の拒否を再現した。
- [ ] Applicationの契約とWeb側の保存実装を区別できる。
- [ ] Options、Scoped、画面のDI注入の順序を説明できる。
- [ ] メモリ保存とDBの永続保存の違いを理解した。

[コラム04：5ステップの復習](../columns/column16-20.md)と[特別コラム05：外部ライブラリのDI](../columns/column-library-di.md)へ進みます。
