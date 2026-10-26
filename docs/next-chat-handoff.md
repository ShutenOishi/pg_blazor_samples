# 次のチャット引き継ぎ：Step51〜55の編集・競合・詳細表示

## 維持する方針
- Step46〜50を実装。Step46 DTO JOIN、47 IDキーセット、48 Itemsの描画仮想化、49 VirtualizeServerData、50 デバウンス・再試行。
- EF Core 10のために層別プロジェクトを増やさず、Web内のDataとServicesを活用する。
- 日本語UI、MudBlazor.Translations、日本語ソースコメント、5ステップごとのコラム、学習者向け日本語PDFを維持。
- Step40と41の間にあるMudBlazor用途別特別コラムは移動・削除しない。
- DB実操作は開発用PostgreSQL 18のみ。AD本人認証と業務認可はStep61〜70で実装。画面フィルターを認可扱いしない。

## 続き
1. 最新main、PR、CI（Windows/Linux、ブラウザー、PostgreSQL 18、PDF）を確認し、未成立ならログから修復。
2. Step51〜55の編集DTO、Dialog、選択と一括操作、xmin競合、詳細画面・N+1回避を実装。
3. コラム12・日本語PDF・画面操作・実DBの否定試験を同じPRで追加し、保護マージ・main後のCIを確認。
