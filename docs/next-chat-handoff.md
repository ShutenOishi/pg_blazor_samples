# 次のチャットへの引き継ぎ：Step21〜25 MudBlazor受入とStep26〜30

## 最優先
- 認証済みGitHubで本PRのWindows/Linux CI、PDF生成、Chrome操作、公式テンプレート生成の実行結果を確認する。
- FAILUREがあれば実際のログを確認して原因を修正し、成功するまで保護PRへ更新する。
- 全必須CI成功時だけGitHubの保護ルールを守り、expected-headを固定して通常マージする。
- mainの真正pushによるpostmergeを確認し、成功証跡をREADME・サンプル一覧へ反映する。未実施を成功と書かない。

## 今回実装したもの
- .NET 10／Visual Studio 2026。既存Step01〜20、Application、UnitTestsは保持。
- Step21〜25：MudBlazor 9.11.0のRazor画面、DI登録、Provider、CSS/JS、Interactive Server。
- MudBlazor.Translations 3.6.0（MudBlazor 9系互換）を追加。AddMudTranslations、AddLocalization、RequestLocalizationのja-JP、既存DefaultThreadCultureを採用。
- MudBlazor.Templates 4.3.0で別の練習用プロジェクトを生成・ビルドするCIと、日本語のテンプレート演習を追加。
- Step21通知、22 AppBar/Drawer、23入力、24日本語フォーム検証、25MudTableとページャー、コラム06。
- PDF目次にStep21〜25、コラム06を追加。PDF本文は日本語の学習テーマを表示し、レポジトリやPR情報を出さない。
- 日本語HTTP/Chromeの操作試験と、従来Step01〜20の回帰試験を同一ワークフローで確認する。

## 重要な留意点
- MudBlazor.Translationsはライブラリの共通表示を翻訳する。日本語入力項目や業務固有の検証文はRazorで明示。全コンポーネントの翻訳が完備していると決めつけない。
- バージョン固定と日本語化について実CIログ・スクリーンショットで確認する。未検証のバージョンは推測しない。
- 公式テンプレートで現在の教材を上書きしない。必要な設定だけを取り込み、別アプリ生成で仕組みを比較する。
- Windows Server 2025 / IIS / AD / PostgreSQL 18 / linq2db は未実接続。実証済みと記載禁止。
- 既存の成功CI・マージ・PDF Artifactは再実施・再発行せず、失敗箇所だけ修復する。
- 本教材の通常コラム12本＋特別編1本は維持。現在コラム01〜06。
- 次はStep26〜30：MudDataGrid、Dialog、Snackbar、テーマ/アクセシビリティ、UI総合演習とコラム07。ソース・日本語教材・PDF・Windows/Linuxビルド・HTTP/Chromeを同一PRで実装する。

## 工程の進め方
- 仕様説明だけで止めず、実装→修復→必須CI→保護マージ→postmergeまで自律的に実行する。
- 真正なCIが走っていない場合は未検証と明記し、手動起動と定期実行待ちを分ける。
