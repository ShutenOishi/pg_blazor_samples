# 次のチャット引き継ぎ：Step74〜75

- Step71 PR #27、Step72 PR #28、Step73の段階的保護PRを作成済み。最新main・各PR HEADとWindows/Linux/Chrome/PostgreSQL18/PDFのCIを認証済みGitHub GETで確認し、失敗は修復して全成功後に**順番に**保護マージ。各PRのbaseを最新mainへ整合させる。
- Step71: ログイン名大文字正規化・重複拒否・改名試験。Step72: HTTP Negotiate認証→CSRF付きPOST→10分Cookie、DP鍵共有と匿名拒否。Step73: 更新に `Authorization:TransactionalLedgerConfirmed=true` を要求、同一トランザクションの台帳行 `FOR UPDATE` と課題更新、競合テスト。FDW/別DBの原子性が未検証なら更新拒否。実機AD/IIS/FDW認証をCI成功と同一視しない。
- Step74: 永続監査。実行者・操作・対象・成否・correlation_id・UTC、PII最小化、同一トランザクションで更新と監査を保存。監査書込障害は業務処理も拒否、否定試験を追加。application_name=固定アプリ名は個人監査ではない。
- Step75: Windows Server 2025 / IIS / Kerberos、CSRF/CSP、Cookie+Data Protection鍵共有・失効、SignalR切断復旧、FDW停止、1万人台帳と異なる同時接続数・プール負荷の手順と検証、コラム16を追加。
- 日本語Razor・step別教材・5ステップコラム・PDF目次・Chrome/PG18統合試験を同期。PDFはZIP配布しない。main/pushのPDF CI成功後に Release learning-textbook のPDF単体アセットを再取得検証。
- Web内Data/ServicesとEF Core10を維持し、層別プロジェクトは増やさずMudBlazor.Translationsも保持。ADは本人認証とDOMAIN名だけ。登録UIは禁止。ログイン名再利用禁止または不変ID契約を維持。
