# 次のチャット引き継ぎ：Step66〜70の業務認可・監査・IIS

## 観測済みmain
- Step51〜55 PR #22、Step56〜60 PR #23は保護マージ済み。
- Step56〜60のmain/pushアプリ・PDF CIはSUCCESS。
- Step61〜65は `feat/step61-65-windows-sid-roles` に実装。PRの最新CIとmain状態を再確認すること。

## Step61〜65で追加したもの
- 公式Negotiate認証の設定分岐、Blazor認証状態、SID確認。
- WebのData/内にapp_user、app_role、app_permission、関連テーブルをEF Coreマッピング。開発用DDLは002_app_access.sql。
- WindowsUserAccessServiceで未認証・SIDなし・未登録・無効・権限なし・DB障害を拒否。サービス認可の範囲はまだ限定的。
- Step61〜65の日本語画面、コラム14、5章の日本語教材、PDF索引、Chrome匿名拒否・PostgreSQL18結合試験。
- EF CoreはWeb内のData/とServices/のみを使い、新たな層別プロジェクトを作らない。MudBlazor.TranslationsとStep40/41間の用途別特別コラムは維持。

## 続き（重要）
1. 最新PRのWindows/Linux/実Chrome/PostgreSQL18/PDF CIを確認。失敗は修復し、全成功後だけ保護マージしてmain/pushを受け入れる。
2. Step66：AuthorizeRouteView・AuthorizeViewとFallbackPolicyによる画面認可。匿名の安全な教材を明示して、業務部分だけを確実に保護する。
3. Step67：Count・Items・詳細・CSV・一括更新を含む実業務サービスのすべての経路で**操作ごとに**権限を判定。権限変更・DB障害はフェイルクローズ。
4. Step68：ロールと利用者の管理操作は管理者権限とCSRF対策付きで実装。任意のSID入力で本人を偽装しない。
5. Step69：個人情報を最小化した監査（実行者・対象・成否・時刻、失敗含む）。
6. Step70：IIS/Windows Server 2025/KerberosとSPNの構築確認。CI代替と実AD・IIS実機受入を区別する。
7. コラム15・日本語画面・コード・PDFを同一PRで同期し、必須CI全成功を完了条件にする。

## 安全契約
- **Step66〜70のサービス認可完成までは開発用DB以外につなげない**。WindowsAuthentication:Enabled=falseは業務公開不可。
- SIDは認証済み主体のみから取り、HTTPヘッダーや入力値、単なるログイン名へフォールバックしない。
- CIで実AD/IISのKerberos成立を実証したと主張しない。
