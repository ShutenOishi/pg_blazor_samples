# 次のチャット引き継ぎ：Step61〜65のWindows統合認証

## 現在の成果

- Step51〜55：PR #22。編集DTO、Dialog、最大50件一括更新、xmin競合、詳細DTO。
- Step56〜60：期間の重なり検索、Scopedの架空利用者キャッシュ、GroupBy・件数制限つきCSV、EXPLAIN・小標本P95/P99、一覧方式演習。コラム13と学習PDFも同期。
- Web内のData・Servicesを維持し、EF Coreのための層別プロジェクトは作らない。日本語UIとMudBlazor.Translationsを維持。
- Step40と41の間の用途別MudBlazor特別コラムは移動しない。
- AD認証・業務認可はStep61〜70まで未実装。開発用DB以外を扱わない。

## 続き

1. Step56〜60の保護PRについてWindows/Linux/Chrome/PostgreSQL18/PDFを検証してからマージし、main後CIも確認する。
2. Step61〜65でNegotiate/IIS Windows統合認証、SID、DB利用者との対応、未登録・無効利用者拒否、ロール構造を導入する。
3. Step66〜70で画面・サービス双方の業務認可、監査、IIS/Kerberosを完成する。実機AD/IISとCI代替試験の区別を明記する。
4. コラム14、日本語ソース・教材・PDFを同一PRで同期し、CIが成立するまで未完成として扱う。
