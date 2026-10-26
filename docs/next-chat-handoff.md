# 次チャットへの引き継ぎ：80ステップ完了後の品質確認・実機受入

対象: https://github.com/ShutenOishi/pg_blazor_samples

## 教材v2全面改稿（進行中・2026-10-26）

- 作業対象：[Draft PR #36](https://github.com/ShutenOishi/pg_blazor_samples/pull/36)。mainには未マージ。最新HEADはGitHub認証済みGETで再取得する。既存URL `/learn/01`〜`/learn/80` は維持し、一括改番しない。
- WinForms＋Dapper経験者が初めてWeb開発を学ぶ前提で、既存の**80ステップ・通常コラム16本・特別コラム3本、合計99本すべてを一度は全面改稿**する。追記・置換だけで完了扱いにしない。
- 改稿状況：[docs/curriculum-v2-rewrite-progress.md](curriculum-v2-rewrite-progress.md)。現時点ではStep11〜25の15本、通常コラム03・04・06の3本、DI特別コラム1本の**計19本**が本文改稿済み。PDF・実装・テスト最終受入とは区別する。
- **7段階の概要ページ**は `docs/stages/stage01.md`〜`stage07.md` に作成し、PDFの目次・本文に各段階の先頭として挿入。Webの学習ロードマップとドキュメント目次にも導線を追加。
- Step16〜20ではOptions、CancellationToken/ILogger、固定時計xUnit、Application分離、ITodoServiceとInMemoryTodoServiceを実装と照合して改稿した。Step19の**全60ステップ**は練習サービス上の固定値であり、教材全80ステップと混同しない。
- Step21〜25ではMudBlazor 9.11.0／MudBlazor.Translations 3.6.0のDI、Provider、Interactive Server、日本語化、MudForm、MudTableのItemsとServerDataの違いを実装に合わせて改稿した。
- CI：[.github/workflows/build.yml](../.github/workflows/build.yml)の日本語IPAフォント取得が前回120秒タイムアウトしたため、再試行と取得猶予を修正した。修正後の完了済みSHAでPDF・Windows・Linux（ブラウザー）・PostgreSQL18の4系統が成功したことを確認済み。**PRの最新HEADのCIは必ず再照会し、成功済みRunを無用に再実行しない。**
- 次の作業は **Step26〜30と通常コラム07の全面改稿**。続いてStep31〜40（EF Core・Npgsql）と通常コラム08・09、他の既存ステップ・コラム、追加Web基礎30テーマへ進む。各5ステップ＋対応コラム＋PDF・テストを単位とする。
- PRは引き続きDraft。全99本改稿と新規テーマの整備は未完了のため「完成」「配布済み」と記載しない。日本語PDFはZIPにせず単体で公開する方針を維持する。

## 既存80ステップの確定状態（2026-10-26 JST）

- Step01〜80のRazor画面・日本語教材・5ステップごとの復習コラムを実装。Step76〜80はPR #33（https://github.com/ShutenOishi/pg_blazor_samples/pull/33）を通常マージ済み。マージSHA: `1f4361478d3a57a7355c2b7f645d4d11de94149b`。
- PR #33のHEAD `125d0d3753dbce5d3ed09ffdf82f41e336bf2d90` でWindows 2025/Linux Release・xUnit・Chrome・PostgreSQL18・PDF生成CIは成功済み。
- main/push PDF成功Run: https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40329086098 。単体PDFは https://github.com/ShutenOishi/pg_blazor_samples/releases/download/learning-textbook/blazor_learning_textbook_ja.pdf へ反映済み（生成元SHAは上記マージSHA）。ZIP化しない。
- main/pushビルドRun: https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40329085870 。再開時に最終結果を照会し、失敗があれば優先修復する。

## 続きで実施すること

1. 必ず最新の認証済みGitHub GETでmain、PR、GitHub Actions、Release実物・単体PDFの生成元SHAを再照合する。完了したCI/Releaseの不必要な再実行・再発行はしない。
2. 教材80ステップの内容・Razor画面・日本語PDF・ソースコード・xUnit/Chrome/実DBテストの整合性を改善する。CIの不足や実装不具合が見つかれば保護PR・必須CI・通常マージ・postmergeまで自律的に修復する。
3. 実機受入は別扱い：Windows Server 2025/IIS・Kerberos/SPN・証明書・明示Cookie鍵共有と失効・SignalR・FDW/外部台帳・権限行ロック・監査の永続性・10,001人以上の台帳・負荷を現地で検証し、証跡がある場合だけ受入済みとする。

## 決して変えない前提

- .NET 10、EF Core 10/NpgsqlをWebプロジェクト内で使用、MudBlazor.Translations、日本語UI/教材、5ステップごとのコラム。
- ADは認証とドメイン付きユーザー名のみ取得。アプリに登録UIは作らず、事前整備済みの既存DB/外部テーブルを権限の正本として都度認可する。
- 更新は権限行ロック・xmin・監査を同一トランザクションで保証。FDW/別DBで原子性が証明できない場合はフェイルクローズ。機密業務データをキャッシュせず、application_nameを個人監査の代用としない。
- PDFは単体 `blazor_learning_textbook_ja.pdf` として配布。ZIPにしない。実AD/IISを模擬PrincipalのCIで代用したと主張しない。
