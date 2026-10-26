# ステップ14：BlazorのScopedと対話回線を知る

## このステップで学ぶこと
WinFormsの画面状態は、通常そのフォームのインスタンスで管理します。Blazor Interactive Serverでは、ブラウザーとサーバーの間の継続的な対話を**circuit（対話回線）**が支えます。Scopedサービスの状態が親子コンポーネントで共有されることと、共有状態を変えても表示更新は別に考える必要があることを学びます。

## 前提と完成画面
ステップ13でScopedの意味を学んでから`/learn/14`を開きます。DB・ADは不要です。カウンターは練習用で、サーバー再起動後も保存されるDBデータではありません。

## 操作の手順
1. 親と子のカウントがともに`0`と表示されていることを確認する。
2. 「親から1増やす」で双方`1`、「子の表示から1増やす」で双方`2`になることを確かめる。
3. 「0に戻す」を押し、双方が`0`になることを確認する。
4. 別のプライベートウィンドウで`/learn/14`を開き、新しい対話状態が独立しているか比較する。
5. サーバー停止や回線切断後の再接続は、同じ状態が必ず復元されるという保証にはならないことを確認する。

## サービスとDI登録
カウントを保持する`ScopedCounter`は次のクラスです。

```csharp
public sealed class ScopedCounter
{
    public int Count { get; private set; }
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}
```

`Program.cs`の登録は次の1行です。

```csharp
builder.Services.AddScoped<ScopedCounter>();
```

## 親と子のコードを結び付ける
親ページは`ScopedCounter`を受け取り、子へ現在値と通知用コールバックを渡します。

```razor
@page "/learn/14"
@inject ScopedCounter Counter

<p>親が見ている値：@Counter.Count</p>
<button @onclick="Increase">親から1増やす</button>
<button @onclick="Reset">0に戻す</button>
<SharedCounter CurrentCount="@Counter.Count" OnChanged="Refresh" />

@code {
    private void Increase() => Counter.Increment();
    private void Reset() => Counter.Reset();
    private void Refresh() => StateHasChanged();
}
```

子部品`SharedCounter`にもScopedのカウンターを注入し、クリック後に親へ通知します。

```razor
@inject ScopedCounter Counter

<p>子が見ている値：@CurrentCount</p>
<button @onclick="IncreaseAsync">子の表示から1増やす</button>

@code {
    [Parameter] public int CurrentCount { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }

    private async Task IncreaseAsync()
    {
        Counter.Increment();
        await OnChanged.InvokeAsync();
    }
}
```

実ページでは見出しやナビゲーションも設けています。ここで`CurrentCount`は親から受け取る値であり、`Counter`はDIから受け取るサービスです。別々の仕組みをあえて同時に使っています。

## 子のボタンを押したときの処理順
1. ブラウザーからクリックが通知され、サーバーの`SharedCounter.IncreaseAsync`を実行する。
2. 子が注入された`ScopedCounter`の`Count`を1増やす。
3. `OnChanged.InvokeAsync()`が親の`Refresh`を呼び、親に再描画を促す。
4. 親が`Counter.Count`を表示し直し、新しい`CurrentCount`を子へ渡す。
5. 子の表示も同じ値になる。**Scopedサービスが同じというだけで別コンポーネントが自動再描画されるわけではありません。**

## 実務上の注意
- 別タブ・別ウィンドウの挙動は接続方法や再接続に影響されます。独立比較にはプライベートウィンドウを使います。
- Scoped状態を業務データの永続化先にしてはいけません。サーバー再起動や回線喪失で失われ得ます。
- 利用者固有のカウンターをSingletonに置くと別利用者間で状態が共有されるため、不適切です。
- 業務の認証・認可は、`ScopedCounter`のような画面内状態だけで決めてはいけません。後続の認可編でDB側の権限確認を学びます。

## 演習と解答
**演習1：** 子の`OnChanged.InvokeAsync()`を削ると、何が問題になりますか。

**解答：** カウンターの共有状態自体は増加しますが、親の再描画が通知されず、親から渡している`CurrentCount`や表示が最新値へ更新されない可能性があります。

**演習2：** ブラウザーを再読込したときに前回のカウントが必ず残る、と設計してよいですか。

**解答：** いけません。新しいcircuitやサーバー再起動などで状態が失われ得るため、永続化が必要なデータは別に保存します。

## 完了確認
- [ ] 親と子の表示が連動する理由を説明できる。
- [ ] 共有されたサービスとUI再描画の役割を分けて説明できる。
- [ ] circuit状態と永続DBを混同しない。

次は[ステップ15：注入方法](step15.md)です。
