# ステップ23：MudBlazorで文字・選択・日付を入力する

## このステップでできるようになること
WinFormsのTextBox、ComboBox、DateTimePickerに相当する入力機能を、MudBlazorの`MudTextField`・`MudSelect`・`MudDatePicker`で扱います。値とC#の状態の結び付け方、`DateTime?`の日付型、`ja-JP`による表示を理解します。

## 操作する画面
`/learn/23`へ移動します。実DBとADは不要です。

1. 初期状態は氏名が空、学習テーマが「基礎」、学習予定日が2026年10月26日です。
2. 架空の名前「練習太郎」を入力すると、入力確認欄に反映される。
3. 「学習テーマ」で「DI」を選び、確認欄の選択値が変わる。
4. 日付を変更し、確認欄が「yyyy年M月d日」の日本語表記へ変わる。

## 実際の入力コード

```razor
@using System.Globalization

<MudTextField T="string" Label="練習用の氏名"
              @bind-Value="_name" Immediate="true" />
<MudSelect T="string" Label="学習テーマ" @bind-Value="_topic">
  <MudSelectItem T="string" Value="@("基礎")">基礎</MudSelectItem>
  <MudSelectItem T="string" Value="@("DI")">DI</MudSelectItem>
  <MudSelectItem T="string" Value="@("UI")">UI</MudSelectItem>
</MudSelect>
<MudDatePicker Label="学習予定日" @bind-Date="_studyDate"
               Culture="@Japanese" DateFormat="yyyy/MM/dd" />

<p role="status">
入力確認：@(_name ?? "未入力")／@_topic／
@(_studyDate?.ToString("yyyy年M月d日", Japanese) ?? "日付未入力")
</p>

@code {
    private static readonly CultureInfo Japanese =
        CultureInfo.GetCultureInfo("ja-JP");
    private string _name = "";
    private string _topic = "基礎";
    private DateTime? _studyDate = new(2026, 10, 26);
}
```

これは実装ページの入力と状態の主要部分です。実画面は`MudPaper`、`MudStack`、`MudDivider`で配置しています。`Immediate="true"`を指定した文字入力は、入力中の文字列をすぐ状態へ反映する例です。

## データの流れ
1. ブラウザーで文字・選択・日付を操作する。
2. MudBlazorの入力コンポーネントが`Value`または`Date`の変更をBlazorに伝える。
3. サーバー上の`_name`、`_topic`、`_studyDate`が更新される。
4. Razorの入力確認欄が再描画され、値の組み合わせが日本語で表示される。

ここでは画面内の状態を変更するだけで、DBへは保存しません。

## 日付の型と翻訳の境界
`MudDatePicker`の`Date`はこの例では`DateTime?`です。後続のEF Core編で使うPostgreSQLの`date`や.NETの`DateOnly`とは直接同じ型ではありません。実DBへ保存するときは型変換とタイムゾーンを確認します。

`Culture`と`DateFormat`は日付の解釈と表示、`MudBlazor.Translations`は共通UI文言の翻訳を扱います。「学習予定日」のような**画面固有のラベル**は自分で日本語に指定します。

## よくある問題
- `@bind-Value`と`@bind-Date`は対象のプロパティが異なります。
- 文字列がすぐ反映されない場合は`Immediate`設定を確認する。
- 日付が空の場合、`DateTime?`は`null`になり得ます。`?.`と`??`で表示を切り替えています。
- 画面上で日付が正しく見えても、DBへの登録型や境界日時が正しいとは限りません。

## 演習と解答
**問：** `_studyDate = null`になった場合、確認欄はどう表示しますか。

**答え：** `?? "日付未入力"`によって「日付未入力」と表示されます。

## 完了確認
- [ ] 名前とテーマの変更を確認欄で追えた。
- [ ] 日付の初期値と書式を説明できる。
- [ ] `DateTime?`と将来の`DateOnly`を区別できる。

次は[ステップ24：フォーム検証](step24.md)です。
