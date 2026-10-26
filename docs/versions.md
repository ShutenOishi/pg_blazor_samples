# バージョン管理方針

## 2026年10月26日時点の採用基準

| 対象 | 採用する系列または安定版 | 備考 |
| --- | --- | --- |
| Visual Studio | 2026 | .NET 10・C# 14に対応 |
| .NET SDK / ターゲット | 10 / `net10.0` | SDKの修正パッチは最新の10系を優先 |
| ASP.NET Core | .NET 10付属 | ランタイムとパッチを揃える |
| PostgreSQL | 18系 | セキュリティ修正を含む18系最新パッチ |
| MudBlazor | 9.11.0 | UI部品（Step21〜25に参照追加） |
| MudBlazor.Translations | 3.6.0 | MudBlazor 9系用コミュニティ翻訳（ja-JP） |
| MudBlazor.Templates | 4.3.0 | .NET 10向け生成テンプレート（学習時にCLIで別アプリを生成） |
| EF Core | 10系安定版 | Step31以降の標準ORM。実装時に正確なパッチを固定 |
| Npgsql.EntityFrameworkCore.PostgreSQL | **10.0.3** | Webへ導入したEF Core 10向けの安定版プロバイダー |
| Npgsql | 10.0.3（調査時候補） | PostgreSQLドライバー。プロバイダーとの互換を優先 |
| linq2db / linq2db.PostgreSQL | 標準採用しない | 上級編で比較する場合のみバージョンを選定 |
| Microsoft.AspNetCore.Authentication.Negotiate | 10.0.13 | Windows統合認証 |

**注意:** この表のEF Core/プロバイダーは採用系列であり、まだ実装・復元・ビルドによって確定したパッケージではありません。Step31〜40の実装PRで両者の安定版パッチを揃え、依存関係と実DBの型マッピングを検証して固定します。

**配備先:** Windows Server 2025 + IIS + .NET 10 Hosting Bundle。開発端末のVisual Studioバージョンと、サーバー側の実行ランタイム/Hosting Bundleは区別します。

リポジトリ内で参照する実装パッケージの組み合わせは、NuGet依存関係と結合テストを確認して初めて確定します。上記は調査時点の**採用候補の基準値**であり、現時点でビルド済みの保証ではありません。

## 最新と再現性の両立

1. 新しく採用するライブラリは、その時点の**最新の安定版**を候補にする。`preview`、`rc`は原則採用しない。
2. `Directory.Packages.props`による集中管理を使い、受け入れた**正確なバージョン**をコミットする。`*`や無制限の浮動バージョンは使わない。
3. 新しいバージョンが出ても自動的に本番教材を書き換えず、復元・ビルド・テスト・学習画面の破壊的変更の確認後に更新する。
4. .NETのメジャーアップグレードは教材全体の整合性を確かめてから行う。.NET 10固定の学習教材を黙って.NET 11に移さない。
5. 脆弱性対策パッチは優先し、依存関係監査をCIで実行する。
6. 更新時はこの文書の日付とバージョン表、破壊的変更の説明を同時に更新する。

## 互換性の未検証項目

- EF Core 10系とNpgsql.EntityFrameworkCore.PostgreSQL 10系のパッチ互換、PostgreSQL 18実接続、DateOnly/TimeOnly/Range型・トランザクション・xmin競合。
- MudBlazor 9.11.0とMudBlazor.Translations 3.6.0のCIビルド、ja-JP表示、UIのイベント・事前描画の動作（作業PRのCIで検証する）。
- IIS/NegotiateとWindows認証の構成、アプリ内の認可処理。
- PostgreSQL 18の日時、日本語ソート、ページング。

**未検証の組み合わせを「動作確認済み」と記載しません。**

公式の確認先は[参考資料](references.md)です。
