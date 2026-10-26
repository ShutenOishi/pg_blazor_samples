# 次のチャットへの引き継ぎ：pg_blazor_samples Step07〜10

更新日：2026-10-26

## 最優先の実行依頼

あなたはGitHub [ShutenOishi/pg_blazor_samples](https://github.com/ShutenOishi/pg_blazor_samples) の.NET 10 Blazor実装担当です。最新の認証済みGitHub GETとIssue/PR/Actionsの証跡を読んで、未完了作業を自律的に進めてください。細かな工程ごとの継続指示をユーザーへ求めず、可能な範囲をまとめて実装・検証・保護PR・マージ・結果報告まで行ってください。

## 実装済み

- Step01〜03: 最初の画面、ルーティング、Razor構文。PR #8、main上の最初の実装コミット `52898c0ca2cd238b56926d911b1e7f1f7f6617f2`
- Step04〜06: `@onclick`のカウンター、`@bind:event="oninput"`の日本語入力、`@if`/`@foreach`の条件付一覧。PR #10（最新マージ状況はGitHubから再確認）
- Step04〜06のUbuntu/Windows Releaseビルド、HTTP疎通、Chrome + Playwrightの操作検証： [Actions Run 40260767597](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40260767597)で成功
- PDF: GitHub Actionsの日本語技術ハンドブック生成。今回のStep04〜06を含む成功Runは [40260767554](https://github.com/ShutenOishi/pg_blazor_samples/actions/runs/40260767554)。PDFは最新のコミットに対応するRunのArtifactを優先
- 既存Issue #7: Visual Studio 2026 GUI、SignalR切断・再接続、アクセシビリティの実機検証は引き続き未完了（成功済みと偽らない）

## 次に実装する内容

Step07〜10を1〜2件のレビュー可能なPRに分けて実装する。

1. Step07 `[Parameter]`：親から子へ値を渡す独立コンポーネント。複数親画面での再利用
2. Step08 `EventCallback`：子コンポーネントのイベントを親へ通知し、状態を更新
3. Step09 `OnInitializedAsync`・`OnParametersSetAsync`・`IDisposable`：ライフサイクルと後処理、二重初期化の学習
4. Step10 `EditForm` + DataAnnotations：必須・文字数・日本語の検証メッセージ、安全な保存処理（DBなし）
5. 既存のトップ・NavMenuへ新URL`/learn/07`〜`/learn/10`を追加し、完成済みのStep01〜06を壊さない
6. 同一PRで `docs/samples/step07.md`〜`step10.md` とサンプル一覧・README・PDF索引を更新
7. CI：Windows/Linux `net10.0` Releaseビルド・発行、日本語HTTP画面確認、Chromeでの実操作、PDF生成とソースハッシュを必ず確認
8. PR条件が成立したもののみ通常の保護マージを行い、再取得した`main`のHEADと実装を確認する

## 守るべき方針

- 画面、ラベル、通知、入力検証エラー、コードコメント、教材、操作手順はすべて**日本語**。HTMLは`lang="ja"`、カルチャーは`ja-JP`
- Visual Studio 2026、.NET 10（C#14）、Blazor Web App Interactive Server。高コントラスト、ダークモード、文字サイズ、キーボード操作を配慮
- 最初の10ステップまでは単独の`PgBlazorSamples.Web`のみ。DIの学習はStep11、UnitTestsはStep18、ApplicationはStep19、MudBlazorはStep21、DomainはStep31、InfrastructureはStep33、PostgreSQLはStep31以降、ADはStep41以降、IIS実機は後期ステップ
- ADとPostgreSQLの利用者の業務データを公開リポジトリに入れない。初級サンプルは架空の固定値のみ
- 「ソース追加」「CIビルド成功」「ブラウザーテスト成功」「AD/IIS実機成功」を明確に区別し、未確認を成功扱いしない
- コードを変更したら日本語教材とPDFを同一PRで更新し、GitHub Actionsのログを照合
- 既存CI・PR・成功した成果物の不必要な再実行を避ける

## 参照先

- [リポジトリ](https://github.com/ShutenOishi/pg_blazor_samples)
- [PR #10](https://github.com/ShutenOishi/pg_blazor_samples/pull/10)
- [Issue #9](https://github.com/ShutenOishi/pg_blazor_samples/issues/9)
- [60ステップ一覧](sample-catalog.md)
- [ソースに準拠する日本語技術ハンドブック](implementation-handbook.md)
- [PDF自動生成](pdf-generation.md)

まず最新GitHub GETで `main`、PR #10、Issue #9、成功済みCI、最新PDF Artifactを確認し、状態に応じてStep07〜10の実装を開始してください。工程途中の提案だけで停止せず、実行可能なことを進めてください。
