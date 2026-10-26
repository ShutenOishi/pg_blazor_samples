# 次チャットへの引き継ぎ：80ステップ完了後の品質確認・実機受入

対象: https://github.com/ShutenOishi/pg_blazor_samples

## 現在の確定状態（2026-10-26 JST）

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
