# 次チャットへの引き継ぎ：Step76〜80 CI受入と実務品質の仕上げ

対象: https://github.com/ShutenOishi/pg_blazor_samples

- Step71〜75はPR #27〜31がmain保護マージ済み。main 18b0adf5...でWindows/Linux・Chrome・PostgreSQL18・日本語PDF生成のpush CIが成功済み。
- 今回の新規ブランチ feat/steps-76-80-operations でStep76〜80の日本語Razor画面、ソース、xUnit、教材・コラム17、PDFの目次、匿名ブラウザー確認、/health/liveを追加。再開時は最新PR/HEADとCIの成否をGitHub GETで必ず確認する。
- 失敗したCIは能動修復し、必須CI成功・元HEAD照合後に通常マージ、main/pushのWindows/Linux・Chrome・PostgreSQL18・PDF成功を受け入れる。
- PDFはReleaseの単体アセット blazor_learning_textbook_ja.pdf として公開しZIP化しない。main/push成功後に配布URL・更新時刻・内容を再取得する。
- ADは認証とユーザー名のみ。登録UIなし・既存DBの権限を都度検証。更新の権限行ロック・xmin・監査は同一トランザクション、FDW原子性未実証は拒否。Step77は機密データをキャッシュしない。
- 実AD・IIS・Kerberos・FDW・Cookie共有・10,001人規模の現地受入はCIとは別であり、未実施を完了と表示しない。
