# ステップ11：DI（依存性注入）とは何か

## このステップで学ぶこと
WinFormsでは、フォームのイベント処理から`new GreetingService(...)`のように業務クラスを生成していたかもしれません。Blazorでも直接生成はできますが、利用するサービスを外から受け取るDI（Dependency Injection：依存性注入）を標準的に使用できます。この違いを、時計から挨拶を作る実際の画面で確かめます。

**完了後の目標**は、サービスを自分で作る場合とDIで受け取る場合の違い、登録する場所、画面から呼ぶまでの処理順序を説明できることです。

## 前提と動作条件
ステップ01〜10のRazor画面とイベントの理解が必要です。PostgreSQL、AD、JavaScriptの追加実装は不要です。Visual Studio 2026でWebアプリを起動し、`/learn/11` を開きます。

## 実際に動かす
1. 「直接作成した時計」「DIで受け取る時計」の挨拶を見る。
2. 「時刻を読み直す」を押し、両方が再計算されることを確認する。
3. 実際のサーバー時刻が午前なら「おはようございます」、それ以外なら「こんにちは」と表示される。時刻の境目では2回の取得結果が変わる可能性もあります。

この画面の違いは表示文字列ではなく、**誰が時計とサービスを組み立てたか**です。サーバー側でC#のイベント処理が実行され、画面の再描画結果がブラウザーへ反映されます。ブラウザー内で`SystemClock`が動くわけではありません。

## 実装の最小構成

時計を表す約束と実装は次のとおりです。

```csharp
public interface IClock
{
    DateTimeOffset GetNow();
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset GetNow() => DateTimeOffset.Now;
}
```

サービスは時計を`new`しません。コンストラクターで受け取ります。

```csharp
public sealed class GreetingService(IClock clock)
{
    public string GetMessage() => clock.GetNow().Hour < 12
        ? "おはようございます"
        : "こんにちは";
}
```

Webアプリの`Program.cs`では、依存する部品を登録します。実装では`IClock`がSingleton、`GreetingService`がTransientです。

```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddTransient<GreetingService>();
```

Razor画面では次のように受け取り、ボタンで更新します。`directMessage`と`injectedMessage`は画面の状態です。

```razor
@page "/learn/11"
@inject GreetingService Greeting

<button type="button" @onclick="Refresh">時刻を読み直す</button>
<p>直接作成した時計：@directMessage</p>
<p>DIで受け取る時計：@injectedMessage</p>

@code {
    private string directMessage = "";
    private string injectedMessage = "";

    protected override void OnInitialized() => Refresh();

    private void Refresh()
    {
        directMessage = new GreetingService(new SystemClock()).GetMessage();
        injectedMessage = Greeting.GetMessage();
    }
}
```

これは説明に必要な表示と処理を抜き出したものです。実際のページには日本語見出し・スタイル・前後の移動リンクもあります。`@inject`による受け取りはコンポーネント生成時に行われ、`Refresh`で毎回DIが解決されるわけではありません。

## 操作したときの処理順
1. ブラウザーから`/learn/11`へ移動し、Blazorが画面コンポーネントを作る。
2. 登録済みの`GreetingService`をDIから受け取り、その作成に必要な`IClock`もDIが渡す。
3. `OnInitialized`で`Refresh`を呼び、直接生成した時計と注入したサービスの両方で挨拶を計算する。
4. 計算結果がRazorの`@directMessage`と`@injectedMessage`に表示される。
5. ボタンを押すとサーバーで`Refresh`が動き、変化した表示がブラウザーへ届く。DBやHTTP APIへの追加呼び出しはありません。

## 間違えやすいことと確認方法
- `@inject`を書いただけでは使えません。未登録のサービスを注入しようとすると、画面の生成時に解決エラーになります。まず`Program.cs`の登録と型を確認します。
- DIはインターフェイスの使用を必須としません。ここでは`GreetingService`自体を登録し、`IClock`はインターフェイスで登録しています。
- DIはテスト結果を自動的に保証しません。テストしやすい設計を作る手段であり、実際の試験は後続のステップ18で行います。
- `SystemClock`の時刻はサーバーのものです。ブラウザーのタイムゾーンで動く処理と混同しないでください。

## 演習と解答
**演習1：** `GreetingService`の中で`new SystemClock()`を行う形へ変更すると、何が不便になりますか。

**解答：** 呼び出す側から時計を渡せなくなり、特定時刻の挨拶を再現する試験や別実装への交換が難しくなります。

**演習2：** `GreetingService`だけDI登録して`IClock`を登録しなかった場合、何が起こりますか。

**解答：** DIが`GreetingService`のコンストラクターに渡す`IClock`を解決できず、生成に失敗します。

## 完了確認
- [ ] 直接生成とDIの違いを、作成する責任の違いとして説明できる。
- [ ] `Program.cs`と`@inject`がそれぞれ何をするか説明できる。
- [ ] サーバーで処理してブラウザーに表示する順序を説明できる。

次は[ステップ12：インターフェイスで実装を交換する](step12.md)で、依存部品を交換する効果を確かめます。
