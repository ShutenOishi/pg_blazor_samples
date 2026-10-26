# ステップ08：EventCallbackによる親への通知

**実装：** [親画面](../../src/PgBlazorSamples.Web/Components/Samples/Step08/CallbackPage.razor)／[子部品](../../src/PgBlazorSamples.Web/Components/Samples/Step08/TopicPicker.razor)

## 到達目標
子のボタン操作を`EventCallback<string>`で親に伝え、親が画面状態を更新する流れを理解する。

## 操作手順
1. `/learn/08`を開き「まだ選択されていません」を確認する。
2. 「Razor構文」を押す。現在の選択が変わり、回数が1回になる。
3. 「双方向バインディング」を押す。現在の選択が変わり、回数が2回になる。
4. どのボタンを押したかが子だけでなく親の表示に反映されることを確認する。

## ソースを読む
子`TopicPicker.razor`の契約：
~~~csharp
[Parameter]
public string SelectedTopic { get; set; } = "";

[Parameter]
public EventCallback<string> OnSelected { get; set; }

private Task ChooseAsync(string topic) => OnSelected.InvokeAsync(topic);
~~~

親`CallbackPage.razor`：
~~~razor
<TopicPicker SelectedTopic="@selectedTopic" OnSelected="OnTopicSelected" />
~~~

親は`OnTopicSelected`内で`selectedTopic`と`selectionCount`を更新する。`[Parameter]`は親→子、`EventCallback`は子→親。`InvokeAsync`によってBlazorに通知を渡し、親が描画を更新する。

## 演習・否定テスト
- [ ] 別のテーマへ切り替えてもカウントは継続する
- [ ] 親が状態を所有することを説明できる
- [ ] ボタンの操作結果を子の内部だけで書き換える実装との違いが分かる

前：[Step07](step07.md)／次：[Step09](step09.md)