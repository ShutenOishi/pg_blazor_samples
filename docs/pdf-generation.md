# 学習者向けPDFの生成方法

## 学習者に渡すファイル

学習者が読むのは **「Blazor 実践学習テキスト」**（`blazor_learning_textbook_ja.pdf`）です。

- 日本語の学習タイトルを用いた目次
- ステップ01〜25の日本語解説とコード例
- 5ステップごとの復習コラム01〜04とコラム06、特別コラム05
- 次に学ぶこと、操作手順、よくある間違い、理解度チェック

学習者に不要なリポジトリ、PR、Actions、ソースファイルハッシュなどの管理情報は本文に出しません。目次もファイル名ではなく「ステップ11：DI（依存性注入）の基本」など学習テーマを表示します。

## PDFの作成

[GitHub ActionsのPDF生成](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build-docs-pdf.yml)で最新mainから自動作成します。成功したRunのArtifactsに`blazor_learning_textbook_ja`があります。

ローカルではPython 3.12、Noto CJKフォントと必要なネイティブ依存を導入した後、次のように生成します。

```powershell
python -m pip install "weasyprint==66.0" "markdown-it-py==4.0.0"
python scripts/build_docs_pdf.py
```

出力：`dist/blazor_learning_textbook_ja.pdf`。

## 開発側の整合性管理（学習者には配布不要）

同じ処理で`dist/blazor_learning_source_manifest.json`を生成します。Markdownとソースのハッシュ、コミット情報はそこだけに保存します。PDF内部に載せないことで、教材と開発管理情報の関心を分離します。

コードの意味と文章の整合性は自動ハッシュだけでは保証されません。各ステップの操作テストとレビューが別に必要です。実AD・PostgreSQL・IIS実機テストの完了も意味しません。

## EF Core 10と一覧連携の先取りガイド

標準ORMはEF Core 10へ変更しました。Step31〜80は現時点で未実装のため、PDFへ存在しない`stepNN.md`を実装済み教材として追加しません。ただし、学習者がデータ連携の設計判断を先に理解できるよう、[EF Core 10の役割分担・寿命管理・型](efcore10-data-access.md)と[DataGridの10方式比較・選択](data-grid-patterns.md)を**『先取りガイド（実装予定）』**として既存Step25/コラム06の後に収録する方針です。各ステップが実装された際、StepNN/5ステップごとのコラムを正しい位置に追加します。

学習本文は日本語の題名とコード例に限定し、GitHub・PR・ハッシュなど管理情報は従来どおり別のマニフェストに分離します。

## Step40と41の間の特別コラム

学習者向けPDFに「用途から選ぶMudBlazorコンポーネント事典」をStep40・コラム09の直後、Step41より前に挿入します。開発用のパス・PR等は本文で説明しません。
