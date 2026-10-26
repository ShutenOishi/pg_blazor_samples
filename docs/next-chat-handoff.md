# 次のチャット引き継ぎ：Step73〜75

- Step01〜70は保護マージ済み。Step71はPR #27（大小文字の正規化、重複拒否、改名、10,001人索引、教材・PDF）で最新CIとマージを確認すること。
- Step72はPRで明示Windows認証→専用HTTP確認フォーム→CSRF検証済POST→署名・暗号化10分Cookie、POSTログアウト、DataProtectionKeysPath本番必須、通常認可の都度DB照合、匿名拒否を実装。**最新PR HEADのCI・マージ・実AD受入状態はGitHub GETで検証すること。**
- Step73：既存台帳と業務更新の原子性。同一ローカルDBは同じトランザクション・適切なロックで競合を保証。別DB/FDWでは保証できない場合、安全拒否または権限正本と同期する業務契約を用意。
- Step74：本人、操作、対象、結果、correlation_id、UTCを永続監査。PIIを必要最小限にし、監査障害時は拒否を検証。application_name固定だけでは個人監査にならない。
- Step75：IIS Windows/匿名経路、Kerberos、Data Protection共有、CSP・CSRF、Cookie失効、SignalR reconnect、FDW切断、同時接続数・プール負荷を実機受入。CIの仮Principalを実AD成功とは呼ばない。
- Step71〜75終了時にはコラム16、日本語Razor画面、各教材、目次、PDF単体Release、Chrome/PG18試験を同期。
- **必須CI全成功後にのみ正規保護マージ、main/pushとPDF Release単体の再取得を確認。ZIP配布は禁止。**
- DBの層別プロジェクト増設は禁止。Web内のData/ServicesとEF Core10、MudBlazor.Translations維持。
- ADは認証とDOMAIN付き名だけ。既存DB/FDWが権限正本、利用者登録UIなし。アカウント名再利用の識別不能性、外部表の失効・障害時拒否を維持。
