# 教材v3 再編・実装・最終全面改稿の進捗

## 2026-10-27 最新HEAD最終受入とPDF生成一時停止（現在の運用方針）

- **停止していたChatの作業を認証済みGitHub GETで再照合済み**。記録更新後のPR #36 HEAD `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71` は Draft/open のまま、main `3121697d38caf8341557a841753b21876881bbb1` に未マージ。
- 同一HEADの [アプリCI Run 40464102017](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40464102017) **SUCCESS**（Windows Server 2025、Ubuntu 24.04、Releaseビルド、xUnit、実Chrome操作、PostgreSQL 18実DB結合、既存安全拒否）。[日本語PDF CI Run 40464102117](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40464102117) も**SUCCESS**（教材整合・日本語PDF生成・Preview更新）。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) の生成元SHAは同HEAD。日本語PDF単体1件、Asset ID `680751957`、**1,468,826バイト**、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`。Release GETとAsset個別GETのname/size/digestを照合。ZIPなし、正式Releaseは変更なし。
- **確定進捗：新規初稿30/30、最終全面改稿50/110、通常復習10/22、段階概要5/9**。第10組046〜050・第5段階概要を受入済み。
- **ユーザーからPDF作成・更新の明示的な再開指示があるまでPDF生成を停止する。** PR #36の`.github/workflows/build-docs-pdf.yml`はPDF生成ジョブをスキップする設定。今後の追加改稿でPDFスクリプトを実行せず、日本語PDFのビルド・再生成・PreviewのAsset更新・正式Release再発行・ZIP化をしない。現状のPreview PDFは保持する。
- **開発作業と検証は停止しない。** Markdown教材、Razor/C#、Web目次、NavMenu、前後リンク、`scripts/build_docs_pdf.py`内の索引の整合、`scripts/check-learning-materials.py`による静的品質検査、Windows/Linux・xUnit・Chrome・PostgreSQL18・安全拒否のアプリCIを継続する。**PDF停止中の新しいHEADではPreview PDFの生成元SHAが旧HEADのままでも正常であり、不一致を修復しない。** 新しいHEADの受入ではアプリCI全SUCCESSと静的教材検査を条件とし、PDF生成・配布検査は保留として明記する。
- 次工程は**学習順051〜055（旧Step35、db-bridge-02、db-bridge-03、旧Step36・旧Step37）と第11組復習**。実Razor/C#/SQLに合わせて各本文3,300文字以上・実コード2例以上・WinForms＋Dapper比較・診断表・操作・失敗例・詳しい解答を整え、アプリCIで検証する。後続051〜055の終了時も次回用の簡潔な継続プロンプトを作る。
- **この運用方針の更新によってPR HEADが変わる。** 新HEADのアプリCI成功とPDFジョブが実行されないことを確認する。ただし停止中のPDF AssetをHEADに追随させてはいけない。旧80URL・Draft PR・main・既存セキュリティ拒否条件を維持し、実AD/IIS/Kerberos/FDW/大規模実機をCI成功と混同しない。


## 2026-10-27 第10組046〜050の実装受入・第5段階概要改稿

- **実装受入HEAD `c5ce7d4cf07059c3d21a4ae12e1cb1fc954e3fcc`**。アプリ [Run 40463569089](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40463569089) はWindows Server 2025とUbuntu 24.04 Release、xUnit、Chrome実操作、PostgreSQL18実DB統合・既存安全拒否試験のジョブがすべてSUCCESS。日本語PDF [Run 40463568971](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40463568971) は教材品質・PDF生成・Preview更新を含めSUCCESS。両Runのhead_shaは同じ実装HEAD。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) は上記実装HEADから生成。単体`blazor_learning_textbook_ja.pdf`、Asset ID **680739783**、**1,468,825バイト**、SHA256 **`0993dc3dc202b2868e31c521f88c435dcf98dd2b788a1d2225ec4a2324bf25b4`**。Release本文とAsset個別GETのname・size・digest一致。ZIPなし、正式Release更新なし。
- 学習順046〜050の各教材本文は**3,904／4,001／4,148／4,164／4,229文字**。第10組復習4,066文字、第5段階概要3,958文字。実Razor/C#/SQL、WinForms＋Dapper比較表、障害切り分け表、実操作、失敗例、演習と詳しい解答を実装と照合。Razor、Web・PDF目次、NavMenu、前後リンク、教材品質検査、Chromeを同期。
- **実装内容の受入：新規初稿30/30、最終全面改稿50/110、通常復習10/22、段階概要5/9。** 第5段階全15本文の完成とは区別する。旧80 URL、Draft PR #36、main、正式Releaseを維持。
- **次工程：学習順051〜055（旧Step35、db-bridge-02、db-bridge-03、旧Step36・37）と第11組復習**。各本文3,300文字以上を目安に実コード2例以上、比較・診断表、操作・詳答・安全拒否、Web/PDF/Chromeと同一HEADで受入する。
- **本記録とcurriculum.jsonの更新で新HEADになる。記録更新後のHEADも新しいアプリ/PDF CIの全SUCCESSとPreview PDF生成元SHA・Asset個別GETを照合し、最終新HEAD証跡はPRコメントで記録する。** 前HEADの成功を新HEADの成功と見なさない。実AD・IIS・Kerberos・FDW・1万人規模実機試験は未受入。


## 2026-10-27 学習順046〜050・第10組復習・第5段階概要（実装時点の記録：受入は上記を参照）
- 対象：db-bridge-01、旧Step31〜34、第10組復習、第5段階概要。Razor、Data/LearningTask、LearningDbContext、LearningDbService、Program.cs、PostgreSQL18開発用DDLを照合。各本文3,300字以上、実コード2例、WinForms＋Dapper比較・障害切り分け・画面操作・詳しい解答を整備。
- 画面の学習順046〜050と比較・診断表、第5段階概要と第10組復習、ホームとNavMenu、第10組の配置、Web/PDF目次、Chrome実操作とscripts/check-learning-materials.pyの品質条件を同期。
- **同一HEADのアプリCI、日本語PDF CI、Preview単体PDFの生成元SHA・Asset digestの最終受入前なので、確定進捗は45/110、9/22、4/9のまま**。全SUCCESSの受入後のみ50/110、10/22、5/9へ更新する。記録更新コミット後も最新HEADの全CI・Previewを再照合する。
- 後続：学習順051〜055（旧Step35、db-bridge-02、db-bridge-03、旧Step36〜37）、第11組復習。旧80 URL、Draft PR #36、main、正式Release、個人監査・DB台帳の都度認可、安全拒否を維持。実AD/IIS/Kerberos/FDWと1万人規模の実機試験は未受入。


**現在の確定進捗：新規初稿30/30、最終全面改稿50/110、通常復習10/22、段階概要5/9。**

**2つの進捗を分けて追跡します。** 以前の教材v2で本文改稿した成果は無効化せず保存しますが、新しい前提順で読み直すまでは最終改稿と数えません。

## 2026-10-27 学習順041〜045・第9組復習・第4段階完了の受入

- **教材実装の受入HEAD** `3cab5c0fc430ea473675c6eb4acf04e0ead7103b`。アプリ [Run 40447468064](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40447468064) はWindows Server 2025/Linux Release・xUnit・Chrome実操作・PostgreSQL18実DB結合と既存安全拒否の全ジョブSUCCESS。日本語PDF [Run 40447468058](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40447468058) も教材品質ゲート・PDF生成・Preview公開SUCCESS。
- **Preview Release** [learning-textbook-v3-preview](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) は生成元HEAD一致。単体 `blazor_learning_textbook_ja.pdf`、Asset ID `680389176`、**1,416,502バイト**、SHA256 `8514f1eaa4c3bbe0cf13416615329de73ee8d23cfff2bed7afce247898cfb50f`。Release GETとAsset個別GETのname・size・digest一致、ZIPなし。
- 学習順041〜045（browser-01〜05）の全面改稿は各**5,066／5,283／5,668／5,638／5,744文字**、第9組復習**5,843文字**。実Razor/C#/JS、WinForms＋Dapper比較、診断表、実操作、拒否、演習・詳しい解答を収録。第4段階概要、5画面Razorの比較表、復習画面、ホーム/NavMenu、前後リンク、Web/PDF索引、品質ゲート、Chromeを整合。
- 初回実装HEAD `a9815703638d22484f8b1c6f7c417338549d916a` のChrome検査は学習順のゼロ埋め欠落（41対041）でFAILURE。新HEAD `3cab5c0...` でテスト識別子を修正し、失敗した旧HEADを成功扱いせず全CIで受入。成功済みCIの手動再実行は実施していない。
- **確定進捗：新規初稿30/30、最終全面改稿45/110、通常復習9/22、段階概要4/9。第4段階031〜045と第7〜9組復習が完了。** Draft PR・旧80URL・正式main教材と正式Releaseは維持。AD認証は本人とドメイン名だけ、認可は既存DB/外部台帳を正本として都度判定し、監査・権限失効・更新競合・失敗時の安全拒否を維持。実AD/IIS/Kerberos/FDW・1万人超実機負荷は未受入。
- **次工程：学習順046〜050（db-bridge-01、旧Step31〜34）と第10組復習、第5段階概要**。EF Core10・Npgsql・DbContextFactory・PG18の実Razor/SQL/C#と照合して各3300文字以上、実コード2以上、WinForms＋Dapper比較、切り分け、操作、演習と詳答を全面改稿。第5段階先頭、前後リンク、Web/PDF、日本語、Chrome・品質ゲート・安全拒否を同期。同一HEADのWindows/Linux・xUnit・Chrome・PG18・PDF成功とPreview PDF生成元SHA一致後にのみ50/110・10/22・5/9へ進む。
- **記録更新によりHEADが進んだ場合**、必ずその新HEADのApp/PDF CI全SUCCESSおよびPreview Release生成元SHA・単体PDF AssetのGET一致を再照合すること。旧HEADの成功を新HEADの証跡として流用しない。

## 2026-10-27 学習順036〜040・第8組復習の受入

- **受入対象HEAD** `42c394f9f8bd921ac149bc8317d886f4f7b754f7`。アプリ [Run 40444727667](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40444727667)・日本語PDF [Run 40444727659](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40444727659) ともSUCCESS。Windows Server 2025/Linux、xUnit、Chrome画面操作、PostgreSQL18実DB/既存安全拒否、日本語PDFと教材品質ゲートを同一SHAで受入。
- 5本文の文字数は5,037・4,012・4,259・4,349・5,520字、第8組復習は5,041字。WinForms＋Dapper比較、コード、切り分け、演習・解答、Razor表、Web/PDF目次、040→第8組復習→041とChromeを同期。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) の本文生成元SHAは上記HEADと一致。単体PDF `blazor_learning_textbook_ja.pdf`、Asset ID 680332166、1,348,776バイト、SHA256 `5415dff04fc5b1e8d4cf8d217e288319838a01376457feeb937b8d272134307c`。Release GET・Asset個別GETで照合。ZIPなし。
- **確定進捗：新規初稿30/30、最終全面改稿40/110、通常復習8/22、段階概要4/9。** 第4段階の残り学習順041〜045と第9組復習は未全面改稿。
- この進捗記録の更新後に新HEADとなるため、更新HEADに対する同一SHA全CI成功とPreview PDF生成元の再照合は引き続き必要。成功CIの不要な再実行は禁止。
- 次は学習順041〜045（browser-01〜05）・第9組復習を全面改稿し、第4段階を完成させる。Draft、旧URL、正式mainと正式PDF、本人認証・既存DB台帳による都度認可と監査・安全拒否を維持。実AD/IIS/Kerberos/FDW・大規模実機未受入を維持。

## 既にある成果
- 既存の実装：旧Step01〜80の80本。
- v2で先行本文改稿済み：旧Step11〜30の20本とコラム5本。これらの成果を保持し、v3での最終照合は学習順001から順次実施中。
- 現行Draft PRのWeb目次・日本語PDFには旧80件と新規30件の初稿を配置済み。全110本文と9段階の最終全面改稿は未完了。mainの正式Release PDFは別に維持。

## 新規ステップ（初稿と画面追加済み 30 / 30、総合演習のCI成功）
- [x] web：5件のRazor・Markdown・復習・索引を追加、HEAD 9e12130のWindows/Linux・実ブラウザー・PG18・PDF CI成功（Razor＋教材＋復習＋テスト＋PDFを同じ単位で実装）
- [x] runtime：5件（HEAD 6f22c9dのWindows/Linux・Chrome・PG18・PDF CI成功）（Razor＋教材＋復習＋テスト＋PDFを同じ単位で実装）
- [x] browser：5件（初稿・画面・第9組復習・第4段階概要を追加。HEAD 9a9df7eのWindows/Linux・Chrome・PG18・PDF CI成功）
- [x] db-bridge：5件（画面・教材初稿・概要・復習第10〜12組・Chrome・PostgreSQL18・PDF CI成功）
- [x] security：5件（画面・日本語初稿・第7段階概要・第17組復習・安全拒否単体テスト・Chrome/Win/Linux/PG18/PDF CI成功）
- [x] capstone：5件（Razor・日本語初稿・第9段階概要・第22組復習・Web/PDF目次・匿名拒否Chrome試験・PostgreSQL18結合試験を追加し、全CI成功）

## 最終全面改稿（受入済み 45 / 110）
- [x] 第1段階：学習順001〜005、概要・本文5本・関連復習・PDFとUIの一致を受入（HEAD f48474d、Windows/Linux/Chrome/PG18/PDF CI SUCCESS）
- [x] 第2段階：学習順006〜020の15本、段階概要、第2〜4組復習を実装・教材・PDF・Chrome・PG18同一HEADでCI受入済み
- [x] 第3段階：学習順021〜030の10本文、第5・第6組復習、段階概要、画面とPDFの整合をCI受入済み
- [x] 第4段階：学習順031〜045の15本文、第7〜9組の復習、段階概要、Razor・Web/PDF・品質ゲート・全CIの一致を受入済み
- [ ] 第5段階：学習順046〜060、概要・本文15本・関連復習・PDFとUIの一致を受入
- [ ] 第6段階：学習順061〜080、概要・本文20本・関連復習・PDFとUIの一致を受入
- [ ] 第7段階：学習順081〜095、概要・本文15本・関連復習・PDFとUIの一致を受入
- [ ] 第8段階：学習順096〜105、概要・本文10本・関連復習・PDFとUIの一致を受入
- [ ] 第9段階：学習順106〜110、概要・本文5本・関連復習・PDFとUIの一致を受入

## 復習と特別コラム（最終改稿受入済み 9 / 25）
- [ ] 学習順5件ごとの通常復習22本の本文・実装・索引を再構成（第1〜9組受入：9/22）
- [ ] DI特別コラム
- [ ] 用途別MudBlazor特別コラム
- [ ] 1万人規模認証・既存DB認可特別コラム

## 段階概要（最終受入 4 / 9）
- [x] 9段階の**計画用概要草稿**を作成し、第1・第2・第4・第5・第7・第9段階をPRのPDFへ先行収録
- [ ] 最終PDF/Webの段階先頭へ9ページを配置し、本文・演習と整合（第1〜第4段階概要は受入：4/9。第4段階031〜045は全件改稿受入済み）

## 5件単位の受入条件
- [ ] 前の単位と用語・前提がつながる。
- [ ] Razor、C#、DI、DBの実装と日本語教材・コード例が一致する。
- [ ] ブラウザー・Win/Linux・PG18（該当時）・PDFに関するCIが成功する。
- [ ] 復習の問題・答え・前後リンクと段階概要の到達目標が一致する。
- [ ] 旧URLの互換性が維持され、未実装の画面を実装済みとして掲載しない。

## 2026-10-27 今回の作業記録
- [x] PR #36 の HEAD `9e12130` のアプリCIとPDF CIがともに成功（Run 40348357127 / 40348357519）。Web基礎5件について旧「CI待ち」を訂正。
- [x] runtime-01〜05：Razor画面・Markdown初稿・第2段階の概要・第2〜4組の復習画面と本文・日本語目次・PDF挿入・ブラウザーテストを追加。
- [ ] 今回の新HEADについて Windows/Linux・実ブラウザー・PDF・PG18 を確認し、失敗があれば修復する。CI完了まではruntimeを受入済みと数えない。
- [ ] 次に browser-01〜05を新規作成し、学習順041〜045に挿入する。段階概要と第9組の復習を整備する。
- [ ] 最終全面改稿は全30新規ステップの配置後、学習順001から開始。

## 2026-10-27 browser 5件を実装した作業記録
- [x] HEAD 6f22c9dに対するアプリCI Run 40351271699・PDF Run 40351271704 はすべてSUCCESS。Chrome実操作、Windows/Linux、xUnit、PostgreSQL18、PDFを確認し、再実行不要。
- [x] browser-01〜05を学習順041〜045に配置。JS interop、アップロードの64 KiB上限、CSV/PDFダウンロード、ErrorBoundaryと取消、キーボード/拡大表示を実装。
- [x] 第4段階概要・第9組復習・Web目次/メニュー・日本語PDF目次・整合チェック・Chrome操作試験を追加。
- [ ] 新しいHEADに対するCIの最終結果を確認し、不具合があれば修正。結果を確認するまで第4段階の最終受入には数えない。
- [ ] 次は db-bridge-01〜05を学習順046、052、053、058、059へ実装。第5段階概要、第10〜12組復習、Razor、Markdown、SQL安全境界、PostgreSQL18テスト、PDFを同期する。
- [ ] 新規30本をすべて実装してから、学習順001から全110本と22復習、3特別コラム、9概要の最終全面改稿へ進む。

### Chrome試験で発見した不整合の修復
- [x] browser-03のCSV/PDFダウンロードおよびファイル名の判定後、画面の状態文字列を小文字 `.pdf` で表示しているのに試験だけ大文字「PDF」を期待してタイムアウトしたことを確認（Run 40353250122）。実際の表示に一致する断言へ修正した。
- [x] 学習順040（旧Step30）→第8組の復習→041（browser-01）および第9組の復習→旧Step31への前後リンクを修正。未実装のdb-bridge-01へのリンクはまだ出さない。
- [ ] 修正後HEADのChromeを含むCIがすべて成功した時点で、第4段階の新規5件のCI受入を記録する。

### 2026-10-27 第4段階のCI受入実績
- [x] browser-01〜05の最新実装HEAD `9a9df7e9f9d4c0b763b979958614ff1880fe1072` に対し、アプリCI [Run 40353992330](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40353992330) が SUCCESS。Windows Server 2025/Linux Release、xUnit、Chrome実ブラウザー操作、PostgreSQL18実接続が成功。
- [x] 同HEADの単体日本語PDF生成・学習内容検査 [Run 40353992474](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40353992474) が SUCCESS。Draft PRのため正式Releaseは行わず、既存公開PDFは更新していない。
- [x] 学習順040→第8組の復習→041〜045→第9組の復習→次の既存学習画面のリンクを確認。db-bridge-01の実装後に後続導線を再度更新する。
- [ ] browserの最終全面改稿受入は未実施。新規15/30初稿と全面改稿0/110の区別を維持。

## 2026-10-27 第5段階追加の継続記録
- [x] 前回HEAD c947980のアプリRun40354908903と日本語PDF Run40354908873は両方SUCCESS。
- [x] db-bridge-01〜05を学習順046/052/053/058/059に追加。第5段階の概要、復習第10〜12組、Razor、Markdown、Web/PDF目次、教材チェック、Chrome試験を同期。
- [x] DbContextFactoryによるSQL生成・追跡、Keylessモデル定義をDB非接続で練習。業務用の台帳や認可処理には触れない。
- [x] PDFは既存の正式版と区別してReleasesの`learning-textbook-v3-preview`に公開。最初の検証済み版はcommit 1af26d95、PDF単体872186バイト。今回の新HEADは全CI完了後だけプレビューを更新する。
- [ ] PostgreSQL18でFromSql/SqlQueryのSQLインジェクション否定検証を追加しCIを受入する。
- [ ] 次はsecurity-01〜05および第7段階概要・復習、続いてcapstone-01〜05。30件配置後に最終全面改稿001〜110。

### 2026-10-27 PostgreSQL 18実DB試験の修復
- [x] 新規DbBridgeIntegrationTestsの初回Run 40357737630で`FromSql`の`SELECT *`が`xmin`を返さず失敗することを実測。
- [x] モデルで必須の更新競合列`xmin`を`SELECT xmin, *`で明示し、教材Razorと実DB試験と解説を一致させた。SQL注入否定条件は維持。
- [ ] 新HEADのPG18・Win/Linux・Chrome・PDF・Preview Releaseへの反映を確認する。

### 2026-10-27 第5段階のCI受入とReleases更新
- [x] 実装HEAD `34ffe89a2f2ae77d61eb21e3fdf0f58030f45b20` の [アプリCI Run 40358248623](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40358248623) は Windows Server 2025/Linux Release・xUnit・Chrome操作・PostgreSQL18実DB・SQL注入否定試験すべてSUCCESS。
- [x] [PDF Run 40358248514](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40358248514) もSUCCESS。学習順046/052/053/058/059、段階概要、第10〜12組復習を含む日本語PDFの構築・内容検査・Releaseアップロードを確認。
- [x] **[教材v3プレビューRelease](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)** のPDF assetを同HEADから更新。生成元SHAがRelease notesと一致、単体PDFは899298バイト、ZIP化せず公開。正式版 `learning-textbook` は変更なし。
- [x] 初回の実DB否定試験で`FromSql`が`xmin`不足により失敗したため、`SELECT xmin, *`を使用するようRazor・教材・テストを統一。修正後実DB試験合格。
- [ ] 今後はsecurity-01〜05（学習順081〜085）と第7段階概要、第17組復習を実装し、その後capstone-01〜05（学習順106〜110）を配置。
- [ ] **最終全面改稿は0/110**。新規30本の配置完了後に、概要9本・復習22本・特別3本を含め学習順001から全件改稿する。

## 2026-10-27 第7段階security-01〜05の実装・CI受入・PDF Release反映
- [x] 前HEAD `d1e686c00e8f9af2cb26e831f51474b7a7b703ea` のアプリ Run 40359052176 と PDF Run 40359052135 は両方SUCCESS、同HEADのプレビューRelease asset存在・生成元SHA一致を再確認した。
- [x] 新規5件を学習順081〜085へ配置。Razor画面・日本語Markdown・第7段階概要・第17組復習・Web目次・NavMenu・前後リンク・PDF索引・教材整合検査・Chrome E2Eを同期した。旧Step01〜80のURLを維持。
- [x] Windows認証/Cookie/circuit、Middleware/HTTPS/Antiforgery、XSS/CSRF/SQLパラメーター、SecretとData Protection鍵、接続失効/権限版の模擬否定試験を追加。**本物のAD認証や既存DB台帳を新規教材画面で偽装しない。**
- [x] 実装HEAD `d16ec614bc8f863cb8feb9a3e0143702e59ea16a` の [アプリCI Run 40360843077](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40360843077) は Windows/Linux Release、全単体テスト、Chrome E2E、PostgreSQL18実DB試験すべてSUCCESS。
- [x] 同HEADの [PDF Run 40360843042](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40360843042) もSUCCESS。プレビューRelease `learning-textbook-v3-preview` に単体日本語PDF（926066バイト）を更新し、生成元SHAを再取得して照合済み。ZIP化・正式main版の上書きなし。
- [ ] 次はcapstone-01〜05（学習順106〜110）、第9段階概要、第22組復習をRazor/本文/Web/PDF/否定・Chrome・PG18テストと同時実装する。
- [ ] 30件全初稿後に、全110ステップ・22復習・3特別コラム・9概要を**学習順001から**最終全面改稿する。最終改稿の受入済みは0/110。

## 2026-10-27 第9段階の総合演習5件配置
- [x] capstone-01〜05を学習順106〜110へ追加。要件・画面遷移、DB検索/生成SQL、DTO編集・競合・取消、認証と既存台帳認可・監査、E2E/IIS運用受入の5画面を実装。
- [x] PostgreSQL18検索・部署の分離・10件ページング・xmin競合・トランザクションロールバックを実DBで試す専用結合試験を追加。既存の匿名・権限失効・監査失敗の否定試験を維持。
- [x] 第9段階概要と第22組の復習、Web/PDF目次、教材整合検査、Chrome操作（匿名の一覧・詳細・集計は拒否）を追加。
- [x] 22回の5本単位による全110本文・22復習・3特別コラム・9段階概要の[全面改稿手順](full-rewrite-roadmap.md)を作成。最初は001〜005。
- [ ] 総合演習追加後HEADのWindows/Linux・Chrome・PostgreSQL18・PDFのCI全成功と、DraftプレビューReleaseの単体PDF（生成元SHA一致）を確認する。**未成功のまま受入完了としない。**
- [ ] 全面改稿の受入0/110。新規初稿30/30とは別にカウントする。

### 2026-10-27 新規30/30配置のCI・Preview Release受入
- [x] 実装HEAD `790a58ee5b975b288a6707ac7341fc3f873ad9b8` のアプリ Run [40364677020](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40364677020) と PDF Run [40364677179](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40364677179) が両方SUCCESS。Windows Server2025/Linux、xUnit、Chrome、PostgreSQL18の結合・競合・ロールバック・安全拒否を確認。
- [x] 新しい110件全面改稿計画を追加したHEAD `d7aebd3796fafd3a56fad3751c50c4d003813363` の [App Run 40365372365](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40365372365) と [PDF Run 40365372317](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40365372317) もSUCCESS。
- [x] `learning-textbook-v3-preview` の単体日本語PDF `blazor_learning_textbook_ja.pdf` 952877バイトと、Release notes生成元SHA `d7aebd3796fafd3a56fad3751c50c4d003813363` をGitHub GETで確認。ZIP化せず、main正式版には触れていない。
- [x] 学習順001〜110のすべてにRazor画面と初稿が配置され、通常復習22組・段階概要9件・特別3コラムの全面改稿順も[手順](full-rewrite-roadmap.md)に明記した。
- [ ] **全面改稿の受入は0/110。次は001〜005を先頭から全面改稿し、第1段階概要と第1組復習のテキスト・Razor・Web目次・PDFを同じ単位で検証する。**

## 2026-10-27 第1組（学習順001〜005）の全面改稿・CI受入
- [x] web-01〜05の短い初稿を、WinForms＋Dapper経験者向けの**約4,000〜5,000文字/本文**へ全面改稿。実装に対応するRazor・C#・HTML・CSSコード抜粋、動作場所、比較表と失敗時の切り分け表、手順・演習と解答を追加した。第1段階概要、第1組復習も長文に改稿。
- [x] 第1段階と復習のRazor画面にも比較/診断表と実コードを追加し、Chrome E2Eで実画面から照合。既存80URLと第1段階の5 URLは維持。
- [x] 今後の全面改稿にも適用する「長文・表・コード2ブロック以上・演習と解答・失敗時の切り分け」を[全面改稿手順](full-rewrite-roadmap.md)へ追加し、001〜005の機械的な品質検査を`scripts/check-learning-materials.py`へ実装。
- [x] 初回改稿のPDF検査で管理用の`src/PgBlazorSamples.Web`パスが混入しFAIL（Run 40368982985）。学習者向け内容にはファイル名のみ残すよう修正。品質検査で「失敗」の単語だけを強制してFAIL（Run 40369922298/40369922299）したため、切り分け・症状等の自然な見出しも受け入れるよう修正。どちらも安全条件の迂回ではなく、教材内容の修正と検査誤判定の解消。
- [x] **実装HEAD `f48474d3882359626d1efedda73ac9737461b121`** の [アプリCI Run 40370279965](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40370279965) はWindows Server2025、Ubuntu、xUnit、Chrome操作、PG18結合試験すべてSUCCESS。[PDF Run 40370279958](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40370279958) もSUCCESS。
- [x] `learning-textbook-v3-preview` に単体の日本語PDF **1020439バイト**を更新、生成元SHA `f48474d3882359626d1efedda73ac9737461b121` がRelease notesと一致することをGitHub GETで確認。ZIP圧縮なし、正式main版への変更なし。
- [x] **最終全面改稿の受入は5/110、復習1/22、段階概要1/9**。30新規初稿は30/30のまま。
- [ ] 次は学習順006〜010（旧Step01〜04＋runtime-01）、第2段階概要と第2組復習。先頭から同じ長文・実コード・複数表・演習・解答基準で全面改稿し、全CI成功後のみ受入数10/110とする。

## 2026-10-27 第2組（学習順006〜010）の全面改稿・CI受入
- [x] 学習順006〜010（旧Step01・02・03、新runtime-01、旧Step04）を実Razor/C#に照合して全面改稿。各本文4,147〜4,634文字、2件以上のコードブロック、WinForms＋Dapper比較、切り分け表、実習、演習と詳しい解答を追加。第2段階概要と第2組復習も全面改稿。
- [x] Razorの学習順表示、前後リンク、段階概要・復習の画面内容、Web目次を同期。PDF索引は既に正しい章順だったため維持。旧80のURL、mainの正式教材、Draft状態、AD本人認証とDB台帳認可の安全境界は変更なし。
- [x] `scripts/check-learning-materials.py`の品質ゲートを006〜010へ拡張し、長文・複数表・実コード2件以上・PDF順・前後リンクを検査。Chromeの画面遷移、カウンターと0未満防止、復習の結合試験を追加。
- [x] 初回Chrome E2EのPlaywrightの曖昧な`Form`文字列検索で失敗（App Run 40408904299）。実画面の問題ではなくテストロケータを`getByRole("cell", {name:"Form", exact:true})`に修正。
- [x] **実装・修復HEAD `d87ee9a96b7fb790645e5ef7d624008009f92980`** の[アプリRun 40409325752](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40409325752)（Windows Server 2025/Linux、xUnit、Chrome実操作、PostgreSQL18）が全SUCCESS。[PDF Run 40409325725](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40409325725)もSUCCESS。
- [x] [学習テキストv3プレビュー](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)の単体PDFは**1,079,388バイト**で、Release notesの生成元SHAと実装HEADが一致。ZIP化と正式main版の上書きなし。
- [x] **確定：新規初稿30/30、全面改稿10/110、通常復習2/22、段階概要2/9。** 第2段階15本のうち残り10本は未改稿。
- [ ] 次は学習順011〜015（旧Step05〜08＋runtime-02）と第3組の復習を同じ長文・実コード・比較/診断表・詳細解答の基準で改稿。第2段階概要・Web/前後リンク/PDF目次とテストを同期し、同一HEADの全CIとPDFプレビュー照合後のみ15/110へ進める。

## 2026-10-27 第3組（学習順011〜015）の全面改稿・CI受入
- [x] 旧Step05〜08およびruntime-02を実際のRazor/C#と照合して全文改稿。本文は4,231〜4,780文字で、WinForms＋Dapper比較、実コード2ブロック以上、比較・診断表、操作手順、故障の切り分け、演習と詳しい解答を追加。第3組復習は4,990文字。
- [x] 第2段階概要とRazorの学習順表示・前後リンク、Web目次の復習位置、PDF章順、第3組復習のRazor表・確認問題を同期。品質ゲートは3300文字以上・比較表・診断表・実コード・用語・前後リンクを検証。Chrome実操作で文字入力・4→2→4件絞り込み・親子カード・EventCallback・対話回数・第3組復習への遷移を確認。
- [x] **実装HEAD `dcae9e218569f09b0749f94d53228f1d837396c3`** の[アプリCI Run 40412865629](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40412865629)がWindows Server 2025/Linux・xUnit・Chrome実操作・PostgreSQL 18実DB結合の全ジョブSUCCESS。[PDF CI Run 40412865704](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40412865704)もSUCCESS。
- [x] [学習テキストv3プレビュー](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)の日本語PDF単体は**1,141,020バイト**、Asset SHA256 `b646ebc7af8e8cf01c57ca043965fe5b238c5f2d8af006e8d754db34668ef99e`。Release notesの生成元SHAと上記実装HEADが認証済みGETで一致。ZIP配布なし、main正式版は変更なし。
- [x] **確定進捗：新規初稿30/30、最終全面改稿15/110、通常復習3/22、段階概要2/9。** 実AD/IIS/Kerberos/FDWの実機受入とは区別。
- [ ] **次は学習順016〜020（旧Step09・runtime-03・旧Step10・runtime-04・runtime-05）と第4組復習を全面改稿。** 第2段階概要・Razor・学習順・前後リンク・Web目次・PDF・品質ゲート・Chrome操作を合わせる。全CIとPreviewの同一HEADが確認できた場合だけ20/110へ進める。

## 2026-10-27 第4組（学習順016〜020）の全面改稿・CI受入
- [x] 旧Step09・runtime-03・旧Step10・runtime-04・runtime-05を実Razor/C#と照合し、各本文**4,534〜5,227文字**へ全面改稿。初期化/破棄、事前レンダリング、フォームの日本語検証、URLと再読込、状態の保存先を取り違えない構成とし、実コード2ブロック以上、WinForms＋Dapper比較、診断表、操作手順、演習と詳細解答を追加した。第4組復習は5,642文字。
- [x] 第2段階概要、Razorの実装説明・比較表と前後リンク、Web目次の既存順序、PDF章順、品質スクリプトの文字数/表/コード/導線検査、Chromeでの016→017と020→第4組復習の遷移を同期。旧80URLを保持。
- [x] **実装HEAD `897153ba5c4e4c65bf91c6fa6c2c151d0e86a582`** の[アプリCI Run 40418321144](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40418321144)（Windows Server 2025、Linux Chrome、xUnit、PostgreSQL 18実DB、匿名・権限・監査など既存の安全拒否）が全SUCCESS。[PDF CI Run 40418321149](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40418321149)もSUCCESS。
- [x] [プレビューRelease](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)の日本語PDF単体`blazor_learning_textbook_ja.pdf`は**1,204,237バイト**、SHA256 `00b2247f3f2eee2fd92944719403dd02ba9370c88f2cc3dba45cde16fefb42d4`。認証済みGitHub GETで生成元SHAと実装HEADの一致を確認。ZIP・正式main版の更新なし。
- [x] **受入進捗：新規初稿30/30、最終全面改稿20/110、通常復習4/22、段階概要2/9。** 第2段階の15本と復習3組が受入済み。実AD/IIS/Kerberos/FDW・大規模負荷の実機受入とは区別する。
- [ ] **次は学習順021〜025（旧Step11〜15：DIの基本、インターフェイス、Singleton/Scoped/Transient、Blazor Scopedとcircuit、injectとコンストラクター注入）および第5組復習・第3段階概要を全面改稿。** 既存のサービス・画面実装を詳細に読み、各本文3,300文字以上、実コード2ブロック以上、WinForms＋Dapper比較表と診断表、操作手順、演習と詳しい解答を整備する。Web/PDF章順・前後リンク・Chrome/安全ゲートを同一HEADで照合し、成功時のみ25/110・復習5/22・概要3/9へ進める。
- [ ] 進捗記録更新後のPR最新HEAD自体について、新しいアプリ/PDF CIとPreview Releaseの生成元SHA・Assetを認証済みGETで再照合し、PRコメントに記録する。既存成功Runの再実行はしない。

## 2026-10-27 学習順021〜025の全面改稿・同一HEAD CI受入
- [x] 実Razor/C#とProgram.csのDI登録、GreetingService/IClock/FixedClock/Marker/ScopedCounter/Formatterを照合。旧Step11〜15（学習順021〜025）の各本文4,625〜5,300文字、第5組復習3,604文字、第3段階概要3,048文字を全文改稿。
- [x] 各本文に実装からの複数コード、WinForms＋Dapper比較、比較表と診断表、手順、演習・詳しい解答を追加。
- [x] 第3段階概要Razor、021〜025のRazor比較表/コード/診断、復習Razor、前後リンク、ホーム/メニュー、PDF章順、品質ゲート、Chrome E2Eを同期。
- [x] 実装HEAD `5472f4edf054b62c998f8d431a3222f874f6d739` のアプリCI [Run 40422500932](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40422500932) はWindows Server 2025/Linux・xUnit・Chrome実操作・PostgreSQL18実DB結合と既存安全拒否試験を含む全ジョブSUCCESS。PDF [Run 40422500829](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40422500829) もSUCCESS。
- [x] [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) にZIPなしの日本語PDF単体を同HEADから公開。認証済みGitHub GETで生成元SHA `5472f4ed...` とPDF Asset ID `679868970`、1,243,463バイト、SHA256 `8875f5e4371134099a49fe4f17189d8c62a89ccc5f9da2dd704a7fedfc6a0cbd` を照合。
- **確定受入：新規初稿30/30、最終全面改稿25/110、通常復習5/22、段階概要3/9。** 次は026〜030・第6組復習、DI特別コラムとの接続を全面改稿する。記録更新後の最新HEADも新たに同一HEAD CIとPreview Asset GETで確認する。

## 2026-10-27 学習順026〜030・第6組復習の全面改稿実装（CI受入待ち）
- 既存Razor/C#のOptions、ILoggerとCancellationToken、固定時計xUnit、Applicationの進捗サービス、ITodoServiceとInMemoryTodoServiceを読み、旧Step16〜20の本文を長文改稿。WinForms＋Dapper比較表、診断・切り分け表、実コード2ブロック以上、操作・演習・詳細解答を整備した。
- 画面に学習順026〜030と比較・診断表を追加し、第6組復習の表、既存Web/ナビ/PDF順、ライブラリDI特別コラムへの導線を同期。Step17の重複開始で古い非同期処理の完了・取消が新しい表示を上書きする競合を現行CancellationTokenSourceの参照照合で防止した。
- 教材品質ゲートを第6組の3300文字・複数表・コード・Web/PDF順へ拡張。Chrome E2Eに026〜030の表表示、Step19練習用60上限、Step20上限5件・6件目拒否、第6組復習を追加。Step19の60は全110本の教材総数と区別する。
- **同一HEADのWindows Server 2025/Linux、xUnit、Chrome、PostgreSQL18実DBと安全拒否、日本語PDFの全CI成功、Preview Release生成元SHAとAsset照合が完了するまでは、確定進捗を新規初稿30/30、全面改稿25/110、通常復習5/22、段階概要3/9に据え置く。** 実AD/IIS/Kerberos/FDW未受入。正式main/Release、旧80 URL、Draft、認可・監査・安全拒否の条件を維持。
- 成功受入後の次工程は学習順031〜035（旧Step21〜25：MudBlazor導入・レイアウト・日本語入力・MudForm・MudTable）、第7組復習と第4段階概要の改稿。実Razor・日本語長文・Web/PDF・品質ゲート・Chromeを揃えて検証する。

## 2026-10-27 学習順026〜030・第6組復習の同一HEAD最終CI受入
- **確定進捗：新規初稿30/30、最終全面改稿30/110、通常復習6/22、段階概要3/9。** 第3段階021〜030本文10本・第5/6組の復習を受入。段階概要3/9の件数は変更しない。
- **実装・受入HEAD `4d8c652aa0c58c795a111ccc9afbda9c504b39d2`**。アプリ [Run 40429979992](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40429979992) SUCCESS：Windows Server 2025/Linux Release、xUnit、Chrome実操作、PostgreSQL18実DB結合と安全拒否の既存試験。PDF [Run 40429979990](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40429979990) SUCCESS：教材品質ゲート、日本語PDF生成とPreview反映。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) の生成元SHAは実装HEADと一致。単体Asset `blazor_learning_textbook_ja.pdf` **ID 680012256、1,272,480バイト、SHA256 `1fcdcc375ddb62f2ec10505682687639f06b1fe2cda806bfb61637feb219fbff`**。Release GET・Asset個別GETの名前・サイズ・digest一致。ZIPなし、正式main版未変更。
- 次工程は**学習順031〜035（旧Step21〜25：MudBlazorと日本語翻訳・レイアウト・日本語入力・MudForm・MudTable）と第7組復習、第4段階概要**を実Razor・C#に合わせて全面改稿する。各本文3300文字以上を目安に、実コード2ブロック以上、WinForms＋Dapper比較表・切り分け表・操作・演習と詳しい解答、前後リンク、Web/PDF目次、品質ゲート、Chromeを同期。全CIとPreviewの同一HEAD照合後のみ35/110・7/22・概要4/9へ更新する。
- 引き続きDraft PR #36、旧80URL、AD本人認証と既存DB/外部台帳による都度認可・監査・安全拒否を維持。実AD/IIS/Kerberos/FDW/大規模負荷の現地未受入は成功扱いしない。成功済みCIの不要な再実行、正式Release再発行、mainへのマージは禁止。
- **この記録更新コミット自体も新HEADになるため、そのHEADに対するApp/PDF CIとPreview Release生成元SHAの再照合が必要。** 前HEADの成功を新HEADの証跡として流用しない。

## 2026-10-27 学習順031〜035・第7組復習・第4段階概要の同一HEAD受入
- [x] 既存Razor/C#を確認し、旧Step21〜25（学習順031〜035）の日本語教材を各3,919〜4,774文字へ改稿。2件以上の実コード、WinForms＋Dapper比較、診断表、画面操作、演習と詳しい解答を充実した。
- [x] 第7組復習4,258文字、第4段階概要3,609文字へ再構成。Razorの学習順・比較表・復習・段階概要・Web目次・前後リンク・NavMenu、PDF収録順と3300文字品質ゲートを同期。旧Step10のChrome検証はエラー文言をフォーム内の該当項目に限定し、同文言の重複誤検出を防いだ。
- [x] **実装受入HEAD `9d700f3ea0d043f4dd23bf0ab22fa3e6616f461e`** の[アプリCI Run 40442654275](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40442654275)と[日本語PDF Run 40442654171](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40442654171)はともにSUCCESS。Windows 2025/Linux、xUnit、Chrome実操作、PostgreSQL18結合・安全拒否、教材品質検査を受入。
- [x] [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)は生成元上記HEAD・単体PDF `blazor_learning_textbook_ja.pdf` 1,311,087バイト、SHA256 `eb327fa99af1745be0acf2d09fc7e8751f0985c60e41149177904a8499988720`。正式mainのPDF・ZIP・Draft状態は変更なし。
- [x] **確定：新規初稿30/30、最終全面改稿35/110、通常復習7/22、段階概要4/9。** 第4段階の残り036〜045は未改稿。認証・既存DB台帳による認可・監査・安全拒否は維持。実AD/IIS/FDW・1万人規模の現地受入は未確認。
- [ ] 次は学習順036〜040（旧Step26〜30：MudDataGrid、確認ダイアログ、通知/処理中、ダークモード、総合演習）と第8組復習。各本文3300文字以上、実コード・比較/切り分け・演習/詳答、Razor/Web/PDF/品質ゲート/Chromeを揃える。
- [ ] **この記録・curriculum・導線の更新コミット自体もHEADが変わるため、最新HEADのアプリ/PDF CIとPreview生成元SHAを再照合して最終証跡をPRコメントに残す。** 前回SUCCESSを別SHAへ流用しない。
