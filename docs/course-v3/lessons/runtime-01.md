# 学習順009：Static SSRとInteractive Serverの違い

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage02/L009StaticSsrInteractivePage.razor)、[Static SSR用Razor](../../../src/PgBlazorSamples.Web/Components/Samples/Runtime/StaticSsr009Component.razor)、[GET経路のProgram.cs](../../../src/PgBlazorSamples.Web/Program.cs)。画面の正規URLは `/learn/009` です。

## 到達目標と前提

Blazor Web Appで初めに混同しやすいのは「HTMLが見えたこと」と「ボタンのC#イベントが動くこと」です。Static Server-Side Rendering（Static SSR）は、要求に応じてサーバーがHTMLを作り、ブラウザーへ返す方式です。Interactive Serverは、対話型接続を通じてC#イベントをサーバーで実行できます。この教材の実画面は後者であり、サーバー側カウンターを使ってイベントを確認します。Static SSRは同じアプリ内の隔離GET経路 `/__learning_static_ssr_009__` で、Razorコンポーネントを静的なHTMLとして返して実証します。Interactive Serverの全体設定は変更しません。

この章の対象は .NET 10、Visual Studio 2026、Blazor Web App（Interactive Server）です。第1段階のHTTPとブラウザーの役割を学習済みであることを前提とします。教材画面の操作にはActive Directory・PostgreSQL・業務データを必要としません。実際の画面は **`/learn/009`** です。前のテーマから順に進んでも、この章だけを開いて確認できるよう、コードと操作方法を以下に示します。

## 1. WinForms＋Dapperとの対応を確認する

| 観点 | WinForms＋Dapper経験者の見方 | 本教材のBlazor | 
| --- | --- | --- |
| HTMLの生成 | 要求時にサーバーが生成 | 要求時のHTMLと対話中の更新 |
| C#のボタンクリック | 対話接続なしでは@onclickを実行しない | 接続後サーバー側で実行 |
| 通信 | 要求と応答が基本 | 初期要求のほか対話型接続を利用 |
| 値の寿命 | 要求ごとに処理するモデル | 対話中のコンポーネント状態 |
| WinFormsとの類似 | フォームの描画結果を受け取るようなイメージ | イベントに対応するが処理場所はサーバー |
| 本教材の実証 | 別GET経路による実Razorの静的HTMLとC#クリック不動作、フォームGETを確認 | 対話型C#イベントを確認 |

比較する際は、WinFormsで自分が操作するPCと、業務アプリのサーバー側に配置されるC#が同じ場所とは限らない点を意識してください。ここに示すDapperは経験上の比較対象です。今回のサンプルはDapperやEF Coreを実行しません。後続のDB編では、業務データを取得する際に認可を行った上でサーバー側からアクセスします。

## 2. 実際のRazor画面コードを読む

次は実装中の主要部分の抜粋です。コンポーネント全体の他のHTMLや見出しは省略していますが、掲載した要素・表示文字列と識別子は現行実装に対応します。

```razor
<p>サーバーでの操作回数：<strong data-testid="runtime01-count">@count</strong></p>
<button type="button" @onclick="Increment">サーバー側の回数を増やす</button>
```

`L009StaticSsrInteractivePage.razor`は`@page "/learn/009"`です。画面は`count`を表示し、「サーバー側の回数を増やす」を押すと`Increment()`がサーバーで実行されます。実際のアプリの`App.razor`では`<Routes @rendermode="InteractiveServer" />`を設定しています。したがってこの画面で行う操作はInteractive Server方式の試験です。表示された表のStatic SSR側は比較説明であり、同じページのレンダーモードを切り替える実験ではなく、別のStatic SSR専用GET経路と比較します。

### Static SSRの実装と負例

冒頭の実装ソース欄にある`StaticSsr009Component.razor`と`Program.cs`の専用GET経路を確認します。`RazorComponentResult`はコンポーネントを静的HTMLとして返し、SignalRの対話回線を開始しません。

```csharp
app.MapGet("/__learning_static_ssr_009__", (int? count) =>
{
    if (count is < 0 or > 2)
        return (IResult)Results.BadRequest("学習用の回数は0〜2だけ指定できます。");
    return (IResult)new RazorComponentResult<StaticSsr009Component>(
        new Dictionary<string, object?> { ["Count"] = count ?? 0 });
}).AllowAnonymous();
```

このページの`@onclick`ボタンを押してもC#側の`Increment`は動きません。一方、HTMLの`form method="get"`を送信すると、`?count=1`で新しいDocument GETが起き、静的HTMLが再生成されます。`?count=-1`はHTTP 400を返します。匿名の学習用固定カウントだけを扱い、本人認証、DB、業務監査は実施しません。

## 3. C#または関連する実コードを確かめる

次も現行実装から抜き出した部分です。画面や状態の理解に必要な部分を残しています。先のコードと照らして、HTMLが担当する表示とサーバー側の処理を切り分けてください。

```razor
@code {
    private int count;
    // サーバー上の状態を更新し、接続中のブラウザーへ結果を反映する。
    private void Increment() => count++;
}
```

ブラウザーでDocumentの取得を観察すると、最初のHTML取得が見えます。ここでHTMLが表示できるという事実だけでは、対話型接続が確立したか、サーバーC#がクリックを処理したかまでは分かりません。カウンターを押して値が変わること、必要ならVisual Studioで`Increment()`にブレークポイントを置いて実行場所を追うことが必要です。WinFormsのイベントハンドラーは通常同じPCで動きますが、Interactive Serverではブラウザーとサーバーの間の通信が必要です。通信が切れたときの再接続、circuit、事前レンダリングによる初期化回数については後続の学習順015〜020で詳しく扱います。このステップの`count`はコンポーネントの状態であって、DBに保存された操作履歴でも個人の監査記録でもありません。

## 4. ブラウザー・サーバー・状態の境界

今回扱う値や表示は、画面が提供する学習用の例です。Interactive ServerのC#はサーバー上で動き、ブラウザーがHTMLを描画します。初期のHTTP応答で文字が見えることと、対話型イベントに対応するC#が実行できることは同じ確認ではありません。ブラウザーのNetwork、Consoleと、Webサーバーの実行ログやVisual Studioのブレークポイントは、それぞれ調べられる範囲が異なります。

また、画面を開けたことをDB保存の完了や業務権限の成立と取り違えないでください。実業務では利用者のAD本人認証に加え、既存DB台帳を正本とした権限判定、必要な操作記録が別に必要です。ここで扱うサンプルには、それらの業務処理を追加したとは主張しません。

## 5. 順番に操作して確かめる

1. `/learn/009`を開き、比較表と操作回数0を確認する。
2. Chrome開発者ツールのNetworkでDocumentの要求を調べ、最初のHTMLを取得したことを確認する。
3. 対話型接続の準備が済んだ後、「サーバー側の回数を増やす」を2回押す。
4. 画面に2が表示されることを確認し、必要に応じて`Increment()`のブレークポイントでサーバー側実行を確認する。
5. ページを再読み込みし、状態が初期化されることを観察する。
6. Static SSR実習ページを開き、C#ボタンを押しても0から変わらないことを確認する。
7. GETフォームを送信してDocumentが再取得され、`?count=1`に変わることを観察する。
8. `?count=-1` のHTTP 400拒否を確認し、Interactive Serverとの違いを整理する。

画面の文言を変更する演習では、保存後に表示が更新されたことだけを確認するのではなく、**どのRazor/C#の行を変えたことで、どのHTMLや表示状態が変わったか**を説明してください。学習用データには架空の値を使用し、実在する利用者の名前・認証情報は入力しません。

## 6. 症状別の切り分け表

| 症状 | 確認箇所 | 判断と対応 |
| --- | --- | --- |
| HTMLは表示されるがクリックが動かない | Interactive Serverの接続とConsole | 最初のHTML取得成功とは別の故障として扱う |
| クリック前の数字が想定と違う | 現在の対話状態と再読込 | 以前の操作履歴をDB保存と混同しない |
| NetworkでDocumentしか見えない | 通信フィルターと接続状態 | Document以外の対話型通信も確認 |
| Static SSR側でC#イベントが動かない | 隔離したRazorComponentResultの静的応答 | HTMLのGETフォームとは別の仕組みである |
| GETの数値が範囲外 | `?count=-1` | HTTP 400で拒否する |
| サーバーで例外が出た | C#ブレークポイント・サーバーログ | ブラウザーConsoleだけで結論しない |

診断の順序は、まずサーバーへ接続できるか、次にURLとHTMLが一致するか、その後に対話型接続とC#イベントが働くか、最後に表示・値の仕様が正しいかという順番です。ChromeのConsoleに表示がないからといって、サーバー側のC#が正常だったとは判断できません。逆に、意図的に無効化したボタンの動作を通信障害と決め付けることも避けます。

## 7. 演習問題と詳しい解答

### 問題1

Static SSRで表示された普通のボタンを押すと、必ず`@onclick`が動きますか。

**解答と根拠：** いいえ。Static SSR単独ではBlazorの対話型C#イベントを有効にしません。HTMLフォーム送信等でサーバーへ新しい要求を送る設計とは分けて考えます。

### 問題2

HTMLがすぐ表示されればInteractive Serverの接続も完了していますか。

**解答と根拠：** 断定できません。初期HTMLの応答と対話型接続の成立は別段階だからです。

### 問題3

カウンター2は監査された操作履歴ですか。

**解答と根拠：** 違います。表示用のフィールド値を増やしているだけです。永続保存、本人識別、監査イベントの記録はありません。

## 8. このステップの完了確認

- [ ] 学習用URLを直接開き、画面の日本語表示を実装と照合した。
- [ ] WinFormsの画面イベントと、ブラウザー・サーバーの役割の違いを説明した。
- [ ] 上の2つのコード抜粋について、表示側と実行側の役割を説明した。
- [ ] 正常な操作と、故障・想定された拒否を区別できた。
- [ ] この学習用画面では業務DB・認証・監査を実証していないと説明できた。

このステップの新規URLは`/learn/009`です。次は学習順010のボタン画面へ進み、イベント処理と表示側の無効化、サーバー側の防御を確認します。

次：[学習順010：ボタンのイベント](../../samples/step04.md)
