# ステップ12：インターフェイスで実装を交換する

## このステップで学ぶこと
ステップ11ではDIからサービスを受け取りました。ここでは、サービスが使う部品の**機能の約束**をインターフェイスで表し、同じサービスへ異なる時計を渡します。WinFormsでの`IRepository`やDapperの検索用インターフェイスと考え方は同じですが、今回はDBのない小さな例に限定します。

## 前提と完成画面
ステップ11の`IClock`と`GreetingService`を理解してから`/learn/12`を開きます。PostgreSQL・ADは不要です。

1. 「実際の時計」を押し、現在時刻に応じた挨拶を見る。
2. 「固定時計（午前8時）」を押すと、選択中が「固定時計」、時刻が`2026/01/01 08:00`、挨拶が「おはようございます」と表示されます。
3. 実際の時計へ戻し、画面の表示が切り替わることを確かめます。

`FixedClock`はサーバー上の固定値を返します。ブラウザーの時計を変更しているわけではありません。

## 実装の最小構成
二つのクラスはどちらも`IClock`に定義された`GetNow`を備えています。

```csharp
public interface IClock
{
    DateTimeOffset GetNow();
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset GetNow() => DateTimeOffset.Now;
}

public sealed class FixedClock : IClock
{
    public DateTimeOffset GetNow() =>
        new(2026, 1, 1, 8, 0, 0, TimeSpan.FromHours(9));
}

public sealed class GreetingService(IClock clock)
{
    public string GetMessage() => clock.GetNow().Hour < 12
        ? "おはようございます"
        : "こんにちは";
}
```

実装された`Program.cs`の登録は次のとおりです。

```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<FixedClock>();
builder.Services.AddTransient<GreetingService>();
```

この登録により、`@inject IClock SystemClock`は標準の`SystemClock`、`@inject FixedClock FixedClock`は固定時計のインスタンスを受け取ります。画面の切り替えでDI登録自体は変更しません。

```razor
@page "/learn/12"
@inject IClock SystemClock
@inject FixedClock FixedClock

<button @onclick="() => useFixed = false">実際の時計</button>
<button @onclick="() => useFixed = true">固定時計（午前8時）</button>
<p>選択中：@(useFixed ? "固定時計" : "実際の時計")</p>
<p>取得した時刻：@SelectedClock.GetNow().ToString("yyyy/MM/dd HH:mm")</p>
<p>挨拶：@(new GreetingService(SelectedClock).GetMessage())</p>

@code {
    private bool useFixed;
    private IClock SelectedClock => useFixed ? FixedClock : SystemClock;
}
```

画面を理解するための最小抜粋です。実装画面では見出しや状態表示も付けています。ここで`new GreetingService(...)`としているのは、**選択した時計を即座に注入する例を見せるため**です。通常のアプリ全体の交換はDI登録や実装選択を設計します。

## 画面操作から結果が出るまで
1. ブラウザーで固定時計のボタンを押す。
2. サーバー上のRazorイベントが`useFixed = true`にする。
3. `SelectedClock`が`FixedClock`を選ぶ。
4. `GreetingService`が`IClock.GetNow`を呼ぶ。具体的な時計クラスへの分岐はサービス内にない。
5. 固定日時と挨拶が再描画されて表示される。実DBやADへの要求はありません。

## 間違えやすいこと
- **DIとインターフェイスは別物です。** インターフェイスは呼び出せる機能の約束、DIはその依存先を渡す仕組みです。
- `IClock`を切り替えるだけで、既に注入済みの`GreetingService`の時計が自動的に置き換わるわけではありません。この画面は選んだ時計で`GreetingService`を新しく作っています。
- `FixedClock`を`AddSingleton<IClock, FixedClock>()`で追加登録すると、同じ`IClock`の登録を上書きするなど期待しない解決になる可能性があります。まず登録先と取得方法を区別してください。

## 演習と解答
**演習1：** 固定時計が午後3時を返すように変更したら、どの文字列が表示されますか。

**解答：** 日付表示の時刻は15:00となり、挨拶は「こんにちは」になります。

**演習2：** `GreetingService`を変更せず、時計の動作を切り替えられる理由は何ですか。

**解答：** コンストラクターで受け取る型が`IClock`であり、両方の実装が同じ`GetNow`を提供しているためです。

## 完了確認
- [ ] 実際の時計と固定時計の表示を比較した。
- [ ] インターフェイスとDI登録の違いを説明できる。
- [ ] テスト時に固定時計が役立つ理由を説明できる。

次は[ステップ13：サービスの生存期間](step13.md)です。
