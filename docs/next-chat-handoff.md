# 次のチャット引き継ぎ：大規模認証編のPR #24受入とStep66〜70

## 最新の基本状況

- mainはStep01〜60まで。Step61〜65の実装はPR #24 `feat/step61-65-windows-sid-roles` にある。**必ず最新PR HEADとCIを再取得する**。
- ユーザー要件によりPR #24の初期SID/app_user登録モデルは置換。以後は**ADはWindows認証とドメイン付きユーザー名取得だけ、利用者登録なし、既存テーブル/外部テーブルを正本**にする。
- WebのData/Servicesを維持し、EF Coreのための層別プロジェクトを新設しない。MudBlazor.TranslationsとStep40/41間の特別コラムを保持。

## PR #24の追加実装

- ReadTrustedLoginName：認証済みWindows主体から`DOMAIN\\user`を得る。SIDやHTTPヘッダーへ依存しない。
- LearningDbContextに読み取り専用の`learning_user_access_v`をキーレスでマッピング。0件・複数件・無効・権限なし・障害時は拒否する。
- `database/schema/002_app_access.sql`は**CI専用の架空1万1人の台帳fixture**。業務アプリ自体は利用者を登録しない。
- Step61〜65、コラム14、特別コラム「1万人規模の認証・既存台帳・明示ログイン・監査」、設計資料、PDF索引とChrome操作、PostgreSQL18試験。
- Windows/Linux/実Chrome/PostgreSQL18/PDFが最新PR HEADで全成功するまで未完成。成功すれば通常の保護マージとmain/push CIを検証する。

## 続き

1. PR #24の元HEAD CIを見て、失敗があれば能動的に直す。全成功後マージしてmain CIを見る。
2. Step66〜70のルート・サービスの実認可へ進み、Count/Items/詳細/CSV/編集/集計と部署の絞込を必ず共有する。
3. 外部台帳/FDWの遅延・索引pushdown・キャッシュ反映・ユーザー名再使用・重複・DB障害の否定検証を強化する。
4. 明示ログインが必要な業務だけ専用Windows認証HTTP endpoint→短命Cookieを採用。IIS匿名可否/経路、データ保護キー、失効、HTTP SignInを分離。**ADパスワード入力は禁止**。
5. 監査はアプリ側の確実なレコードを優先し、PostgreSQL application_nameは補助的な固定名またはトランザクション限定相関値にする。
6. 日本語画面、コラム15、学習テキストPDFとソースの整合性、実機AD/IISとCI代替の区別を維持する。

## 安全境界

- Step66〜70のサービス強制認可が完了するまで、実データは接続不可。
- ドメインを省くユーザー名照合は不可。ユーザー名再使用/変更の運用リスクを明示。
- 非Windows認証スキームの自己申告Claimを業務本人として受理しない。
