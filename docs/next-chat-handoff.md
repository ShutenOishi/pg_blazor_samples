# 次のチャット引き継ぎ：Step41〜45のEF Core・MudBlazor一覧連携

## 必須方針
- EF CoreはWeb内のData/LearningPageQuery.csとServices/Learning/LearningPageService.csで完結し、Domain/Infrastructure等の層別プロジェクトは作らない。
- Step41は小規模Items。Step42 MudTable ServerData、Step43 MudDataGrid ServerData、Step44複数列ソートの許可リスト、Step45日付/状態/文字列の複合検索。
- ServerDataはCancellationTokenをEFのCountAsync/ToListAsyncまで伝え、クエリはDTOへ射影。最大50件/ページ、ソート最大3列、最終主キーを追加。
- PostgreSQL 18のCI結合テストでページ境界、Count、ソート・検索、異常条件、キャンセルを確認する。
- コラム10と日本語PDFをコードに対応させる。既存のStep40→MudBlazor用途別特別コラム→Step41の順序は変えない。
- 画面のフィルターは業務認可ではない。AD/DB業務認可は後続のStep61〜70。実DBに認可条件を適用する際はCountとRowsの双方に適用する。

## 続き
1. 現在のPR・CIの最新headを確認し、不成立ならログから原因修復。
2. Windows/Linux/Chrome/実PostgreSQL 18/PDFの受入後、保護マージとmainのpostmergeを確認。
3. Step46〜50のDTO関連表、キーセットページング、仮想化、VirtualizeServerData、検索UXを実装する。
