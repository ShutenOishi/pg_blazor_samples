# ステップ39：xminによる楽観的排他制御

## できるようになること

DbContextのIsRowVersionでPostgreSQLのxminをConcurrencyTokenとして扱います。更新前にVersionを比べ、更新中の競合はDbUpdateConcurrencyExceptionを受けて再読込を促します。

## 実行前の準備

Step31〜33は画面の基本をDBなしで学べます。Step34以降の実DBの操作には、開発専用のPostgreSQL 18、SQLスキーマの適用、接続文字列の環境変数設定が必要です。業務DB、実在利用者データ、資格情報のGit登録は禁止します。

## 実際に操作する

1. 学習画面 /learn/39 を開き、日本語で説明される対象機能を確認します。
2. 別タブで同じ行を読み込んで片側を更新した後、もう片側で更新を試す。
3. 競合したデータを無言で上書きしない。

## ソースコードとの対応

すべて既存WebプロジェクトのData/LearningTask.cs、Data/LearningDbContext.cs、Services/Learning/LearningDbService.csを使います。必要になるまで別の層別プロジェクトやRepositoryの抽象化を追加しません。Razor側は画面と入力、Service側はEF Coreの1操作1DbContext、NpgsqlはPostgreSQL接続と型変換を担当します。

## 失敗時・実務上の注意

- 接続先が未設定なら日本語でその旨を表示し、DB操作が成功したとは扱いません。
- 検索は件数上限と安定した並べ替えを持ち、保存処理は入力を検証します。
- 保存時の競合、DB停止、タイムアウト、キャンセル、認可不足を同一視しません。
- Range型はDBの半開期間と画面上の終了日を変換し、端点の解釈を検証します。

## 確認問題

**Q. EFを使うとDomain/Infrastructureの別プロジェクトを必ず作る必要がある？**

A. いいえ。この教材ではWebのData/とServices/だけで十分です。

**Q. 画面が表示できればPostgreSQL 18への接続試験も合格？**

A. いいえ。明示的な接続・SQL・日付型・競合の実DB検証が必要です。

## 完了条件

- [ ] 別タブで同じ行を読み込んで片側を更新した後、もう片側で更新を試す
- [ ] 競合したデータを無言で上書きしない
- [ ] DBの未設定・エラーと正常系を区別できる

次：ステップ40
