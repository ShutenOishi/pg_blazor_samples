# 学習順014：子から親へ通知する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step08/CallbackPage.razor)。画面の正規URLは `/learn/014` です。

## 何をできるようにするか
学習順013では親が`[Parameter]`を使い、子に表示名を渡しました。次は、子のボタンを押したとき親の状態を変更する方法です。画面`/learn/014`の親`CallbackPage.razor`、子`TopicPicker.razor`を読み、値は親が保持し、子が`EventCallback<string>`を通じて通知する役割分担を理解します。WinFormsのUserControlが独自イベントを公開し、フォームがそのイベントを購読する設計に近いですが、Blazorではコンポーネント引数としてイベントの受け口を渡します。

Visual Studio 2026、.NET 10、ChromeのInteractive Server画面を使います。AD認証・DB・EF Core・Dapperは呼び出しません。選択肢は「Razor構文」「イベント処理」「双方向バインディング」の3つの固定文字列です。選択回数は永続的な操作履歴でも監査記録でもなく、親のフィールドです。子ボタンがサーバーでC#を実行するため、対話接続が必要です。

## WinForms＋Dapperとの対応
| 観点 | WinFormsの考え方 | Blazorの実装 |
| --- | --- | --- |
| 親と子 | FormとUserControl | `CallbackPage`と`TopicPicker` |
| 親から子の値 | プロパティ値の設定 | `SelectedTopic`の`[Parameter]` |
| 子のイベント | イベントを公開しForm側で購読 | `EventCallback<string> OnSelected` |
| 発火 | イベント引数を通知 | `OnSelected.InvokeAsync(topic)` |
| 親の処理 | 選択とカウントの変更 | `OnTopicSelected` |
| 永続化 | Dapper.Execute等を明示的に実行 | この画面ではDBへ保存しない |

この教材では子が状態を保持しません。どのテーマが選ばれたかを知るのは親の`selectedTopic`で、選択回数も親の`selectionCount`です。画面で選択を何度も変えても子の内部に二重の真実は作りません。業務用の複数部品が同じ選択結果を共有する場合も、所有者を決めることが重要です。

## 実コード1：親が選択値を渡し、通知を受ける
```razor
@page "/learn/014"
@using PgBlazorSamples.Web.Components.Samples.Step08
<TopicPicker SelectedTopic="@selectedTopic" OnSelected="OnTopicSelected" />
<p role="status" data-testid="step08-selection">現在の選択：<strong>@selectedTopic</strong></p>
<p>選択した回数：<strong data-testid="step08-count">@selectionCount</strong>回</p>
```

```csharp
// 親が画面全体の状態を所有します。
private string selectedTopic = "まだ選択されていません";
private int selectionCount;

private void OnTopicSelected(string topic)
{
    selectedTopic = topic;
    selectionCount++;
}
```

`SelectedTopic`に現在の値を渡し、`OnSelected`には親のメソッドを指定しています。ボタンを押した回数だけ`selectionCount`を増やし、同じテーマを再び選んでも回数は増えます。親のメソッドは現行実装では`void`ですが、DBや外部通信を伴う実務処理を追加する場合は非同期の完了・例外・取消・重複操作を別途検討します。画面内の`role="status"`は表示の更新を補助技術に知らせる用途です。

## 実コード2：子から通知する
```razor
<div class="practice-actions" role="group" aria-label="復習するテーマを選択">
    @foreach (var topic in topics)
    {
        <button type="button" @key="topic" class="secondary-button"
                aria-pressed="@(SelectedTopic == topic)"
                @onclick="() => ChooseAsync(topic)">@topic</button>
    }
</div>

@code {
    [Parameter]
    public string SelectedTopic { get; set; } = "";

    [Parameter]
    public EventCallback<string> OnSelected { get; set; }

    private readonly string[] topics = ["Razor構文", "イベント処理", "双方向バインディング"];

    private Task ChooseAsync(string topic) => OnSelected.InvokeAsync(topic);
}
```

ボタンの`@onclick`は、そのボタンに対応する`topic`を`ChooseAsync`へ渡します。`InvokeAsync(topic)`は指定した値を親の受け口へ通知し、戻り値の`Task`を返します。`aria-pressed`は親から渡された`SelectedTopic`と各ボタンの文字列を比較するため、選ばれているボタンの状態がわかります。`@key`には固定テーマの文字列を使っています。重複テーマを追加する場合には一意性を見直してください。

## 処理の順番を追跡する
1. `/learn/014`を開くと親の`selectedTopic`は「まだ選択されていません」、`selectionCount`は0です。
2. 親が子へ現在の値とイベントの受け口を渡して描画します。
3. 「Razor構文」を押すと、そのボタンから`ChooseAsync("Razor構文")`が実行されます。
4. 子の`OnSelected.InvokeAsync`が親の`OnTopicSelected`を通知します。
5. 親が`selectedTopic`を「Razor構文」に変更し、回数を1にします。
6. 親の再描画によって`TopicPicker`の`SelectedTopic`も変わり、選択ボタンの`aria-pressed`と文章が一致します。
7. 「双方向バインディング」を選ぶと回数が2になります。同じテーマを2回押した場合にも回数は増えます。
8. 再読込すると回数が初期値に戻ります。保存・監査が実装されていないことを確認します。

## 画面実習
Chromeで`/learn/014`を開き、最初はどのテーマも選択されていないことを確認してください。順番に3つのボタンを押し、画面下の現在選択と回数が変化することを観察します。Elementsで`aria-pressed`が選択に従い変わることを確認すると、見た目だけではわかりにくいアクセシビリティ情報も学べます。開発者ツールのNetworkで対話接続を確認し、ブラウザーの表示変更だけで処理が終わっていないことを理解してください。検証環境では、通知メソッドにブレークポイントを設定すると親子の呼び出し順がわかります。

## 故障・失敗の切り分け
| 症状 | 確認する箇所 | 考え方 |
| --- | --- | --- |
| ボタンが押せるが選択が変わらない | `OnSelected`の指定、SignalR接続 | 子の通知が親まで届いているか |
| 選択が変わるが回数が増えない | `OnTopicSelected`内の更新 | 回数更新が親から抜けていないか |
| `aria-pressed`だけ古い | `SelectedTopic`の引数 | 親の最新値を子に渡しているか |
| 同じテーマで回数が増える | `selectionCount++` | 現行実装では正常。変更を数える仕様とは異なる |
| 再読込で回数が0へ戻る | 親の初期フィールド | DB保存がなく正常 |
| 通知後にサーバー例外が出る | サーバーログ・例外・Task | 非同期処理の完了を見ず成功を表示しない |

## 演習と詳しい解答
**演習1：** なぜ子が自分の`SelectedTopic`に直接代入してはいけないのでしょうか。

**解答1：** それは親からの入力だからです。子だけを書き換えても、親の`selectedTopic`と一致しなくなります。親に`InvokeAsync`で通知し、親が値を更新する経路を保ちます。

**演習2：** 「イベント処理」を3回押したとき現在の選択と回数はどうなりますか。

**解答2：** 現在の選択は「イベント処理」、回数は3です。現行`OnTopicSelected`には前回と同じ値かどうかの条件がありません。「変更した回数」ではなく「押して通知を処理した回数」です。

**演習3：** 親側で非同期のDB保存を追加した場合、`EventCallback`を呼んだ時点で必ず書込み成功と言えますか。

**解答3：** 言えません。非同期処理の完了・エラー・権限拒否・排他競合・永続監査を別途確認する必要があります。現行コードはそもそも保存処理を持たず、画面上の通知結果のみです。

**演習4：** 表示文字列が同じボタンを2個追加した場合、`@key="topic"`のままでよいでしょうか。

**解答4：** 一意性が崩れるため避けます。テーマごとに一意のIDを作り、その値をキーにするなどモデルを変更します。イベントの文字列が同一でも、表示項目の識別子は別問題です。

**演習5：** WinFormsの`UserControl`と比べた主な違いを説明してください。

**解答5：** 親子間の入出力を分ける設計は共通ですが、Interactive Serverでは子のクリックから親のC#イベントまで通信を伴い得ます。切断、再接続、状態の寿命を理解し、業務操作では本人認証・既存DB台帳認可と監査をサーバー側に置きます。

## 次の学習
次の学習順015では、この通知を運ぶ通信方式SignalRとサーバー側のcircuitの寿命を学びます。前：[学習順013](step07.md)、次：[学習順015](../course-v3/lessons/runtime-02.md)。
