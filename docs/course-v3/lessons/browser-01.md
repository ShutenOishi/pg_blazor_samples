# 学習順041：JavaScript interopの利用判断

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Samples/Browser/Browser01Page.razor)。画面の正規URLは `/learn/041` です。

## 到達目標とこの画面の位置
第4段階の最後の5本では、業務画面の見た目だけでなく、ブラウザーとサーバーの間を往復する機能を扱います。このステップでは、JavaScript interop（JS相互運用）を使う必要がある処理を見分けます。Blazor Interactive ServerではRazorのイベント処理がサーバー側で実行される一方、documentやDOM、ダウンロードなどのブラウザーAPIはブラウザーの中にあります。画面を作るために何でもJavaScriptへ置き換える必要はありません。

到達条件は、(1) C#の状態更新とDOM操作の実行場所の違い、(2) IJSRuntimeとJavaScriptモジュールの呼び出し、(3) 切断と例外、(4) 必要な後始末、の4点を説明できることです。ここでは画面上の回数を増やし、HTML要素のlang属性を読みます。業務DBやActive Directoryには接続しません。

## 実装と実行環境
Visual Studio 2026で.NET 10のWebアプリを起動し、学習順041のURL /learn/041 にアクセスします。サーバーでRazorコンポーネント Browser01Page.razor が動き、ブラウザーから wwwroot/js/browser-learning.js をモジュールとして読み込みます。表示される日本語はアプリ独自のメッセージです。JavaScriptモジュールは認証情報を読みません。事前レンダリング段階ではブラウザー側のDOMはまだ呼び出せないため、実装はユーザーのクリックに続く対話イベントでJSを実行しています。

| WinForms＋Dapperでの経験 | この画面での対応 | 判断する理由 |
| --- | --- | --- |
| Button.Clickで画面の回数を増やす | Razorの@onclickとC#フィールド | サーバー状態の変更だけなのでJSは不要 |
| Formのコントロールプロパティを読む | document.documentElement.langをJSで読む | DOMはブラウザー側にある |
| DLLのメソッドを直接呼ぶ | IJSRuntimeとimportしたモジュールを呼ぶ | 回線越しの非同期呼び出しになる |
| FormClosingで資源を解放する | IAsyncDisposableでIJSObjectReferenceを解放する | ページ離脱と接続切断を考慮する |
| SQLの権限をサーバーで確認する | DB/外部台帳で毎操作認可する | JSで認可を代行しない |

## 実際のRazorとC#を読む
次は教材の実装の中心部分です。ボタンにブラウザー固有APIを直接書いているわけではありません。回数を増やす操作はC#だけで完結します。

```razor
@using Microsoft.JSInterop
@inject IJSRuntime JS
@implements IAsyncDisposable

<button type="button" @onclick="Increase">C#だけで回数を増やす</button>
<p>操作回数：@count</p>
<button type="button" @onclick="ReadLanguage">JavaScriptでHTMLの言語設定を読む</button>
<p role="status">@message</p>

@code {
    private int count;
    private string message = "まだ取得していません。";
    private IJSObjectReference? module;
    private void Increase() => count++;
}
```

@onclickで発生したイベントはサーバーに届き、countを書き換えた結果が再描画で伝えられます。WinFormsでコントロールのTextを直接書き換える方法との違いは、サーバーとブラウザーが別の場所にあることです。回線の往復があるため、クリックを大量に連続させる高頻度処理は実際の利用状況で検証が必要です。

ブラウザーのlang属性を確認する処理では、次のようにJSモジュールを最初の実行時に読み込み、以降は同じ参照を再利用します。表示するメッセージは内部例外文ではなく、利用者に必要な説明だけです。

```csharp
private async Task ReadLanguage()
{
    try
    {
        module ??= await JS.InvokeAsync<IJSObjectReference>(
            "import", "./js/browser-learning.js");
        var language = await module.InvokeAsync<string>("readLanguage");
        message = "HTMLの言語設定：" + language;
    }
    catch (JSDisconnectedException)
    {
        message = "サーバーとの接続が切れました。再接続してください。";
    }
    catch (JSException)
    {
        message = "ブラウザーの情報を取得できませんでした。";
    }
}
```

実際のモジュールには次の関数があります。読み取るのはHTMLのlang属性だけであり、ユーザーIDやDBの権限を参照していません。

```javascript
// ブラウザーのHTML言語設定のみを読む。
export function readLanguage() {
    return document.documentElement.lang || "未設定";
}
```

呼び出しはIJSRuntimeを通る非同期通信であり、C#の同期メソッドから直接documentを扱うものではありません。JSDisconnectedExceptionはサーバーとの接続が切れた場合、JSExceptionはJSモジュール内の実行失敗などを表します。NetworkでJSモジュールの読込みが成功していても、回線切断があればその後の呼び出しは失敗し得ます。

## モジュールの寿命と動作順
1. /learn/041を開いた時点では、操作回数0、言語の表示は「まだ取得していません。」です。
2. 「C#だけで回数を増やす」を押すと、サーバー側のcountが増え、画面の数値が1になります。JSモジュールを使う必要はありません。
3. 「JavaScriptでHTMLの言語設定を読む」を押すと、初回だけimportし、readLanguageがDOMを確認します。
4. HTMLのlangがjaなら「HTMLの言語設定：ja」と表示されます。設定によっては異なる値または「未設定」になります。
5. ページの破棄ではIAsyncDisposableでモジュールを解放します。切断中はDisposeAsync自体がJSDisconnectedExceptionになる場合があるため、実装はその例外を受け止めます。

```csharp
public async ValueTask DisposeAsync()
{
    if (module is null) return;
    try { await module.DisposeAsync(); }
    catch (JSDisconnectedException)
    {
        // 切断後はブラウザー資源の解放要求が届かない場合がある。
    }
}
```

この画面ではOnAfterRenderAsyncからDOMを読む設計にしていません。事前レンダリング中にJS呼び出しを始めると利用不能になるためです。初期表示でどうしてもブラウザー情報が必要なら、対話開始後のOnAfterRenderAsync(firstRender)で呼ぶ設計を別途検討します。

## 失敗を切り分ける
| 症状 | 最初に見る場所 | 対処・判断 |
| --- | --- | --- |
| 数値も変わらない | Interactive Server接続、ブラウザーConsole、サーバーログ | JS以前にBlazorイベントが届いているか確認 |
| 回数は増えるがlangを取得できない | Networkでbrowser-learning.js、Consoleのimportエラー | モジュールパスと静的ファイル配信を調べる |
| 切断メッセージが出る | 回線・circuit再接続状態 | 再接続を試し、要求を自動で無制限再送しない |
| langが想定と違う | 開発者ツールのhtml要素のlang属性 | ブラウザーの表示言語とは別の属性であることを確認 |
| ページ離脱後に例外ログが出る | DisposeAsyncと切断の時点 | 後始末中の切断を区別し、業務処理の失敗を隠さない |

ブラウザーが保持する値をサーバーへ渡す時点で、その値は利用者側で変更可能な入力として扱います。lang属性は表示確認用であり、認可や監査記録の本人識別には使えません。Windows統合認証で本人を確認し、登録済みのDB/外部台帳で業務権限を毎操作判定する設計を、JavaScriptで省略してはいけません。

## 演習と詳しい解答
**問1：** C#で増減できる回数を、document.querySelectorで書き換える方法に統一するべきですか。**解答：** 統一しません。この画面の回数はBlazorのコンポーネント状態であり、@onclickと再描画で足ります。JS側だけを変更するとサーバーが持つcountとDOMが食い違う場合があります。ブラウザーAPIが必要な部分だけをJSへ分離します。

**問2：** 最初のHTMLをサーバーで描く段階でJSからlangを読むコードを書いたら、なぜ失敗する可能性がありますか。**解答：** 事前レンダリングではブラウザーとの対話接続がまだ成立していません。IJSRuntimeによるDOM操作は、対話を開始した後のユーザーイベントや適切な描画後処理で実行します。サーバーの処理とブラウザーの処理場所を区別します。

**問3：** 一度importできたJSモジュールは、回線切断後も必ず使えますか。**解答：** 保証されません。呼び出し自体が非同期の接続を必要とし、切断時にはJSDisconnectedExceptionになり得ます。画面に一般的な再接続案内を出し、内部情報を公開しない設計にします。

**問4：** DOMに表示した「部門A」をJSで読み、それを認可の根拠にできますか。**解答：** できません。DOMやJSの値は利用者が変更できます。サーバーで認証済み本人を確認したうえで、既存台帳を読み、操作対象・部門・有効期限を業務ごとに検査します。

## 追加実習：JS例外・参照解放と実証範囲
画面の「学習用JS例外を発生させる」は、実際の `browser-learning.js` の `throwLearningError` が例外を投げ、C#の `JSException` 捕捉によって内部の例外文を公開せず一般的な案内を表示します。「JSモジュール参照を解放する」では保持していた `IJSObjectReference` に `DisposeAsync` を呼び、参照の解放に成功したか、切断やJS失敗で完了が確認できないかを表示します。再度言語設定を読むとimportをやり直します。この操作だけでESモジュール本体のアンロードやブラウザーのメモリ回収が証明されたわけではありません。

[JS実装](../../../src/PgBlazorSamples.Web/wwwroot/js/browser-learning.js)、[失敗メッセージの分類](../../../src/PgBlazorSamples.Web/Services/Learning/L041InteropMessages.cs)、[xUnit検査](../../../tests/PgBlazorSamples.UnitTests/L041InteropMessagesTests.cs)、[実Chrome操作試験](../../../scripts/browser-smoke.cjs)をRazorと併せて確認します。xUnitは`JSDisconnectedException`と`JSException`の分類が異なることを確認しますが、テスト内で生成した切断例外は実WebSocket切断の証明にはなりません。学習順041自身の隔離WebSocket切断実習には、「切断捕捉の待機処理を開始する」ボタンを使用します。通常の画面操作だけでは切断しません。専用Chrome試験`scripts/browser-circuit-smoke.cjs`が学習順015とは別のBrowserContextと独立した041 WebSocketを作り、接続中にJSモジュールを読み、待機表示を確認してからWebSocketを実切断します。C#側の遅延後のJS呼び出しで`JSDisconnectedException`を捕捉した場合だけ、例外内部情報を含まない識別子`L041_REAL_DISCONNECT_CAUGHT`をサーバーログへ出力します。Chromeが単に切断モーダルを見ただけでは受入にせず、そのサーバーログ識別子を別途照合します。実際に捕捉されずタイムアウトした場合はNOT_TESTEDのまま維持し、実接続・C#例外を人工的に作った試験で代用しません。2026-10-28の試行では、HEAD 90b4f13のアプリRun40638392110で専用041のWebSocket切断とブラウザー再接続案内まで確認したものの、20秒の監視期間内にC#捕捉ログが成立しませんでした。ブラウザー側の切断とC#例外捕捉は別条件です。次のHEAD fc17ec87 のアプリRun40638970262は全CI成功ですが、隔離041の実WebSocket切断時の診断は `invokeStarted=true`・`invokeCompleted=true`・`jsError=false`・`otherError=false` であり、C#例外は発生していません。この結果から再接続方式やタイミングを断定せず、WebSocketだけの遮断ではJS呼び出し成功を防げなかった事実として記録します。後続の診断では学習順041専用BrowserContextをオフラインにしてHTTP代替経路も遮断し、切断捕捉の有無を独立検査します。捕捉できない場合はNOT_TESTEDを明示して、実証項目だけを未受入に残します。実WebSocket切断と切断時の`JSDisconnectedException`捕捉は独立に確認し、未実証の経路をPASS扱いしません。切断からの復帰は本番の業務認可成立を意味しません。ここではDB・AD・IIS・FDW・実利用者権限の試験は行いません。

## 完了確認と次の学習
- [ ] C#だけのクリックとJSが必要なクリックをそれぞれ動かした。
- [ ] NetworkとConsoleでモジュールの読み込み先を調べた。
- [ ] 回線切断時の扱いとIAsyncDisposableの理由を説明できる。
- [ ] JSの値が業務認可の根拠にならないことを説明できる。

次の[学習順042：ファイルアップロードと入力制約](browser-02.md)では、ブラウザーが選んだファイルをサーバーで扱うときの拒否条件を確認します。