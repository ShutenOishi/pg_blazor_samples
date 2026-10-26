# ステップ50：検索UX・障害対策

## できるようになること

検索待機・取消・0件・DB障害・再試行を区別する。Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor 9、EF Core 10を使用します。

## 前提と実行環境

Step41〜45のServerDataとEF Coreの基礎を終えてから進みます。この画面のDB結果を確認するにはPostgreSQL 18の開発用DBを設定します。

## 画面で操作する
1. ステップ50を開き、短い語を入力します。
2. 続けて別の文字を入力し、入力中に毎回即DBへ問合せしないことを確認します。
3. 400ミリ秒止まると検索条件が適用されます。
4. 「再読み込み」を押し、DB未設定・0件・実DB取得の区別を確認します。

## ソースの読み方
MudTextFieldはImmediate=trueで文字入力を通知しますが、Task.Delay(400, token)中に新たな入力が来ると前回の待機を取消します。検索用の値を確定してからMudDataGrid.ReloadServerDataを呼びます。画面のCancellationTokenSourceはDisposeで破棄します。

DBアクセス自体は既存のLearningPageService.LoadAsyncが担当し、DbContextFactory、CountAsync、ToListAsyncへMudDataGridのCancellationTokenを渡します。

## 実務のUX判断
| 状態 | 画面の案内 |
| --- | --- |
| 入力が続く | 検索を待機・前の待機を取消 |
| 一致行なし | 条件に合う課題はありません |
| DB未設定 | 開発用DBが未設定 |
| DBエラー | 読込失敗を日本語で案内 |
| 利用者の再試行 | 「再読み込み」からサーバー再取得 |

## 安全性
最大100文字を超える語をDBへ渡しません。画面に見える行を絞ることは業務認可ではありません。認可は後続の学習範囲でサーバー側のWhereに適用します。

## 確認問題と答え
**Q. デバウンスだけで古いDB検索結果を必ず止められる？** いいえ。ServerDataからDBまでCancellationTokenを渡す必要があります。

## 完了条件
- [ ] 待機時間・取消・再読み込みを確認した
- [ ] 空表示と障害表示を区別できる


## 次に学ぶ内容

コラム11で5方式の使い分けを整理し、Step51から編集・保存処理に進みます。
