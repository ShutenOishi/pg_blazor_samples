# 次チャットへの引き継ぎ：Step71〜75の受入とStep76以降

対象: https://github.com/ShutenOishi/pg_blazor_samples

- Step71 PR #27、Step72 PR #28はCI成功後にmainへ保護マージ済み。Step73 PR #29、Step74 PR #30、Step75 PRは段階的PR。最新GitHub GETを必ず取得し、各HEADのWindows/Linux・Chrome・PostgreSQL18・日本語PDF CIを確認。失敗を能動修復して順に保護マージし、main/push受入。
- PDFはReleaseの単体アセット `blazor_learning_textbook_ja.pdf` として配布しZIPにはしない。main/push成功後に実物URL・内容・更新コミットを再確認。
- Step73は同一DBの権限行ロックを実機で保証できる場合だけ`Authorization:TransactionalLedgerConfirmed=true`。FDW/別DBは原子性未検証なら更新拒否。
- Step74は`Authorization:DurableAuditConfirmed=true`で永続監査、更新と監査を同一トランザクションでコミット。失敗時は拒否。CIの疑似認証はAD実機試験ではない。
- Step75はWindows Server 2025/IIS/AD/Kerberos、SPN・証明書・Cookie失効・Data Protection鍵・SignalR・FDW・高負荷の実機チェック手順。現地証跡を確認するまで受入済みと称さない。
- Step76〜80へ進むときもWeb内EF Core10、MudBlazor.Translations、学習5ステップごとの日本語コラム、Razor/PDF/CIの同期、登録UIなし、既存DB認可を維持。
