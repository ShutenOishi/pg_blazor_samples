# 学習順003：HTML・CSS・フォームの基礎

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L003HtmlCssFormPage.razor)、[CSS](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage01/L003HtmlCssFormPage.razor.css)、[Chrome試験](../../../scripts/browser-smoke.cjs)。画面の正規URLは `/learn/003` です。

## 到達目標

Web画面の見た目や入力欄はWinFormsの`TextBox`や`Label`のようにC#だけで宣言されるのではなく、**HTMLが要素の意味を定義し、CSSが表示方法を指定**します。このステップでは、入力欄へ文字を入力してプレビューへ反映し、`label`と`input`の結び付け、Razorの双方向バインディング、CSSのスタイル指定を説明できることを目指します。

対象ファイルは`L003HtmlCssFormPage.razor`と`L003HtmlCssFormPage.razor.css`です。実習では架空の名前だけを扱い、個人情報やDBは使用しません。検証の自動試験は`scripts/browser-smoke.cjs`で確認します。

## 1. WinFormsコントロールとの対応

| 目的 | WinFormsの例 | このページのHTML/Razor |
| --- | --- | --- |
| 項目名を表示 | `Label` | `<label>` |
| 文字を入力 | `TextBox` | `<input type="text">` |
| 入力値をC#へ渡す | `TextChanged`等 | `@bind`と`@bind:event` |
| 文字列を表示 | `label.Text` | Razorの`@name` |
| 枠線・余白を変更 | `BorderStyle`等 | CSSの`border`、`padding` |
| キーボード操作 | `TabIndex`等 | HTMLの要素、ラベル、フォーカスCSS |

Webでは`<div>`を並べるだけでなく、見出しには`<h1>`や`<h2>`、入力のラベルには`<label>`を使います。要素の意味を正しく指定すると、読み上げ機能やキーボード操作にも役立ちます。

## 2. Razor画面の実際のコード

以下は`L003HtmlCssFormPage.razor`の主要部分です。文字入力と結果表示は同じC#フィールド`name`を使用しています。

```razor
<section class="lesson-details web-foundation-card">
    <h2>ラベルと入力欄を使う</h2>
    <label for="web03-name">練習用の名前</label>
    <input id="web03-name" type="text" maxlength="30"
           @bind="name" @bind:event="oninput" @bind:after="ResetValidation" />
    <p role="status" data-testid="web03-preview">
        入力内容：@(string.IsNullOrWhiteSpace(name) ? "未入力" : name)
    </p>
    <button type="button" @onclick="ValidateName">サーバー側で入力を検証する</button>
    <p role="status" aria-live="polite" data-testid="web03-validation">@validationMessage</p>
</section>

@code {
    private string name = "";
    private string validationMessage = "未検証";

    private void ResetValidation() => validationMessage = "未検証";

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            validationMessage = "未入力のため拒否しました。";
        }
        else if (name.Length > 30)
        {
            validationMessage = "30文字を超えたため拒否しました。";
        }
        else
        {
            validationMessage = "入力を受け付けました（保存はしていません）。";
        }
    }
}
```

`for="web03-name"`と`id="web03-name"`が一致しているため、ラベルをクリックすると対応する入力欄を操作できます。`maxlength="30"`はHTML側の入力文字数制限です。開発者ツールで属性を取り除くこともできるため、**C#側でも空欄と30文字超を別々に拒否します**。この教材は結果の表示までで、名前をDBやファイルに保存しません。業務の入力検証・認可・保存処理とは区別してください。

`@bind="name"`はHTMLの入力とC#のフィールドを関連付けます。入力後に`@bind:after="ResetValidation"`が呼ばれ、過去の検証結果を「未検証」に戻します。`@bind:event="oninput"`を指定しているので、このページでは入力のたびに`name`へ反映されます。既定の`onchange`とのタイミングの違いにも注意してください。`string.IsNullOrWhiteSpace(name)`は空文字や空白だけの入力を「未入力」と表示するためのC#式です。

**Razorが文字列を表示する際はHTMLエンコードします。** たとえば入力した`<b>練習</b>`は通常の文字列として表示し、実際の太字タグにはしません。機密情報の入力を許可するという意味ではなく、HTMLとして解釈させない基本的な防御です。

### C#側の検証を実際に動かす

「サーバー側で入力を検証する」を押すと、`ValidateName()`がサーバー側で実行されます。通常の入力は受け付け、空欄・空白だけの入力、30文字超の入力は拒否します。`maxlength`を一時的に外して31文字入力した場合も、C#の`name.Length > 30`で拒否します。`name.Length`は.NET文字列のUTF-16コード単位で計数するため、文字の種類によって見た目の文字数とは異なる場合があります。ここでの学習条件は30コード単位までです。

受け付けた場合も保存はしません。表示された「受け付けました」という結果だけで、業務DBへ登録されたとは判断できません。

## 3. CSSの実コードとスコープ

`L003HtmlCssFormPage.razor.css`には、このコンポーネント向けのCSSを記述しています。拡大表示やキーボード利用者を想定し、入力欄の幅とフォーカスの見た目を指定しています。

```css
/* HTMLの意味はRazorで定義し、色や余白はCSS側に分ける。 */
.web-foundation-card {
    border: 2px solid #628aa1;
    border-radius: 0.5rem;
    padding: 1rem;
}
.web-foundation-card input {
    display: block;
    width: min(100%, 27rem);
    font: inherit;
    padding: 0.6rem;
    margin-block: 0.5rem;
}
.web-foundation-card input:focus-visible {
    outline: 3px solid #275c84;
    outline-offset: 2px;
}
```

`L003HtmlCssFormPage.razor.css`はBlazorのCSS分離機能を利用したファイルです。クラス名は`web-foundation-card`で、同じ名前をHTML要素の`class`に記述しています。CSSはHTMLの意味やDBの保存内容を変更しません。

| CSSの指定 | 役割 | 観察ポイント |
| --- | --- | --- |
| `border` | 枠線を描画する | 入力部分を囲む線 |
| `border-radius` | 角を丸める | 枠の四隅 |
| `padding` | 枠線と内容の間隔 | 読みやすさ |
| `width: min(100%, 27rem)` | 利用可能な幅を超えない | 拡大・狭い画面 |
| `font: inherit` | 周囲の文字設定を引き継ぐ | 文字サイズ・フォント |
| `:focus-visible` | キーボード操作のフォーカスを表示 | Tabキーで入力欄へ移動 |

見た目の調整を始めるときは、HTMLの`label`や`input`の意味を壊さないことが大切です。視覚的に近くに配置されていても、`for`と`id`が一致しなければ読み上げ機能などで正しく関連付けられない場合があります。

## 4. サーバーとブラウザーで何が起きているか

入力欄はブラウザーに存在します。しかしこの教材はInteractive Serverなので、`name`フィールドはサーバー側のコンポーネントに保持されます。入力イベントが発生するとBlazorの対話型接続を通ってサーバーへ通知され、サーバーが更新表示を返します。

WinFormsで`TextBox.TextChanged`から`Label.Text`を更新する経験は役立ちますが、Webでは**通信状態と描画**も関与する点を意識してください。特に入力のたびにサーバーへイベントを送る設計は、通信遅延や大量入力時の振る舞いを考える必要があります。業務フォーム全体を`oninput`にするのが常に最適というわけではありません。

## 5. 操作手順

1. `/learn/003`を開き、最初は「入力内容：未入力」と表示されていることを確認します。
2. 「練習用の名前」のラベルをクリックし、入力欄へフォーカスが移るか確認します。
3. `練習太郎`と入力します。入力内容がそのままプレビューへ反映されます。
4. 入力内容を消去します。再び「未入力」になることを確認します。
5. キーボードのTabキーで入力欄へ移動し、フォーカスの輪郭を観察します。
6. ブラウザーの拡大率を上げても、入力欄が親領域より不自然に広くならないか確認します。
7. 文字列`<b>練習</b>`を入力し、HTML要素として実行・描画されないことを確認します。
8. `練習太郎`を入力して「サーバー側で入力を検証する」を押し、保存なしの受入表示を確認します。
9. 入力を空にして同じボタンを押し、「未入力のため拒否しました。」を確認します。
10. 学習用ページでだけ開発者ツールから`maxlength`を外し、`あ`を31文字入力して検証します。「30文字を超えたため拒否しました。」となることを確認し、再読み込みして元の属性へ戻します。実際の業務入力や本番環境では実施しません。

## 6. エラー・失敗時の切り分け

| 現象 | よくある原因 | 確認方法 |
| --- | --- | --- |
| ラベルを押しても入力欄が選ばれない | `for`と`id`の不一致 | Elementsで対応する属性を確認 |
| プレビューが入力の途中では変わらない | `onchange`等のイベントを使用 | `@bind:event="oninput"`を確認 |
| 画面が更新されない | Blazorの接続が切れた | Networkと接続表示、サーバーログを確認 |
| CSSが効かない | CSSファイル名やクラス名の違い | `.razor.css`と`class`を確認 |
| タグのような文字が表示される | Razorが安全にエンコードしている | DOMに実際の`b`要素がないことを確認 |
| 30文字超がC#で受理される | HTML側制限だけで判定している | `ValidateName()`の文字数条件とテストを確認 |
| 入力を変えても古い判定が残る | `ResetValidation()`が呼ばれない | `@bind:after`と「未検証」への更新を確認 |

## 7. 演習と解答

**問1：** 入力欄とラベルを関連付けるために必要な2つの属性は何ですか。

**解答：** `label`の`for`と、`input`の`id`です。この教材では両方`web03-name`です。

**問2：** 文字の入力途中からプレビューが変わるのはなぜですか。

**解答：** `@bind:event="oninput"`が入力イベントごとにフィールドの値を更新するためです。

**問3：** CSSの`padding`を変更するとPostgreSQLの業務データも変更されますか。

**解答：** 変更されません。CSSは表示の指定です。業務データの保存はサーバー側の認可された別処理です。

**問4：** `maxlength="30"`があればサーバー側の入力検証を省略できますか。

**解答：** 省略できません。ブラウザーの入力制限だけを信頼せず、サーバーでも文字数・必須・形式を検証します。

## 完了チェック

- [ ] `label`、`input`、`p`の意味を説明できた。
- [ ] `@bind`と`oninput`のタイミングを説明できた。
- [ ] `.razor.css`の各プロパティと画面の対応を確認できた。
- [ ] HTMLエンコードの結果と表示されるDOMを区別できた。
- [ ] ブラウザー側の`maxlength`とC#側の空欄・上限拒否をそれぞれ確認できた。
- [ ] [Chrome試験ソース](../../../scripts/browser-smoke.cjs)で正常系・異常系の期待値を確認できた。

次は[学習順004：開発者ツール](web-04.md)で、実際のHTML要素と通信を観察します。
