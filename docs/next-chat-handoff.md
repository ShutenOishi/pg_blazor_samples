# 引き継ぎ：EF Core 10のWeb内実装とMudBlazor用途別コラム

## 最重要決定
- EF CoreはWeb/Data、Web/Services、Program.csのみ。追加のDomain/Infrastructure/Repositoryプロジェクト不要。
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3でStep31〜40の教材コードとDDL、DBなし時の案内を追加。
- Step40とStep41の間に用途別MudBlazor特別コラムを追加。MudSimpleTable、MudTable、MudDataGrid、各種入力、階層、通知、配置を比較。
- コラムは通常16本＋特別2本＝18本。PDFもStep40とコラム09直後に配置。
- Step41〜60はDB側の検索・ページング・編集・集計・大量データ連携。新プロジェクトでなく同じWebのサービスを拡張する。

## 未検証の区別
- ビルド・画面/ブラウザー検証と本物のPostgreSQL 18への接続検証は別。
- connection stringがないときは起動できるがDB操作は未実施。
- 本番の資格情報はGitへ置かず、開発専用DBにだけDDL/Seedを適用する。

## 次の自律作業
1. 最新PR/CIを確認。ビルド失敗ならログに基づき修復。
2. Linux/Windowsビルド、ブラウザー操作、資料索引、PDFの成功を確定させ正規マージ→main postmerge確認。
3. PostgreSQL 18の専用環境でDateOnly/TimeOnly/Range/xmin/CRUD/トランザクションの統合テストを追加・検証する。
4. Step41〜60のDataGrid ServerDataと大規模一覧の実装へ進む。