# 次チャットへの引き継ぎ：教材v3の30新規ステップと全110本の順次再編

対象: https://github.com/ShutenOishi/pg_blazor_samples

## 2026-10-27 学習順001〜005の全面改稿・第1組受入（最新）
- 新規30/30本の初稿実装は完了。最終全面改稿は**5/110**、第1組復習1/22、第1段階概要1/9。Draft PR #36、旧80URL、既存安全ゲートを維持。
- 実装HEAD `f48474d3882359626d1efedda73ac9737461b121` のApp Run 40370279965、PDF Run 40370279958 は全SUCCESS。Windows/Linux、Chrome、PG18、教材品質検査とPDF公開を確認。
- PDF単体1020439バイト。Release `learning-textbook-v3-preview` の生成元SHAが同HEADと一致。正式main `learning-textbook` は変更しない。
- web-01〜05に比較表・切り分け表、実Razor/C#/HTML/CSSコード、操作の時系列、長文解説、演習と解答を追加。第1段階概要・復習も同水準に改稿。
- 今後の全110本文は`docs/course-v3/full-rewrite-roadmap.md`の「長文教材の標準構成」に従う。初回品質ゲートの対象は001〜005、次の単位にも順次拡張する。
- 次は**学習順006〜010（旧Step01〜04＋runtime-01）と第2段階概要・第2組復習の全面改稿**。初学者がテキストだけで分かるように長文・比較表・2個以上の実コード・手順・失敗例・演習と解答を整備し、RazorとPDF・Web目次・前後リンクを同期する。CI全SUCCESS後だけ改稿10/110へ進める。
- 各回の最終返答で**次回引き継ぎ用プロンプトをコピーするボタン**を付ける。ボタンは次の工程・正本・最新PRコメント・安全条件を含む自立した文面とする。
- 再開時に最新HEADのApp/PDF CIとプレビューReleaseの生成元をGETで照合。記録専用コミットによるCIの最新結果も確認してから次の改稿へ進む。

## 2026-10-27 新規30/30初稿のCI・Preview PDF完了（履歴）
- Draft PR #36で新規30件のRazor/教材初稿を配置・CI受入済み。既存80件のURLは不変、**最終全面改稿0/110**。
- 実装HEAD `790a58ee5b975b288a6707ac7341fc3f873ad9b8` のApp Run 40364677020 / PDF Run 40364677179はSUCCESS。
- 全面改稿計画を追加したHEAD `d7aebd3796fafd3a56fad3751c50c4d003813363` のApp Run 40365372365 / PDF Run 40365372317もSUCCESS。単体PDF952877バイトとRelease `learning-textbook-v3-preview` の生成元SHA一致をGETで検証済み。
- 110本の改稿順・22復習・3特別・9段階概要の5本単位の受入規則は `docs/course-v3/full-rewrite-roadmap.md` が正本。
- **次工程：学習順001〜005と第1段階概要・第1組復習の全文を実装に合わせて全面改稿。** GUI・日本語教材・Web/PDF目次、Chrome・Windows/Linux・PG18を同期し、成功して初めて最終改稿5/110とする。
- 最新HEADの記録更新後に発生したCIとPreview PDFの生成元を再開時に照合し、不整合は修復する。正式main版は上書きせず、Draft PR維持。

## 2026-10-27 総合演習5件の配置・全面改稿準備（履歴）
- Draft PR #36は新規30/30件のRazor・日本語初稿を配置。全110件の最終全面改稿は0/110。
- capstone-01〜05（学習順106〜110）、第9段階概要・第22組復習・Web/PDF目次・Chrome匿名拒否・PostgreSQL18の競合/ロールバック結合試験を追加。
- 22回の5本単位の最終改稿順は `docs/course-v3/full-rewrite-roadmap.md` に確定。次の実装工程は**学習順001〜005と第1段階概要・第1組復習の全面改稿**。
- 今回追加後HEADのCI・PDF/Preview Releaseの最新生成元を必ず認証済みGitHub GETで検証。失敗時は修復。既存の `learning-textbook` 正式版・旧80URL・Draft PRは変更しない。

## 2026-10-27 セキュリティ5件のCI受入・プレビューPDF更新（履歴）
- Draft PR #36：新規初稿25/30。既存80のURL維持。全面改稿0/110。
- security-01〜05（学習順081〜085）、第7段階概要、第17組復習、Razor・Markdown・Web/PDF目次・模擬安全拒否単体テスト・Chrome否定操作を追加。
- 実装HEAD `d16ec614bc8f863cb8feb9a3e0143702e59ea16a`：アプリCI Run 40360843077（Windows/Linux/Chrome/PG18）と PDF Run 40360843042 が全SUCCESS。
- `learning-textbook-v3-preview` に同HEADの日本語PDF単体926066バイトが公開済み。正式mainのRelease `learning-textbook` は上書きしない。
- 再開時にPR HEAD・進捗・最新PRコメント・CI・プレビューReleaseの生成元を照合し、失敗があれば修正する。
- 次はcapstone-01〜05（学習順106〜110）、第9段階概要、第22組復習。実装と日本語教材・Windows/Linux・Chrome・PG18・PDFの同期を行う。
- 30新規配置後は学習順001から全110本文・22復習・3特別コラム・9段階概要を最終全面改稿し、各5件単位で受入する。

## 2026-10-27 第5段階のCI成功とPDF Releases更新（履歴）
- PR #36で新規20/30本の初稿とRazorを実装・CI受入済み。最終全面改稿は0/110、PRはDraft。
- 実装HEAD `34ffe89a2f2ae77d61eb21e3fdf0f58030f45b20`：アプリ/Chrome/Windows/Linux/PG18 Run 40358248623、PDFとReleasesへの反映 Run 40358248514 がSUCCESS。
- `learning-textbook-v3-preview` のPDFは同HEADの生成元をRelease notesで確認済み。ZIP化しない。正式mainのReleaseは変更しない。
- 次の作業はsecurity-01〜05（学習順081〜085）を第7段階に挿入し、第17組復習・第7段階概要・日本語教材・アプリ/Chrome/PG18/PDFを同期。次にcapstone-01〜05（学習順106〜110）。
- 新規30件の配置完了後、全110ステップ・22復習・3特別コラム・9概要を学習順001から全面改稿する。

## 2026-10-27 新規20/30の第5段階実装（履歴）
- PR #36にdb-bridge-01〜05（学習順046/052/053/058/059）、第5段階概要、第10〜12組復習、Razor/教材/Web/PDF/E2Eを追加。新規初稿20/30、最終全面改稿0/110。
- Releasesには正式mainと別タグ `learning-textbook-v3-preview` の単体PDFを公開済み。今後も同一HEADのアプリCI（Chrome/PG18/Win/Linux）とPDF成功の場合に限りPreview Assetを更新する。
- 再開時は最新HEADのCIとPreview Releaseのbodyに記された生成元SHA・Assetを照合し、不整合があれば修復する。
- 次はsecurity-01〜05を学習順に挿入し、概要・復習・安全境界・PDF・テストを同期。

## 2026-10-27 第4段階CI完了（履歴）
- PR #36：新規初稿15/30（web 5、runtime 5、browser 5）。最終全面改稿0/110、旧80画面維持。PRはDraftでmain未反映。
- HEAD `9a9df7e9f9d4c0b763b979958614ff1880fe1072` のアプリ/Chrome/Win/Linux/PG18 CI Run 40353992330、単体日本語PDF Run 40353992474が全SUCCESS。成功済みRunを再実行しない。
- browser-01〜05は学習順041〜045に追加済み。第4段階概要と第9組の復習、PDF、ブラウザーテスト、旧URL・前後リンクを同期し、CI受入済み。
- 次は **db-bridge-01〜05**（学習順046/052/053/058/059）をRazor・Markdown・第5段階概要・第10〜12組復習・安全なPG18実試験・Web/PDF目次とともに実装する。
- 新規30件の配置完了後、全110本＋復習22本＋特別3本＋概要9本を学習順001から全面改稿。最新の状況は `docs/course-v3/progress.md` を参照。

## 2026-10-27 browser初稿作成時の状態（履歴）
- PR #36 は新規15/30本のRazor/教材初稿、最終全面改稿0/110。browser-01〜05（学習順041〜045）、第4段階概要、第9組復習を追加。
- HEAD 6f22c9d のCI：Chromeを含むアプリ Run 40351271699、PDF Run 40351271704が完全SUCCESS。成功済みRunは再実行しない。
- browser追加後の初回CIではWindows/Linuxビルド、PostgreSQL18、PDFが成功、Chromeのbrowser-03結果表示（大文字PDF/小文字.pdf）の断言だけ失敗して修正済み。最新HEADのChrome再検証結果を確認する。browserをCI成功前に受入済みとしない。
- 次は db-bridge-01〜05。学習順046/052/053/058/059に挿入し、第5段階概要・第10〜12組復習・画面・本文・PDF/テストを同期。その後security→capstone。
- 新規30本配置後に全110ステップを学習順001から全面改稿する。最新記録は `docs/course-v3/progress.md`。

## 2026-10-27 以前の進捗（履歴）
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
