# 教材v3 学習順の確定案（全110ステップ）

**学習順001〜110の配置はすべて完了しました。** 既存80件に新規30件のRazor画面・日本語教材初稿を加え、段階概要・復習とWeb/PDF目次へ順次反映しています。総合演習106〜110のWindows/Linux・Chrome・PG18・日本語PDFのCIとプレビューReleaseの公開も成功しています。**全110本の最終全面改稿は学習順001〜010の受入済み10/110**で、別に進捗管理します。

第1・2組の復習は完了済み（2/22）、第1・2段階の概要はCI受入済み（2/9）。第2段階の残り011〜020は未改稿です。次に011〜015と第3組復習を改稿します。

元の`/learn/01`〜`/learn/80`は維持します。表の「学習順」は新しい読者向けの番号、「旧Step」は既存URLに対応する識別子です。新規30テーマのURLは実装済みです。全本文の最終改稿と全9段階概要の最終整合はこれから行います。

## 段階別の構成

| 段階 | 学習順 | 概要 |
| --- | --- | --- |
| 第1段階 | 001〜005 | [Web開発の前提](stages/stage01.md) |
| 第2段階 | 006〜020 | [Blazorの基本と実行原理](stages/stage02.md) |
| 第3段階 | 021〜030 | [DI・サービスと単体テスト](stages/stage03.md) |
| 第4段階 | 031〜045 | [MudBlazorとブラウザーの業務UI](stages/stage04.md) |
| 第5段階 | 046〜060 | [DapperからEF Core 10・PostgreSQL 18へ](stages/stage05.md) |
| 第6段階 | 061〜080 | [一覧データ連携・検索・編集](stages/stage06.md) |
| 第7段階 | 081〜095 | [Webセキュリティ・AD認証とDB認可](stages/stage07.md) |
| 第8段階 | 096〜105 | [監査・競合制御・Windows Server運用](stages/stage08.md) |
| 第9段階 | 106〜110 | [業務アプリの通し演習](stages/stage09.md) |

## 完成後の全ステップ順

| 学習順 | 段階 | 5本単位 | 題材 | 対応先 | 完成後のURL |
| --- | --- | --- | --- | --- | --- |
| 001 | 1 | 1 | WinFormsとWebの処理場所を比較する | 新規 web-01 | /learn/web-01 |
| 002 | 1 | 1 | URL・HTTPと要求／応答を理解する | 新規 web-02 | /learn/web-02 |
| 003 | 1 | 1 | HTML・CSS・フォームの基礎 | 新規 web-03 | /learn/web-03 |
| 004 | 1 | 1 | 開発者ツールでDOM・Network・Consoleを見る | 新規 web-04 | /learn/web-04 |
| 005 | 1 | 1 | Visual Studio 2026でWebアプリを起動・デバッグする | 新規 web-05 | /learn/web-05 |
| 006 | 2 | 2 | 最初のBlazor画面を表示する | 旧Step01 | /learn/01 |
| 007 | 2 | 2 | URLと画面遷移を理解する | 旧Step02 | /learn/02 |
| 008 | 2 | 2 | RazorでC#の値を表示する | 旧Step03 | /learn/03 |
| 009 | 2 | 2 | Static SSRとInteractive Serverの違い | 新規 runtime-01 | /learn/runtime-01 |
| 010 | 2 | 2 | ボタンのイベントを扱う | 旧Step04 | /learn/04 |
| 011 | 2 | 3 | 入力値を双方向に結び付ける | 旧Step05 | /learn/05 |
| 012 | 2 | 3 | 条件分岐と繰り返しで表示する | 旧Step06 | /learn/06 |
| 013 | 2 | 3 | 親から子へ値を渡す | 旧Step07 | /learn/07 |
| 014 | 2 | 3 | 子から親へ通知する | 旧Step08 | /learn/08 |
| 015 | 2 | 3 | SignalR・circuit・再接続 | 新規 runtime-02 | /learn/runtime-02 |
| 016 | 2 | 4 | 初期化・更新・破棄を理解する | 旧Step09 | /learn/09 |
| 017 | 2 | 4 | 事前レンダリングと初期化の実行回数 | 新規 runtime-03 | /learn/runtime-03 |
| 018 | 2 | 4 | 入力フォームを日本語で検証する | 旧Step10 | /learn/10 |
| 019 | 2 | 4 | URL・履歴・再読込と画面状態 | 新規 runtime-04 | /learn/runtime-04 |
| 020 | 2 | 4 | circuit・ブラウザー・DBの状態保存の選択 | 新規 runtime-05 | /learn/runtime-05 |
| 021 | 3 | 5 | DI（依存性注入）の基本 | 旧Step11 | /learn/11 |
| 022 | 3 | 5 | インターフェイスで部品を交換する | 旧Step12 | /learn/12 |
| 023 | 3 | 5 | Singleton・Scoped・Transient | 旧Step13 | /learn/13 |
| 024 | 3 | 5 | BlazorのScopedと対話回線 | 旧Step14 | /learn/14 |
| 025 | 3 | 5 | injectとコンストラクター注入 | 旧Step15 | /learn/15 |
| 026 | 3 | 6 | Optionsで設定を受け取る | 旧Step16 | /learn/16 |
| 027 | 3 | 6 | 非同期処理とログ | 旧Step17 | /learn/17 |
| 028 | 3 | 6 | DIによる単体テスト | 旧Step18 | /learn/18 |
| 029 | 3 | 6 | Applicationへ処理を分離 | 旧Step19 | /learn/19 |
| 030 | 3 | 6 | DIで課題管理を作る | 旧Step20 | /learn/20 |
| 031 | 4 | 7 | MudBlazorと日本語翻訳を導入する | 旧Step21 | /learn/21 |
| 032 | 4 | 7 | MudBlazorのレイアウトを学ぶ | 旧Step22 | /learn/22 |
| 033 | 4 | 7 | 日本語で入力部品を使う | 旧Step23 | /learn/23 |
| 034 | 4 | 7 | MudFormの入力検証を学ぶ | 旧Step24 | /learn/24 |
| 035 | 4 | 7 | MudTableで一覧とソート | 旧Step25 | /learn/25 |
| 036 | 4 | 8 | MudDataGridで検索・並べ替えする | 旧Step26 | /learn/26 |
| 037 | 4 | 8 | 確認ダイアログで実行と中止を選ぶ | 旧Step27 | /learn/27 |
| 038 | 4 | 8 | Snackbarと処理中の状態を表示する | 旧Step28 | /learn/28 |
| 039 | 4 | 8 | ダークモードとキーボード操作を学ぶ | 旧Step29 | /learn/29 |
| 040 | 4 | 8 | MudBlazor日本語UIを総合演習する | 旧Step30 | /learn/30 |
| 041 | 4 | 9 | JavaScript interopの利用判断 | 新規 browser-01 | /learn/browser-01 |
| 042 | 4 | 9 | ファイルアップロードと入力制約 | 新規 browser-02 | /learn/browser-02 |
| 043 | 4 | 9 | CSV・PDFファイルのダウンロード | 新規 browser-03 | /learn/browser-03 |
| 044 | 4 | 9 | ErrorBoundary・取消・再試行 | 新規 browser-04 | /learn/browser-04 |
| 045 | 4 | 9 | HTMLの意味・フォーカス・拡大表示の最終確認 | 新規 browser-05 | /learn/browser-05 |
| 046 | 5 | 10 | DapperとEF Coreの役割・選定を比較する | 新規 db-bridge-01 | /learn/db-bridge-01 |
| 047 | 5 | 10 | モデルとDTOを理解する | 旧Step31 | /learn/31 |
| 048 | 5 | 10 | PostgreSQLで学習テーブルを定義する | 旧Step32 | /learn/32 |
| 049 | 5 | 10 | EF Core 10をWebへ登録する | 旧Step33 | /learn/33 |
| 050 | 5 | 10 | DbContextFactoryと接続プール | 旧Step34 | /learn/34 |
| 051 | 5 | 11 | LINQ検索とAsNoTracking | 旧Step35 | /learn/35 |
| 052 | 5 | 11 | SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL | 新規 db-bridge-02 | /learn/db-bridge-02 |
| 053 | 5 | 11 | Dapperの接続／ExecuteとDbContext・追跡・SaveChanges | 新規 db-bridge-03 | /learn/db-bridge-03 |
| 054 | 5 | 11 | EF Coreの登録・更新・削除 | 旧Step36 | /learn/36 |
| 055 | 5 | 11 | DateOnly・TimeOnly・Rangeの型変換 | 旧Step37 | /learn/37 |
| 056 | 5 | 12 | トランザクションを理解する | 旧Step38 | /learn/38 |
| 057 | 5 | 12 | xmin楽観的排他を確認する | 旧Step39 | /learn/39 |
| 058 | 5 | 12 | 既存DB・ビュー・キーなしモデル・FDWを扱う | 新規 db-bridge-04 | /learn/db-bridge-04 |
| 059 | 5 | 12 | FromSql／SqlQueryのパラメーターとDapper併用 | 新規 db-bridge-05 | /learn/db-bridge-05 |
| 060 | 5 | 12 | PostgreSQL 18の実接続確認 | 旧Step40 | /learn/40 |
| 061 | 6 | 13 | Itemsを使う一覧の条件 | 旧Step41 | /learn/41 |
| 062 | 6 | 13 | MudTableのServerDataでDB検索 | 旧Step42 | /learn/42 |
| 063 | 6 | 13 | MudDataGridのServerDataとEF Core | 旧Step43 | /learn/43 |
| 064 | 6 | 13 | 複数列ソートを安全に組み立てる | 旧Step44 | /learn/44 |
| 065 | 6 | 13 | 日付と完了状態で絞り込む | 旧Step45 | /learn/45 |
| 066 | 6 | 14 | 関連テーブルをDTOへ射影する | 旧Step46 | /learn/46 |
| 067 | 6 | 14 | キーセットページングを理解する | 旧Step47 | /learn/47 |
| 068 | 6 | 14 | 取得済み一覧を部分描画する | 旧Step48 | /learn/48 |
| 069 | 6 | 14 | DB取得と仮想スクロールを連携する | 旧Step49 | /learn/49 |
| 070 | 6 | 14 | デバウンスと障害時の再試行 | 旧Step50 | /learn/50 |
| 071 | 6 | 15 | 編集DTOをフォームで検証して保存する | 旧Step51 | /learn/51 |
| 072 | 6 | 15 | ダイアログで保存と取消を分ける | 旧Step52 | /learn/52 |
| 073 | 6 | 15 | 複数選択と原子的な一括処理 | 旧Step53 | /learn/53 |
| 074 | 6 | 15 | xmin競合と最新値の確認 | 旧Step54 | /learn/54 |
| 075 | 6 | 15 | 詳細DTOとN+1の防止 | 旧Step55 | /learn/55 |
| 076 | 6 | 16 | 日本語日付範囲とDB検索 | 旧Step56 | /learn/56 |
| 077 | 6 | 16 | キャッシュと権限境界 | 旧Step57 | /learn/57 |
| 078 | 6 | 16 | DB側集計とCSV出力 | 旧Step58 | /learn/58 |
| 079 | 6 | 16 | EXPLAINと応答時間計測 | 旧Step59 | /learn/59 |
| 080 | 6 | 16 | 一覧パターンを要件から選ぶ | 旧Step60 | /learn/60 |
| 081 | 7 | 17 | Windows認証・Cookie・HTTP・circuit | 新規 security-01 | /learn/security-01 |
| 082 | 7 | 17 | Middlewareの順序・HTTPS・IISの経路 | 新規 security-02 | /learn/security-02 |
| 083 | 7 | 17 | XSS・CSRF・SQL注入の防御境界 | 新規 security-03 | /learn/security-03 |
| 084 | 7 | 17 | 環境別設定・Secret・Data Protection鍵 | 新規 security-04 | /learn/security-04 |
| 085 | 7 | 17 | 接続切断・セッション失効・権限再検証 | 新規 security-05 | /learn/security-05 |
| 086 | 7 | 18 | Windows統合認証を構成する | 旧Step61 | /learn/61 |
| 087 | 7 | 18 | 認証済みドメインユーザー名を扱う | 旧Step62 | /learn/62 |
| 088 | 7 | 18 | 認証済みユーザー名と既存台帳を結ぶ | 旧Step63 | /learn/63 |
| 089 | 7 | 18 | 未登録・無効利用者を拒否する | 旧Step64 | /learn/64 |
| 090 | 7 | 18 | 既存台帳の業務権限を確認する | 旧Step65 | /learn/65 |
| 091 | 7 | 19 | サービス側で認可を強制する | 旧Step66 | /learn/66 |
| 092 | 7 | 19 | 部署ごとのCountとItems | 旧Step67 | /learn/67 |
| 093 | 7 | 19 | 集計とCSVにも認可を適用 | 旧Step68 | /learn/68 |
| 094 | 7 | 19 | 保存直前の権限とxminを再確認 | 旧Step69 | /learn/69 |
| 095 | 7 | 19 | Windows Server 2025の実機受入を分ける | 旧Step70 | /learn/70 |
| 096 | 8 | 20 | 既存台帳のキー正規化・重複拒否と索引 | 旧Step71 | /learn/71 |
| 097 | 8 | 20 | 明示Windowsログインと短命Cookie | 旧Step72 | /learn/72 |
| 098 | 8 | 20 | 権限行ロックと更新の原子性 | 旧Step73 | /learn/73 |
| 099 | 8 | 20 | 本人・操作・結果を永続監査する | 旧Step74 | /learn/74 |
| 100 | 8 | 20 | IIS・Kerberos・負荷・障害の実機受入 | 旧Step75 | /learn/75 |
| 101 | 8 | 21 | 二人の編集競合とxmin | 旧Step76 | /learn/76 |
| 102 | 8 | 21 | キャッシュの有効期限と権限失効 | 旧Step77 | /learn/77 |
| 103 | 8 | 21 | ヘルスチェック・構造化ログ・運用監視 | 旧Step78 | /learn/78 |
| 104 | 8 | 21 | 実DB・ブラウザー・認可の結合/E2E試験 | 旧Step79 | /learn/79 |
| 105 | 8 | 21 | Windows Server 2025/IISの配布・切替・復旧 | 旧Step80 | /learn/80 |
| 106 | 9 | 22 | 業務要件・画面遷移・WinForms移行の設計 | 新規 capstone-01 | /learn/capstone-01 |
| 107 | 9 | 22 | PostgreSQL検索とページング・生成SQL | 新規 capstone-02 | /learn/capstone-02 |
| 108 | 9 | 22 | DTO編集・競合・取消・例外の統合 | 新規 capstone-03 | /learn/capstone-03 |
| 109 | 9 | 22 | 認証済み本人と既存台帳による権限・監査 | 新規 capstone-04 | /learn/capstone-04 |
| 110 | 9 | 22 | E2E・障害復旧・IIS受入表 | 新規 capstone-05 | /learn/capstone-05 |

## 5本ごとの復習と特別コラム

- 学習順001〜005：第1組の復習（新しい復習コラム：`docs/course-v3/reviews/review-01.md`）
- 学習順006〜010：第2組の復習（新しい復習コラム：`docs/course-v3/reviews/review-02.md`）
- 学習順011〜015：第3組の復習（新しい復習コラム：`docs/course-v3/reviews/review-03.md`）
- 学習順016〜020：第4組の復習（新しい復習コラム：`docs/course-v3/reviews/review-04.md`）
- 学習順021〜025：第5組の復習（既存コラムを再改稿：`docs/columns/column11-15.md`）
- 学習順026〜030：第6組の復習（既存コラムを再改稿：`docs/columns/column16-20.md`）
- 学習順031〜035：第7組の復習（既存コラムを再改稿：`docs/columns/column21-25.md`）
- 学習順036〜040：第8組の復習（既存コラムを再改稿：`docs/columns/column26-30.md`）
- 学習順041〜045：第9組の復習（新しい復習コラム：`docs/course-v3/reviews/review-09.md`）
- 学習順046〜050：第10組の復習（新しい復習コラム：`docs/course-v3/reviews/review-10.md`）
- 学習順051〜055：第11組の復習（新しい復習コラム：`docs/course-v3/reviews/review-11.md`）
- 学習順056〜060：第12組の復習（新しい復習コラム：`docs/course-v3/reviews/review-12.md`）
- 学習順061〜065：第13組の復習（既存コラムを再改稿：`docs/columns/column41-45.md`）
- 学習順066〜070：第14組の復習（既存コラムを再改稿：`docs/columns/column46-50.md`）
- 学習順071〜075：第15組の復習（既存コラムを再改稿：`docs/columns/column51-55.md`）
- 学習順076〜080：第16組の復習（既存コラムを再改稿：`docs/columns/column56-60.md`）
- 学習順081〜085：第17組の復習（新しい復習コラム：`docs/course-v3/reviews/review-17.md`）
- 学習順086〜090：第18組の復習（既存コラムを再改稿：`docs/columns/column61-65.md`）
- 学習順091〜095：第19組の復習（既存コラムを再改稿：`docs/columns/column66-70.md`）
- 学習順096〜100：第20組の復習（既存コラムを再改稿：`docs/columns/column71-75.md`）
- 学習順101〜105：第21組の復習（既存コラムを再改稿：`docs/columns/column76-80.md`）
- 学習順106〜110：第22組の復習（新しい復習コラム：`docs/course-v3/reviews/review-22.md`）

特別コラムは学習順030の後に「ライブラリのDI」、060の後に「用途別MudBlazorコンポーネント」、090の後に「1万人規模の認証と既存DB認可」を配置します。

## 番号とURLの扱い

- 既存URL・Razor `@page`・Markdownファイルは、すべて当面維持します。連番を機械的にリネームしません。
- 学習順番号を表示する正式な新しい目次は`curriculum.json`から生成する方針です。既存HTML/Markdownの手入力順序を正本にしません。
- 新しいURLと教材は**Razor実装・日本語本文・E2E・PDF収録がそろってから公開**します。
- 既存ステップへの参照は学習順番号で置き換えるが、アプリ内部識別子は維持します。旧URLのリダイレクトを急ぎません。
- 概要は必ず各段階の第1項目の前、復習は各5ステップの直後に置きます。特別コラムは対応する復習の後です。
