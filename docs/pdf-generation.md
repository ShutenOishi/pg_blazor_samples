# PDFの生成・同期運用

このリポジトリのPDFは**GitHub上のMarkdownを正本**とし、実行時にリポジトリのファイルを読み直して作成します。コピーして手編集したPDFを正本にはしません。

## 作り方

### GitHubで生成

[GitHub Actions](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build-docs-pdf.yml) の「日本語ドキュメントPDF生成」を開き、「Run workflow」を実行します。mainへの資料・ソース変更が入った場合も自動生成されます。実行成功後、Runの「Artifacts」から `pg_blazor_samples_technical_handbook` を取得します。成果物の保存期間は30日です。

### ローカルで生成

Python 3.12および日本語フォント（Noto Sans CJK JPなど）が必要です。

```powershell
python -m pip install "weasyprint==66.0" "markdown-it-py==4.0.0"
python scripts/build_docs_pdf.py
```

結果は `dist/pg_blazor_samples_technical_handbook.pdf` です。Windows上ではWeasyPrintのネイティブ依存関係を別途準備する必要があるため、GitHub Actions上のUbuntuランナーでの生成を標準とします。

## ソースコードとPDFの整合性

- PDF生成時のGitコミットを表紙と付録に記録します。
- `src/`、`tests/`、`database/` にソースがあれば**存在したファイルパスとSHA-256の先頭16桁**を付録に記録します。
- ソースが存在しない場合は**実装未着手**と明示します。
- PDFは `README.md`、`docs/`、`SECURITY.md` 等のMarkdownを**直接読み込む**ため、編集したMarkdownがそのまま次回PDFへ反映されます。
- Step05、10の学習コラムを収録済み。以後も5ステップごとのコラムをソース・教材と同一変更で生成対象へ追加します。
- ドキュメントとコードの意味的な整合性は自動では証明できません。PRレビューでは説明対象の実ファイル・クラス・メソッド、画面動作、ビルド・テスト結果を確認してください。
- 結合試験が未実施なら、PDFに「Windows Server 2025/AD環境で確認済み」と記載してはいけません。

## 利便性と公開情報の注意

生成PDFはGitHub ActionsのArtifactsに30日間保存されます。**本番のAD構成情報、アカウント、接続文字列、ユーザー情報、社内サーバー名を公開リポジトリへ記載しないでください。** 開発用の例は架空値に限定します。

PDF生成失敗の場合は旧PDFを最新と見なさず、該当ワークフローRunのエラーを修正します。実装に変更があり資料修正が必要な場合は、両者を同一PRに含めます。
