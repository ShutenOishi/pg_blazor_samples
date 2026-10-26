# 次のチャット引き継ぎ：Step71〜75（既存台帳・明示Windowsログイン・監査）

## 現在の確定状態

- mainにStep01〜70の実装、5ステップごとの日本語コラム01〜15を保護PRで取り込んでいる。
- PR #24（Step61〜65）は保護マージ・Windows/Linux/Chrome/PostgreSQL 18/PDFの受入が完了。
- PR #25（Step66〜70）も保護マージ済み。WindowsUserAccessServiceの既存台帳照合、SecuredLearningServiceによる操作単位の認可、部署単位のCount/Items/詳細/CSV/集計/編集・一括更新、旧教材DBサービスのWindows認証時停止、CI実DBの拒否試験、画面と学習教材・コラム15を実装した。
- PR #25の元HEADではWindows 2025、Linux実Chrome、PostgreSQL 18実DB、PDFの4チェックが全成功。再開時は最新main/pushもGitHub GETで確認すること。
- 学習PDFはZIP配布しない。mainのPDF CI成功時にlearning-textbook GitHub Releaseへblazor_learning_textbook_ja.pdf単体を公開する。公開状態とダウンロード可能性は再開時にGET確認すること。Actions ArtifactのZIPは教材配布に使わない。
- Webプロジェクト内のData/Servicesを維持し、EF Core専用の層別プロジェクトを追加しない。MudBlazor.TranslationsとStep40/41の特別コラムも保持。

## 次の実装（Step71〜75、必要なら前後に追加）

1. 1万人以上の既存利用者台帳/FDWを想定し、login_name一意性・索引・JOIN/検索pushdown・外部表遅延/切断を否定試験。名前変更・再使用・重複拒否を強化。
2. Windows認証後にログイン状態を保持する業務のみ、専用のHTTP認証エンドポイント→暗号化短命Cookieと明示ログイン案内を実装。IISの匿名/Windows経路を分け、ADパスワード入力を禁止。自己申告Claim禁止、Data Protectionキー共有、失効、HTTP SignInとBlazor circuitの違いまで試験。
3. 編集時の権限変更と書込みの競合・原子性を整理し、既存台帳の途中変更を安全に扱う。
4. 監査を業務本人・操作・対象行・結果・correlation_id・UTC時刻のサーバー側証跡として設計。現行application_name=PgBlazorSamples.Webはアプリ識別だけで個人監査を保証しない。永続化、PII最小化、障害時拒否を試験。
5. Windows Server 2025 + IIS/AD/Kerberos実機の確認手順、CSP/CSRF、Cookie、SignalR reconnect、同時接続・プール負荷試験を充実。CIテスト主体を実AD受入と称さない。
6. 5ステップごとのコラム・日本語Razor教材・PDF索引・実ChromeとPostgreSQL 18試験を同期。PDFも単体Releaseで配布し、ZIPに戻さない。

## 安全境界

- Step66〜70は開発用の架空データでの認可パターン。既存台帳/FDW/IIS/AD実機の運用受入、監査永続化は未完成。本番DB接続は実機受入まで禁止。
- 認証だけで許可しない。can_read/can_editとunit_codeは既存DBから操作時に確認。登録・ロール付与UIを作らない。
- DOMAINを省略した照合は禁止。任意Cookie/HTTPヘッダーの自己申告をWindows本人と取り違えない。
- CI失敗を能動修復し、元PR HEADのWindows/Linux/Chrome/PG18/PDFが全成功してから保護マージ。main/pushとPDF Release実物も検証する。
