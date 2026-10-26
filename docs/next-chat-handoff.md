# 次のチャット引き継ぎ：Step72〜75（Windows明示ログイン・権限変更・監査・実機受入）

## 確定済み
- Step01〜70は保護マージ済みで、Windows/Linux・Chrome・PostgreSQL18・PDF検証済み。
- Step71は既存台帳の大文字正規化、case collision拒否、式索引、改名後の旧名拒否、匿名拒否画面、教材・PDF索引を実装するPRで作業開始。**CI/マージ/配布結果は最新GitHub GETで確認**する。
- PDFはZIP化しない。main/push成功後に learning-textbook Release へPDF単体アップロードする。
- DBデータアクセスの層別プロジェクト追加を禁止。Web/Data・Servicesを使う。MudBlazor.Translationsを維持。

## 継続指示
1. Step71 PRの最新HEAD Windows/Linux/Chrome/PostgreSQL18/PDFがすべて成功したら正規マージし、main/pushとPDF単体Releaseを確認。失敗時は能動修復。
2. Step72：**認証ハンドラーのみが取得したWindowsログイン名**による専用HTTP認証経路、短命の暗号化Cookie、Data Protectionキー共有、CSRF、失効を実装。任意クレーム・ADパスワード入力・ブラウザーからの自己申告は禁止。IIS Windows認証と匿名経路の共存、SignInがHTTP応答に必要なこと、Blazor circuit再接続を試験。
3. Step73：編集途中の権限剥奪に対応。既存台帳と業務行が同一DBでロック可能な場合は一貫したトランザクション、FDW/別DBでは**原子性の保証がないことを明示して安全な拒否または正本側APIとの連携契約**を選択。権限変更と書込みの競合否定試験を追加。
4. Step74：本人・操作・対象・結果・correlation_id・UTCを永続化。PII最小化、監査保存失敗時拒否、処理の成否と証跡の一貫性を確認。application_name単独では個人監査にならない。
5. Step75：Windows Server 2025/IIS/AD/Kerberos、CSP/CSRF、Cookie/SignalR reconnect、負荷、FDW切断の受入手順と検証。CIの偽Principalを実ADと称さない。
6. Step71〜75終了でコラム16、Razor画面、日本語教材、PDF索引、実Chrome・PG18試験を同期。保護PRの必須CI成功後のみマージ。

## 安全境界
- 1万人の登録人数と同時接続数は別。ADは本人認証とドメイン付きユーザー名のみ、登録UIなし、既存DB/FDW正本による都度認可。
- login_nameの**再利用**はユーザー名だけでは原理的に識別不能。上位正本の非再利用契約、または不変ID追加が必要。正本の重複、遅延、障害時は拒否。
- 開発用の架空台帳は本番DBへ適用禁止。Pooled Npgsqlのapplication_nameは固定アプリ識別のみ。
