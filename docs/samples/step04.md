# ステップ04：イベント処理（ボタンとカウンター）

**学習テーマ：** `@onclick` とC#メソッドによる画面状態の変更

**実装ソース：** [EventPage.razor](../../src/PgBlazorSamples.Web/Components/Samples/Step04/EventPage.razor)

## 前提

Step01〜03でRazor画面とC#の値表示を学習済みであること。AD・PostgreSQL・MudBlazorは不要です。

## 動かし方

1. `PgBlazorSamples.slnx`をVisual Studio 2026で開き、Webプロジェクトを起動します。
2. トップページの「ステップ04を学ぶ」または`/learn/04`を開きます。
3. 初期値「現在の数値：0」を確認します。
4. **「1増やす」**を2回押すと「2」になります。
5. **「1減らす」**を1回押すと「1」になります。
6. **「0に戻す」**を押すと「0」になります。
7. 「0」のときに「1減らす」が無効化され、マイナスにならないことを確認します。

## ソースの読み方

実コードから抜粋します。

```razor
<button type="button" @onclick="Increment">1増やす</button>
<button type="button" @onclick="Decrement" disabled="@(count == 0)">1減らす</button>
<button type="button" class="secondary-button" @onclick="Reset">0に戻す</button>
```

```csharp
// ボタン操作に対応するイベント処理です。
private void Increment() => count++;

// 演習を何度でも試せるよう、初期値に戻します。
private void Reset() => count = 0;
```

- `@onclick`はBlazorが処理するクリックイベントです。操作後にコンポーネントが再描画されます。
- `count`はコンポーネントに保持した整数の状態です。ほかの利用者やアプリのDBと共有しません。
- `disabled`は画面上の無効化です。実際の`Decrement`メソッドにも「0以上」という条件があり、UIだけに頼っていません。
- 表示には`role="status"`と`aria-live="polite"`を指定し、変化の通知を補助しています。

## 演習

- 1回のクリックで2増えるボタンを新設してください。
- 初期値を5にする場合、説明文・ボタンの動作・教材をどう変えるか考えてください。
- `Increment`を呼び出す前後で画面表示がどう変化するか、デバッガーで観察してください。

## よくある間違い

- `@onclick="Increment()"`とメソッド呼び出しを書くのではなく、イベントハンドラを渡す構成に注意します。
- `type="button"`を指定し、将来フォーム内で使う際に不要な送信を起こさないようにします。
- 画面単位の状態は再読み込みや接続終了で失われます。永続化はDB学習以降です。

## 完了の判定

- [ ] 増やす・減らす・初期化を操作できる
- [ ] 0未満にならないことを確認できる
- [ ] `@onclick`とC#メソッドのつながりを説明できる

次：[ステップ05](step05.md)
