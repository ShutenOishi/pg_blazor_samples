# 次チャットへの引き継ぎ：Step75、前段PR受入、PDF単体配布

- Step71 PR #27 と Step72 PR #28 はWindows/Linux/Chrome/PG18/PDF受入後mainへ保護マージ済み。
- Step73 PR #29 は行ロックと同一トランザクションによる書込み認可。Step74は新PRで業務DBと同一トランザクションの永続監査、匿名・監査失敗時の拒否を追加。まず各PRの最新版HEAD・CIを認証済みGitHub GETで確認し、修復・順次保護マージ、main/push受入を行う。
- Step75: Windows Server 2025 + IIS / AD / Kerberosの実機手順、CSP・CSRF・DataProtection鍵共有・Cookie失効・SignalR再接続・1万人台帳/同時接続とプール負荷試験、FDW遅延/切断と監査保管/バックアップの受入を追加。5ステップごとの日本語コラム16、Razorページ、step75教材、PDF目次・Chrome・PostgreSQL18 CIを同期。
- 本番DBや実ADの接続テストはCIの模擬Principal成功では代用しない。元表/FDWにロック原子性がないときは更新拒否。認可は既存DBのみ、登録・権限付与UIなし。本人名再利用は元表側禁止または不変ID。
- 日本語教材とコード・PDFを同じPRで更新、失敗は積極修復。Windows/Linux/Chrome/PG18/PDFの必須CI全成功のみ保護マージ。main/pushでPDF Release assetを検証し、ZIP配布しない。
- EF Core用の層別プロジェクトは増やさずWeb内Data/Services。MudBlazor.Translations維持。
