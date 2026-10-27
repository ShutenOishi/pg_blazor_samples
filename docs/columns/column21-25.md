# 第7組の復習：学習順031〜035

## 5本を振り返る
WinForms＋Dapperでのフォーム作成経験と比較し、MudBlazorのDI、レイアウト、入力型、検証、一覧の動作を整理します。毎回の操作をサーバー上のC#状態と結び付けて説明してください。画面は架空データを使用し、業務DBの登録やAD認証を行いません。

| 学習順 | 実際に試す操作 | WinFormsとの相違点 |
| --- | --- | --- |
| 031 | 日本語の通知ボタンを押す | ISnackbarとProviderで表示 |
| 032 | メニューを閉じて開く | @bind-Openとcircuitの状態 |
| 033 | 氏名、テーマ、日付を変更 | DateTime?とCultureを区別 |
| 034 | 空欄・正常入力を検証 | ValidateAsyncとIsValid |
| 035 | 4件の表を2件ずつ表示・ソート | RowsPerPageはDB取得件数ではない |

## 実装のコードの関連
```csharp
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddMudTranslations();
```
```razor
<MudForm @ref="_form">
    <MudTextField T="string" Label="課題名（必須）"
                  @bind-Value="_title" Required="true"
                  RequiredError="課題名を入力してください。" />
</MudForm>
```
共通翻訳の登録とRequiredErrorに書いた日本語の違いを説明してください。前者はライブラリの共通文字列、後者はアプリが定める検証条件です。検証に成功しても権限が付与されるわけではありません。

## 故障の切り分け
| 問題 | 原因の候補 | 確認手順 |
| --- | --- | --- |
| 通知が出ない | Providerがない | DIとレイアウトを確認 |
| メニューが開かない | 状態の更新がない | OnClickと_menuOpenを見る |
| 日付が空欄 | nullか表示書式違い | DateTime?とja-JPを分ける |
| 検証に失敗する | 必須欄が未入力 | ValidateAsyncの後にIsValidを確認 |
| 表が重い | 全件Itemsで保持 | ServerDataへの変更を検討 |

## 演習問題と詳しい解答
**問1：** クリックで状態文字列だけ変わり通知が出ません。**解答：** イベント処理は動いていると推測できるため、ISnackbarとMudSnackbarProvider、共通レイアウト、CSS/JSを順番に調べます。DBやDapperは通知の直接の依存ではありません。

**問2：** メニューが非表示なら機能のアクセス制限になりますか。**解答：** なりません。非表示はUI状態で、URLの直接指定で要求が届きます。サーバーの既存台帳による都度認可が必要です。

**問3：** MudDatePickerの値をPostgreSQLのdateへ文字列連結してよいですか。**解答：** よくありません。日付の型と表示形式を分け、後続ではDateOnlyへの変換やパラメーター化を検討します。時刻・タイムゾーンをむやみに追加しません。

**問4：** MudFormのIsValidがtrueなら更新してよいですか。**解答：** いいえ。入力の妥当性と、本人・部署の権限、DB制約、同時更新競合、監査の成立は別です。業務サービス側で安全に確認します。

**問5：** Itemsが4件でRowsPerPageが2ならDBからの取得は2件ですか。**解答：** 違います。固定4件はすでにサーバー側にあり、UIが2件ずつ表示します。後続のServerDataでサーバー取得と表示を同期します。

## 第8組へ進む条件
画面の状態と業務の権限を区別でき、比較表や障害の切り分け表を使って説明できたら学習順036に進みます。第4段階の残り036〜045と復習第8・9組は引き続き今後の最終改稿対象です。


**対応：ステップ21〜25。** このコラムでは、UIライブラリを導入するところから一覧表示までを、WinFormsの開発経験と対応させて確認します。

## この5ステップのつながり
| 学習順 | 学んだこと | 実務上の区別 |
| --- | --- | --- |
| Step21 | MudBlazorの登録・日本語翻訳・通知 | DI、Provider、CSS/JS、描画モードを組み合わせる |
| Step22 | AppBar・Drawer・Gridの構成 | レイアウトの表示状態と権限は異なる |
| Step23 | テキスト・選択・日付の入力 | `Value`・`Date`とC#の型、文化設定を区別 |
| Step24 | MudFormの必須入力検証 | 画面の入力検証とサービス側の制約・認可を区別 |
| Step25 | MudTableのItems・ソート・ページャー | 表示のページングとDB側ページングを区別 |

## 導入から通知表示までを追う
1. パッケージ`MudBlazor`と`MudBlazor.Translations`を導入する。
2. `Program.cs`で`AddMudServices`・`AddLocalization`・`AddMudTranslations`を登録する。
3. `App.razor`でCSS/JSを読み、Interactive Serverを有効にする。
4. `MainLayout.razor`で`MudSnackbarProvider`などを配置する。
5. Step21が`@inject ISnackbar Snackbar`を受け取り、クリック時に`Snackbar.Add`を呼ぶ。
6. 日本語の通知が画面に現れ、Razorの状態メッセージも更新される。

ライブラリの部品を使うためには、**パッケージ・サービス・描画・Providerの条件**が必要です。特別コラム05の「既存ライブラリが持つDIを読んで使う」という考え方がここで役立ちます。

## WinFormsの知識と対応させる
| WinFormsで親しみのある部品 | この教材のMudBlazor |
| --- | --- |
| PanelやToolStrip | MudLayout・MudAppBar・MudDrawer |
| TextBox・ComboBox | MudTextField・MudSelect |
| DateTimePicker | MudDatePicker（`DateTime?`を扱う） |
| 入力値の検査 | MudForm・`ValidateAsync` |
| DataGridViewの小規模な一覧 | MudTableの`Items` |
| 通知用の画面部品 | `ISnackbar`と`MudSnackbarProvider` |

Webではブラウザーに表示されても、Interactive ServerのイベントとC#の処理はサーバーで動く点が異なります。

## 日本語化を3つに分ける
- **文化設定**：`ja-JP`で日付・数値などの扱いを整える。
- **共通UIの翻訳**：`MudBlazor.Translations`に、ページャーなどの共通メッセージを任せる。
- **業務固有の表示**：「課題名を入力してください。」などはアプリ側が日本語で指定する。

共通翻訳を導入したからといって、業務固有のラベルや検証エラーが自動生成されるわけではありません。

## Itemsによる一覧の限界
Step25の`RowsPerPage=2`は表示上のページサイズであり、4件は`Items`へ渡されています。全件を取得する方式は小規模なら簡単ですが、大規模データにそのまま適用しません。後続のStep41〜60では`MudTable/MudDataGrid ServerData`、検索、絞り込み、仮想化、競合と性能を比較します。

## 確認問題と解答
**問1：** `AddMudServices()`だけ実行すれば、どんな画面でも通知を表示できますか。

**答え：** いいえ。`ISnackbar`を使う場合、対応するProvider、描画モード、必要な静的ファイルも確認します。

**問2：** メニューを閉じる機能を設ければ、権限のない利用者のアクセスを防げますか。

**答え：** いいえ。表示制御とサービスの認可は別です。

**問3：** `MudDatePicker`の`DateTime?`はPostgreSQLの`date`と同一の型ですか。

**答え：** いいえ。実DB保存では.NET側の`DateOnly`との型変換を検討します。

**問4：** `MudForm.IsValid=true`ならDB更新を許可してよいですか。

**答え：** いけません。フォームの検証は認可やトランザクション制約を保証しません。

**問5：** `MudTable Items`に4件を渡して`RowsPerPage=2`に設定した場合、DBから2件だけ取得されますか。

**答え：** されません。`Items`の4件を渡した後、表が2件ずつ表示します。この教材ではそもそもDBへ接続していません。

## 次のステップ
Step26〜30ではMudDataGrid、Dialog、Snackbar、操作性、総合演習を学びます。**画面の見た目とデータ取得・業務の権限判定は別**という点を引き続き意識してください。
