# ステップ27：MudBlazorの確認ダイアログ

## できるようになること
IDialogServiceとMudDialogProviderを組み合わせ、実行と中止の結果を分けます。

## 前提
Step21のAddMudServices・Provider、Step15のDIを理解します。実DBは不要です。

## 実際に操作する
1. 「/learn/27」で「確認ダイアログを開く」を押します。
2. 「中止」を選び、画面に「中止しました。」と表示されることを確認。
3. 再度開き「実行する」を選び、保存を伴わない結果の表示を確認します。

## 重要なコード
```csharp
bool? confirmed = await Dialogs.ShowMessageBoxAsync(
    "練習用の確認",
    "架空の処理を実行しますか？",
    yesText: "実行する",
    cancelText: "中止");
_result = confirmed == true ? "実行を選びました（保存はしていません）。" : "中止しました。";
```

## よくある間違い
ダイアログ確認だけで保存してよいと考えること。Step51以降では画面の確認後にサーバー側入力検証・操作権限・競合確認を行います。閉じる/中止は変更しない動作を推奨します。

## 演習
文言を変更し、Cancelしたときは処理結果を明示してください。

## 完了条件
- [ ] OKと中止で異なる結果が表示される
- [ ] IDialogServiceとProviderの役割を説明できる
- [ ] 確認UIは認可ではないと説明できる
