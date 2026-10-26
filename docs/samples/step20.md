# ステップ20：DIで学習課題管理を作る

## このステップでできるようになること
入力画面、業務サービスの契約、メモリ上の保存実装、設定のOptionsを一つの機能にまとめます。WinForms＋Dapper経験者にとっては、フォームから`ITodoService`のメソッドを呼ぶ形と似ていますが、今回はSQLやDB接続の代わりに**短時間だけ保持するメモリ上の一覧**を使います。

## 前提と完成画面
Step11〜19のDI・Options・Scopedを学び、`/learn/20`を開いてください。DB・ADは不要です。

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
@page "/learn/20"
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
- 設定値の表示（Step16）と実際のサービス側制限は別です。
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

## 完了確認
- [ ] 空欄・6件目の拒否を再現した。
- [ ] Applicationの契約とWeb側の保存実装を区別できる。
- [ ] Options、Scoped、画面のDI注入の順序を説明できる。
- [ ] メモリ保存とDBの永続保存の違いを理解した。

[コラム04：5ステップの復習](../columns/column16-20.md)と[特別コラム05：外部ライブラリのDI](../columns/column-library-di.md)へ進みます。
