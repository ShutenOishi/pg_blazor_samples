# 学習順045：HTMLの意味・フォーカス・拡大表示の最終確認

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L045AccessibilityReviewPage.razor)。画面の正規URLは `/learn/045` です。

## 到達目標
第4段階の最終ステップでは、MudBlazorやRazorで画面を作れるだけで終わらず、入力項目・操作・結果が実際に利用者へ伝わることを確認します。文字が小さい、マウスでしか操作できない、色だけで成功を示す、読み上げる順番が不明といった問題は、見た目が整っていても業務画面の利用を難しくします。HTMLの意味、フォーカス、キーボード、拡大表示、通知を確認し、WinFormsの画面設計で培った「利用者が迷わず操作できること」をWebで検証します。

到達条件は、(1)labelとinputの対応、(2)Tab・Shift+Tabの移動、(3):focus-visible、(4)role="status"とaria-live、(5)200%拡大時のレイアウト確認を説明し、操作できることです。この画面は架空の短いメモを表示する練習であり、入力内容を業務DBへ保存しません。高コントラストと文字サイズ切替は教材用の簡易設定であって、すべてのアクセシビリティ要件への適合を保証するものではありません。

## WinForms＋Dapperから見た対応
| WinFormsでの確認 | Webでの確認 | 誤解しやすい点 |
| --- | --- | --- |
| LabelのTextと対象コントロール | labelのforとinputのid | 画面上で隣にあるだけでは関連付けにならない |
| TabIndexとTabStop | DOM順序とTab・Shift+Tab | CSSによる見た目の並べ替えで順序が変わらない場合がある |
| Focus枠やActiveControl | :focus-visibleのアウトライン | マウスでも常に同じ表示になるとは限らない |
| エラーラベルやMessageBox | role="status"とaria-live | 色だけで結果を伝えるのは不十分 |
| DPIとフォント倍率 | ブラウザー200%ズームと文字拡大 | 横スクロールや要素の重なりを実画面で確認する |
| 保存ボタンの有効・無効 | 操作できることと認可は別 | 認可はサーバーで都度判定する |

このステップではキーボードで確実に対象を選べること、操作結果が文字情報として存在すること、狭い領域でも必要な操作が失われないことを重視します。色の違いは補助とし、文字で「入力内容：…」「大きい文字」「高コントラスト」などを伝えます。視覚的な装飾だけを追加しても、支援技術で適切に読み取れるとは限りません。

## Razorの実コード：意味のある関連付け
次はL045AccessibilityReviewPage.razorの中心部分です。for="browser05-note"とid="browser05-note"の一致により、ラベルが入力欄を説明する関係を作ります。@bind:event="oninput"は入力中にRazorのnoteを更新し、条件に応じて文字で結果を表示します。role="status"とaria-live="polite"は、操作結果の通知を支援技術に伝える意図です。

```razor
<label for="browser05-note">練習用の表示内容</label>
<input id="browser05-note"
       type="text"
       @bind="note"
       @bind:event="oninput"
       placeholder="短いメモを入力" />
<p role="status" aria-live="polite" data-testid="browser05-preview">
    @(string.IsNullOrWhiteSpace(note)
      ? "まだ入力されていません。"
      : "入力内容：" + note)
</p>
```

placeholderは入力例の補助であり、ラベルの代わりにはなりません。入力内容が空かどうかを見てメッセージを変えます。WinFormsでTextChangedイベントを扱うことに似ていますが、Interactive Serverでは入力と結果がサーバーとの対話処理を伴います。高頻度の入力で通信遅延が生じることがあるため、業務画面では必要に応じてイベント頻度や通信量を検討します。

画面全体の表示サイズとコントラストは、Razorの条件付きクラスとstyleで切り替えています。ボタンが切り替えるのはコンポーネント内のbool値であり、OS全体の表示設定やユーザーのブラウザー設定を変更するものではありません。

```razor
<section class="lesson-details browser-accessibility @(strongContrast ? "high-contrast" : "")"
         style="font-size:@(largeText ? "1.35rem" : "1rem")">
    <button type="button" @onclick="ToggleText">文字の大きさを切り替える</button>
    <button type="button" @onclick="ToggleContrast">表示のコントラストを切り替える</button>
    <p data-testid="browser05-settings">
        @(largeText ? "大きい文字" : "標準の文字") /
        @(strongContrast ? "高コントラスト" : "通常のコントラスト")
    </p>
</section>

@code {
    private string note = "";
    private bool largeText;
    private bool strongContrast;
    private void ToggleText() => largeText = !largeText;
    private void ToggleContrast() => strongContrast = !strongContrast;
}
```

CSSでは:focus-visibleを使い、キーボード操作中の対象を視覚的に区別します。inputやbuttonが親のフォント設定を引き継ぐようfont:inheritを指定していることにも注目します。利用者の文字サイズを拡大したのにボタンの文字だけ変わらない、という問題を防ぐ意図があります。

```css
.browser-accessibility {
    max-width: 100%;
    overflow-wrap: anywhere;
}
.browser-accessibility input {
    display: block;
    font: inherit;
    max-width: 100%;
    width: min(100%, 34rem);
}
.browser-accessibility button { font: inherit; }
.browser-accessibility :focus-visible {
    outline: 3px solid currentColor;
    outline-offset: 3px;
}
.browser-accessibility.high-contrast {
    background: #111;
    color: #fff;
}
```

上記は実CSSの主要部分です。コントラストの切替では入力欄とボタンにも別の背景・文字色を与えますが、実装色だけで十分なコントラスト比を証明したものではありません。現地で使う支援技術と複数の表示条件でテストしてください。

## フォーカスを実際に移すサンプル

この画面では「入力欄へフォーカスを移す」ボタンを追加し、Blazorの`ElementReference.FocusAsync()`を実行します。見た目のCSSだけで焦点を表現するのではなく、実際に入力欄へキーボードフォーカスが移ることを確認します。

```razor
<input id="browser05-note" type="text" @ref="focusTarget"
       @bind="note" @bind:event="oninput" />
<button type="button" @onclick="FocusInputAsync">
    入力欄へフォーカスを移す
</button>
```

```csharp
private ElementReference focusTarget;
private async Task FocusInputAsync() => await focusTarget.FocusAsync();
```

実行後にブラウザーの`document.activeElement.id`が`browser05-note`になったことをChrome操作試験でも確認します。実際のスクリーンリーダー読み上げや200%ズームの受入は別途目視・実機で確認する必要があります。フォーカスを自動的に移し過ぎると利用者の操作を妨げるため、この練習では利用者が専用ボタンを押したときだけ移動します。

## 実操作の手順
1. /learn/045を開き、マウスを使わずTabキーで入力欄に進みます。Shift+Tabで前の操作対象に戻れることを確認します。
2. 入力欄に「読み上げを確認」と入力し、結果欄に「入力内容：読み上げを確認」と文字で表示されることを確認します。入力を消すと「まだ入力されていません。」へ戻ります。
3. Tabで「文字の大きさを切り替える」に移動し、EnterまたはSpaceで切り替えます。表示が「大きい文字」に変わることを確かめます。
4. 「表示のコントラストを切り替える」をキーボードで操作し、「高コントラスト」と文字で表示されることを確認します。色の違いを見られない場合でも状態が分かるかを判断します。
5. 「入力欄へフォーカスを移す」を押し、入力欄にキーボードフォーカスが移ったことを確かめます。その後、ブラウザーのズームを200%に設定し、入力欄、2つのボタン、操作説明、ページ移動のリンクが隠れず利用できるか確認します。さらにウィンドウを狭くしたときに横方向のスクロールが必要になるか調べます。
6. スクリーンリーダーを利用できる環境では、入力欄のラベル、見出し、statusの変化が期待どおり読み上げられるかを確認します。読み上げの実証はChrome操作試験の通過とは別に扱います。

この画面のdetails/summaryには「表示を確認する手順」があります。Tabでsummaryを選び、Enterで展開できます。詳細は見出しと手順で説明されており、単なるアイコンや色だけに依存していません。表示の拡大はブラウザーのズームと教材内の文字サイズ切替の両方を試してください。両者は同じ設定ではありません。

## 失敗時の切り分け
| 症状 | 確認先 | 対処・判断 |
| --- | --- | --- |
| 入力欄の名前を読み取れない | labelのfor、inputのid | 両者が一致しているか調べる |
| Tabで操作対象を飛ばしてしまう | DOMの順序、disabled、表示状態 | キーボードの到達経路を実際にたどる |
| フォーカスが見えない | :focus-visible、CSS上書き、輪郭色 | ブラウザーのキーボード操作で確認する |
| 入力しても文字が更新されない | @bind:event、Interactive Server接続 | 入力イベントがサーバーに届いたか調べる |
| 色だけ変わり状態が分からない | browser05-settingsの文字 | 表示状態の文言を確認する |
| 200%拡大で操作が隠れる | 要素幅、overflow、固定配置 | 縦スクロールや折り返しで使えるか確認する |
| 読上げが期待と違う | role、aria-live、DOMと支援技術 | 実際の読み上げ環境で確認し修正する |

## 業務画面への適用
MudBlazorのコンポーネントを使っても、利用者に伝わる名前、エラー、状態、フォーカス移動の設計は必要です。入力不正なら「何が誤りで、どう直すか」を示し、通信障害や権限拒否なら処理が完了していないことを文字でも知らせます。ErrorBoundaryのエラー案内、アップロードの拒否、ダウンロード開始の通知は、それぞれ意味が違います。すべてを同じ成功色・失敗色だけで表すのは不適切です。

アクセシビリティと認可も別です。たとえキーボードから操作できないようにしたボタンでも、要求先が残っていれば不正な操作を防止したことにはなりません。サーバー側の既存台帳による都度認可と安全拒否を維持してください。現在の画面はWeb UIの練習であり、Active Directory、PostgreSQL、IISの実機受入を示しません。

## 演習と詳しい解答
**問1：** placeholderに「氏名」と書けばlabelは不要ですか。**解答：** 不要にはなりません。placeholderは入力時に消える場合があり、ラベルとしての一貫した関連付けを保証しません。for/idによって入力項目を明示的に説明します。

**問2：** 保存に成功するとボタンを緑に変えるだけで、結果通知は十分ですか。**解答：** 十分ではありません。色の違いを見分けられない利用者や読み上げ利用者がいます。保存が完了したのか、開始しただけか、拒否されたのかを適切な文字情報と状態通知で示します。

**問3：** 200%ズームで入力欄が見えれば確認は完了ですか。**解答：** 完了ではありません。操作ボタン、エラー表示、復習リンク、説明文も読めるか、フォーカスを追えるか、横スクロールや重なりが操作を阻害しないか調べます。

**問4：** CSSの:focus-visibleを指定すれば、すべてのスクリーンリーダーの動作を保証しますか。**解答：** 保証しません。これは主に視覚的なフォーカス表示です。ラベル、HTMLの構造、aria-liveや実際の支援技術との組合せを別途検証します。

## 完了確認と次の学習
- [ ] TabとShift+Tabだけで入力、切替、説明の展開を行った。
- [ ] 入力結果と表示設定の文字情報を確認した。
- [ ] 200%ズームと文字サイズ切替の両方で表示を確認した。
- [ ] DOMの意味・フォーカス・認可が異なる責務であると説明できる。

次は[第9組の復習：ブラウザー機能と安全な操作](../reviews/review-09.md)です。5ステップをまとめて操作し、サーバーとブラウザーの責務を再確認します。