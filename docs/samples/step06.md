# 学習順012：条件分岐と繰り返しで表示する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step06/ConditionsPage.razor)。画面の正規URLは `/learn/012` です。

## 到達目標と実習環境
学習順011では入力した文字列を表示に反映しました。今回は、利用者の選択で条件を切り替え、同じデータから異なる一覧を描画する方法を学びます。`@if`・`@else`・`@foreach`・`@key`を組み合わせ、表示する配列と元のデータを区別して説明できることが目標です。WinFormsのCheckBox.CheckedやDataGridViewの行追加に慣れていても、Blazorでは「状態に応じて表示内容を宣言する」書き方を練習します。

画面は`/learn/012`、実装は`ConditionsPage.razor`です。Visual Studio 2026、.NET 10、Chromeで起動します。ここでは架空の4件をC#に固定しており、PostgreSQL・Dapper・EF Core・AD・業務認可はいずれも呼び出しません。テーブルの実検索や権限制御の説明と、今実際に動く機能を混同しないでください。

## 表示とデータの処理場所を整理する
| 観点 | WinForms＋Dapper | この教材のRazor |
| --- | --- | --- |
| 絞り込み指定 | CheckBox.CheckedやComboBox | checkboxの`@bind` |
| 条件に応じた表示 | 行を手続き的に消す・追加する | `@if`で案内を選ぶ |
| 一覧の反復 | foreachでDataGridViewRowを追加 | `@foreach`でliを描画 |
| 件数 | Rows.Countなど | `VisibleTasks.Length` |
| 元データ | DBからDapper.Queryで取得する場合が多い | 4件の`LearningTask`固定配列 |
| セキュリティ | データ取得時にも権限条件が必要 | まだDBと認可は接続しない |

画面で2件になったことは、元データが削除されたという意味ではありません。元の配列`tasks`は4件を保ち、見せる配列`VisibleTasks`だけを切り替えています。これをWinFormsのデータ表示に置き換えると、データソースを保持したまま表示用ビューのフィルターを変える考え方に近いものです。ただし、この画面の`Where`はサーバー側のメモリー上の配列で働き、SQLの`WHERE`句が発行されたわけではありません。数万件のデータを全取得してから利用者ごとに絞り込む設計は、性能面でも権限面でも別途検討が必要です。

## 実コード1：入力と条件分岐
```razor
<label class="checkbox-label">
    <input type="checkbox" @bind="showOnlyIncomplete" />
    未完了の課題だけ表示する
</label>
@if (showOnlyIncomplete)
{
    <p class="hint" data-testid="filter-state" role="status">未完了の課題に絞っています。</p>
}
else
{
    <p class="hint" data-testid="filter-state" role="status">すべての課題を表示しています。</p>
}
```

チェックを入れると真、外すと偽になります。`@if`は真のときだけそのブロックのHTMLを描画します。`@else`は偽のときの案内を描画します。`role="status"`は操作の結果を補助技術へ伝えるための指定です。画面のHTMLが切り替わるだけで、別のURLへ移動してはいません。ChromeのElementsで表示される文章を観察し、C#の状態とDOMの関係を確認してください。

## 実コード2：件数と反復表示
```razor
<p>表示中：<strong data-testid="visible-count">@VisibleTasks.Length</strong>件</p>
<ul class="learning-list" data-testid="task-list">
    @foreach (var task in VisibleTasks)
    {
        <li @key="task.Id">
            <span>@task.Title</span>
            <span class="task-status">@(task.IsCompleted ? "完了" : "未完了")</span>
        </li>
    }
</ul>
```

`@foreach`は表示対象の各課題からli要素を作ります。`@key="task.Id"`に指定するIDは項目の同一性を安定して扱うための値です。行番号とは別で、一覧の順序が変わっても同じ課題を識別できます。今回の4件のIDは1〜4で重複しません。`@(条件 ? A : B)`は表示文字列の選択で、C#の三項演算子です。これらは読み取り用表示であり、チェックボックスを操作しても課題自体の`IsCompleted`を書き換えません。

## 実コード3：C#の固定配列と表示条件
```csharp
// 学習用の架空データです。実際の利用者情報を使用しません。
private readonly LearningTask[] tasks =
[
    new(1, "ステップ01：最初の画面を確認する", true),
    new(2, "ステップ02：ページを移動する", true),
    new(3, "ステップ04：ボタンを押してみる", false),
    new(4, "ステップ05：入力を変更する", false)
];
private bool showOnlyIncomplete;
private LearningTask[] VisibleTasks => showOnlyIncomplete
    ? tasks.Where(task => !task.IsCompleted).ToArray()
    : tasks;
private sealed record LearningTask(int Id, string Title, bool IsCompleted);
```

C#の`record`を同一Razorファイル内で定義しているので、この学習のためだけにプロジェクトを増やす必要はありません。`readonly`は配列フィールドへの再代入を制限しますが、配列要素の完全な不変性を保証するものではありません。`Where`は条件に合う要素を抽出し、`ToArray`で配列化します。未完了はID3と4の2件です。真・偽によって`VisibleTasks`が別の配列を返すため、表示件数も4→2→4と変わります。計算量と再評価の仕組みは、業務で大量データを扱う際に別途考慮してください。

## 順番に操作する
1. `/learn/012`を開き、全件4件・案内「すべての課題を表示しています。」を確認します。
2. それぞれの行の完了・未完了を読み、前の2件だけが完了であることを確認します。
3. 「未完了の課題だけ表示する」にチェックを入れ、2件・「未完了の課題に絞っています。」を確認します。
4. ChromeのElementsで`data-testid="task-list"`のliが2件になっていることを確認します。
5. チェックを外し、4件の元データが再び現れることを確かめます。
6. 実験として架空データの完了状態を一件だけ変え、件数がどうなるか予想してから実行します。
7. ブラウザーを再読込し、チェックが初期状態に戻ることを観察します。永続化は実装していません。

## 問題を切り分ける
| 症状 | 確認方法 | 判断・対応 |
| --- | --- | --- |
| チェックしても4件のまま | `showOnlyIncomplete`の値とSignalR接続 | イベント未到達か条件が違う |
| 件数だけ2だが一覧は4行 | `@foreach (var task in VisibleTasks)` | 件数と表示の参照先がずれていないか |
| 何も表示されない | 固定データとフィルター条件 | 本当に該当0件か、実行例外かを区別 |
| 順序変更で行が不安定 | `@key`の値の一意性 | IDの重複や不適切なインデックス指定を調べる |
| チェックが再読込で消える | フィールドの初期値 | DB・URLへ保存していないので正常 |
| 全件表示が安全と思える | サーバー側取得と認可の有無 | 表示フィルターは業務認可の代わりにならない |

## 演習と詳しい解答
**演習1：** なぜフィルターで2件にしたあとチェックを外すと4件へ戻るのでしょうか。

**解答1：** `tasks`自体を削除・更新していないためです。`VisibleTasks`は表示時に`showOnlyIncomplete`を評価して選択します。表示結果とデータの保存状態は別です。

**演習2：** 5件目に`new(5, "ステップ06：表示条件を試す", false)`を追加すると、初期件数と未完了件数はどうなりますか。

**解答2：** 初期は5件、未完了に絞ると3件です。追加したレコードの`IsCompleted`をtrueへ変更すると未完了は2件です。ただしこのサンプルの固定値を修正するため、ソース変更後に再ビルドが必要です。

**演習3：** 0件のとき「表示する課題はありません。」を表示するには何を追加しますか。

**解答3：** `@if (VisibleTasks.Length == 0)`の分岐を一覧の直前へ追加する方法があります。これは説明用の拡張で、現行4件のサンプルには該当0件を作る操作がない点を区別してください。利用者が0件を失敗と誤認しないよう日本語の案内を表示します。

**演習4：** 部署Aに所属する利用者へ部署Bのデータを送った後で、画面側のフィルターで隠せば安全ですか。

**解答4：** 安全ではありません。画面上で非表示にする操作は認可ではありません。将来の実DB検索では、要求のたびに本人と既存DB台帳の権限を照合し、取得範囲・件数・詳細・CSVまでサーバー側で制限する必要があります。このサンプルには本人・部署の情報自体がありません。

**演習5：** `@key`に表示位置のインデックスを選ぶ場合と安定したIDを使う場合の違いを説明してください。

**解答5：** フィルター・並べ替え・追加で表示位置は変わり得ます。安定したIDであれば、同じ項目が同じ識別子として扱われます。フォーム要素など内部状態を持つ行を追加する段階で特に重要になります。

## 次へ
学習順013では一覧の項目表示を別のRazor部品へ分け、親コンポーネントが渡した値を子の`[Parameter]`で受け取ります。前：[学習順011](step05.md)、次：[学習順013](step07.md)。
