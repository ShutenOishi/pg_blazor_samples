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

## 2026-10-27 学習順046〜050・第10組復習・第5段階概要（実装時点の記録：受入は上記を参照）
- 対象：db-bridge-01、旧Step31〜34、第10組復習、第5段階概要。Razor、Data/LearningTask、LearningDbContext、LearningDbService、Program.cs、PostgreSQL18開発用DDLを照合。各本文3,300字以上、実コード2例、WinForms＋Dapper比較・障害切り分け・画面操作・詳しい解答を整備。
- 画面の学習順046〜050と比較・診断表、第5段階概要と第10組復習、ホームとNavMenu、第10組の配置、Web/PDF目次、Chrome実操作とscripts/check-learning-materials.pyの品質条件を同期。
- **同一HEADのアプリCI、日本語PDF CI、Preview単体PDFの生成元SHA・Asset digestの最終受入前なので、確定進捗は45/110、9/22、4/9のまま**。全SUCCESSの受入後のみ50/110、10/22、5/9へ更新する。記録更新コミット後も最新HEADの全CI・Previewを再照合する。
- 後続：学習順051〜055（旧Step35、db-bridge-02、db-bridge-03、旧Step36〜37）、第11組復習。旧80 URL、Draft PR #36、main、正式Release、個人監査・DB台帳の都度認可、安全拒否を維持。実AD/IIS/Kerberos/FDWと1万人規模の実機試験は未受入。

# 教材v3 全110ステップの全面改稿・受入手順

## 2026-10-27 第10組046〜050の実装受入・第5段階概要改稿

- **実装受入HEAD `c5ce7d4cf07059c3d21a4ae12e1cb1fc954e3fcc`**。アプリ [Run 40463569089](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40463569089) はWindows Server 2025とUbuntu 24.04 Release、xUnit、Chrome実操作、PostgreSQL18実DB統合・既存安全拒否試験のジョブがすべてSUCCESS。日本語PDF [Run 40463568971](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40463568971) は教材品質・PDF生成・Preview更新を含めSUCCESS。両Runのhead_shaは同じ実装HEAD。
- [Preview Release](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) は上記実装HEADから生成。単体`blazor_learning_textbook_ja.pdf`、Asset ID **680739783**、**1,468,825バイト**、SHA256 **`0993dc3dc202b2868e31c521f88c435dcf98dd2b788a1d2225ec4a2324bf25b4`**。Release本文とAsset個別GETのname・size・digest一致。ZIPなし、正式Release更新なし。
- 学習順046〜050の各教材本文は**3,904／4,001／4,148／4,164／4,229文字**。第10組復習4,066文字、第5段階概要3,958文字。実Razor/C#/SQL、WinForms＋Dapper比較表、障害切り分け表、実操作、失敗例、演習と詳しい解答を実装と照合。Razor、Web・PDF目次、NavMenu、前後リンク、教材品質検査、Chromeを同期。
- **実装内容の受入：新規初稿30/30、最終全面改稿50/110、通常復習10/22、段階概要5/9。** 第5段階全15本文の完成とは区別する。旧80 URL、Draft PR #36、main、正式Releaseを維持。
- **次工程：学習順051〜055（旧Step35、db-bridge-02、db-bridge-03、旧Step36・37）と第11組復習**。各本文3,300文字以上を目安に実コード2例以上、比較・診断表、操作・詳答・安全拒否、Web/PDF/Chromeと同一HEADで受入する。
- **本記録とcurriculum.jsonの更新で新HEADになる。記録更新後のHEADも新しいアプリ/PDF CIの全SUCCESSとPreview PDF生成元SHA・Asset個別GETを照合し、最終新HEAD証跡はPRコメントで記録する。** 前HEADの成功を新HEADの成功と見なさない。実AD・IIS・Kerberos・FDW・1万人規模実機試験は未受入。


## 現在の達成状況

新規30ステップの**Razor画面・日本語教材初稿を配置済み**。総合演習5件のCIと、同じHEADによる日本語PDFのPreview Release公開を検証済みです。**全面改稿と受入は第1〜12組001〜060のCI受入済み60/110**であり、初稿実装と混同しません。既存80ステップのURLは維持し、Draft PR #36からmainへ未検証のままマージしません。

## 最終成果物と受入条件

1. 110本すべてを**学習順001から5本単位**で原文を読み直し、WinForms＋Dapper経験者が単体の日本語教材だけで理解・操作・失敗原因の切り分け・復習できる本文へ全面改稿します。旧Stepの名前を新学習順へ機械置換しただけでは受入としません。
2. 各ユニットの先頭は対応する第1〜9段階概要、末尾は対応する通常復習（全22組）を配置します。DI・用途別MudBlazor・1万人認証/既存台帳認可の特別3コラムも指定の位置に挿入します。
3. ページタイトル・本文・コード例・Razor画面・データ型・コメント・画面の前後リンク・Web目次・PDF目次を同じ学習順にします。PDFは日本語でソースコードと一致し、ZIP化せずDraft専用Release `learning-textbook-v3-preview` に同一HEADの全CI成功後だけ配布します。
4. Windows Server 2025/LinuxのビルドとxUnit、Chrome実操作、PostgreSQL18の実DB結合、SQL注入/未認証/部署外/失効/監査失敗/更新競合の否定試験を5本単位で維持・拡張します。実AD/IIS/Kerberos/FDW・1万人規模負荷はCIの模擬成功とは分離し、実機受入済みとは記載しません。
5. 運用設定や権限を弱めず、ADからは本人認証とユーザー名のみ取得。既存DB台帳または外部ビューを業務認可の正本とし、実業務の一覧・Count・CSV・集計・詳細・更新は毎操作サーバーで認可します。原子的な監査・ロックを保証できない更新は拒否します。
6. EF Core 10/NpgsqlのDbContextFactoryはWebプロジェクト内で操作単位に利用し、学習のためだけにDomain/Infrastructure/Repositoryのプロジェクトを増設しません。Dapperとの併用は選定理由とパラメーター化を明記します。

## 長文教材の標準構成（全110本に適用）

各本文は単に画面操作を数行で示すのではなく、**教材単独で十分に理解できる分量**を確保します。初学者がWinForms＋Dapperの既知の仕組みと対比できるように、以下を各5本単位の完成条件とします。

- 各本文：到達目標、前提と実行場所、WinFormsとの対応、実際のRazor/C#/HTML/SQLの抜粋を含む**複数のコードブロック**、比較表・診断表、順番付き実習、正常系と拒否/失敗例、演習問題と根拠を伴う解答、後続への接続を含む。コードは実装の所在・動作・寿命・副作用が確認できる形で示す。
- 少なくとも1つの比較表と、1つの失敗時の切り分け表を含める。対象テーマに適した列を選び、表で単なる言い換えを繰り返さない。
- 実際の画面コードと、概念説明のためだけの参考コードを明確に区別する。ステップに存在しない仕組みを「実装済み」として書かず、実AD/IIS/FDWの未検証事項も記す。
- 概要は学習順・到達点・必要な環境・操作の流れをまとめた独立した説明とし、復習は5本をつないだ実践課題、コードの読み取り、よくある誤解と解答を備える。
- PDFには**コードや学習内容をしっかり掲載**する一方、リポジトリ管理用URL・ソースの管理パスなど、学習者に不要な内部情報は入れない。ファイル名・言語・コードは、理解に必要な範囲で記載する。
- 初回001〜005には最低3300字程度/本文、比較・診断表、2コードブロックの自動検査を導入。後続の5本にも同等の品質基準とCI検査を拡張する。

## 学習順001〜110の22回の改稿と受入

各行が**1つの受入単位**です。下記の順に処理し、前の5本を合格にする前に次の5本の最終改稿完了を宣言しません。内容の変更に伴い後続の前提が変わった場合は、その後の本文と復習も調整します。

| 作業順 | 段階 | 学習順 | 本文5件と復習 | 完成判定 |
| --- | --- | --- | --- | --- |
| 01（受入済み） | 第1段階 Web開発の前提 | 001〜005 | WinFormsとWebの処理場所を比較する／URL・HTTPと要求／応答を理解する／HTML・CSS・フォームの基礎／開発者ツールでDOM・Network・Consoleを見る／Visual Studio 2026でWebアプリを起動・デバッグする、復習第1組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 02 | 第2段階 Blazorの基本と実行原理 | 006〜010 | 最初のBlazor画面を表示する／URLと画面遷移を理解する／RazorでC#の値を表示する／Static SSRとInteractive Serverの違い／ボタンのイベントを扱う、復習第2組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 03（受入済み） | 第2段階 Blazorの基本と実行原理 | 011〜015 | 入力値を双方向に結び付ける／条件分岐と繰り返しで表示する／親から子へ値を渡す／子から親へ通知する／SignalR・circuit・再接続、復習第3組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 04（受入済み） | 第2段階 Blazorの基本と実行原理 | 016〜020 | 初期化・更新・破棄を理解する／事前レンダリングと初期化の実行回数／入力フォームを日本語で検証する／URL・履歴・再読込と画面状態／circuit・ブラウザー・DBの状態保存の選択、復習第4組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 05（受入済み） | 第3段階 DI・サービスと単体テスト | 021〜025 | DI（依存性注入）の基本／インターフェイスで部品を交換する／Singleton・Scoped・Transient／BlazorのScopedと対話回線／injectとコンストラクター注入、復習第5組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 06（受入済み） | 第3段階 DI・サービスと単体テスト | 026〜030 | Optionsで設定を受け取る／非同期処理とログ／DIによる単体テスト／Applicationへ処理を分離／DIで課題管理を作る、復習第6組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 07（受入済み） | 第4段階 MudBlazorとブラウザーの業務UI | 031〜035 | MudBlazorと日本語翻訳を導入する／MudBlazorのレイアウトを学ぶ／日本語で入力部品を使う／MudFormの入力検証を学ぶ／MudTableで一覧とソート、復習第7組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 08（受入済み） | 第4段階 MudBlazorとブラウザーの業務UI | 036〜040 | MudDataGridで検索・並べ替えする／確認ダイアログで実行と中止を選ぶ／Snackbarと処理中の状態を表示する／ダークモードとキーボード操作を学ぶ／MudBlazor日本語UIを総合演習する、復習第8組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 09（受入済み） | 第4段階 MudBlazorとブラウザーの業務UI | 041〜045 | JavaScript interopの利用判断／ファイルアップロードと入力制約／CSV・PDFファイルのダウンロード／ErrorBoundary・取消・再試行／HTMLの意味・フォーカス・拡大表示の最終確認、復習第9組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 10 | 第5段階 DapperからEF Core 10・PostgreSQL 18へ | 046〜050 | DapperとEF Coreの役割・選定を比較する／モデルとDTOを理解する／PostgreSQLで学習テーブルを定義する／EF Core 10をWebへ登録する／DbContextFactoryと接続プール、復習第10組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 11 | 第5段階 DapperからEF Core 10・PostgreSQL 18へ | 051〜055 | LINQ検索とAsNoTracking／SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL／Dapperの接続／ExecuteとDbContext・追跡・SaveChanges／EF Coreの登録・更新・削除／DateOnly・TimeOnly・Rangeの型変換、復習第11組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 12 | 第5段階 DapperからEF Core 10・PostgreSQL 18へ | 056〜060 | トランザクションを理解する／xmin楽観的排他を確認する／既存DB・ビュー・キーなしモデル・FDWを扱う／FromSql／SqlQueryのパラメーターとDapper併用／PostgreSQL 18の実接続確認、復習第12組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 13 | 第6段階 一覧データ連携・検索・編集 | 061〜065 | Itemsを使う一覧の条件／MudTableのServerDataでDB検索／MudDataGridのServerDataとEF Core／複数列ソートを安全に組み立てる／日付と完了状態で絞り込む、復習第13組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 14 | 第6段階 一覧データ連携・検索・編集 | 066〜070 | 関連テーブルをDTOへ射影する／キーセットページングを理解する／取得済み一覧を部分描画する／DB取得と仮想スクロールを連携する／デバウンスと障害時の再試行、復習第14組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 15 | 第6段階 一覧データ連携・検索・編集 | 071〜075 | 編集DTOをフォームで検証して保存する／ダイアログで保存と取消を分ける／複数選択と原子的な一括処理／xmin競合と最新値の確認／詳細DTOとN+1の防止、復習第15組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 16 | 第6段階 一覧データ連携・検索・編集 | 076〜080 | 日本語日付範囲とDB検索／キャッシュと権限境界／DB側集計とCSV出力／EXPLAINと応答時間計測／一覧パターンを要件から選ぶ、復習第16組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 17 | 第7段階 Webセキュリティ・AD認証とDB認可 | 081〜085 | Windows認証・Cookie・HTTP・circuit／Middlewareの順序・HTTPS・IISの経路／XSS・CSRF・SQL注入の防御境界／環境別設定・Secret・Data Protection鍵／接続切断・セッション失効・権限再検証、復習第17組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 18 | 第7段階 Webセキュリティ・AD認証とDB認可 | 086〜090 | Windows統合認証を構成する／認証済みドメインユーザー名を扱う／認証済みユーザー名と既存台帳を結ぶ／未登録・無効利用者を拒否する／既存台帳の業務権限を確認する、復習第18組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 19 | 第7段階 Webセキュリティ・AD認証とDB認可 | 091〜095 | サービス側で認可を強制する／部署ごとのCountとItems／集計とCSVにも認可を適用／保存直前の権限とxminを再確認／Windows Server 2025の実機受入を分ける、復習第19組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 20 | 第8段階 監査・競合制御・Windows Server運用 | 096〜100 | 既存台帳のキー正規化・重複拒否と索引／明示Windowsログインと短命Cookie／権限行ロックと更新の原子性／本人・操作・結果を永続監査する／IIS・Kerberos・負荷・障害の実機受入、復習第20組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 21 | 第8段階 監査・競合制御・Windows Server運用 | 101〜105 | 二人の編集競合とxmin／キャッシュの有効期限と権限失効／ヘルスチェック・構造化ログ・運用監視／実DB・ブラウザー・認可の結合/E2E試験／Windows Server 2025/IISの配布・切替・復旧、復習第21組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |
| 22 | 第9段階 業務アプリの通し演習 | 106〜110 | 業務要件・画面遷移・WinForms移行の設計／PostgreSQL検索とページング・生成SQL／DTO編集・競合・取消・例外の統合／認証済み本人と既存台帳による権限・監査／E2E・障害復旧・IIS受入表、復習第22組 | 本文・画面・概要・復習・PDF・CI・否定試験の一致 |

## 2026-10-27 第4段階完成の受入証跡

- **教材実装の受入HEAD** `3cab5c0fc430ea473675c6eb4acf04e0ead7103b`。アプリ [Run 40447468064](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40447468064) はWindows Server 2025/Linux Release・xUnit・Chrome実操作・PostgreSQL18実DB結合と既存安全拒否の全ジョブSUCCESS。日本語PDF [Run 40447468058](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40447468058) も教材品質ゲート・PDF生成・Preview公開SUCCESS。
- **Preview Release** [learning-textbook-v3-preview](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview) は生成元HEAD一致。単体 `blazor_learning_textbook_ja.pdf`、Asset ID `680389176`、**1,416,502バイト**、SHA256 `8514f1eaa4c3bbe0cf13416615329de73ee8d23cfff2bed7afce247898cfb50f`。Release GETとAsset個別GETのname・size・digest一致、ZIPなし。
- 学習順041〜045（browser-01〜05）の全面改稿は各**5,066／5,283／5,668／5,638／5,744文字**、第9組復習**5,843文字**。実Razor/C#/JS、WinForms＋Dapper比較、診断表、実操作、拒否、演習・詳しい解答を収録。第4段階概要、5画面Razorの比較表、復習画面、ホーム/NavMenu、前後リンク、Web/PDF索引、品質ゲート、Chromeを整合。
- 初回実装HEAD `a9815703638d22484f8b1c6f7c417338549d916a` のChrome検査は学習順のゼロ埋め欠落（41対041）でFAILURE。新HEAD `3cab5c0...` でテスト識別子を修正し、失敗した旧HEADを成功扱いせず全CIで受入。成功済みCIの手動再実行は実施していない。
- **確定進捗：新規初稿30/30、最終全面改稿45/110、通常復習9/22、段階概要4/9。第4段階031〜045と第7〜9組復習が完了。** Draft PR・旧80URL・正式main教材と正式Releaseは維持。AD認証は本人とドメイン名だけ、認可は既存DB/外部台帳を正本として都度判定し、監査・権限失効・更新競合・失敗時の安全拒否を維持。実AD/IIS/Kerberos/FDW・1万人超実機負荷は未受入。
- **次工程：学習順046〜050（db-bridge-01、旧Step31〜34）と第10組復習、第5段階概要**。EF Core10・Npgsql・DbContextFactory・PG18の実Razor/SQL/C#と照合して各3300文字以上、実コード2以上、WinForms＋Dapper比較、切り分け、操作、演習と詳答を全面改稿。第5段階先頭、前後リンク、Web/PDF、日本語、Chrome・品質ゲート・安全拒否を同期。同一HEADのWindows/Linux・xUnit・Chrome・PG18・PDF成功とPreview PDF生成元SHA一致後にのみ50/110・10/22・5/9へ進む。
- **記録更新によりHEADが進んだ場合**、必ずその新HEADのApp/PDF CI全SUCCESSおよびPreview Release生成元SHA・単体PDF AssetのGET一致を再照合すること。旧HEADの成功を新HEADの証跡として流用しない。

## 特別3コラムの扱い

- 学習順030の後：`library-di`（`docs/columns/column-library-di.md`）。直前のユニットと後続の初学者に必要な前提を合わせて全面改稿・内容照合。
- 学習順060の後：`mudblazor-by-purpose`（`docs/columns/column-mudblazor-by-purpose.md`）。直前のユニットと後続の初学者に必要な前提を合わせて全面改稿・内容照合。
- 学習順090の後：`auth-at-scale`（`docs/columns/column-auth-at-scale.md`）。直前のユニットと後続の初学者に必要な前提を合わせて全面改稿・内容照合。

## 9段階の概要ページ（先頭に置く）

- 第1段階 001〜005：`docs/course-v3/stages/stage01.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第2段階 006〜020：`docs/course-v3/stages/stage02.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第3段階 021〜030：`docs/course-v3/stages/stage03.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第4段階 031〜045：`docs/course-v3/stages/stage04.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第5段階 046〜060：`docs/course-v3/stages/stage05.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第6段階 061〜080：`docs/course-v3/stages/stage06.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第7段階 081〜095：`docs/course-v3/stages/stage07.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第8段階 096〜105：`docs/course-v3/stages/stage08.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。
- 第9段階 106〜110：`docs/course-v3/stages/stage09.md`。到達目標・事前知識・操作範囲・学習順・失敗時の確認と復習の位置を整合させる。

## 5本単位の実作業サイクル
1. `curriculum.json`に従って先頭から対象5本・レビュー・段階概要・前後の画面の現状を読み、無理な学習前提がないか確認する。
2. **日本語本文を全面改稿**。到達目標・事前条件・画面操作・コード例・実行場所・演習問題と解答・失敗と復旧・次のステップへの接続を説明する。
3. 必要なRazor/C#/DB/DI・テストを修正する。業務機密を出さず、認可や監査の安全条件を保持。新規画面は演習のための模擬表示と実DBの結果を明確に区分。
4. 概要・復習・特別コラムの該当箇所・前後リンク・ホーム・ナビ・PDF章順を更新。古い`/learn/01`〜`/learn/80`は残す。
5. 教材整合スクリプト・Windows/Linuxビルド/xUnit・Chrome・PostgreSQL18（該当時）・PDFを同一HEADで確認し、失敗は修正して再検証。PDF Releaseの生成元SHAとAssetの存在をGETで確認。
6. 成功した5件のみ`progress.md`の全面改稿受入数を5増やし、PRコメントと`next-chat-handoff.md`に証跡と次の5件を残す。**未成功CI・既存のv2改稿実績をカウントしない**。

## 着手順
- 最初の受入：学習順001〜005（Web基礎5本、第1段階概要、第1組復習）。
- 次：006〜010、011〜015、016〜020。必要に応じて次段階冒頭の不足前提を前方へ移す。
- 最後：106〜110、第22組復習と第9段階概要を全面改稿したあと、**110/110・22/22復習・3/3特別・9/9概要**で最終受入を行う。
- 正式Releaseへの変更とDraft解除・mainマージは、最終合格とユーザーの判断まで実施しません。


### 第1組受入証跡

学習順001〜005・第1段階概要・第1組復習はHEAD `f48474d3882359626d1efedda73ac9737461b121`で全面改稿済み。Windows/Linux/Chrome/PG18 App Run 40370279965、PDF/Preview Release Run 40370279958 がSUCCESS。第2組006〜010はHEAD `d87ee9a96b7fb790645e5ef7d624008009f92980`で受入済み（App Run 40409325752、PDF Run 40409325725 とPreview asset生成元SHA一致）。次の作業は学習順011〜015と第3組復習です。

### 第2組受入証跡と次の作業

学習順006〜010、概要・復習、Razor導線、品質CI、Chrome E2Eを同時改稿。実装HEAD `d87ee9a96b7fb790645e5ef7d624008009f92980` のWin2025/Linux/Chrome/xUnit/PG18 App Run 40409325752、PDF Run 40409325725は全SUCCESS。単体PDF 1,079,388バイト、Preview Releaseにおける生成元SHA一致を認証済みGETで確認。**最終全面改稿10/110・通常復習2/22・段階概要2/9**。第2段階の残り10本は未受入。

次は **011〜015（旧Step05:双方向入力、旧Step06:条件と繰り返し、旧Step07:親から子、旧Step08:子から親、runtime-02:SignalR・circuit・再接続）と第3組復習**を改稿する。第2段階概要は全体説明として維持し、概要の学習順・前後導線・Web/PDF目次・Chrome操作を確認する。全文を読んでコードに一致させ、3,300文字以上、2コード・比較表・診断表・手順・失敗と復旧・演習と詳しい解答を守る。全CIとPreview PDFのSHA一致後のみ次の受入15/110を計上する。

### 第3組受入証跡と次の作業

- [x] 旧Step05〜08およびruntime-02を実際のRazor/C#と照合して全文改稿。本文は4,231〜4,780文字で、WinForms＋Dapper比較、実コード2ブロック以上、比較・診断表、操作手順、故障の切り分け、演習と詳しい解答を追加。第3組復習は4,990文字。
- [x] 第2段階概要とRazorの学習順表示・前後リンク、Web目次の復習位置、PDF章順、第3組復習のRazor表・確認問題を同期。品質ゲートは3300文字以上・比較表・診断表・実コード・用語・前後リンクを検証。Chrome実操作で文字入力・4→2→4件絞り込み・親子カード・EventCallback・対話回数・第3組復習への遷移を確認。
- [x] **実装HEAD `dcae9e218569f09b0749f94d53228f1d837396c3`** の[アプリCI Run 40412865629](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40412865629)がWindows Server 2025/Linux・xUnit・Chrome実操作・PostgreSQL 18実DB結合の全ジョブSUCCESS。[PDF CI Run 40412865704](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40412865704)もSUCCESS。
- [x] [学習テキストv3プレビュー](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook-v3-preview)の日本語PDF単体は**1,141,020バイト**、Asset SHA256 `b646ebc7af8e8cf01c57ca043965fe5b238c5f2d8af006e8d754db34668ef99e`。Release notesの生成元SHAと上記実装HEADが認証済みGETで一致。ZIP配布なし、main正式版は変更なし。
- [x] **確定進捗：新規初稿30/30、最終全面改稿15/110、通常復習3/22、段階概要2/9。** 実AD/IIS/Kerberos/FDWの実機受入とは区別。
- [ ] **次は学習順016〜020（旧Step09・runtime-03・旧Step10・runtime-04・runtime-05）と第4組復習を全面改稿。** 第2段階概要・Razor・学習順・前後リンク・Web目次・PDF・品質ゲート・Chrome操作を合わせる。全CIとPreviewの同一HEADが確認できた場合だけ20/110へ進める。


### 第4組受入証跡・次の第5組
- 学習順016〜020（Step09、runtime-03、Step10、runtime-04、runtime-05）の実画面・C#に照合した全文は4,534〜5,227文字。第4組復習は5,642文字。第2段階の概要、Razor前後リンク、Web/PDF順序、品質検査、Chrome実操作を同期した。
- **実装HEAD `897153ba5c4e4c65bf91c6fa6c2c151d0e86a582`** はApp Run [40418321144](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40418321144)・PDF Run [40418321149](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40418321149)が全SUCCESS。Preview ReleaseのPDF単体は1204237バイト、SHA256 `00b2247f3f2eee2fd92944719403dd02ba9370c88f2cc3dba45cde16fefb42d4`、生成元SHA一致を認証済みGETで確認。
- **確定：20/110本文、4/22通常復習、2/9段階概要**。第2段階は完了。第3段階はこれからでありDIの既存実装やv2改稿を未照合のまま計上しない。
- **次工程05：学習順021〜025（旧Step11〜15：DI、インターフェイス、ライフタイム、circuitのScoped、注入の使い分け）と第5組復習、第3段階概要**。既存Razor/C#のDIサービス登録・スコープ/破棄・学習画面を照合し、3300文字以上・実コード・比較/診断表・演習/詳答とWeb/PDF/Chrome品質ゲートを同期する。全CIと単体PDFの同一HEAD検証後にのみ25/110、5/22、3/9へ進める。

### 第5組学習順021〜025の受入記録（2026-10-27）

旧Step11〜15に対応する各本文、第5組復習、第3段階概要を実コードと照合して改稿済み。第3段階のRazor概要、DI比較表、前後リンク、Web/PDF目次、品質ゲート、Chrome導線も同期した。実装HEAD `5472f4edf054b62c998f8d431a3222f874f6d739` のアプリ Run 40422500932、PDF Run 40422500829 はすべてSUCCESS。Preview Releaseの生成元SHAと日本語PDF単体Asset（1,243,463バイト、SHA256 `8875f5e4371134099a49fe4f17189d8c62a89ccc5f9da2dd704a7fedfc6a0cbd`）も認証済みGETで一致したため、25/110・第5組5/22・第3段階概要3/9を受入計上する。記録更新後の最新HEADについてもCIとPreview Release生成元を再照合する。

次の第6組026〜030は旧Step16〜20である。順にOptions、非同期処理とログ、xUnit、Application分離、DI課題管理を全面改稿し、復習第6組と特別コラム（ライブラリDI）の後続導線を一致させる。既存の旧80ステップURLとDraft状態を維持する。

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

### 第7組・学習順031〜035の受入（2026-10-27）
学習順031〜035、通常復習第7組、第4段階概要は実コードに照合して本文長文・実コード・WinForms＋Dapper比較・故障切り分け・演習と詳細解答を改稿した。Razor/ホーム/ナビ/前後導線・PDF目次・Chrome・品質ゲートを同期。実装HEAD `9d700f3ea0d043f4dd23bf0ab22fa3e6616f461e` に対するアプリRun 40442654275、日本語PDF Run 40442654171がSUCCESS。プレビューPDF生成元SHA一致、単体1,311,087バイト。**確定：35/110、通常復習7/22、段階概要4/9。** 次は036〜040（旧Step26〜30）と第8組復習。段階全15本文の完成とは区別する。記録更新後の最新HEADも再受入する。

### 第8組学習順036〜040の最終受入

実装HEAD `42c394f9f8bd921ac149bc8317d886f4f7b754f7` のアプリRun 40444727667とPDF Run 40444727659は同一SHAでSUCCESS。Win/Linux、xUnit、Chrome、PG18、安全拒否、日本語PDFとPreview単体PDFの生成元SHAを照合済み。**全面改稿40/110、第8組復習8/22、第4段階概要4/9**。次はbrowser-01〜05＝学習順041〜045と第9組復習を同じ品質基準で全文改稿する。進捗更新コミットも新HEADとして再検証する。
