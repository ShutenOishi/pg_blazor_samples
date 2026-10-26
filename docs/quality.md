# 品質管理とCI方針

## ステップごとの完成定義

実装済みと判定するため、少なくとも次を確認します。

- [ ] .NET 10の復元・ビルドが成功する。
- [ ] 該当ステップを画面から開ける。
- [ ] 正常操作・異常操作を確認する。
- [ ] 日本語の説明と演習がある。
- [ ] 関連の単体テストが成功する。
- [ ] 外部データを扱う機能は認可・SQLパラメーター化・入力検証を確認する。
- [ ] DB・ADを使うステップでは実接続テストの証跡を確認する（モック成功では代用しない）。
- [ ] 他のステップと共通UIが壊れていないことを確認する。

## 現在実装済みのCI

- [x] Ubuntu 24.04で.NET 10の復元、Releaseビルド、日本語HTML画面のHTTP疎通、発行を検証。
- [x] Windows Server 2025ランナーで.NET 10の復元、Releaseビルド、発行を検証。
- [x] 不明な学習URLで日本語の説明が表示されることを確認。
- [x] RazorでHTMLタグに似た文字列が、タグとして出力されないことを確認。
- [x] Step04〜06の実ブラウザークリック、日本語入力、一覧フィルター、初歩的なキーボードフォーカスを検証。
- [ ] より広範なキーボード操作・読み上げ、SignalRの強制切断・再接続は今後検証。

CIの実行履歴：[Blazorビルド](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build.yml)／[PDF生成](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build-docs-pdf.yml)。公開Runnerから社内ADへの接続は行いません。

## Step04〜06の実ブラウザー試験

`scripts/browser-smoke.cjs`で、GitHub ActionsのUbuntu上のChromeとPlaywright Coreを使用して、次の動作を検証します。

- Step04：カウンターを2回増加、1回減少、初期値へ戻すこと。0で減算できないこと
- Step05：日本語の名前入力、即時プレビュー、タグ文字列のHTMLエンコード、入力消去
- Step06：全4件から未完了2件へ絞り込み、その後4件へ復帰
- キーボードのフォーカスが移動すること（詳細なWCAG検証は別に行う）
- 成功時に日本語画面のスクリーンショットをActionsのArtifactへ保存
- Linuxのスクリーンショットで日本語が□などの代替記号にならないよう、Chrome起動前にNoto Sans CJK JPを導入して日本語用fontconfigの解決先を検証

上記の実ブラウザー試験は [GitHub Actions Run 40260767597](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40260767597) で **Ubuntu/Windowsビルド・ブラウザー操作ともSUCCESS** を確認しました。操作の画面画像は同RunのArtifactsから取得できます。

## Step07〜10の追加試験（GitHub ActionsでSUCCESS）

追加テストは[Run 40264058164](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40264058164)で、Ubuntu/Windowsビルド、UbuntuのHTTP・Chrome操作、双方の発行が成功しました。PDFは[Run 40264058244](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40264058244)で生成・検査が成功しました。



- Step07：親が入力した文字だけが1枚目の再利用カードに反映される
- Step08：子の選択が親の表示・選択回数へ反映される
- Step09：パラメーター更新、非表示による破棄、再表示によるインスタンス再作成を確認する
- Step10：必須未入力、文字数不足、正常入力時のみ確認欄更新を検証する
- コラム01・02：日本語画面をHTTPで表示できる
- 計画しているコラム03〜12は未実装。実装した段階で個別試験を追加する

## 今後追加するGitHub Actions

| 検証 | 条件 | 目的 |
| --- | --- | --- |
| `dotnet restore` | 全PR | 依存関係を解決 |
| `dotnet build -c Release` | 全PR | 警告・コンパイル不具合を検出 |
| `dotnet test -c Release` | 全PR | 単体・結合可能なテスト |
| `dotnet list package --vulnerable`相当 | 全PR | 既知の依存脆弱性を調査 |
| ドキュメントリンク確認 | 文書変更PR | 参照切れを防止 |
| PostgreSQL 18結合テスト | DB変更PR | スキーマ・問い合わせの整合性 |
| Windows認証手動または専用Runnerテスト | 認証変更PR | 実際のAD、IIS、ブラウザー検証 |

公開GitHub Runner上で本物の社内ADに接続しません。認証の単体テストに使用する疑似主体と、専用環境で実行する実Windows認証テストを分けて記録します。

## 最低限の否定テスト

- 不正な入力・空文字・桁あふれとSQLインジェクション文字列
- 未登録・無効・権限なし・拒否後の業務APIアクセス
- DB接続不良・タイムアウト・キャンセル
- 二重送信、複数利用者による同時更新
- コンポーネント破棄、切断と再接続
- 日本語の検索と日時変換、アクセシビリティ

## 学習教材の再現性

サンプル追加時に環境前提・NuGetバージョン・DB初期化・確認コマンドを同じPRで更新します。作業未完了の項目はチェックを付けず、失敗を隠さない方針とします。

## Windows Server 2025 / IISの受入チェック

- [ ] .NET 10 Hosting BundleとIIS/Windows認証/WebSocket構成を実機確認
- [ ] 内部DNS、HTTPS証明書、Kerberosの使用状況を確認
- [ ] AD認証済みでもDB未登録・無効・権限なし・DB障害なら拒否
- [ ] アプリプール再起動・回線切断・権限変更・同時更新を検証
- [ ] 利用者の権限とIIS実行ID・DB接続IDの権限を分離
- [ ] 運用手順とロールバック、ログ・バックアップ/復元手順を確認

これらは検証項目であり、**今回のドキュメントPRでは未検証**。

## 学習者向けPDFの品質基準

- PDF表紙は「Blazor 実践学習テキスト」。目次の項目には学習テーマの日本語タイトルを表示する。
- 目次や本文にリポジトリ、PR、GitHub、コミットハッシュや開発管理用のファイルパスを表示しない。
- 検証に必要なコミット・Markdownとソースのハッシュは別の`blazor_learning_source_manifest.json`へ記録。
- 5ステップごとのコラムは、対応するステップの直後に配置する。
- PDFの文字検索で日本語タイトルとコラムを確認する。PDF生成CIとアプリのビルド・ブラウザーテストは別々に成功判定する。

## Step11〜15とコラム03の検証記録

- [Windows/LinuxビルドとChrome操作](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40267851541)：SUCCESS
- [学習者向けPDF](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40267851605)：SUCCESS
- Step11：DIと直接生成の両方で挨拶を表示
- Step12：固定時計の午前8時に切り替え「おはようございます」を確認
- Step13：Transient再取得で新しい識別子を確認（対話描画完了を待つ）
- Step14：親と子で同じScoped状態を共有し、パラメーター更新によって双方を再描画
- Step15：Razorへの注入とコンストラクター注入による挨拶表示
- PDF：学習者向け日本語目次、章タイトル、不要な開発管理文言の不在を確認

実AD・DB・IIS接続の確認ではありません。
