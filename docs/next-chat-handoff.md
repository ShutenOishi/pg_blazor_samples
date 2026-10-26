# 次チャットへの引き継ぎ：教材v3の30新規ステップと全110本の順次再編

対象: https://github.com/ShutenOishi/pg_blazor_samples

## 2026-10-27 最新進捗（最優先）
- PR #36で新規 Web 5件と runtime 5件の初稿・Razorを整備済み。WebのCI成功はHEAD 9e12130のRun 40348357127／40348357519。runtimeの新HEADは必ずCI結果を確認する。
- runtime-01〜05は学習順009/015/017/019/020に配置し、第2段階概要・復習第2〜4組・PDF・ブラウザー試験を同じ変更で同期。新規10/30の初稿実装であり、最終改稿0/110のまま。
- 次は新HEADのCI結果確認・不整合修復。成功後に browser-01〜05を学習順041〜045へ実装し、その後db-bridge→security→capstoneへ進む。
- 正本は `docs/course-v3/progress.md` と `docs/course-v3/curriculum.json`。本文・概要・索引・Razor・テスト・単体PDFを同一変更にする。

## 最優先：教材v3の再編・新規実装を先行する

- [9段階・110ステップの再編計画](course-v3/implementation-plan.md)と[110件の詳細順序](course-v3/learning-sequence.md)、[実装・最終改稿の進捗](course-v3/progress.md)を正本とする。
- **新規30ステップをまず正しい位置に実装**し、その後に既存80ステップと新規30本、各5件の復習22本、特別コラム3本、段階概要9本を**学習順001から**全件書き直して検証する。
- 旧Step11〜30とコラム5本の先行改稿は保存したまま、最終改稿は未受入とする。以前の「次はStep31〜35を改稿」という指示は破棄。
- `web-01`〜`web-05`のRazor/Markdown、復習、Web先頭の目次とPDF索引・ブラウザー検証をPR #36へ追加済み。**CI結果を再照合し、失敗を修復するのが次の作業**。その後 runtime→browser→db-bridge→security→capstoneの順に新規残り25件を実装。
- 9段階の概要草稿は`docs/course-v3/stages/`。第1段階の概要はWeb基礎5件とともにPRのPDFへ先行収録済み。残り8段階は計画用で、完成後に現行7段階から順次切り替える。未実装のページをPDFへ載せない。
- 既存`/learn/01`〜`/learn/80`は維持。新しい学習順番号と旧識別子を分離し、`curriculum.json`を並び順の正本とする。日本語PDF単体配布、未検証の実AD/IIS受入を成功扱いしない。

## 以前の教材v2先行改稿（履歴、実施順は失効・2026-10-26）

- 作業対象：[Draft PR #36](https://github.com/ShutenOishi/pg_blazor_samples/pull/36)。mainには未マージ。最新HEADはGitHub認証済みGETで再取得する。既存URL `/learn/01`〜`/learn/80` は維持し、一括改番しない。
- WinForms＋Dapper経験者が初めてWeb開発を学ぶ前提で、既存の**80ステップ・通常コラム16本・特別コラム3本、合計99本すべてを一度は全面改稿**する。追記・置換だけで完了扱いにしない。
- 改稿状況：[docs/curriculum-v2-rewrite-progress.md](curriculum-v2-rewrite-progress.md)。現時点ではStep11〜30の20本、通常コラム03・04・06・07の4本、DI特別コラム1本の**計25本**が本文改稿済み。PDF・実装・テスト最終受入とは区別する。
- **7段階の概要ページ**は `docs/stages/stage01.md`〜`stage07.md` に作成し、PDFの目次・本文に各段階の先頭として挿入。Webの学習ロードマップとドキュメント目次にも導線を追加。
- Step16〜20ではOptions、CancellationToken/ILogger、固定時計xUnit、Application分離、ITodoServiceとInMemoryTodoServiceを実装と照合して改稿した。Step19の**全60ステップ**は練習サービス上の固定値であり、教材全80ステップと混同しない。
- Step21〜30ではMudBlazor 9.11.0／MudBlazor.Translations 3.6.0のDI、Provider、Interactive Server、日本語化、MudForm、MudTable/MudDataGridのItemsとServerData、Dialog、Snackbar、操作性と架空の総合演習を実装に合わせて改稿した。
- CI：[.github/workflows/build.yml](../.github/workflows/build.yml)の日本語IPAフォント取得が前回120秒タイムアウトしたため、再試行と取得猶予を修正した。修正後の完了済みSHAでPDF・Windows・Linux（ブラウザー）・PostgreSQL18の4系統が成功したことを確認済み。**PRの最新HEADのCIは必ず再照会し、成功済みRunを無用に再実行しない。**
- 次の作業は **Step31〜35と通常コラム08の全面改稿**。続いてStep36〜40と通常コラム09、他の既存ステップ・コラム、追加Web基礎30テーマへ進む。各5ステップ＋対応コラム＋PDF・テストを単位とする。
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
