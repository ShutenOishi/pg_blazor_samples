# ステップ30：日本語業務UI総合演習

## できるようになること
MudDataGridと検索・入力・確認ダイアログ・Snackbarを合わせ、小規模な架空の利用者管理画面を構成します。

## 前提
Step26〜29を完了。現時点では認証/DB接続なし。業務の利用者情報を入力してはいけません。

## 実際に操作する
1. 「/learn/30」で架空の3名を確認。
2. 「利用者名を検索」に「練習」を入力すると1名に絞られます。
3. 「追加する架空の利用者名」が空ならエラーになることを確認。
4. 「テスト 四郎」と入力→「確認して追加する」→中止では件数が変化しないことを確認。
5. 再度「追加する」では行が増え、日本語のSnackbarと本文の結果が現れます。

## 重要なコードと連携の境界
```csharp
private bool Matches(PracticeUser user) =>
    string.IsNullOrWhiteSpace(_keyword)
    || user.Name.Contains(_keyword.Trim(), StringComparison.OrdinalIgnoreCase);
private Func<PracticeUser, bool> Filter => Matches;
```
現段階は`Items`、架空データ、メモリ内編集です。後続の実務アプリでは、UIの表示モデル→Applicationのサービス→EF Core 10/Npgsql（DbContextFactoryによる1操作1Context）→PostgreSQL 18へ進化させます。画面内にDbContextを保持せず、無制限の全件取得はしません。

## 間違いやすい点
- 画面の確認ダイアログがあるからといって権限があるとは限りません。
- 画面の行数とDBの総件数は同じ概念ではありません。
- フィルター文字列や列名はEF Core側へ無検証で渡さないでください。
- 複数利用者の編集で`xmin`などの競合検知を行うのはStep54です。

## 演習
架空の部署を変更できるようにし、追加した行が日本語検索に引っ掛かることを確認します。

## 完了条件
- [ ] 検索、未入力拒否、中止、確認後の追加を動作確認
- [ ] 一覧の取得方法を切り替えるタイミングを説明できる
- [ ] DBとADの実接続ではないことを明確に説明できる
