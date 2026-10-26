# 次のチャットへの引き継ぎ：pg_blazor_samples Step11〜15（DI基礎）

更新日：2026-10-26

## 依頼
接続済みGitHubを利用し、[ShutenOishi/pg_blazor_samples](https://github.com/ShutenOishi/pg_blazor_samples)の最新main/PR/Actionsを確認して、Step11〜15と3本目の復習コラムを自律的に実装してください。途中工程ごとの細かい「継続して」は求めず、コード・日本語資料・テスト・PDFを同じPRにまとめて進めてください。

## これまでの確定状況（最新mainは再確認する）

- Step01〜03：最初の画面・URL・Razor、Step04〜06：イベント・双方向入力・条件表示
- Step07〜10：親→子[Parameter]、子→親EventCallback、ライフサイクルと後処理、EditForm/DataAnnotationsの日本語検証
- コラム01（Step01〜05）、コラム02（Step06〜10）：理解を深める文章とアプリ内の復習ページ
- 上記追加は[PR #13](https://github.com/ShutenOishi/pg_blazor_samples/pull/13)で実装。**作業開始時に最新のマージ結果を必ず再確認する**
- [Actions 40264058164](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40264058164)：Windows Server 2025とUbuntuのReleaseビルド・publish、Ubuntu日本語HTTPとChromeの操作・否定テストがSUCCESS
- [PDF 40264058244](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40264058244)：日本語ハンドブック生成・PDFヘッダ検査SUCCESS
- 本リポジトリはVisual Studio 2026 / .NET 10（C# 14）/ Blazor Interactive Serverで、最終配備想定はWindows Server 2025 + IIS + AD + PostgreSQL 18
- 実AD・DB・IISの実機テストは**まだ実施していない**。混同禁止

## Step11〜15の実装内容

1. **Step11 DIとは何か**：画面内で直接生成する時計サービスとDI登録から受け取る場合を比較。必要なサービス以外を導入しない
2. **Step12 インターフェイス**：IClock等の小さな契約と実装を分け、固定時刻への差し替えを示す
3. **Step13 3種類のライフタイム**：Singleton、Scoped、Transientの実際の状態とIDを操作で観察
4. **Step14 BlazorのScoped/circuit**：HTTPリクエストとcircuitの違い、利用者状態をSingletonへ格納しない理由を日本語で説明。公開CIが実ADに接続したと誤認させない
5. **Step15 @injectとコンストラクター**：Razorコンポーネントと通常のC#サービスでの注入方法を比較
6. **コラム03：Step11〜15のまとめ**：DIとは何か、依存の向き、サービス寿命、テスト容易性の関係・誤解・確認問題と解答を追加。サイト上の復習画面とMarkdownを同時追加

## プロジェクト構成を維持する

Step11〜17は既存Webプロジェクト内のServicesと契約を少しずつ増やす。Step18でUnitTests、Step19でApplication、Step31でDomain、Step33でInfrastructure、Step40でIntegrationTests。最初から多層構成を追加しない。MudBlazorはStep21、PostgreSQLはStep31以降、ADはStep41以降に導入する。

## 実装・受入の一括手順

- 最新mainと未完了PRを確認、不要な再実行はしない
- 既存コンポーネントを壊さず、Step11〜15の/learn/11〜/learn/15を追加
- UI、入力検証、コードコメント、エラーメッセージ、解説、CIログの利用者向け文言を日本語に統一
- docs/samples/step11.md〜step15.mdとdocs/columns/column11-15.mdを実ソース準拠で追加、docs/columns/README.mdとPDFスクリプトへ反映
- ステップごとに「前提、操作、動作の仕組み、注意点、演習、否定テスト」を含める
- Linux・WindowsのReleaseビルド、HTTP、Chromeの実操作、PDF生成を確認。成功済みの旧テストも回帰確認
- 成功した場合のみ保護マージし、新しいmainとpostmergeの成功を受け入れる。状態・証跡を正確に更新
- 完了後は次のチャットへの引き継ぎをStep16〜20に更新する

## 重要な仕様
- 60ステップ、5ステップごとにコラムを置く全12回構成（[コラム一覧](columns/README.md)）
- 日本語のUIとコメント・読みやすい文字・ダークモード・キーボード操作を維持
- 環境依存・資格情報・社内ネットワーク情報は公開リポジトリに含めない
- 「コード作成」「CI成功」「AD/IIS実機成功」を別々の事実として扱う
