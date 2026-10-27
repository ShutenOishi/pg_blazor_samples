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
