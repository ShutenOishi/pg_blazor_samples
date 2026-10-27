# 第4段階の概要：MudBlazorとブラウザーの業務UI

## 到達目標と事前知識
第3段階までに学んだDI、画面イベント、Blazor Serverのcircuitを使い、UIライブラリとブラウザーの違いを理解します。WinForms＋Dapperの経験を前提に、Panel・ToolStrip・TextBox・ErrorProvider・DataGridViewと、MudBlazorの部品を対応させます。第4段階は学習順031〜045の15テーマを3組で学ぶ構成です。データは架空のもので、実AD認証や業務DBの認可を実施したことにはなりません。

## 3組の学習内容
| 復習 | 学習順 | 主題 | 到達点 |
| --- | --- | --- | --- |
| 第7組 | 031〜035 | MudBlazor導入・日本語翻訳・配置・入力・検証・一覧 | DIとProvider、値の型、表示件数と取得件数を説明する |
| 第8組 | 036〜040 | MudDataGrid・Dialog・通知・テーマ・総合演習 | UI状態と実処理を区別する |
| 第9組 | 041〜045 | JS連携・アップロード・ダウンロード・エラー・操作性 | ブラウザー／サーバーの境界と拒否条件を説明する |

## WinForms＋Dapperとの比較
| 経験した操作 | MudBlazorの対応 | 注意点 |
| --- | --- | --- |
| PanelとToolStripを配置 | MudLayout・MudAppBar・MudDrawer | メニュー開閉は認可ではない |
| TextBox・ComboBoxへ入力 | MudTextField・MudSelect | ブラウザー操作でサーバーの値が変わる |
| DateTimePickerの日付 | MudDatePicker | DateTime?とDB側dateを区別する |
| ValidatingやErrorProvider | MudFormのValidateAsync | 入力検証とサービス側認可は別 |
| DataGridViewで一覧表示 | MudTable・MudDataGrid | ItemsとServerDataの取得方式を比較する |

## 実コードで見る事前準備
```csharp
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddMudTranslations();
```
ライブラリ側のサービスを登録しただけで通知が画面に出るとは限りません。共通レイアウトのMudSnackbarProviderや、CSS/JavaScript、Interactive Serverの指定を組み合わせる必要があります。独自のラベルは日本語で記述し、共通翻訳だけに依存しないでください。

```razor
<MudTable T="LearningRow" Items="@_items" RowsPerPage="2">
    <PagerContent><MudTablePager /></PagerContent>
</MudTable>
```
この表示は固定4件から2件ずつ表示する例です。DapperでDBから2件だけ取得しているのではありません。次のServerDataの学習では、SQLの絞り込みと安全なページ取得を別途扱います。

## 操作手順と切り分け
最初に学習順031の日本語通知ボタンを操作します。032ではメニューを閉じて再び開き、画面幅に応じた配置を確認します。033では架空の氏名と日付を入力します。034では空欄で2つのエラーを出し、入力後は合格へ変わることを確認します。035では4件の表を2件ずつ表示し、見出しのソートを試します。各5本の後で復習を行ってください。

| 症状 | 切り分け | 判断の目安 |
| --- | --- | --- |
| クリックで変化しない | 対話型接続、OnClick、サーバー側状態 | イベントが届いたかから確認 |
| 通知だけ表示されない | ISnackbar・Provider | 通知の要求と表示を区別 |
| 日付が空欄になる | DateTime?のnullとCulture | 値の存在と日本語書式を分ける |
| 正しい入力でも保存できない | IsValidと業務認可 | 合格はDB保存権限を意味しない |
| ページ切替でも処理が重い | Itemsに渡す件数 | 後続のServerData導入を判断 |

## 演習と詳しい解答
**問1：** メニューを閉じれば権限のない機能は守れますか。**解答：** いいえ。URLから直接要求を送れるので、サービス側が既存DB/外部台帳を正本として都度認可しなければなりません。

**問2：** 翻訳を導入したのに業務固有の見出しを手動で書くのはなぜですか。**解答：** 共通のUI文言と業務固有の項目名は別だからです。Cultureは日付・数値、Translationsは共通文言、RequiredErrorは独自ルールです。

**問3：** MudTableのRowsPerPage=2でDBの取得件数が減りますか。**解答：** 減りません。Itemsのデータを2件ずつ表示するだけです。大量データであれば取得条件とページングをサーバー側で扱います。



## この段階でできるようになること
日本語入力・一覧・ダイアログ・通知に加え、JS連携、ファイル操作、エラー、アクセシビリティを判断できる。

## この段階の前提
第3段階のDI。DB/ADなしで練習できる。

## WinForms・Dapperとのつながり
DataGridViewとは違い、Items全件保持とサーバー側取得を分け、ブラウザーのファイル・DOMに必要な手段を選ぶ。

## 学習の順番と到達点
- 学習順031：MudBlazorと日本語翻訳を導入する（旧Step21に対応）
- 学習順032：MudBlazorのレイアウトを学ぶ（旧Step22に対応）
- 学習順033：日本語で入力部品を使う（旧Step23に対応）
- 学習順034：MudFormの入力検証を学ぶ（旧Step24に対応）
- 学習順035：MudTableで一覧とソート（旧Step25に対応）
- 学習順036：MudDataGridで検索・並べ替えする（旧Step26に対応）
- 学習順037：確認ダイアログで実行と中止を選ぶ（旧Step27に対応）
- 学習順038：Snackbarと処理中の状態を表示する（旧Step28に対応）
- 学習順039：ダークモードとキーボード操作を学ぶ（旧Step29に対応）
- 学習順040：MudBlazor日本語UIを総合演習する（旧Step30に対応）
- 学習順041：JavaScript interopの利用判断（新規初稿実装：browser-01）
- 学習順042：ファイルアップロードと入力制約（新規初稿実装：browser-02）
- 学習順043：CSV・PDFファイルのダウンロード（新規初稿実装：browser-03）
- 学習順044：ErrorBoundary・取消・再試行（新規初稿実装：browser-04）
- 学習順045：HTMLの意味・フォーカス・拡大表示の最終確認（新規初稿実装：browser-05）

## 5ステップごとに確認すること
- 学習順035の後：第7組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順040の後：第8組の復習。直前の5テーマの関係を説明し、動作を再確認する。
- 学習順045の後：第9組の復習。直前の5テーマの関係を説明し、動作を再確認する。

## 練習課題と解答の確認点
架空の課題データで検索・確認・アップロード制約・CSV/PDFダウンロード・キーボード操作を試す。

**確認の基準：** 画面操作の結果だけでなく、処理がブラウザー・サーバー・DBのどこで動くか、例外や拒否をどう確認するかを説明できること。未実装の教材では学習結果の達成を宣言しません。

## 次の段階への準備
画面をPostgreSQLとEF Coreの短命DbContextへ接続する。

全体の順番は[新学習順表](../learning-sequence.md)を参照してください。
