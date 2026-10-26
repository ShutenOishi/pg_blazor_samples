# 次のチャットへの引き継ぎ：EF Core 10採用・80ステップとMudBlazorデータ連携の実装

## 最新の決定

- .NET 10 / Visual Studio 2026 / Blazor Interactive Server / MudBlazor 9系 + MudBlazor.Translations（ja-JP）を継続。
- **標準ORMをEF Core 10（Npgsql.EntityFrameworkCore.PostgreSQL 10系）に決定。** 既定のデータアクセス方式にlinq2dbを使用しない。linq2dbは任意の高度SQL性能比較。
- **60→80ステップ**へ拡張。Step01〜25のソースは変更せず、26〜30 MudBlazor UI、31〜40 EF Core基礎、41〜60 一覧・検索・編集の実DB連携20ステップ、61〜70 AD/業務権限、71〜80 運用。
- 通常コラム16本＋Step20後のライブラリDI特別編1本＝17本。5ステップごとにコラムと日本語学習PDFを同期。
- 正本：[80ステップ一覧](sample-catalog.md)、[ロードマップ](learning-path.md)、[EF Core 10ガイド](efcore10-data-access.md)、[一覧・Gridの10方式の選択ガイド](data-grid-patterns.md)。

## 現在の実装と検証の境界

- Step01〜25・コラム01〜06はソースと日本語教材を追加済み。Step21〜25のWindows/Linux CI、Chrome操作、PDFの最新Runは**必ず再確認**。成功済みのRunを勝手に再実行しない。
- Step26〜80のサンプル画面・EF Core・PostgreSQL実接続はまだ作成していない。新ガイドやPDFへの収録だけで「実装済み」「統合試験成功」にしない。
- AD、PostgreSQL 18、Windows Server 2025/IISの実環境検証は未実証。
- 作業PRの元HEADと必須CIを確かめ、成功分のみ保護ルールを守って通常マージ→mainの真正postmergeを確認する。

## 次にやること（優先順）

1. この教材方針変更PRの差分・リンク・PDF生成・CIを確認。失敗があれば修正し、成功した場合だけ正規マージする。
2. Step21〜25の既存ソースの最新検証状況を確定し、Step26〜30の日本語MudDataGrid・Dialog・Snackbar・アクセシビリティ実装とコラム07を完成させる。
3. Step31〜40を実装。Domain/Infrastructure分離、`IDbContextFactory`、Npgsql接続プール、DateOnly/TimeOnly、Range/Multirange、MudDatePicker変換、AsNoTracking/DTO、xmin競合、トランザクション、DB統合テストとコラム08・09。
4. Step41〜60の連携特化20ステップへ進む。MudTable/MudDataGrid Items・ServerData・VirtualizeServerData・キーセット・全文検索の要否・複数ソート・集計・編集・競合・CSV・性能実測・権限について、**要件から選べる比較と実装**にする。
5. 従来の未実装AD/運用テーマはStep61〜80として継続。Windows Server 2025 / IIS / AD実接続の証拠がないうちは成功扱いしない。

## 必須実装ルール

- `DbContext`をBlazor circuitの`Scoped`として長期間共有しない。標準は`IDbContextFactory`で操作単位に生成・破棄。
- `ServerData`ではページ、フィルター、複数ソート、CancellationTokenをサービスへ引き渡し、`CountAsync`と`Items`両方に権限制限を適用。
- ソート列の許可リスト、主キーまで含む安定順序、DTO射影、AsNoTracking、最大ページサイズ、Virtualizeの高さ/描画とDB取得の区別を学習対象にする。
- MudDatePickerの`DateTime?`と`DateOnly?`、MudDateRangePickerの`DateRange`とPostgreSQL Rangeを直接同じ型にしない。境界を試験する。
- 画面の編集状態とDBコンテキストは分け、`xmin`の競合・再読み込み・失敗時の扱いを説明する。
- 日本語UI・コメント・教材・5ステップごとのコラム・PDFを同じPRで追加し、実ソースとの意味的整合をテストする。
- 部分的なPRやCIが未実行なら結果を明記。成功済みRunや公開成果物を再実行・再発行しない。

## 工程の進め方

実装→日本語教材→単体/統合試験→Windows/Linux CI→Chrome操作→PDF→保護PR→必須CI→保護マージ→真正postmergeまで、実行可能な工程を継続する。学習計画と実装ステータスは常に別に扱う。
