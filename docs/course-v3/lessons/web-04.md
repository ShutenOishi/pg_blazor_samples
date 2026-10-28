# 学習順004：開発者ツールでDOM・Network・Consoleを見る

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L004DomNetworkConsolePage.razor)。画面の正規URLは `/learn/004` です。

## 到達目標

Webアプリの問題を調べるとき、WinFormsと同じようにC#のブレークポイントだけを使っても、HTMLの構造やHTTPの応答、ブラウザー側のエラーは調べきれません。このステップではブラウザーの開発者ツールを使い、**DOM（要素）・Network（通信）・Console（診断）**を見分けられるようにします。

操作するのは`/learn/004`です。ボタンによってサーバーの`expanded`が変わり、ブラウザーのDOMへ説明文が追加されたり削除されたりします。この動きをElementsで実際に観察します。

## 1. 調べたい内容とツールの対応

| 調べたいこと | 開発者ツールの場所 | 見る項目 |
| --- | --- | --- |
| 実際にHTML要素が存在するか | Elements（要素） | 要素名、`id`、テキスト、属性 |
| 入力欄のラベルが対応しているか | Elements | `label for`と`input id` |
| どのページを要求したか | Network（ネットワーク） | URL、Method、Type、Status |
| サーバー接続が切れていないか | Networkとアプリの接続表示 | WebSocket等の接続状況 |
| ブラウザーでエラーが起きていないか | Console（コンソール） | エラーと発生場所 |
| C#のメソッドが呼び出されたか | Visual Studioのデバッガー | サーバー側のブレークポイント |

WinFormsでデザイン画面を見ることと、稼働中に`Control.Controls`の内容を調べることが違うように、Webでも**Razorの元ソース**と**ブラウザーが現在保持するDOM**は同一ではありません。Razorの条件分岐によって表示される要素は、条件が変化するとDOMにも変化が現れます。

## 2. 実際のRazorコードを読む

`L004DomNetworkConsolePage.razor`には、次の処理があります。追加表示する段落にはテストやElementsで見つけやすい`id`が付けられています。

```razor
<button type="button" @onclick="ToggleDetails">
    追加の説明を表示・非表示
</button>
<p data-testid="web04-state">
    状態：@(expanded ? "表示中" : "非表示")
</p>

@if (expanded)
{
    <p id="web04-added" data-testid="web04-added">
        この段落はRazorの条件分岐でDOMに追加されました。
    </p>
}

@code {
    private bool expanded;
    private void ToggleDetails() => expanded = !expanded;
}
```

`private bool expanded;`の初期値は`false`なので、初期画面には`id="web04-added"`の段落がありません。ボタンを1回クリックすると、`ToggleDetails()`が`expanded`を`true`に変更し、`@if (expanded)`が成立します。そこでBlazorの描画差分がブラウザーへ反映され、要素が追加されます。

2回目のクリックでは`expanded`が`false`となり、追加された段落はDOMから削除されます。`display: none`で常に隠している構造ではなく、条件に応じてRazorがその要素を生成するかどうか決めています。

## 3. Elementsでの実習

1. `/learn/004`を開きます。初期表示は「状態：非表示」です。
2. Chrome/Edgeの開発者ツールを開き、Elements（要素）を選びます。
3. 検索機能で`web04-added`を探します。最初は見つからないことを確認します。
4. ブラウザー画面へ戻り「追加の説明を表示・非表示」を1回押します。
5. 表示は「状態：表示中」になり、説明文が現れます。
6. Elementsで`<p id="web04-added">`を探し、DOMの中に存在することを確認します。
7. もう一度ボタンを押して「非表示」に戻し、Elementsで要素がなくなったことを確認します。

`data-testid`は自動試験で画面を特定するための属性です。HTML上に存在しても、それ自体が認証や権限を制御する仕組みではありません。

## 4. Networkでの実習

学習順002と同じ要領でNetworkを開きます。記録を消去してページを再読み込みし、最初のDocument取得のURL・Method・Statusを確認します。

次にWeb04のボタンを操作します。Interactive Serverでは、クリック時に必ず新しいDocument取得が増えるわけではありません。Blazorの対話型接続を使うためです。Networkで通信を追う際は、**初期HTTP取得・JavaScript等のリソース取得・対話型接続**を区別します。

ブラウザーの版やネットワーク環境によって、WebSocketの表示場所や代替トランスポートは異なります。そのため、WebSocketが見えない場合でも、それだけで異常とは決めず、画面の更新とサーバーログを併せて判断します。

## 5. Consoleの役割

Consoleにはブラウザー側のJavaScriptエラーや診断メッセージが表示されます。ただし、このページでは意図的なJavaScript例外を発生させません。**Consoleにエラーがないことは、サーバーのC#処理がすべて成功した証明ではありません**。逆に、サーバーのC#例外はサーバーログなどを確認する必要があります。

原因が分からないときは、次の順で確認すると絞り込みやすくなります。

| 症状 | Elements | Network | Console・サーバーログ |
| --- | --- | --- | --- |
| 説明文が現れない | `web04-added`が生成されたか | 接続が継続しているか | サーバーでイベントが実行されたか |
| ページ全体が表示されない | DOMが構築されたか | DocumentのURL/Status | サーバーの起動・ルーティング |
| 画面に一部だけ文字化け | `lang`や文字列の表示 | Content-Type等 | サーバーの日本語設定 |
| ボタンで通信が切れる | 最後に反映されたDOM | 接続の終了・再接続 | Consoleの接続エラー、サーバー例外 |
| 不審なHTMLタグがある | 意図した要素か | 取得したページの内容 | 未信頼文字列の描画方法 |

開発者ツールのNetworkに含まれるCookie・Authorizationヘッダー・認証関連情報などを、スクリーンショットや問題報告として不用意に公開してはいけません。

## 6. 参考：画面の状態をC#で追う方法

`ToggleDetails()`はブラウザー側のJavaScriptではなく、サーバー側のC#です。コードをもう一度簡略化すると次のようになります。

```csharp
private bool expanded;

private void ToggleDetails()
{
    expanded = !expanded;
    // メソッドの戻り後、Blazorが変更後の表示を反映する。
}
```

学習順005では、このC#メソッドのようなサーバーコードにブレークポイントを置いて追跡します。Elementsは表示結果を確認する場所、Visual Studioはサーバー処理を調べる場所という分担です。

## 7. 演習と解答

**問1：** `expanded`が`false`のとき、`web04-added`の段落はDOMに存在しますか。

**解答：** 存在しません。`@if (expanded)`が成立するときだけ段落が生成されます。

**問2：** サーバーC#が例外を投げてもConsoleに何も表示されなければ問題ありませんか。

**解答：** いいえ。サーバー側のログとデバッガーも確認します。

**問3：** Elementsにボタンが見えていれば、権限のない人も業務操作を実行できると判断できますか。

**解答：** できません。業務操作の認可はサーバー側で行う必要があります。HTMLの有無は権限の証明になりません。

**問4：** ボタンのクリックごとにNetworkのDocument GETが増えない理由を説明してください。

**解答：** Blazor Interactive Serverが対話型接続でイベントと表示更新を処理し、ページ全体のHTMLを毎回GETし直さないためです。

## 8. 自動試験の確認

[Chrome実操作試験](../../../scripts/browser-smoke.cjs)では、正規URL `/learn/004`を開いた際のDocument GET 200、初期状態に`web04-added`がないこと、ボタン操作後のDOM追加、再操作後のDOM削除を順に確認します。さらにクリック中に新しいDocumentナビゲーションが起きず、未処理のブラウザーページ例外が発生しないことを確認します。

これは**今回の画面操作がページ全体の再取得を必要としない**ことを示す試験です。すべてのNetworkリクエストが発生しない、あるいはすべてのサーバーエラーが存在しないと保証する試験ではありません。対話型接続の詳細やConsoleの警告・診断情報は、上記の手順で開発者ツールから引き続き確認します。

## 完了チェック

- [ ] Elementsで`web04-added`の出現と削除を観察した。
- [ ] NetworkでDocument要求と対話型通信を区別した。
- [ ] Consoleとサーバーログで調べられることの違いを説明できた。
- [ ] 未信頼のHTMLや認証ヘッダーを不用意に公開しない理由を理解した。

次は[学習順005：Visual StudioでのWebデバッグ](web-05.md)です。
