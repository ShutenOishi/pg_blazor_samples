# 学習順017：事前レンダリングと初期化の実行回数

## 学習の目的
事前レンダリングと初期化の実行回数を、WinFormsのイベント処理と比較しながら理解します。ここではWebサーバー上のC#とブラウザーの表示を区別します。

## 前提と動作環境
学習順001〜016で学んだHTTP、HTML、Razorの基本を使います。.NET 10のInteractive Serverを前提とし、DB/ADは使いません。

## 画面で確かめること
`/learn/runtime-03`を開いてください。初期化と描画後の回数を確認し、ボタンを押しても初期化回数は増えないことを確かめる。

## 動作と処理場所
事前レンダリングと対話型描画は異なるインスタンスになり得ます。画面内の数値は対話側のインスタンスで計測した値です。事前レンダリング側を含む全呼び出し回数ではありません。

## 主要なC#コード
```csharp
private int initialized;
private int afterFirstRender;
private int changes;
protected override void OnInitialized() => initialized++;
protected override void OnAfterRender(bool firstRender)
{
    // 事前レンダリング時には実行されない。最初の対話型描画後のみ記録する。
    if (firstRender)
    {
        afterFirstRender++;
        _ = InvokeAsync(StateHasChanged);
    }
}
private void Add() => changes++;
```
Razorの `@onclick` が上のC#メソッドを呼び出します。完成画面を操作しながらRazorの表示とC#メソッドを確認してください。

## WinForms・Dapper経験者が注意する点
WinFormsではフォーム上の変数がPCのプロセスで維持されます。Interactive Serverでは画面状態がサーバーの対話回線と結び付き、ブラウザーやDBに自動保存されるわけではありません。

## よくある失敗と確認方法
画面が動かない場合はブラウザーのNetworkとConsole、サーバーのログ、再接続の案内を確認してください。表示されているHTMLだけでサーバー側処理の成功を判断しないでください。

## 演習と解答
**演習：** 初期化と描画後の回数を確認し、ボタンを押しても初期化回数は増えないことを確かめる。

**解答：** 画面のイベントによる再描画と、コンポーネントの生成・初期化は異なります。

## 完了確認
- [ ] 画面の表示とボタン操作を確認した。
- [ ] どの状態がサーバー・ブラウザー・DBのどこに存在するか説明できる。
- [ ] 再読込や接続切断に関する注意点を説明できる。

次は[学習順018](../../samples/step10.md)です。
