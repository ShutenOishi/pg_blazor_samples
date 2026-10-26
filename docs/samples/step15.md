# ステップ15：@injectとコンストラクター注入

## このステップで学ぶこと
ステップ11〜14ではDIの目的、インターフェイス、寿命、Scoped状態を学びました。最後に、**Razor画面でサービスを受け取る記法**と、**通常のC#クラスでコンストラクターから受け取る記法**を比較します。WinFormsでフォームのコンストラクターにサービスを渡した経験があれば、考え方は同じです。

## 前提と完成画面
ステップ11〜14の理解が前提です。`/learn/15`を開きます。DB・ADは不要です。

1. 入力欄は最初「練習太郎」です。
2. 「練習花子」に書き換えると、表示が「練習花子さん、おはようございます。」または「練習花子さん、こんにちは。」へ変わります。
3. 挨拶の違いはサーバー時刻によります。名前の変更をDBへ保存したり、ADユーザー名を読んだりする画面ではありません。

## サービスのつながり
`GreetingService`は`IClock`を受け取ります。`GreetingFormatter`はさらに`GreetingService`を受け取って名前付きの文字列にします。

```csharp
public sealed class GreetingService(IClock clock)
{
    public string GetMessage() => clock.GetNow().Hour < 12
        ? "おはようございます"
        : "こんにちは";
}

public sealed class GreetingFormatter(GreetingService greeting)
{
    public string Format(string name)
        => $"{name}さん、{greeting.GetMessage()}。";
}
```

.NET 10のC#で使う`GreetingFormatter(GreetingService greeting)`はプライマリコンストラクターです。通常の`public GreetingFormatter(GreetingService greeting) { ... }`と同じく、依存するサービスをコンストラクター経由で受け取る目的に使えます。

`Program.cs`は次の依存関係を登録します。

```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddTransient<GreetingService>();
builder.Services.AddTransient<GreetingFormatter>();
```

DIは`GreetingFormatter`を生成するため`GreetingService`を用意し、さらにその`IClock`を解決します。手で`new GreetingFormatter(new GreetingService(new SystemClock()))`と組み立てる必要がありません。

## Razor画面の最小コード

```razor
@page "/learn/15"
@inject GreetingFormatter Formatter

<label for="learner-name">練習用の名前</label>
<input id="learner-name" maxlength="20"
       @bind="learnerName" @bind:event="oninput" />
<p role="status">@Formatter.Format(learnerName)</p>

@code {
    private string learnerName = "練習太郎";
}
```

実画面ではCSSクラスや日本語見出し、移動リンクも含みます。`@bind:event="oninput"`により入力のたびに`learnerName`が更新され、サーバーが`Formatter.Format`を評価して表示を返します。ブラウザー側で`GreetingFormatter`が実行されるわけではありません。

## 画面が表示されるまでの処理順
1. Blazorが`/learn/15`のRazorコンポーネントを生成する。
2. DIが`GreetingFormatter`の依存関係をたどって`GreetingService`と`IClock`を渡す。
3. Razorは`learnerName`を引数に`Formatter.Format`を呼ぶ。
4. 名前に挨拶を結合した文字列をブラウザーへ表示する。
5. 名前を入力すると`oninput`の通知によりC#の状態が変わり、新しい文字列を表示する。

## 使い分けとよくあるエラー
- Razorコンポーネントでは`@inject`、通常のC#サービスではコンストラクター注入が分かりやすい標準です。
- `@inject`してもDI登録は必要です。`GreetingFormatter`だけを登録し、`GreetingService`が未登録ならコンストラクターの依存関係を解決できません。
- 名前の文字入力とDI解決は別です。キーを押すたびにサービスを再生成する仕組みではありません。
- `GreetingFormatter`をSingletonに変更するときは、依存先の寿命やスレッド安全性を別途検討してください。すべてSingletonにすればよいわけではありません。

## 演習と解答
**演習1：** 名前を変えるだけの要件なら、`GreetingFormatter`がブラウザーのDOMを直接操作する必要はありますか。

**解答：** ありません。Razor画面が値の入力と表示を担当し、サービスは文字列の組み立てを担当します。

**演習2：** `GreetingFormatter`が依存するサービスの生成を自分で行わなくてもよい理由は何ですか。

**解答：** DIに登録されたコンストラクターの依存関係をコンテナーが解決するためです。

## 完了確認
- [ ] `@inject`とコンストラクター注入の使い分けを説明できる。
- [ ] サービスの依存関係が`Formatter → GreetingService → IClock`の順に解決されることを理解した。
- [ ] 日本語の入力から再描画までの経路を説明できる。

[学習コラム03：DIの全体を整理する](../columns/column11-15.md)で、5ステップを関連付けて復習します。
