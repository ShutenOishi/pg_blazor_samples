# 実装計画・マイルストーン（80ステップ）

## 目的と実態

- **標準**：Visual Studio 2026 / .NET 10 / Blazor Interactive Server / MudBlazor 9系 + MudBlazor.Translations（ja-JP）/ **EF Core 10 + Npgsql** / PostgreSQL 18。
- 最終実行環境：ADドメイン内のWindows Server 2025 + IIS。画面は日本語、ソースの説明コメントも日本語。
- **実装済みソース：Step01〜25とコラム01〜06**。Step21〜25のCI結果は必ず最新Runで確認し、ソース追加だけで検証済みと表記しない。
- Step26〜80、実PostgreSQL接続、実AD/IIS検証は未実装または未実証。今回の変更は**教材の採用方針と計画**の更新であり、EF Coreソースのビルド・DB実行済みを意味しない。
- 学習教材・説明・画面実装・コラム・PDFを同じPRで更新し、ソースが存在しない設計例を「実装済み」としない。

## フェーズ構成

| フェーズ | ステップ | 実装テーマ | 状態 |
| --- | --- | --- | --- |
| 1 | 01〜10 | Razor、バインド、フォーム | ソース作成済み |
| 2 | 11〜20 | DI、ライフタイム、単体テスト、Application | ソース作成済み |
| 3 | 21〜30 | MudBlazor Templates・翻訳・日本語UI・DataGrid基本 | 21〜25ソース作成済み、26〜30計画中 |
| 4 | 31〜40 | Domain、Infrastructure、EF Core 10、Npgsql、日付・Range、変更追跡、排他、トランザクション | 計画中 |
| 5 | 41〜60 | データ一覧・検索・編集を20ステップで網羅（選択の根拠と性能実証） | 計画中 |
| 6 | 61〜70 | AD認証とDB権限、IIS/Kerberos | 計画中 |
| 7 | 71〜80 | 大量処理、監視、キャッシュ、テスト、Windows Server 2025配備 | 計画中 |

## 優先実装順

### 次回：Step21〜25受入 → Step26〜30

1. Step21〜25の実体・CI・PDF Artifact・Chrome操作の成功可否を最新Runで確認し、失敗している場合のみ必要箇所を修正。
2. Step26は**DBなし**でMudDataGridの`Items`、フィルター、ページャー、ローカル処理の限界を学ぶ。
3. Step27〜30はDialog、通知、アクセシビリティ、モックの総合画面を実装。コラム07とPDFへ反映。
4. CIのWindows/LinuxビルドとChrome操作が成功した範囲だけ「検証済み」に変更。

### DB基盤：Step31〜40

1. Step31 Domain、32 PostgreSQL DDL・架空データ、33 InfrastructureとEF Coreマッピング。
2. Step34で`IDbContextFactory`、BlazorのScopedとの違い、Npgsql物理接続プールとDbContextプールを比較。
3. Step35〜36でAsNoTracking/DTO射影、CREATE/UPDATE/DELETE、追跡とNoTrackingを学ぶ。
4. Step37でDateOnly/TimeOnly、MudDatePickerの変換、Range/Multirangeの実DB型演算。
5. Step38〜39でトランザクション、`xmin`競合、二重保存、キャンセル・プール・負荷。
6. Step40で隔離したPostgreSQL 18上のIntegrationTestsと実行計画。コラム08、09とPDFを追加。

### 最優先の連携編：Step41〜60

[一覧表示10パターン比較](data-grid-patterns.md)のすべてを実際のEF Core 10サービスに接続し、**上級のDataGridがなくても使える設計を含めて**適切な選択を学習する。

- 41〜45：Items・MudTable ServerData・MudDataGrid ServerData・安全なソート・検索。
- 46〜50：DTO射影、オフセット/キーセット、描画仮想化、VirtualizeServerData、初回検索とエラーUX。
- 51〜55：編集DTO、Dialog、選択・一括承認、`xmin`競合、階層データ。
- 56〜60：DateRange・DB Range、利用者別キャッシュ・権限、集計/CSV、実測/性能改善、選定演習。

**各5ステップでコラムを1本追加**（コラム10〜13）。最終演習は「どの方式を選ぶか、選ばなかった方式はなぜ不適切か」を説明する提出課題にする。

### 認証・運用：Step61〜80

現行の未実装Step41〜50を61〜70へ、51〜60を71〜80へ移動する。ADによる本人認証、DBでの業務認可、認証なし時の既定拒否、Kerberos、IIS、運用・性能を学ぶ。学習用の架空SIDは実AD接続成功の証拠としない。

## ソースと資料の合格条件

- [ ] 各ステップに`/learn/NN`の画面、`docs/samples/stepNN.md`、日本語UI・注釈、演習・失敗ケースがある。
- [ ] 5ステップ到達時は対応コラムの`Razor`画面・Markdown・PDF章を必ず追加する。現在は通常16回＋特別編1回を計画。
- [ ] EF Coreは10系、Npgsql EF Coreプロバイダーは互換する10系の安定版パッチを固定し、Restore/Releaseビルド/依存関係監査を行う。
- [ ] DB利用ステップの検証には**本物のPostgreSQL 18**での正常/異常系、Range、競合、索引、キャンセルを含む。
- [ ] 41〜60ではGridの`Items`/`ServerData`/`VirtualizeServerData`とサービス側`IQueryable`の責務を説明し、CountとItemsの権限制限を実装・検査。
- [ ] 公開Runnerから実ADへ接続しない。AD・IIS運用は専用の環境で別途検証する。
- [ ] 成功済みのCI・PDF Artifactを不要に再実行しない。次のPRで対象変更を含む検証のみを実行する。

## 参照

- [80ステップ一覧](sample-catalog.md)
- [ロードマップ](learning-path.md)
- [EF Core 10学習ガイド](efcore10-data-access.md)
- [MudBlazor一覧とデータ連携の選択ガイド](data-grid-patterns.md)
- [コラム計画](columns/README.md)
- [PDFのソース同期](pdf-generation.md)
