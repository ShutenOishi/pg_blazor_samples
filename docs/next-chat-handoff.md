## 2026-10-28 教材v3・Draft PR #39継続開発（最新の引き継ぎ・先頭が優先）

**対象**：https://github.com/ShutenOishi/pg_blazor_samples ・[Issue #38](https://github.com/ShutenOishi/pg_blazor_samples/issues/38)・[Draft PR #39](https://github.com/ShutenOishi/pg_blazor_samples/pull/39)。GitHub認証済みGETを唯一の正本とし、最初にPR最新HEAD、Issue、`docs/course-v3/implementation-audit.json`、`source-map.json`、`source-layout-and-migration.md`、PR最新コメント、**最新HEADと一致するアプリ3ジョブおよびPDF検証CI**を取得する。本節の追記によりHEADが変わるので、下記のコミットを現在HEADと決めつけない。

- **本節更新直前の実装HEAD**：`90de6ec76bd09481f1f7457e2a83640d044dda5e`。前の確定受入HEAD `cc3a997474cc9789df63839b63689321ec303c79` は、アプリ [40605109148](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40605109148) Windows/Ubuntu Chrome/PG18全3ジョブSUCCESS、PDF検証 [40605109201](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40605109201) SUCCESSを認証済みGETで確認した。成功済みCIの手動再実行は不要。
- **今回の追加**：学習順003 `Web03Page.razor`にHTML maxlengthとは別のC#未入力/30文字超拒否、入力変更時の検証結果リセット、保存しない受入表示を実装。Chromeでラベル→フォーカス、入力反映、HTMLエンコード、正常/空欄/31文字拒否を検証するコードを追加。学習順004はDocument GET 200、DOM追加削除、イベント時のDocument再取得なし、未処理ページ例外なしをChrome試験へ追加。教材003/004と`source-map.json`・`implementation-audit.json`を同期した。**これらの追加分は新HEADのCIが全件SUCCESSになるまで受入確定しない。**
- **追加30テーマ監査台帳**：002・020・045・052がChrome/PG18等の対象CI受入済み4/30、003・004が実装済み・新HEAD CI待ち2/30、残る24/30は`AUDIT_PENDING`。既存画面があることは全機能PASSではない。002/052の証跡はHEAD `cc3a9974...` のアプリRun40605109148。
- **教材進捗**：110/110本文全面改稿、通常復習22/22、段階概要9/9は完了済み。110正規URL `/learn/001`〜`/learn/110`とHome/NavMenuを統一、互換URLは維持。本文・画面・復習・概要・補足の学習番号表示を広く統一したが、旧番号残存の完全監査は未完了。物理ソース配置は未移行で、110件のRazor→サービス/DTO→EF/SQL/JS→テストの参照追跡も未完了。
- **次工程の順序**：①新HEADアプリ3ジョブとPDF検証をGETし、新規失敗のみ修復、②追加30件の次の監査・機能補強（例：001/005または次の未監査テーマ）を正常/異常・Chrome/xUnit/学習専用PG18で実証、③110件の依存参照を実コードで確認し、段階単位で学習順に分かる物理配置へ安全に移行、④教材・source-map・audit・静的検査・Web/PDF目次ソースを各変更に同期、⑤全必要CIと残項目の受入後にPR #39のmainマージと正式PDF更新を別々に判断する。
- **安全境界**：Draft PR #39は全工程受入までマージしない。正式Release `learning-textbook` のPDF Asset 683032765はこの作業で変更しない。学習DBだけを使用し、実AD/IIS/Kerberos/FDWや現地負荷試験は許可なしに行わない。PRE-01〜07は未確定、SITE PASS 0/15・NOT_RUN 15/15。既存DB台帳による操作・行認可、監査、権限失効、xmin競合、FDW原子性不明時の安全拒否を維持。
- **毎回の必須引き継ぎ**：作業完了時にはPR #39へ受入根拠をコメントし、この引き継ぎ欄を最新化し、ユーザーへの回答末尾にも次回そのまま貼り付けられる引き継ぎプロンプトを作成する。未確認のCIや操作をPASSと記録しない。

---

## 2026-10-28 教材正式公開の承認と現地実機受入の未完了事項

ユーザーの2026-10-28の明示指示に基づき、**PR #36のmainマージ・日本語教材PDFの正式Releaseに限って**以前の停止指示を解除する。これは教材公開の許可であり、業務システムの本番配置・現地実機試験・実AD/FDWへの変更の許可ではない。後記の「Draftをマージしない」「PDF停止」はこれ以前の履歴であり、本節が教材配布に関する新しい指示を記録する。

GitHub GETで同一PR HEAD `8460562a2740d857a9cf32c620030e1f5a702c6c` のアプリ[Run 40551412178](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40551412178)がWindows/Linux/PostgreSQL18全3ジョブ**completed/SUCCESS**、[PDF Run 40551412530](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40551412530)が**completed/SKIPPED**と確認。教材の初稿30/30、全面改稿110/110、復習22/22、段階概要9/9は完成。成功済みCIの手動再実行と再改稿は不要。mainのPDF Workflowと正式Releaseを独立に検証し、その結果をPRコメントに記録する。

**現地は引き続きPRE-01〜07未確定、SITE-01〜15：PASS 0/15、NOT_RUN 15/15。** 実機受入や本番Goは未承認である。DB台帳による都度操作・行認可、権限失効、`xmin`競合、監査失敗、FDW原子性不明時の安全拒否を維持する。

現地受入のPRE責任者候補・評価値・原本証跡の割付は[現地実機受入計画](https://github.com/ShutenOishi/pg_blazor_samples/blob/main/docs/course-v3/on-site-acceptance-plan.md)を参照。実機をPASSへ変更していない。

## 2026-10-28 現地実機受入の未完了事項・証跡計画を策定（PDF停止継続）

- **認証済みGitHub GETの基準正本**：Draft PR #36 `22aae38d3ddf3a27a611e07050efda783f9d8a7f` はopen/draft・main未マージ。[アプリCI 40548486534](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40548486534)はWindows/Linux/PG18の3ジョブすべてSUCCESS、[PDF Workflow 40548486604](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40548486604)はSKIPPED。[PRコメント #6448459478](https://github.com/ShutenOishi/pg_blazor_samples/pull/36#issuecomment-6448459478)を照合。成功済みCIは再実行していない。
- **進捗維持**：新規初稿**30/30**、全面改稿**110/110**、通常復習**22/22**、段階概要**9/9**。全110本と成功済み教材CIを再実施しない。`curriculum.json`は110ステップの全件`final-reviewed-ci-passed`、通常復習22件、段階概要9件をGET/JSON解析で確認済みで、教材の改稿状態は変更しない。
- **新規成果**：[`docs/course-v3/on-site-acceptance-plan.md`](course-v3/on-site-acceptance-plan.md)にPRE-01〜07の前提、SITE-01〜15の現地試験手順・PASS条件・証跡・停止条件、秘匿化した原本の管理規則を整備。**実機試験は一件も実施しておらず、SITE PASS 0/15、NOT_RUN 15/15**。PRE-01〜07も未確定。
- **未受入**：実AD/IIS/Kerberos/SPN/実FDW、操作・行権限失効、`xmin`競合、Data Protection鍵、監査の本番永続性、台帳10,001人以上と独立した同時circuit/RPS/DBプール、障害復旧・切戻し。CI/モックを実機受入とみなさない。
- **安全境界**：ADは本人認証、操作・行単位認可は既存DB台帳を都度参照。監査・失効・競合・FDW原子性不明時の安全拒否を保持。Draft/open・main未マージのまま。
- **絶対禁止**：明示的なユーザーの再開指示までPDF生成・再生成、Preview Release・正式Release更新禁止。停止Workflow維持。既存Previewの生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f` を変更しない。
- **次工程**：PRE-01〜07の責任者・評価環境・基準・証跡保管先を確定し、許可済み環境でSITE-01〜15を段階的に実施。記録更新後の新HEADに対応するアプリ3ジョブSUCCESSとPDF Workflow SKIPPEDは別途GETで確認し、未完了なら完了扱いしない。

## 2026-10-28 第22組106〜110の改稿・同期HEAD全CI受入確定（PDF停止継続）

- **認証済みGitHub GETの受入証跡：** 教材7本文・Razor/目次ソース/NavMenu/静的検査/Chrome・進捗/カリキュラムの同期HEAD `248a6deeb33ca33ff889cbbf84a07ea37b045b50` に対し、[アプリRun 40547653260](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40547653260) は **completed/SUCCESS**。Windows Server 2025 Release/xUnit/教材静的検査、Ubuntu 24.04 Release/xUnit/Chrome実操作、PostgreSQL18実DB結合/安全拒否の **3ジョブがすべてcompleted/SUCCESS**。同一HEADの [PDF Workflow 40547653349](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40547653349) は **completed/SKIPPED**。成功Runを手動再実行していない。
- **教材の最終改稿：** 106〜110は3,694／4,193／4,243／3,947／4,364文字、第22組復習3,447文字、第9段階概要3,431文字。7本文すべて3,300字以上・C#コード例2件以上、WinForms＋Dapper比較表、操作手順、障害切り分け、演習詳答。5つのRazorと概要・復習、Home/NavMenu、旧80URL、前後リンク、Web/PDF目次**ソースのみ**、静的検査、Chromeと一致。
- **受入達成進捗：** 新規初稿 **30/30**、全面改稿 **110/110**、通常復習 **22/22**、段階概要 **9/9**。**この受入記録を追記するとHEADが変わるので、記録更新後の最新HEADでも同じアプリ3ジョブSUCCESS・PDF SKIPPEDを直接GETしてから最終受入を確定する。** 旧HEAD成功を新HEADに流用しない。
- **絶対禁止：** ユーザーの明示再開までPDF生成・再生成、Preview Release・正式Release更新は全面禁止。PDF停止Workflow維持。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`は更新禁止。認証済みGETで旧Preview不変を確認。
- **安全条件・未受入：** Draft PR #36・main未マージ。ADは本人認証と信頼できる名前取得のみ、既存DB台帳で操作・行単位を都度認可。権限失効・xmin競合・監査障害・FDW原子性不明時の安全拒否を維持。実AD/IIS/Kerberos/FDW、鍵/監査の本番運用、1万人超登録・同時circuit/DBプール/切戻し負荷の実機受入は未完了。
- **次工程：** 記録更新HEADのアプリ全3ジョブSUCCESSとPDF SKIPPEDを照合し、PR #36に最終証跡を確定追記。教材110本の追加改稿・成功CI再実行は不要。PDFの明示再開指示までPDFとReleaseは変更せず、未受入の現地環境・負荷試験の計画と証跡準備を進める。

## 2026-10-28 第22組106〜110・復習・第9段階概要を全面改稿（同一記録HEADのCI受入待ち・PDF停止継続）

- **直前の確定正本：** Draft PR #36 HEAD `092c1dc43f3d8158c3c5b098b0bf94bc67ef064f`、アプリCI [40543274455](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40543274455) Windows/Linux/PG18全3ジョブSUCCESS、PDF Workflow [40543274413](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40543274413) SKIPPED。成功CIの再実行なし。
- **今回の全面改稿：** 学習順106〜110と第22組復習・第9段階概要の7本を、各3,300文字以上・C#実コード2例以上・WinForms＋Dapper比較表・操作手順・障害切り分け・演習詳答で改稿。5つのRazor比較表、段階概要・復習のRazor比較表、Home/NavMenu、旧80URL・前後リンク、静的検査とChrome実操作を同期。PDF目次は既存の正しいソース順序を維持し検査を追加、**PDF自体は生成しない**。
- **改稿予定値：** 新規初稿30/30、全面改稿110/110、通常復習22/22、段階概要9/9。**本資料を含む最新HEADでアプリCI全3ジョブSUCCESSおよびPDF Workflow SKIPPEDを直接GETできるまで正式受入は保留。** 先行HEADの成功で代用しない。
- **最優先の禁止：** ユーザーが明示的に再開を指示するまでPDF生成・再生成、Preview Release・正式Releaseの更新を禁止。PDF停止Workflowを維持。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`は変更しない。
- **安全条件：** Draft PR #36・main未マージ。ADは本人認証と信頼できるログイン名の取得のみ。業務は既存DB台帳による操作・行単位の都度認可、監査と権限失効・xmin競合・FDW原子性不明時の安全拒否を維持。
- **未受入：** 実AD/IIS/Kerberos/FDW、Data Protection鍵・監査の運用、1万人超登録・同時circuit・DBプール・負荷と復旧の現地実機受入。
- **次工程：** 同一最新HEADの3ジョブSUCCESSとPDF SKIPPEDを確認し、PRコメントに結果を記録する。PDF再開の明示指示まではPDF/Releaseに触れず、必要な現地受入証跡の整理に進む。

## 2026-10-28 第21組101〜105・復習の最終受入確定（PDF停止継続）

- **直近の同一HEAD受入：** `5e5d09a1e02b2af3ba6b4616edf67c23ec5b583d`、[アプリ Run 40532324525](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40532324525) は **completed/SUCCESS**。Windows Server 2025 Release・xUnit・教材静的検査、Ubuntu 24.04 Release・xUnit・Chrome実操作、PostgreSQL18実DB結合・既存安全拒否の **全3ジョブcompleted/SUCCESS** を認証済みGitHub GETで確認。[PDF Workflow 40532324527](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40532324527) は **completed/SKIPPED**。成功Runの再実行なし。
- **Chrome復旧証跡：** 記録更新HEAD `bef45631bdcd6d83ec3e6ca404ab107eb739ae29` の[Run 40531472560](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40531472560)ではWindows/PG18 SUCCESS、Linux Chromeが旧Step70の匿名拒否メッセージの応答待ちでFAILURE。対話準備待ちを`scripts/browser-smoke.cjs`の第19組ループに追加し、修復HEAD `5e5d09a1e02b2af3ba6b4616edf67c23ec5b583d` で3ジョブ全SUCCESSを実証。旧成功Runの手動再実行はしていない。
- **確定進捗：** 新規初稿 **30/30**、全面改稿 **105/110**、通常復習 **21/22**、段階概要 **8/9**。学習順101〜105（旧Step76〜80）と第21組復習の6本文は **4,135／4,074／3,838／3,898／4,628／3,701文字**。各3,300文字以上、実コード2例、WinForms＋Dapper比較表、操作手順、障害の切り分け、演習・詳しい解答を収録。Razor5画面と復習、Home/NavMenu、旧80URL、前後リンク、Web/PDF目次**ソースのみ**、静的教材検査、Chromeを同期。第8段階概要は既受入のため変更していない。
- **修正点：** `WindowsUserAccessService.ReadCurrentAsync`が認可成功時にもDBの`AuthorizationVersion`を投影するよう修正し、PostgreSQL18実DB結合で検証。権限失効・xmin・監査INSERT・FDW原子性不明時の安全拒否は維持した。
- **最優先禁止：** 私の明示的な再開指示までPDF生成・再生成、Preview Release・正式Release更新を一切禁止。PDF停止Workflowを維持。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f` を変更しない。
- **未受入：** 実AD/IIS/Kerberos/FDW、Data Protection鍵・監査運用、1万人超登録・同時circuit・DB接続プールと切戻し負荷の実機受入。PR #36はDraft/open・main未マージ。ADは本人認証・ログイン名のみ、操作・行単位は既存DB台帳の都度認可。
- **次工程：** 学習順106〜110（capstone-01〜05）・第22組復習・第9段階概要を各3,300文字以上・コード2例以上、WinForms＋Dapper比較・手順・障害切り分け・演習詳答で全面改稿し、Razor・Web/PDF目次**ソースのみ**・NavMenu・旧80URL・前後リンク・静的検査・Chromeを同期。記録更新後の新HEADのアプリCI全3ジョブSUCCESSとPDF SKIPPEDをGETで受け入れる。最後にPRコメントと引き継ぎ用短縮プロンプトを更新する。

## 2026-10-27 第21組101〜105・復習：実装HEADのCI受入確定（PDF停止継続）

- **認証済みGitHub GETによる受入証跡：** 改稿・Razor・目次ソース・静的/Chrome同期・権限版投影修正を含むHEAD `42d1970c1922b02fd0dfef3fc0603f23d7fe5a0d`。アプリ[Run 40530379119](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40530379119)はWorkflow **completed/SUCCESS**、Windows Server 2025 Release/xUnit/静的教材検査、Ubuntu 24.04 Release/xUnit/Chrome実操作、PostgreSQL18実DB結合と既存安全拒否の **全3ジョブcompleted/SUCCESS**。同一HEADの[PDF Workflow 40530379178](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40530379178)は **completed/SKIPPED**。旧成功Runを再実行していない。
- **今回の確定改稿：** 学習順101〜105（旧Step76〜80）本文4,135／4,074／3,838／3,898／4,628文字、第21組復習3,701文字。6本文とも3,300文字以上、実コード2例以上、WinForms＋Dapper比較表、操作手順、障害切り分け、演習・詳しい解答。5つのRazor比較表・復習表、Home/NavMenu、旧80URL、前後リンク、Web/PDF目次**ソースのみ**、静的検査とChromeを同期。既受入の第8段階概要は変更なし。
- **回帰修正：** `WindowsUserAccessService.ReadCurrentAsync`の認可成功時にDBの`AuthorizationVersion`が欠落していた問題を修正し、PostgreSQL18実DBの統合試験で版1の投影を検証済み。Step77の非機密キャッシュの本人・部署・権限版・30秒TTL契約を保持。
- **確定進捗：** 新規初稿**30/30**、全面改稿**105/110**、通常復習**21/22**、段階概要**8/9**。**本記録追記でHEADがさらに変わるため、記録更新後の最新HEADでもアプリCI全3ジョブSUCCESSとPDF SKIPPEDを直接GETして最終受入する。** 前HEADの成功を新HEADへ代用しない。
- **最優先禁止：** ユーザーの明示再開までPDF生成・再生成、Preview Release・正式Release更新を一切禁止。PDF停止Workflowを維持。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`を変更しない。
- **安全・未受入：** PR #36をDraft/open・main未マージで維持。ADは本人認証・ドメイン名取得のみ、業務の操作・行単位認可は既存DB台帳を都度確認。監査・権限失効・xmin・DB/FDW原子性不明時の安全拒否を維持。実AD/IIS/Kerberos/FDW、Data Protection鍵・監査運用、1万人超登録・同時circuit・DBプール負荷と復旧の現地実機受入は未完了。
- **次工程：** 学習順106〜110（capstone-01〜05）、第22組復習、第9段階概要を3,300字以上・コード2例・比較表・操作手順・障害切り分け・演習詳答で全面改稿。Razor/Web・PDF目次**ソースのみ**/NavMenu/リンク/静的検査/Chromeを同期し、同一記録HEADのCIを受け入れる。次回もGitHubを唯一の正本とし、短い引き継ぎプロンプトを作る。

## 2026-10-27 第21組101〜105・復習の全面改稿（PDF停止継続）

- **前回受入HEAD：** `f6c27a56d202dfb1813e75257564f50bdef762f3`。認証済みGitHub GETでDraft/open・main未マージ、[アプリCI 40523479216](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40523479216) Windows/Linux/PostgreSQL18の全3ジョブSUCCESS、同一HEADの[PDF Workflow 40523479478](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40523479478) SKIPPEDを確認。成功済みRunは再実行しない。
- **今回の改稿：** 学習順101〜105（旧Step76〜80）は **4,135／4,074／3,838／3,898／4,628文字**、第21組復習は**3,701文字**。全6本3,300字以上、実コード2例、WinForms＋Dapper比較、操作手順、障害の切り分け、演習詳答。Razor・SecuredLearningService・AuthorizedLessonHintCache・WindowsUserAccessService・LearningAuditService・Program.cs・CI用PostgreSQL18架空台帳と照合。第8段階概要は受入済みのため変更していない。
- **同期：** 5つのRazorと第21組復習比較表、Home/NavMenu、旧80 URL、100→101〜105→第21組復習→106の前後リンク、Web/PDF目次**ソースのみ**、静的検査とChrome匿名拒否を同期。ADは本人認証と名前取得のみ、操作・行単位都度認可は既存DB台帳の正本。監査・xmin・失効・FDW原子性不明時の安全拒否を緩和していない。
- **予定進捗：新規初稿30/30、全面改稿105/110、通常復習21/22、段階概要8/9。** この記録更新後の最新HEADに対応するWindows/Linux Release・xUnit・Chrome、PostgreSQL18実DB・既存安全拒否のアプリCI全3ジョブSUCCESSおよびPDF Workflow SKIPPEDを確認するまで最終受入は保留。旧HEADのCIで代用しない。
- **最優先の禁止：** ユーザーが明示的に再開を指示するまでPDF生成・再生成、Preview Release・正式Release更新を一切禁止。PDF停止Workflow維持。Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`を変更しない。Draft PR #36・main未マージ維持。
- **実機未受入：** 実AD/IIS/Kerberos/FDW、Data Protection鍵・監査の本番運用、1万人超登録・同時circuit・DBプール・負荷・復旧は現地実機の証拠がない。
- **次工程：** 同一最新HEADのアプリ全3ジョブSUCCESSとPDF SKIPPEDの後、学習順106〜110（capstone-01〜05）、第22組復習、第9段階概要を全面改稿。実コード2例・3,300文字以上・比較表・演習詳答、Razor・目次ソース・NavMenu・前後リンク・静的検査・Chromeを同期し、完了時はGitHub進捗・PRコメントと短い引き継ぎプロンプトを作成。

## 2026-10-27 第20組096〜100・復習・第8段階概要の全面改稿（PDF停止継続）

- **改稿前の受入確定：** Draft PR #36 HEAD `0218641d342a00c9e0609653417ffa651f6c9bbf`。認証済みGitHub GETで同一HEADの[アプリCI 40517851741](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40517851741) は **SUCCESS（全3ジョブ：Windows Server 2025 Release/xUnit/静的教材検査、Ubuntu 24.04 Release/xUnit/Chrome、PostgreSQL18実DB・既存安全拒否）**。先にin_progressだったPostgreSQL18ジョブも `completed/success` をGETで確認。[PDF Workflow 40517851952](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40517851952) は同一HEADで **SKIPPED**。成功済みRunは再実行しない。これにより前工程の**95/110、通常復習19/22、段階概要7/9**を受入確定。
- **今回の全面改稿：** 学習順096〜100（旧Step71〜75）を実`WindowsUserAccessService`、`SecuredLearningService`、`LearningAuditService`、`Program.cs`、PostgreSQL18架空台帳・監査SQLと照合して全面改稿。各本文3,300文字以上、実コード2例以上、WinForms＋Dapper比較、操作手順、障害の切り分け、演習と詳しい解答を整備。第20組復習`docs/columns/column71-75.md`と第8段階概要`docs/course-v3/stages/stage08.md`も同水準に改稿。第8段階の全10本文はまだ完成ではなく、101〜105は次工程。
- **画面・品質ゲート：** 5つの実Razorの比較表と前後リンク、第20組復習表、新しい第8段階概要Razor、Home/NavMenu、旧80URLを維持。Web目次とPDF目次の**ソースのみ**を同期。静的`scripts/check-learning-materials.py`とChrome`scripts/browser-smoke.cjs`へ同一内容の検査を追加。既存サービスのAD本人認証／正本DBの操作・行単位都度認可、監査・権限失効・xmin・FDW原子性不明時の安全拒否は緩和しない。
- **今回の予定進捗：新規初稿30/30、全面改稿100/110、通常復習20/22、段階概要8/9。** `curriculum.json`の改稿状態を同期したが、**この記録更新を含む新しい同一HEAD**でWindows/Linux Release、xUnit、Chrome実操作、PostgreSQL18実DBと既存安全拒否のアプリ全3ジョブSUCCESS・PDF Workflow SKIPPEDが成立したときだけ最終受入とする。CI結果は別途PRコメントへ記録し、旧HEADの成功を新HEADへ流用しない。
- **絶対禁止継続：** ユーザーの明示再開指示までPDF生成・再生成、Preview Release・正式Release更新を一切しない。PDF停止Workflowを維持する。Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`を変更しない。Draft PR #36のままmainへマージしない。
- **未受入：** 実AD/IIS/Kerberos/FDW、Data Protection鍵の本番保護、監査運用、1万人超の登録・同時利用者・circuit・DBプールの負荷および復旧の現地実機受入は未完了。CIの架空PrincipalやChromeを本番受入と扱わない。
- **次工程：** 学習順101〜105（旧Step76〜80）と第21組復習を各3,300文字以上・実コード2例・比較表・演習詳答で全面改稿。第8段階概要は今回改稿済みなので再改稿しない。Razor・目次ソース・NavMenu・前後リンク・静的検査・Chromeを同期し、同一最新HEADの全CIとPDF SKIPPEDで受入。次回も短縮引き継ぎプロンプトを提示する。

## 2026-10-27 第19組091〜095・復習の全面改稿実装（PDF停止継続）

- **編集前GitHub正本HEAD：`55cd2400b8f91004515587721e820c5d60760840`**。PR #36はDraft/open・main未マージ。認証済みGETで進捗、ロードマップ、curriculum、引き継ぎ、PRコメント #6442657157と同一HEADのアプリCI [40508494099](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40508494099) SUCCESS（全3ジョブ）、PDF Workflow [40508493970](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40508493970) SKIPPEDを確認。成功した旧CIは再実行しない。
- **学習順091〜095（旧Step66〜70）と第19組復習の全6本文を全面改稿。** それぞれ3,300文字以上、実C#/Razor/EF CoreとPostgreSQL18の試験実装に照合しコード2例以上、WinForms＋Dapper比較、表、操作手順、障害の切り分け、演習と詳しい解答を追加。認可入口、Count/Items、集計・100件CSV、行ロック/xminと一括更新、AD/IIS/FDWと1万人超負荷の未受入を区別する。
- Razor5画面の学習順と比較表、第19組復習表、Home/NavMenu、前後リンク（特別コラム→091〜095→第19組復習→096）、PDF目次**ソースのみ**、静的教材検査とChrome E2E項目を同期。旧80URL、第7段階概要、既存の安全拒否を維持。
- **教材改稿の予定進捗：新規初稿30/30、全面改稿95/110、通常復習19/22、段階概要7/9。** **本記録更新後の同一最新HEADに対して、Windows/Linux Release、xUnit、Chrome、PostgreSQL18実DB・既存安全拒否の全3ジョブSUCCESSおよびPDF Workflow SKIPPEDを確認した場合にだけ正式受入とする。** 成功した旧HEADのCIは代用しない。
- **最優先：ユーザーからの明示再開指示までPDF生成・再生成、Preview Release・正式Release更新を一切禁止。** PDF停止Workflowを保持する。Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、サイズ1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`を変更しない。
- ADは本人認証とユーザー名取得のみ。操作・行単位は既存台帳を都度認可し、監査、権限失効、xmin競合、DB/FDW原子性不明時の安全拒否を維持。**実AD/IIS/Kerberos/FDWと1万人超の実機負荷受入は未完了。**
- **次工程：学習順096〜100（旧Step71〜75）と第20組復習、第8段階概要**を全面改稿。本文3,300字以上、コード2例、WinForms＋Dapper比較、操作・障害診断・詳答を充実させ、Razor/Web/PDF目次**ソースのみ**、NavMenu・前後リンク・静的検査・Chromeを同期。記録更新HEADの全CIとPDF SKIPPEDで受入し、次チャット用プロンプトを作成する。PDF停止継続。

## 2026-10-27 第18組086〜090・復習の実装HEADでアプリCIを確認（PDF停止継続）

- **教材改稿実装HEAD `c4a90adf45b1cdb3595d956ce3474530f7972455`** のアプリ [Run 40507815883](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40507815883) は **SUCCESS**。Windows Server 2025 ReleaseとxUnit/静的教材検査、Ubuntu 24.04 ReleaseとxUnit/Chrome実操作、PostgreSQL18実DBと既存安全拒否の3ジョブすべてSUCCESS。旧成功CIは再実行していない。
- **同一HEADのPDF Workflow [Run 40507815933](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40507815933) は SKIPPED（意図どおり）。** 日本語PDF生成・再生成、Preview Release・正式Release更新は禁止継続。Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`は認証済みGETで不変確認。
- 第18組の5本文は4,927／5,108／5,069／4,946／5,139文字、復習4,370文字。実C#/Razor/SQL、WinForms＋Dapper比較表、2コード例、操作手順、障害の切り分け、演習と詳しい解答、5画面と復習比較表、前後リンク、Web/PDF目次ソース、NavMenu、静的検査、Chromeを同期。第7段階概要は変更なし。
- **改稿達成数：新規初稿30/30、全面改稿90/110、通常復習18/22、段階概要7/9**。本CI実装HEADは受入条件を満たした。ただしこの記録追記でさらに新HEADとなるため、**記録更新後の同一新HEADでも3ジョブSUCCESSとPDF SKIPPEDを再照合して最終受入確定**する。旧HEADのCIを新HEADの証拠に代用しない。
- PR #36はDraft/open、main未マージ、旧80URLを維持。AD本人認証・ユーザー名取得のみ、既存DB台帳が操作・行単位の都度認可の正本。監査・失効・xmin競合・DB/FDW原子性不明時の安全拒否を維持。**実AD/IIS/Kerberos/FDWと1万人超負荷の実機受入は未完了**。
- **次工程：学習順091〜095（旧Step66〜70）と第19組復習の全面改稿**。各3,300字以上、実コード2例、WinForms＋Dapper比較、診断・演習詳答を用意し、同一HEADのCIで受入する。第7段階概要は完了済みのため再改稿しない。PDF停止継続。

## 2026-10-27 学習順086〜090・第18組復習の全面改稿（PDF停止継続）

- GitHub正本を認証済みGETで照合。改稿前HEAD a3c650f3993a52444e3a59ace40cf082839e282b、アプリCI 40503800141全3ジョブSUCCESS、PDF Workflow 40503800130 SKIPPED、PR #36はDraft/open・main未マージ。
- 旧Step61〜65の5教材本文は4,927／5,108／5,069／4,946／5,139文字、第18組復習4,370文字へ全面改稿。実WindowsUserAccessService、LearningDbContext、既存CI台帳SQL、Razorを照合し、各3,300文字以上、C#コード2例、WinForms＋Dapper比較表、操作手順、障害の切り分け、演習と詳しい解答を追加。
- 5画面のRazor比較表、第18組復習表、Home/NavMenu、学習順085→086〜090→第18組→特別コラムの前後リンク、旧80URL、Web目次、PDF目次のソースだけ、静的教材検査・Chromeを同期。第7段階概要は受入済みで変更しない。
- 改稿後の予定進捗：新規初稿30/30、全面改稿90/110、通常復習18/22、段階概要7/9。**この記録更新後の最新HEADでアプリCI全3ジョブSUCCESS、PDF Workflow SKIPPEDを確認した場合だけ受入確定。** 先行成功CIを最新HEADへ流用しない。
- ユーザーからの明示的な再開指示までPDF生成・再生成、Preview Release・正式Release更新を一切禁止。既存Preview生成元SHA 5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71、Asset ID 680751957、1,468,826バイト、SHA256 276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23fを変更しない。PDF停止Workflowを維持する。
- ADは本人認証・ユーザー名取得まで。既存DB台帳が操作・行単位の都度認可の正本。監査、権限失効、xmin競合、DB/FDW原子性不明の安全拒否を維持。実AD/IIS/Kerberos/FDWと1万人超負荷の現地実機受入は未完了。
- 次工程：学習順091〜095（旧Step66〜70）と第19組復習を全面改稿。第7段階概要は改稿済みで再改稿しない。Web/PDF目次ソース、Razor/NavMenu/前後リンク、静的検査、Chromeを同期して最新記録HEADの全CIを受け入れる。PDF停止継続。

## 2026-10-27 学習順081〜085・第17組復習・第7段階概要の実装CI受入（PDF停止継続）

- **同期実装HEAD：`ecff71ed29c14edfef4530605f813b69b63b1689`**。同一HEADの[アプリCI Run 40503079212](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40503079212)は**全3ジョブSUCCESS**（Windows Server 2025 Release・xUnit・静的教材検査、Ubuntu 24.04 Release・xUnit・Chrome実操作、PostgreSQL18実DB結合＋既存安全拒否）。先行成功Runは再実行していない。
- [PDF Workflow Run 40503079177](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40503079177)は同じHEADで**SKIPPED**。**PDF生成・再生成およびPreview／正式Release更新は禁止継続**。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`をGETで確認した。PDF目次はソースのみ更新。
- 学習順081〜085各 **5,115／5,112／5,069／5,130／5,158文字**、第17組復習**3,994文字**、第7段階概要**4,575文字**。7本すべて3,300文字以上で実コード2例・WinForms＋Dapper比較表・操作手順・障害切り分け・演習詳答を収録。Razor比較表・学習順リンク・Home/NavMenu・Web/PDF目次ソース・静的品質検査・実Chromeを同期した。
- **改稿済み進捗：新規初稿30/30、全面改稿85/110、通常復習17/22、段階概要7/9。** 本進捗・curriculum・引き継ぎ記録のコミット後は新HEADに変わる。**記録更新後HEADのアプリCI全3ジョブSUCCESSとPDF Workflow SKIPPEDを照合した時点でのみ最終受入確定**とする。旧実装HEADの成功を新HEADの成功と混同しない。
- Draft PR #36・main未マージ、旧80URL、ADは本人認証とユーザー名のみ、既存DB台帳による操作・行単位の都度認可、監査・権限失効・xmin競合・FDW原子性不明時の安全拒否を維持。実AD/IIS/Kerberos/FDWと1万人超の現地負荷は未受入。
- **次工程**：学習順086〜090（旧Step61〜65）と第18組復習を各3,300文字以上、コード2例、WinForms＋Dapper比較、表、操作手順、診断、演習詳答で全面改稿。第7段階概要は完成済み。Razor/目次ソース/NavMenu/前後リンク/静的検査/Chromeを同期し、記録更新後同一HEAD全3ジョブSUCCESS・PDF SKIPPEDで受入する。

## 2026-10-27 第17組081〜085・第7段階概要の改稿実装（PDF停止継続）

- **改稿実装HEAD**：`e8eea328e78c2c7f1e38e84edf1095dfcb70f797`。学習順081〜085（security-01〜05）の5本文は **5,115／5,112／5,069／5,130／5,158文字**、第17組復習3,994文字、第7段階概要4,575文字へ全面改稿した。実コード2例以上、WinForms＋Dapper比較表、操作手順、障害切り分け、演習と詳しい解答を追加した。
- 実`SecurityLessonPolicy`、`Security01〜05Page.razor`、`Program.cs`と照合した。Cookie・Negotiate・Middleware・Antiforgery・HTMLエンコード・Data Protection鍵・権限版の模擬挙動を実装どおり説明。**架空条件の模擬成功はAD本人認証やDB業務認可の実証ではない**。
- 第7段階概要Razor、第17組復習Razor、5画面の比較表、Home/NavMenu、学習順080→081〜085→復習→086の前後リンク、Web/PDF目次の**ソースのみ**、静的教材検査、Chrome実操作の検査項目を同期した。旧80URLと既存安全拒否の実装は保持する。
- **記録更新後最新HEADのCI検証待ち。** Windows Server 2025/Linux Release、xUnit、Chrome、PostgreSQL18実DB・既存安全拒否の3ジョブすべてSUCCESS、同じHEADのPDF Workflow SKIPPEDでのみ最終受入とする。先行成功Runは新HEADの証拠としない。
- **PDF生成・再生成、Preview Release・正式Release更新は禁止。** 既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`は変更しない。
- **暫定進捗**：新規初稿30/30、全面改稿85/110、通常復習17/22、段階概要7/9。**CI未確定なので正式な受入カウントは80/110・16/22・6/9のまま**。
- Draft PR #36を維持しmain未マージ。ADは本人認証と名前取得のみ、業務の認可は既存DB台帳を正本として操作・行単位で毎回照合。失効・xmin競合・監査障害・FDW原子性不明時の安全拒否を保持。実AD/IIS/Kerberos/FDW・1万人超負荷の現地受入は未完了。
- **次工程**：同一最新HEAD全3アプリCIとPDF SKIPPEDを照合し、合格時のみ`curriculum.json`・進捗を受入済みに更新する。次は学習順086〜090（旧Step61〜65）、第18組復習を各3,300文字以上で全面改稿する。第7段階概要は改稿済みのため再改稿しない。

## 2026-10-27 第16組076〜080・復習の全面改稿（PDF停止継続）

- **教材・画面同期実装HEAD：`8e00e0c5b523a286ea25f7c8c77dcae51cddea2f`**。学習順076〜080（旧Step56〜60）の本文は **5,612／5,157／5,267／5,445／5,286文字**、第16組復習は **4,090文字**。いずれも3,300文字以上で、実コード2例以上、WinForms＋Dapper比較表、画面操作、障害切り分け、演習と詳しい解答を収録した。
- PostgreSQL daterangeの半開区間とOverlaps、Scopedキャッシュの架空A/B・権限版、DB側GroupByとCSVの100件上限／101件拒否、固定SELECTのEXPLAINと20回のP95/P99、10方式の選定を実`LearningRangeExportService`／`LearningCacheLessonService`／`LearningPerformanceService`／Razorと照合した。学習用UIに実AD本人認証・業務認可が実装されているとは扱わない。
- **同一実装HEADのアプリCI：** [Run 40498774402](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40498774402) **SUCCESS**。Windows Server 2025 Release・xUnit・静的教材品質検査、Ubuntu 24.04 Release・xUnit・実Chrome操作、PostgreSQL18実DB結合・既存安全拒否の全3ジョブSUCCESS。先行した検査のゼロ埋め／索引照合の不一致を修復しており、先行失敗Runを成功扱いしていない。
- **PDF Workflow：** [Run 40498774601](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40498774601) **SKIPPED**。PDF生成・再生成、Preview Release・正式Release更新はユーザーの明示的な再開指示まで**一切禁止**。PDF目次の`scripts/build_docs_pdf.py`は**ソースの章名のみ**同期しておりスクリプトは実行しない。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`を保持する。新HEADとPDF生成元SHAの差は意図どおりである。
- 旧URL `/learn/56`〜`/learn/60`、第16組復習 `/learn/column/60`、Home/NavMenu、前後リンク（第15組復習→076〜080→第16組復習→081）、静的品質検査、ChromeとPDF目次**ソースのみ**を同期。第6段階概要は既改稿のため変更しない。
- **教材実装検証後の進捗：新規初稿30/30、全面改稿80/110、通常復習16/22、段階概要6/9**。**本記録更新コミットでさらに最新HEADが変わるため、記録更新後の同一HEADについて全3アプリCI SUCCESSとPDF Workflow SKIPPEDを再照合してから最終受入確定する。** それまでは実装HEADの合格と記録更新HEADの受入を区別し、PDF関連受入は保留とする。
- 安全条件：PR #36はDraft/openでmain未マージ、旧80 URLを維持。ADは本人認証とユーザー名取得のみ、操作・行単位の業務認可は既存DB／外部台帳を都度照合。監査・権限失効・xmin競合・FDW原子性不明時の安全拒否を維持。**実AD/IIS/Kerberos/FDWと1万人超の実機受入は未完了**でありCIと混同しない。
- **次工程：学習順081〜085（security-01〜05）、第17組復習、第7段階概要**を実Razor/C#/SQLに照合して全面改稿し、本文3,300文字以上、実コード2件以上、WinForms＋Dapper比較表、手順、切り分け、演習・詳答、Web／PDF目次**ソースのみ**、静的検査、Chromeを同期する。次回も同一記録HEADのアプリCI全3成功を受入条件とする。PDF停止継続。

## 2026-10-27 第15組受入・次チャットへの引き継ぎ（PDF生成停止）

- GitHubを唯一の正本として、Draft PR #36の最新HEAD、`docs/course-v3/progress.md`、`full-rewrite-roadmap.md`、`curriculum.json`、本引き継ぎ、最新PRコメント、同一HEADの全CIを**認証済みGET**で確認する。
- **直近の実装HEAD：`b67bc7433c9f40fd5950f94d107478d48e61b8dc`**。アプリCI [40493578738](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40493578738)は全3ジョブSUCCESS、PDF Workflow [40493578748](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40493578748)はSKIPPED。**本記録更新の新HEADはさらに検証すること。**
- 進捗は新規初稿30/30、全面改稿75/110、通常復習15/22、段階概要6/9。第6段階概要は改稿済み。第15組071〜075の教材・Razor・Web目次・PDF目次ソース・Chrome・静的検査の同期は実装HEADで完了。
- **最優先：ユーザーの明示的な再開指示までPDFを生成・再生成せず、Preview/正式Releaseを更新しない。** PDF停止Workflowを維持。既存Preview生成元`5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID 680751957、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`のまま。PDF関連受入は保留。
- **次工程：学習順076〜080（旧Step56〜60）と第16組復習を全面改稿。** 実Razor/C#/SQLに照らして各本文3,300字以上、実コード2件以上、WinForms＋Dapper比較・表・操作・診断・演習と詳答を充実する。Razor/旧URL/Web/PDF目次**ソース**/NavMenu/前後リンク・静的品質検査・Chromeを同期し、新記録HEADのWindows/Linux Release、xUnit、Chrome、PG18実DB、安全拒否の全3ジョブSUCCESS後のみ80/110・16/22へ進む。
- Draft PR #36・main未マージ・旧80 URL・AD本人認証とユーザー名取得・既存DB台帳での操作/行ごと都度認可・監査・失効/競合/監査障害の安全拒否を維持。実AD/IIS/Kerberos/FDW・1万人超負荷の実機受入は未完了。完了したらPRコメントに証跡・残課題・次工程を記録し、次チャット用のコピーボタン付き短縮プロンプトを返す。

## 2026-10-27 第14組066〜070・通常復習の全面改稿（PDF停止継続）

- **本文・画面同期実装HEAD：`51243bbe6046456e5140959834c72eba5a37e966`**。初稿5本は合計30/30のまま。学習順066〜070（旧Step46〜50）は **4,731／4,703／4,673／4,941／5,091文字**へ全面改稿、第14組復習は **4,036文字**。各本文は実コード2例、WinForms＋Dapper比較、操作、障害切り分け、演習と詳しい解答を含む。
- Step46の関連CategoryをDTO射影、47のIdキーセット、48の500件メモリ保持とVirtualize、49の最大200件DB範囲取得、50の400ミリ秒待機・取消・再試行を、実Razor・LearningAdvancedPageService・LearningPageServiceと照合した。
- Razor5画面の学習順表示・比較表、レビュー画面、Home/NavMenu、前後リンク（第13組→066〜070→第14組→071）、PDF目次の**ソース文字列のみ**、静的教材品質ゲート、Chrome操作を同期。第6段階概要は既受入のため再改稿しない。旧80ステップのURLは保持。
- 先行アプリCI [40489203833](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40489203833) のUbuntuコンパイル失敗（Step47の説明文中のRazor `@cursor` 解釈）を `@@cursor` へ修復。修復実装HEADの[アプリCI 40489404815](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40489404815)と[PDF Workflow 40489404804](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40489404804)を確認対象とする。
- **予定進捗：新規初稿30/30、全面改稿70/110、通常復習14/22、段階概要6/9。** 本進捗・カリキュラム記録更新により新HEADとなるため、**記録更新後の最新HEAD**でWindows/Linux Release、xUnit、Chrome、PostgreSQL18実DB・既存安全拒否のアプリCI全3ジョブSUCCESSを確認した場合だけ最終受入を確定する。未成功時は受入未完了とし必要な修復を行う。
- PDF生成・再生成、Preview Release・正式Release更新は禁止のまま。PDF停止WorkflowはSKIPPEDを維持。既存Preview生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71` とAsset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f` を変更しない。現HEADとのSHA差異は正常で、PDF受入は保留。
- Draft PR #36・main未マージ、本人AD認証とユーザー名のみ、既存DB/外部台帳の操作・行単位都度認可、権限失効・xmin競合・監査失敗・FDW原子性不明時の安全拒否を維持する。**実AD/IIS/Kerberos/FDWと1万人超の現地負荷受入は未完了**。
- **次工程：学習順071〜075（旧Step51〜55）と第15組復習**を実コードと照合して全面改稿。各3300字以上、実コード2例、WinForms＋Dapper比較、表、操作、診断、詳答を含める。Razor/目次・PDFソースだけ・Chrome・静的検査を同期し、最新HEAD全3アプリCI SUCCESSで75/110・15/22へ進める。PDF停止継続。

## 2026-10-27 第13組061〜065・第6段階概要の全面改稿受入（PDF停止継続）

- **改稿実装・検証HEAD：24d59a62c01b89f5f5ab9c1c501a5bdd405d9a66**。Draft PR #36はopen/draft、main未マージ。既存80ステップのURLは維持した。
- 同一HEADの[アプリCI 40482470750](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40482470750)は**SUCCESS**。Windows Server 2025 Release・xUnit・静的教材整合、Ubuntu 24.04 Release・xUnit・Chrome実操作、PostgreSQL18実DB結合・既存安全拒否を含む全3ジョブSUCCESS。先行してPDF目次の旧構成概要を欠落させた静的検査失敗を確認し、旧URLの互換索引を保持する修復コミットで解消。成功済みRunを再実行していない。
- [PDF Workflow 40482470822](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40482470822)は**SKIPPED（意図的）**。PDF生成・再生成、Preview Release・正式Release更新はユーザーからの明示的な再開指示まで禁止。PDF目次は `scripts/build_docs_pdf.py` の**ソースだけ**更新し、生成スクリプトは実行しない。
- 既存Previewの生成元SHA `5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f` を維持し、最新HEADへ追随させない。
- 学習順061〜065（旧Step41〜45）はそれぞれ**4,796／5,051／5,021／4,929／5,239文字**へ改稿。実コード2例、WinForms＋Dapper比較表、障害切り分け、操作、演習と詳しい解答を収録。第13組復習 `docs/columns/column41-45.md` は**3,999文字**、第6段階概要 `docs/course-v3/stages/stage06.md` は**4,120文字**。
- 5つのRazor比較表、Web目次/Home/NavMenu、学習順061〜065→第13組復習→066、段階概要ページ、PDF目次**ソース**、静的品質検査とChrome実操作を同期。旧構成の段階概要・全80 URLと、第12組復習から061への導線も維持。
- **確定進捗：新規初稿30/30、全面改稿65/110、通常復習13/22、段階概要6/9。** 第6段階061〜080は最初の5本のみ全面改稿受入であり、残り15本を完了扱いしない。
- **この進捗記録の更新でHEADが変わるため**、記録更新後の最新HEADについてアプリCI全3ジョブSUCCESSとPDF Workflow SKIPPEDを改めて照合すること。旧実装HEADのCI成功を新HEADの成功と混同しない。
- **次工程：学習順066〜070（旧Step46〜50）と第14組復習**。実Razor/C#/SQL・PostgreSQL18のDTO射影、キーセットページング、仮想化、DB取得とVirtualize、検索UXを読み直し、各本文3,300文字以上、コード2例以上、WinForms＋Dapper比較、複数表、操作、障害切り分け、演習と詳しい解答を充実させる。第6段階概要は改稿済み。Web/PDF目次ソース・Razor/NavMenu/前後リンク・静的検査・Chromeを同期し、同一最新HEADの全アプリCI SUCCESSを条件に70/110・14/22へ進む。
- AD本人認証・ユーザー名取得、既存DB/外部台帳の操作・行単位都度認可、監査、権限失効・xmin競合・監査障害時の安全拒否を緩和しない。**実AD/IIS/Kerberos/FDWと1万人超の現地負荷試験は未受入**。Draft・main未マージ・正式Release維持、成功CI再実行禁止を継続する。

## 2026-10-27 第12組056〜060・復習の全面改稿受入（PDF生成停止を維持）

- **改稿実装HEAD：`081f082f1a974f7448c5b402e3be684037d2612f`**、Draft PR #36はopen/draft、main未マージ。既存80ステップのURLを保つ。
- 同一HEADの[アプリCI 40478429173](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40478429173)は**SUCCESS**。Windows Server 2025 Release・xUnit・静的教材検査、Ubuntu 24.04 Release・xUnit・Chrome実操作、PostgreSQL 18実DB結合と既存安全拒否の全3ジョブSUCCESS。以前の成功CIは再実行していない。
- [PDF Workflow 40478429183](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40478429183)は**SKIPPED（意図的）**。PDF生成・再生成、Preview Release・正式Release更新はユーザーの明示指示まで引き続き禁止。目次を`scripts/build_docs_pdf.py`のソースのみで同期し、PDF生成スクリプトは実行していない。
- Previewの既存生成元は`5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`のまま。単体Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`。最新PR HEADと生成元の差異は正常でありPDFで一致させない。
- 学習順056（トランザクション）**4,446文字**、057（xmin）**4,264文字**、058（既存台帳・キーなし・FDW）**4,420文字**、059（FromSql／SqlQueryとDapper）**4,534文字**、060（PostgreSQL18実接続）**4,522文字**、第12組復習**5,396文字**に全面改稿。各本文にコード2例以上、WinForms＋Dapperの比較表、原因切り分け表、操作手順、演習と詳しい解答を追加した。
- 5画面のRazor比較表、第12組復習画面、Home・NavMenu・前後リンク、Web目次、PDF目次**ソース**、静的品質検査、Chrome実操作を同期。既存のAD本人認証・既存DB台帳都度認可・監査・xmin競合・権限失効／失敗時の安全拒否を変更しない。
- **確定進捗：新規初稿30/30、全面改稿60/110、通常復習12/22、段階概要5/9。第5段階046〜060の全面改稿を完了。**
- **重要：この進捗記録更新自体で新HEADとなる**。記録更新後の最新HEADに対するアプリCIの全ジョブSUCCESSとPDF Workflow SKIPPEDを改めて認証済みGETで照合し、その証拠をPRコメントへ記録する。旧HEADの成功を新HEADの成功と混同しない。Previewの生成元SHAは更新しない。
- **次工程：学習順061〜065（第6段階の先頭5件）・第13組復習・第6段階概要**を実Razor/C#・PostgreSQL18で照合して全面改稿。各本文3,300字以上、コード2例以上、WinForms＋Dapper比較、表、操作、障害切り分け、詳しい解答を充実させる。Razor、Web/PDF目次ソース、NavMenu、前後リンク、静的検査、Chromeを同期し、同一最新HEADのアプリCI全SUCCESSで受入する。PDF関連の受入は保留。
- 実AD/IIS/Kerberos/FDW・1万人超負荷の現地試験は**未受入**であり、CI成功と区別する。Draft維持・main未マージ、旧URL、正式Releaseの維持、成功CI再実行・PDF出力禁止を継続する。

## 2026-10-27 第11組051〜055・復習の実装受入（PDF停止継続）

- **GitHub正本の実装HEAD：`03e694f129621a1ac877bfb166332475e5dd1499`**。Draft PR #36はopen/draft、main未マージ。旧80ステップのURLは保持。
- 同一HEADの[アプリCI Run 40473219284](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40473219284)は**SUCCESS**。Windows Server 2025 Release・xUnit・静的教材整合、Ubuntu 24.04 Release・xUnit・Chrome実操作、PostgreSQL 18専用実DB結合・既存安全拒否の3ジョブがすべてSUCCESS。成功Runは再実行しない。
- [PDF Workflow Run 40473219518](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40473219518)は**SKIPPED（意図的）**。PDF生成・再生成、Preview/正式Release更新はユーザーの再開指示まで禁止。PDF索引は`scripts/build_docs_pdf.py`の**ソース文字列だけ**を整理したが、スクリプトは実行していない。
- Preview `learning-textbook-v3-preview` の既存PDFは生成元`5d3d98cf8f9110f3cd2ea09014c1911bc7ff1c71`、Asset ID `680751957`、1,468,826バイト、SHA256 `276fed44ef3e004a67cf861122b659acfe0fce78c057f905c791e625296ba23f`のまま。**実装最新HEADとの差は正常、差を埋めるPDF出力は絶対にしない。**
- 第11組051〜055の5本文を4,656／4,420／4,399／4,420／4,522文字、第11組復習を5,173文字へ全面改稿。各本文に実コード2件、WinForms＋Dapper比較、表、操作手順、障害切り分け、演習と詳しい解答を追加した。
- Razor5ページの比較表、第11組復習画面、Home/ナビメニューの051〜055→復習→056の導線、静的教材品質検査、Chrome操作、PDF索引**ソース**を同期。データ型・生成SQLの非接続性・変更追跡とSaveChanges・xminによる競合拒否は実装に合わせた。
- **確定進捗：新規初稿30/30、最終全面改稿55/110、通常復習11/22、段階概要5/9。** 第5段階全15件はまだ完了していない。
- 本進捗記録のコミットによって新HEADが生じるため、**記録更新後HEADのアプリCI（Win/Linux/xUnit/Chrome/PG18/安全拒否/静的品質検査）が全SUCCESSで、PDF WorkflowがSKIPPEDであることを再照合する。** この項目は実装HEADの既存CI成功と混同しない。Preview PDFを更新しない。
- **次工程：学習順056〜060（旧Step38・39、db-bridge-04・05、旧Step40）と第12組復習を全面改稿。** 各本文3,300文字以上、実コード2例、WinForms＋Dapper比較、複数表、操作、障害切り分け、詳しい解答。Razor、Web/PDF目次のソース、NavMenu、前後リンク、品質ゲートとChromeを同期し、第5段階を完成させる。PDF関連の受入は停止中として保留。
- ADは本人認証とユーザー名のみ、業務認可は既存DB/外部台帳に基づく都度判定、監査・権限失効・更新競合・監査失敗時の安全拒否を維持。実AD/IIS/Kerberos/FDW・1万人超負荷の実機受入は未完了であり、CIの成功と混同しない。

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

## 2026-10-27 学習順041〜045・第9組復習・第4段階完了の同一HEAD受入（最新）

- **教材実装の受入HEAD** `3cab5c0fc430ea473675c6eb4acf04e0ead7103b`。アプリ [Run 40447468064](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40447468064) はWindows Server 2025/Linux Release・xUnit・Chrome実操作・PostgreSQL18実DB結合と既存安全拒否の全ジョブSUCCESS。日本語PDF [Run 40447468058](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40447468058) も教材品質ゲート・PDF生成・Preview公開SUCCESS。
- **Preview Release** [learning-textbook-v3-preview](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) は生成元HEAD一致。単体 `blazor_learning_textbook_ja.pdf`、Asset ID `680389176`、**1,416,502バイト**、SHA256 `8514f1eaa4c3bbe0cf13416615329de73ee8d23cfff2bed7afce247898cfb50f`。Release GETとAsset個別GETのname・size・digest一致、ZIPなし。
- 学習順041〜045（browser-01〜05）の全面改稿は各**5,066／5,283／5,668／5,638／5,744文字**、第9組復習**5,843文字**。実Razor/C#/JS、WinForms＋Dapper比較、診断表、実操作、拒否、演習・詳しい解答を収録。第4段階概要、5画面Razorの比較表、復習画面、ホーム/NavMenu、前後リンク、Web/PDF索引、品質ゲート、Chromeを整合。
- 初回実装HEAD `a9815703638d22484f8b1c6f7c417338549d916a` のChrome検査は学習順のゼロ埋め欠落（41対041）でFAILURE。新HEAD `3cab5c0...` でテスト識別子を修正し、失敗した旧HEADを成功扱いせず全CIで受入。成功済みCIの手動再実行は実施していない。
- **確定進捗：新規初稿30/30、最終全面改稿45/110、通常復習9/22、段階概要4/9。第4段階031〜045と第7〜9組復習が完了。** Draft PR・旧80URL・正式main教材と正式Releaseは維持。AD認証は本人とドメイン名だけ、認可は既存DB/外部台帳を正本として都度判定し、監査・権限失効・更新競合・失敗時の安全拒否を維持。実AD/IIS/Kerberos/FDW・1万人超実機負荷は未受入。
- **次工程：学習順046〜050（db-bridge-01、旧Step31〜34）と第10組復習、第5段階概要**。EF Core10・Npgsql・DbContextFactory・PG18の実Razor/SQL/C#と照合して各3300文字以上、実コード2以上、WinForms＋Dapper比較、切り分け、操作、演習と詳答を全面改稿。第5段階先頭、前後リンク、Web/PDF、日本語、Chrome・品質ゲート・安全拒否を同期。同一HEADのWindows/Linux・xUnit・Chrome・PG18・PDF成功とPreview PDF生成元SHA一致後にのみ50/110・10/22・5/9へ進む。
- **記録更新によりHEADが進んだ場合**、必ずその新HEADのApp/PDF CI全SUCCESSおよびPreview Release生成元SHA・単体PDF AssetのGET一致を再照合すること。旧HEADの成功を新HEADの証跡として流用しない。

## 2026-10-27 学習順036〜040・第8組復習のCI/PDF受入と次工程
- Draft PR #36の実装HEAD `42c394f9f8bd921ac149bc8317d886f4f7b754f7` はアプリ Run 40444727667とPDF Run 40444727659が同一HEAD全SUCCESS。Windows/Linux、xUnit、Chrome、PG18、既存安全拒否、教材品質ゲート成功。
- Preview Release単体日本語PDF：Asset 680332166、1,348,776バイト、SHA256 `5415dff04fc5b1e8d4cf8d217e288319838a01376457feeb937b8d272134307c`、Release生成元SHA一致。
- **確定進捗：新規初稿30/30、最終全面改稿40/110、通常復習8/22、段階概要4/9。** 学習順036〜040本文5,037/4,012/4,259/4,349/5,520字、復習5,041字。Razor・Web/PDF目次・比較表・品質ゲート・Chromeを同期。
- **記録更新後HEADのApp/PDF CIとPreview生成元SHAも必ず再確認する。** 前のCIを新HEADの証跡と混同しない。成功CI再実行不要。
- 次は**学習順041〜045（browser-01〜05）と第9組復習**を各3300文字以上、WinForms＋Dapper比較、実コード2件、表、操作、切り分け、演習・詳答で全面改稿。Razor・目次・前後リンク・Chrome・PG18・安全拒否を維持し、第4段階を完成させる。Draft・旧80URL・正式main版・AD本人認証／DB台帳認可／監査・フェイルクローズ維持。実AD/IIS/FDW現地未受入を成功扱いしない。作業後も最新GitHub正本を参照する簡潔なコピーボタン付き引き継ぎ用プロンプトを作る。

## 2026-10-27 学習順036〜040・第8組復習の改稿実装（同一HEADのCI受入前）
- 旧Step26〜30の実Razor/C#と教材を照合し、学習順036〜040の実コード・WinForms＋Dapper比較/診断表・詳細演習を追加。第8組復習の長文・Razor比較表を同期した。
- ホーム/メニューに第8組復習を追加、学習順040→第8組復習→041の導線を維持。Chromeと教材品質ゲートに第8組の画面・3300字・複数表・2コード・PDF順を追加。
- **未確定：既存の35/110・7/22・4/9を維持。** 新しいHEADのWindows/Linux・xUnit・Chrome・PostgreSQL18・安全拒否・日本語PDF全CI成功、Preview Releaseの生成元SHAと単体PDFを確認した後だけ40/110・8/22に進める。
- Draft、既存URL、正式mainと正式PDF、安全条件を維持。成功済みCI再実行・正式版更新は禁止。

## 2026-10-27 学習順031〜035・第7組復習・第4段階概要の受入
- Draft PR #36 の実装HEAD `9d700f3ea0d043f4dd23bf0ab22fa3e6616f461e` はアプリCI 40442654275、PDF CI 40442654171がともにSUCCESS。Windows 2025/Linux、xUnit、Chrome、PostgreSQL18と既存安全拒否を受入。
- 日本語教材5本を3,919〜4,774文字に改稿し、第7組復習4,258文字、第4段階概要3,609文字、Razor表・Web/PDF索引・前後リンク・品質ゲートを同期。旧Step10のChrome検証の重複文言判定をフォーム内へ限定した。
- Preview Releaseの単体PDFは1,311,087バイト、SHA256 `eb327fa99af1745be0acf2d09fc7e8751f0985c60e41149177904a8499988720`。生成元SHAは上記実装HEADと一致。ZIP化・正式main版の変更なし。
- **確定進捗：新規初稿30/30、最終全面改稿35/110、通常復習7/22、段階概要4/9。** 第4段階の036〜045は未完了。
- まず本記録更新後の最新PR HEADに対してアプリCI/PDF CIとPreview Release生成元SHAの一致を認証済みGitHub GETで照合し、PRコメントに証跡を記録する。成功CI再実行は不要。
- **次は学習順036〜040（旧Step26〜30）と第8組復習**を全面改稿する。各本文3300文字以上、実コード2例以上、WinForms＋Dapper比較・切り分け・操作・演習と詳細解答を充実し、Razor・ホーム/メニュー・Web/PDF目次・品質ゲート・Chrome/PG18/安全拒否を同期する。
- Draft PR・旧80のURL・正式main版を維持し、AD本人認証・既存DB/外部台帳の都度認可・監査とフェイルクローズの条件を変えない。実AD/IIS/FDWと大規模負荷の実機未受入を成功扱いしない。次回完了時もGitHubを正本とした引き継ぎプロンプトとコピーボタンを作成する。


## 2026-10-27 学習順026〜030・第6組復習の同一HEAD最終CI受入
- **確定進捗：新規初稿30/30、最終全面改稿30/110、通常復習6/22、段階概要3/9。** 第3段階021〜030本文10本・第5/6組の復習を受入。段階概要3/9の件数は変更しない。
- **実装・受入HEAD `4d8c652aa0c58c795a111ccc9afbda9c504b39d2`**。アプリ [Run 40429979992](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40429979992) SUCCESS：Windows Server 2025/Linux Release、xUnit、Chrome実操作、PostgreSQL18実DB結合と安全拒否の既存試験。PDF [Run 40429979990](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40429979990) SUCCESS：教材品質ゲート、日本語PDF生成とPreview反映。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) の生成元SHAは実装HEADと一致。単体Asset `blazor_learning_textbook_ja.pdf` **ID 680012256、1,272,480バイト、SHA256 `1fcdcc375ddb62f2ec10505682687639f06b1fe2cda806bfb61637feb219fbff`**。Release GET・Asset個別GETの名前・サイズ・digest一致。ZIPなし、正式main版未変更。
- 次工程は**学習順031〜035（旧Step21〜25：MudBlazorと日本語翻訳・レイアウト・日本語入力・MudForm・MudTable）と第7組復習、第4段階概要**を実Razor・C#に合わせて全面改稿する。各本文3300文字以上を目安に、実コード2ブロック以上、WinForms＋Dapper比較表・切り分け表・操作・演習と詳しい解答、前後リンク、Web/PDF目次、品質ゲート、Chromeを同期。全CIとPreviewの同一HEAD照合後のみ35/110・7/22・概要4/9へ更新する。
- 引き続きDraft PR #36、旧80URL、AD本人認証と既存DB/外部台帳による都度認可・監査・安全拒否を維持。実AD/IIS/Kerberos/FDW/大規模負荷の現地未受入は成功扱いしない。成功済みCIの不要な再実行、正式Release再発行、mainへのマージは禁止。
- **この記録更新コミット自体も新HEADになるため、そのHEADに対するApp/PDF CIとPreview Release生成元SHAの再照合が必要。** 前HEADの成功を新HEADの証跡として流用しない。


## 2026-10-27 学習順026〜030・第6組復習の全面改稿実装（CI受入待ち）
- 既存Razor/C#のOptions、ILoggerとCancellationToken、固定時計xUnit、Applicationの進捗サービス、ITodoServiceとInMemoryTodoServiceを読み、旧Step16〜20の本文を長文改稿。WinForms＋Dapper比較表、診断・切り分け表、実コード2ブロック以上、操作・演習・詳細解答を整備した。
- 画面に学習順026〜030と比較・診断表を追加し、第6組復習の表、既存Web/ナビ/PDF順、ライブラリDI特別コラムへの導線を同期。Step17の重複開始で古い非同期処理の完了・取消が新しい表示を上書きする競合を現行CancellationTokenSourceの参照照合で防止した。
- 教材品質ゲートを第6組の3300文字・複数表・コード・Web/PDF順へ拡張。Chrome E2Eに026〜030の表表示、Step19練習用60上限、Step20上限5件・6件目拒否、第6組復習を追加。Step19の60は全110本の教材総数と区別する。
- **同一HEADのWindows Server 2025/Linux、xUnit、Chrome、PostgreSQL18実DBと安全拒否、日本語PDFの全CI成功、Preview Release生成元SHAとAsset照合が完了するまでは、確定進捗を新規初稿30/30、全面改稿25/110、通常復習5/22、段階概要3/9に据え置く。** 実AD/IIS/Kerberos/FDW未受入。正式main/Release、旧80 URL、Draft、認可・監査・安全拒否の条件を維持。
- 成功受入後の次工程は学習順031〜035（旧Step21〜25：MudBlazor導入・レイアウト・日本語入力・MudForm・MudTable）、第7組復習と第4段階概要の改稿。実Razor・日本語長文・Web/PDF・品質ゲート・Chromeを揃えて検証する。

## 最新の実施状況（2026-10-27・学習順021〜025受入済み）

- Draft PR #36で旧Step11〜15の実コードを読み、学習順021〜025の本文をそれぞれ3,300字以上へ全面改稿。第3段階概要・第5組の復習、Razor、Web目次とメニュー、PDF目次、品質CI、Chromeの遷移を同期した。
- **最終全面改稿25/110、通常復習5/22、段階概要3/9、新規初稿30/30。** 実装HEAD `5472f4edf054b62c998f8d431a3222f874f6d739` のApp Run 40422500932とPDF Run 40422500829はSUCCESS。Preview Releaseの生成元SHAが一致、PDF単体1,243,463バイト、SHA256 `8875f5e4371134099a49fe4f17189d8c62a89ccc5f9da2dd704a7fedfc6a0cbd` を認証済みGETで確認した。
- 次工程はまず記録更新後の最新PR HEADの全CI結果とPreview Releaseを認証済みGETで再確認。不整合があれば修復し、成功済みRunは再実行しない。PRコメントに最終HEADと証跡を記録する。
- 続く作業は学習順026〜030（旧Step16〜20）、第6組復習、特別DIコラムの接続。既存80 URL、Draft、正式main版、AD本人認証・既存DB/外部台帳都度認可・安全拒否を維持。現地AD/IIS/Kerberos/FDW未受入を成功扱いしない。成功済みCIの再実行は禁止。

# 次チャットへの引き継ぎ：教材v3の30新規ステップと全110本の順次再編

対象: https://github.com/ShutenOishi/pg_blazor_samples

## 2026-10-27 学習順016〜020の全面改稿・第4組受入（最新）
- **確定進捗：新規初稿30/30、最終全面改稿20/110、通常復習4/22、段階概要2/9。第2段階は完了。** 旧Step09、runtime-03、旧Step10、runtime-04、runtime-05の5本文を実Razor/C#に照合し4,534〜5,227文字へ改稿、第4組復習を5,642文字へ充実化。比較/診断表、実コード2ブロック以上、操作、失敗と復旧、演習/詳答を収録。
- Razorの表・解説・前後リンク、stage02概要、Web/PDF目次、品質ゲート、Chrome実ブラウザー操作を同期。実装HEAD `897153ba5c4e4c65bf91c6fa6c2c151d0e86a582` は[App Run 40418321144](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40418321144)と[PDF Run 40418321149](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40418321149)が全SUCCESS。Windows Server 2025/Linux、xUnit、Chrome、PG18実DBと既存安全拒否を確認。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)の単体日本語PDFは1204237バイト、SHA256 `00b2247f3f2eee2fd92944719403dd02ba9370c88f2cc3dba45cde16fefb42d4`。生成元SHAと上記実装HEADの一致を認証済みGETで確認済み。正式main版は更新しない。
- **次は021〜025（旧Step11〜15：DI基礎、インターフェイス、Singleton/Scoped/Transient、circuitのScoped、injectとコンストラクター注入）と第5組復習・第3段階概要。** 既存Razor/C#・サービス登録を読み、各本文3300文字以上、WinForms＋Dapper比較、実コード2件以上、比較/診断表、操作、演習/詳答を全面改稿。Web/PDF章順・Razor導線・品質CI・ChromeとPG18/安全拒否を同期し、全CI成功・Preview生成元一致後のみ25/110・5/22・3/9へ進める。
- **再開直後はPR最新HEAD（この記録更新後の新しいHEAD）を認証済みGETで確認し、App/PDF CIとPDF Releaseの生成元SHA・Assetを再照合。** 本節のHEADは教材実装の受入証跡で、記録更新後のHEADではありません。成功済みCI再実行・Release再発行禁止。
- 正本は`docs/course-v3/progress.md`、`docs/course-v3/full-rewrite-roadmap.md`、`docs/course-v3/curriculum.json`、PR #36最新コメント。Draft維持、旧80 URL、AD本人認証＋既存DB/外部台帳の都度認可、監査とフェイルクローズ、実AD/IIS/FDWの実機未受入を厳守。最終返答に引き継ぎ用プロンプトのコピーボタンを設ける。

## 2026-10-27 学習順011〜015の全面改稿・第3組受入（最新）
- **確定進捗：新規初稿30/30、最終全面改稿15/110、通常復習3/22、段階概要2/9。** 第2段階の残り016〜020は未改稿。
- 第3組の5教材を実Razor/C#と照合してそれぞれ4,231〜4,780文字へ全面改稿。第3組復習4,990文字、比較表・診断表・実コード・演習/詳しい解答を収録。第2段階概要、Razor導線、Web目次内の復習位置、PDF章順、Chrome試験と品質ゲートを同期した。
- 実装HEAD `dcae9e218569f09b0749f94d53228f1d837396c3` の[App Run 40412865629](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40412865629)と[PDF Run 40412865704](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40412865704)は共にSUCCESS。Win2025/Linux/xUnit/Chrome/PG18を受入。日本語PDF単体1,141,020バイト、Asset SHA256 `b646ebc7af8e8cf01c57ca043965fe5b238c5f2d8af006e8d754db34668ef99e` は[Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)で生成元SHAが実装HEADと一致。
- 進捗記録を更新したあとの**最新PR HEAD・同一HEADの新CI・Previewの生成元SHAは必ず認証済みGETで照合する**。前のCI成功を別HEADの成功と混同しない。成功済みRunは再実行しない。
- **次は学習順016〜020（旧Step09・runtime-03・旧Step10・runtime-04・runtime-05）と第4組復習の全面改稿。** 第2段階概要・Razor画面・前後リンク・Web/PDF目次・品質ゲート・Chrome E2Eを同期。全CIとPDFのSHAが一致したら20/110、復習4/22、概要2/9へ更新する。
- 正本は`docs/course-v3/progress.md`、`docs/course-v3/full-rewrite-roadmap.md`、`docs/course-v3/curriculum.json`、PR #36の最新コメント。旧80URL、Draft、正式main版、AD本人認証と既存DB台帳認可・監査・安全拒否を保持。実AD/IIS/FDWの実機未確認を成功扱いしない。次回も引き継ぎ用プロンプトのコピーボタンを設ける。

## 2026-10-27 学習順006〜010の全面改稿・第2組受入（履歴）
- **確定実績：新規30/30初稿、最終全面改稿10/110、通常復習2/22、段階概要2/9。** 第2段階の残り011〜020は未改稿。
- 第2組（旧Step01/02/03、runtime-01、旧Step04）の長文本文5本、段階概要、復習、Razorの学習順と前後リンク・Web目次を実コードに合わせて改稿。品質CIは比較/診断表・2個以上のコード・演習/解答・PDF章順・リンクを検査。Chrome E2Eの学習順と0未満防止を追加。
- 実装最終HEAD `d87ee9a96b7fb790645e5ef7d624008009f92980` のApp Run [40409325752](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40409325752)（Win2025/Linux/Chrome/xUnit/PG18）とPDF Run [40409325725](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40409325725)が全SUCCESS。初回PlaywrightのForm曖昧検索のFAILを完全一致で修復。成功Runの再実行は不要。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)は単体日本語PDF 1,079,388バイト、Release生成元SHAと上記HEADが一致。正式main版とDraft PRは維持。
- **次は学習順011〜015（旧Step05〜08＋runtime-02）と第3組復習の全面改稿。** 各本文3300文字以上を目安に、WinForms＋Dapper比較、実装コード2ブロック以上、比較表と診断表、手順、エラーの切り分け、演習と詳しい解答を追加。第2段階概要・Razor・Web/PDF索引・前後リンク・テストを同期。全CIとPreview生成元SHA一致後のみ15/110へ進める。
- 最新HEAD、App/PDF CI、Preview Releaseを認証済みGitHub GETで再確認し、進捗記録だけのコミットも同一SHAのCI/PDF受入対象として照合する。毎回最後にコピーできる引き継ぎプロンプトボタンを設ける。
- `docs/course-v3/progress.md`、`docs/course-v3/full-rewrite-roadmap.md`、`docs/course-v3/curriculum.json`、PR #36の最新コメントを正本として再開する。AD本人認証と既存DB台帳認可・監査・安全拒否、旧80 URL、正式ReleaseとDraft状態を維持。実AD/IIS/FDW実機未確認を成功扱いしない。

## 2026-10-27 学習順001〜005の全面改稿・第1組受入（履歴）
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
