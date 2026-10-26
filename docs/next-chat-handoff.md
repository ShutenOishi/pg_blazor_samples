# 次のチャットへの引き継ぎ：Blazor Step16〜20

1. 最新のGitHub `ShutenOishi/pg_blazor_samples` mainを取得し、Step01〜15とコラム01〜03のCI/postmerge証跡を確認する。
2. Step16「Optionsパターン」、17「非同期処理とログ」、18「DIと単体テスト」、19「Applicationクラスライブラリ分離」、20「DI総合演習のToDoサービス」を実装する。
3. Step18で初めて`PgBlazorSamples.UnitTests`、Step19で初めて`PgBlazorSamples.Application`を追加する。WebからApplicationへの参照方向を守り、DB・ADを先取りしない。
4. 5ステップ終了に合わせて`docs/columns/column16-20.md`と日本語復習ページを追加する。画面、教材、否定テスト、PDF目次、同一版PDFの整合性を守る。
5. PDFは学習者向けの`blazor_learning_textbook_ja.pdf`を使用。学習者向け本文にレポジトリ、CI、PR、コミットハッシュ、開発管理ファイルなどを出さない。**目次は日本語の学習タイトル**で統一する。検証用ソース一覧は別JSONへ保存。
6. Windows/Linux Releaseビルド・日本語HTTP/Chrome・PDF生成・教材欠落チェックを実行し、成功時のみ保護マージ・main postmerge受入へ進む。
7. 実AD・PostgreSQL・IISの検証をしていない場合は明記する。成功済みの作業は理由なく再実行しない。

使用環境：Visual Studio 2026、.NET 10、Blazor Interactive Server。画面・コードコメント・解説は日本語。学習コラムは5ステップごとに全12本を配置する。
