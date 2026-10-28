# 学習順33：日本語で入力部品を使う

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L033MudInputsPage.razor)。画面の正規URLは `/learn/033` です。

## 学ぶ前に
この教材はWinForms＋Dapperを使ってきた開発者が、WebのUIとサーバー側C#の境界を理解するためのものです。学習用の架空データで試し、認証・認可や実業務の保存が成立したとは見なしません。対象画面は /learn/033 です。

## WinForms＋Dapperとの比較
| 観点 | デスクトップでの経験 | MudBlazorでの対応 |
| --- | --- | --- |
| 文字入力 | TextBox.Text | MudTextFieldのValue |
| 候補選択 | ComboBox | MudSelectとMudSelectItem |
| 日付 | DateTimePicker | MudDatePickerのDateTime? |
| DB連携 | Dapperのパラメーター | 後続でDateOnlyと型を照合 |

## 不具合時の切り分け
| 症状 | 確認する場所 |
| --- | --- |
| 文字が反映されない | Immediateと双方向バインド |
| 選択が変わらない | TとValueの型 |
| 日付が空になる | DateTime?とnull条件分岐 |
| 日本語表記にならない | CultureとDateFormat |

## 値の型・表示書式・保存形式を別に考える
MudTextFieldの@bind-Valueは文字列を_nameへ、MudSelectは_topicへ、MudDatePickerの@bind-DateはDateTime?の_studyDateへ結び付けます。Immediate=trueなら文字入力の変更が早く反映されます。ただし、通信を介してサーバー側のC#へ伝えられるので、WinFormsの同一プロセスのTextChangedとは動作する場所が異なります。最初の日付は2026年10月26日の架空の固定値です。

```razor
<MudTextField T="string" Label="練習用の氏名" @bind-Value="_name" Immediate="true" />
<MudDatePicker Label="学習予定日" @bind-Date="_studyDate"
               Culture="@Japanese" DateFormat="yyyy/MM/dd" />
```

```csharp
private static readonly CultureInfo Japanese = CultureInfo.GetCultureInfo("ja-JP");
private string _name = "";
private DateTime? _studyDate = new(2026, 10, 26);
```

画面の「yyyy年M月d日」は日本語の表示形式です。PostgreSQLのdate列へその文字列を連結する設計にはしません。後続ではEF Core 10とNpgsqlの型変換を確認し、年月日だけを扱うDateOnlyとDateTime?を区別して、値をパラメーターとして渡します。

**演習と詳しい解答：** 氏名へ架空の「練習太郎」を入力し、テーマをDIへ変えて結果を観察します。入力と選択の更新がサーバー側の変数へ結び付き、再描画された確認欄へ反映されます。日付を消してnullになったときは、null条件演算子と代替表示により「日付未入力」を出します。画面へ値を表示できたことはDBへ保存されたことを意味しません。Dapperを使うデータ処理と画面の状態管理は別です。

## 実装に対応した操作・コード・詳細解答

## このステップでできるようになること
WinFormsのTextBox、ComboBox、DateTimePickerに相当する入力機能を、MudBlazorの`MudTextField`・`MudSelect`・`MudDatePicker`で扱います。値とC#の状態の結び付け方、`DateTime?`の日付型、`ja-JP`による表示を理解します。

## 操作する画面
`/learn/033`へ移動します。実DBとADは不要です。

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

次は[学習順034：フォーム検証](L034MudValidationPage.md)です。


## 学習順33の追加確認
- [ ] 画面で操作を実際に行い、状態の変化を確認した。
- [ ] 比較表と切り分け表に沿って、WinFormsとの相違点を説明できる。
- [ ] DapperによるDB検索と画面表示の処理を区別できる。
- [ ] 教材の演習に回答し、失敗時の復旧手順を説明できる。
