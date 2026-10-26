# 第1組の復習：Webの処理場所・通信・画面表示

## 復習の目的

学習順001〜005で新しい概念を一度に覚えようとすると、HTMLの表示とC#イベント、HTTPと対話型接続、ブラウザーの開発者ツールとVisual Studioのデバッグを混同しやすくなります。この復習では、これらを**ひとつのボタン操作が完了するまでの順番**に結び付けて説明できるようにします。

## 1. 5ステップの対応表

| 順番 | 学んだ概念 | 確認した操作 | 関連するコード |
| --- | --- | --- | --- |
| 001 | 画面とC#の実行場所 | サーバー側カウンターを2回増やす | `@onclick="Increment"` |
| 002 | URL・HTTP要求と応答 | NetworkでDocumentのGETを調べる | `@page`、`Navigation.Uri` |
| 003 | HTML・CSS・フォーム | 入力欄を変更してプレビューを確かめる | `label for`、`@bind:event="oninput"` |
| 004 | DOM・Network・Console | 段落の生成と削除をElementsで見る | `@if (expanded)` |
| 005 | デバッグとWebサーバー | C#ブレークポイントで停止 | `BreakpointSample()` |

## 2. WinFormsとWebの違い

| 比較対象 | WinForms＋Dapper | Blazor Interactive Server |
| --- | --- | --- |
| 画面の場所 | PCのウィンドウ | ブラウザーのHTML |
| 入力の入口 | フォームのコントロール | HTMLの`input`や`button` |
| C#イベント | 通常は同一PCのプロセス | Webサーバー上のC# |
| DBへ接続する場所 | クライアントから直接接続する構成もある | サーバー側の認可済みサービス |
| 画面の状態 | フォームのフィールド | サーバー側のコンポーネント状態など |
| 通信 | DB接続やAPIなどを選ぶ | ページ取得のHTTPと対話型接続 |
| 主な診断 | Visual Studioのデバッグ | Visual Studioと開発者ツールを併用 |

## 3. コードを通して処理を説明する

実際の教材には、次の3種類の記述が登場しました。

```razor
@page "/learn/web-01"
<button type="button" @onclick="Increment">サーバー側で1増やす</button>

@code {
    private int count;
    private void Increment() => count++;
}
```

```razor
<label for="web03-name">練習用の名前</label>
<input id="web03-name" type="text" maxlength="30"
       @bind="name" @bind:event="oninput" />
```

```razor
@if (expanded)
{
    <p id="web04-added">この段落はRazorの条件分岐でDOMに追加されました。</p>
}
```

1つ目はイベントでサーバー上の値を変えます。2つ目はHTML入力とC#の値を関連付けます。3つ目はC#の条件でHTML要素を表示するか決定します。3つとも、ブラウザーに表示する要素とC#の処理が協調しますが、担当する役割は異なります。

## 4. 演習：症状から確認箇所を選ぶ

**問題A：** URLを入力してもページが見つかりません。最初にどこを確認しますか。

**解答A：** NetworkのDocumentのURLとStatusを調べ、404なら`@page`のパスとURLを照合します。アプリが起動していない場合はKestrelのログも確認します。

**問題B：** Web01のボタンを押しても表示が変わりません。何を調べますか。

**解答B：** 対話型接続の状態をNetworkで確認し、Visual StudioのサーバーC#で`Increment()`が実行されたか調べます。Consoleだけで判断しません。

**問題C：** Web03でラベルの文字をクリックしても入力欄にフォーカスできません。どこを確認しますか。

**解答C：** Elementsで`label for="web03-name"`と`input id="web03-name"`の対応を確認します。

**問題D：** Web04で説明文を非表示にしたとき、その要素は単にCSSで隠れていますか。

**解答D：** いいえ。この教材は`@if (expanded)`によって段落を作るか決めるため、条件がfalseのときはDOMに存在しません。

**問題E：** Web05でC#へブレークポイントを置きました。ChromeのJavaScriptをデバッグしていることになりますか。

**解答E：** なりません。今回はVisual Studioでサーバー上の`BreakpointSample()`を停止させています。ブラウザーは表示側です。

## 5. 実践問題：ボタンを押したときの流れを説明する

次の文章の空欄を埋めてください。

1. ブラウザーは最初にWebサーバーへ（A）要求を送ってページを取得する。
2. 利用者がボタンを押すと、Interactive Serverの（B）を通じて操作が伝わる。
3. サーバー上の（C）メソッドがC#の値を変更する。
4. 変更された表示をブラウザーの（D）が描画する。
5. C#の内部実行を追う場合は（E）のブレークポイントを使う。

**解答：** AはHTTP GET、Bは対話型接続、Cは`Increment()`、DはDOM、EはVisual Studioです。

**注意：** これは学習用のカウンター操作です。実際の業務でDBを読むときは、本人認証・既存台帳の部署権限・監査をサーバー側で毎操作確認する必要があります。

## 6. 自分で確認する最終チェック

- [ ] Web01の2回クリックと再読込後の状態を説明できる。
- [ ] Web02でDocumentのGETと応答Statusを確認できた。
- [ ] Web03でHTMLのラベル・入力とCSSの役割を分けて説明できた。
- [ ] Web04でDOMに現れる要素とC#の条件を照合できた。
- [ ] Web05でブレークポイントを使いサーバーC#を追えた。
- [ ] ブラウザーで表示されることとDB保存・認可の成立が別であると説明できた。

## 次の学習

[学習順006（旧Step01）](../../samples/step01.md)では、Razorコンポーネントの構造と最初の画面を詳しく学びます。この第1組の復習で曖昧な箇所があれば、対応するステップへ戻って操作をやり直してください。
