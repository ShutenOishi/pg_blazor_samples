# ステップ23：日本語の入力と日付選択

## できるようになること
MudTextField、MudSelect、MudDatePickerを使い、入力値を表示へ同期させます。

## 実際に操作する
1. /learn/23 を開き、架空の氏名を入力します。
2. 「学習テーマ」でDIを選択します。
3. 「学習予定日」を変更し、確認欄の日本語の日付が変わることを確認します。

## ソースと対応する説明
テキスト入力は `@bind-Value`、日付入力は `@bind-Date` でC#の値と同期しています。

```razor
<MudDatePicker Label="学習予定日" @bind-Date="_studyDate"
               Culture="@Japanese" DateFormat="yyyy/MM/dd" />
```

MudBlazorの共通操作メッセージはTranslationsで翻訳し、「学習予定日」のような固有の項目名はアプリ側が日本語で指定します。文化設定と翻訳リソースは異なるものです。

## チェック
- [ ] 文字入力とプレビューが同期した
- [ ] 選択肢の切り替えを確認した
- [ ] 日本語の日付書式を確認した

次：ステップ24
