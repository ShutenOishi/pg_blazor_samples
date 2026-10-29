# 学習者向けPDFの生成方法

## 学習者に渡すファイル

学習者が読むのは **「Blazor 実践学習テキスト」**（`blazor_learning_textbook_ja.pdf`）です。

- 日本語の学習タイトルを用いた目次
- 学習順001〜110の日本語解説とコード例
- 段階概要9件、復習22件、DI特別コラムとMudBlazor用途別特別コラム
- 次に学ぶこと、操作手順、よくある間違い、理解度チェック

学習者に不要なリポジトリ、PR、Actions、ソースファイルハッシュなどの管理情報は本文に出しません。目次もファイル名ではなく「ステップ11：DI（依存性注入）の基本」など学習テーマを表示します。

## 2026-10-29の初回公開と次回以降の承認

初回統合PR #39はmain `d2c0720c4c2c94ad0e28d412b926d1dd81af3524` にマージ済みです。mainのアプリRun 40776976708、PDF Run 40776976727は同一HEADで全4ジョブSUCCESSとなり、正式`learning-textbook`のPDF Asset 686490493を一度公開しました。これは教材配布の受入であり、PRE-01〜07やSITE-01〜15の現地実機受入ではありません。

**今後、mainへの通常のpushだけでは正式PDFのRelease/Assetを再発行しません。** 再公開には別途明示承認を得たmainの完全なコミットSHAをGitHub Repository Variable `LEARNING_TEXTBOOK_PUBLISH_HEAD` に設定する必要があります。未設定・不一致では公開ステップをスキップします。PDFソースのCI検証と正式Releaseの更新は区別し、既存正式AssetとPreview Assetを承認なく上書きしません。

## PDFの作成

[GitHub ActionsのPDF生成](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build-docs-pdf.yml)でPDFソースを検証します。**正式Releaseへの単体PDF公開には前節のHEAD単位の明示承認を必要とします。標準Artifact（ZIP）は配布に使用しません。**

[Blazor実践学習テキスト（PDF単体ダウンロード）](https://github.com/ShutenOishi/pg_blazor_samples/releases/download/learning-textbook/blazor_learning_textbook_ja.pdf) （2026-10-29の初回配布済み。次回更新は別承認）

ローカルではPython 3.12、Noto CJKフォントと必要なネイティブ依存を導入した後、次のように生成します。

```powershell
python -m pip install "weasyprint==66.0" "markdown-it-py==4.0.0"
python scripts/build_docs_pdf.py
```

出力：`dist/blazor_learning_textbook_ja.pdf`。

## 開発側の整合性管理（学習者には配布不要）

同じ処理で`dist/blazor_learning_source_manifest.json`を生成します。Markdownとソースのハッシュ、コミット情報はそこだけに保存します。PDF内部に載せないことで、教材と開発管理情報の関心を分離します。

コードの意味と文章の整合性は自動ハッシュだけでは保証されません。各ステップの操作テストとレビューが別に必要です。実AD・PostgreSQL・IIS実機テストの完了も意味しません。

## EF Core 10と一覧連携の教材

Step31〜40の日本語教材、コラム08・09を既存のStep30の後へ配置します。Step40と41の間にMudBlazor用途別特別コラムを収録し、その後ろにEF Core 10と一覧連携の参考ガイドを配置します。**Step41〜70の一覧・検索・編集・認証・認可を収録します。実AD/IISの検証は別途必要です。**

## Step40と41の間の特別コラム

学習者向けPDFに「用途から選ぶMudBlazorコンポーネント事典」をStep40・コラム09の直後、Step41より前に挿入します。開発用のパス・PR等は本文で説明しません。
