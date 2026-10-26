# 学習順019：URL・履歴・再読込と画面状態

## 学習の目的
URL・履歴・再読込と画面状態を、WinFormsのイベント処理と比較しながら理解します。ここではWebサーバー上のC#とブラウザーの表示を区別します。

## 前提と動作環境
学習順001〜018で学んだHTTP、HTML、Razorの基本を使います。.NET 10のInteractive Serverを前提とし、DB/ADは使いません。

## 画面で確かめること
`/learn/runtime-04`を開いてください。履歴を選び、操作回数を1増やして再読込する。表示条件だけが残ることを確認する。

## 動作と処理場所
クエリ文字列はブラウザーのURLに残ります。ブラウザーの履歴や再読込で再現できます。C#フィールドにだけ入れた値は永続化されません。

## 主要なC#コード
```csharp
[SupplyParameterFromQuery(Name = "tab")]
[Parameter] public string? Tab { get; set; }
private int count;
// URLパラメーターには公開可能な表示条件だけを含める。
private void Choose(string value) => Navigation.NavigateTo($"/learn/runtime-04?tab={value}");
private void Increase() => count++;
```
Razorの `@onclick` が上のC#メソッドを呼び出します。実装は `src/PgBlazorSamples.Web/Components/Samples/Runtime/Runtime04Page.razor` と対応しています。

## WinForms・Dapper経験者が注意する点
WinFormsではフォーム上の変数がPCのプロセスで維持されます。Interactive Serverでは画面状態がサーバーの対話回線と結び付き、ブラウザーやDBに自動保存されるわけではありません。

## よくある失敗と確認方法
画面が動かない場合はブラウザーのNetworkとConsole、サーバーのログ、再接続の案内を確認してください。表示されているHTMLだけでサーバー側処理の成功を判断しないでください。

## 演習と解答
**演習：** 履歴を選び、操作回数を1増やして再読込する。表示条件だけが残ることを確認する。

**解答：** URLの条件は再読込後も読めますが、コンポーネントのフィールド値は初期化されます。

## 完了確認
- [ ] 画面の表示とボタン操作を確認した。
- [ ] どの状態がサーバー・ブラウザー・DBのどこに存在するか説明できる。
- [ ] 再読込や接続切断に関する注意点を説明できる。

次は[学習順020](runtime-05.md)です。
