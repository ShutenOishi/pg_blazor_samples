# pg_blazor_samples

日本語のBlazor実践学習教材です。**学習順001〜110**を現在の履修順として使用します。WinForms・Dapper経験者が、Visual Studio 2026／.NET 10／Blazor Web App（Interactive Server）／MudBlazor／EF Core 10／PostgreSQL 18を段階的に学べる構成です。業務アプリの運用想定にはWindows Server 2025、IIS、Windows統合認証を含みます。

## 学習の開始とソースコード

**[学習順001〜110：本文と実際のRazorソースを直接開ける索引](docs/course-v3/source-index.md)**

[学習項目とソースファイルの対応データ](docs/course-v3/source-map.json)にも全110件の学習順・タイトル・教材・画面・URLを収録しています。教材とRazorの対応に加え、各学習項目の正規URLは `/learn/001` から `/learn/110` です。以前のURLは既存リンクを保護する互換経路として残します。番号の読み替えやコードファイル名からの逆引きは不要です。

Visual Studio 2026で `PgBlazorSamples.slnx` を開き、`PgBlazorSamples.Web` を起動してください。コマンドラインでも起動できます。

```powershell
dotnet restore PgBlazorSamples.slnx
dotnet run --project src/PgBlazorSamples.Web
```

学習アプリのトップ画面から各学習順へ移動できます。学習内容によりPostgreSQL 18の学習用DBが必要です。実AD・IIS・FDWへ接続しない基礎演習と実機試験は別のものです。

## 教材v3の構成

| 段階 | 学習順 | テーマ | 概要 |
| --- | --- | --- | --- |
| 1 | 001〜005 | Web開発の前提 | [段階概要](docs/course-v3/stages/stage01.md) |
| 2 | 006〜020 | Blazorの基本と実行原理 | [段階概要](docs/course-v3/stages/stage02.md) |
| 3 | 021〜030 | DI・サービスと単体テスト | [段階概要](docs/course-v3/stages/stage03.md) |
| 4 | 031〜045 | MudBlazorとブラウザーの業務UI | [段階概要](docs/course-v3/stages/stage04.md) |
| 5 | 046〜060 | DapperからEF Core 10・PostgreSQL 18へ | [段階概要](docs/course-v3/stages/stage05.md) |
| 6 | 061〜080 | 一覧データ連携・検索・編集 | [段階概要](docs/course-v3/stages/stage06.md) |
| 7 | 081〜095 | Webセキュリティ・AD認証とDB認可 | [段階概要](docs/course-v3/stages/stage07.md) |
| 8 | 096〜105 | 監査・競合制御・Windows Server運用 | [段階概要](docs/course-v3/stages/stage08.md) |
| 9 | 106〜110 | 業務アプリの通し演習 | [段階概要](docs/course-v3/stages/stage09.md) |

全110件の教材本文、通常復習22件、段階概要9件を収録しています。追加30テーマについては、画面・教材の存在と実際のサンプル処理の充実を区別し、[実装・教材・試験照合計画（Issue #38）](https://github.com/ShutenOishi/pg_blazor_samples/issues/38)に沿って確認・補完します。既存のサンプルを利用できる場合は重複実装しません。

## 技術構成とサンプルの考え方

- 主なアプリは `src/PgBlazorSamples.Web` に置き、学習用の画面、サービス、Data、JavaScriptを分けます。計算用の契約と実装もWeb内の `Services/Learning` に配置し、テストは `tests/PgBlazorSamples.UnitTests` で行います。
- EF Core 10とNpgsqlを標準のDBアクセスに使用します。WinForms＋Dapperとの比較は学習用の専用データで行い、無許可の業務DB接続を行いません。
- 一覧/検索、データ編集、トランザクション、競合、監査は、画面の表示とサーバー側の業務サービスの責任を区別します。
- 学習用の模擬チェックやSQL文字列の表示を、DB実行や本番構成の確認が完了した証拠として扱いません。

## 自動検証と教材PDF

アプリCI：[Windows/Linux Release・xUnit・Chromeの操作検証・PostgreSQL 18実DB試験](https://github.com/ShutenOishi/pg_blazor_samples/actions/workflows/build.yml)。教材の順序・対応関係は `docs/course-v3/curriculum.json` と `scripts/check-learning-materials.py` で管理します。

現在公開済みの日本語教材は [Blazor実践学習テキスト（正式Release）](https://github.com/ShutenOishi/pg_blazor_samples/releases/tag/learning-textbook) から取得できます。[PDF生成方法](docs/pdf-generation.md)も参照してください。ソースの補完・教材の更新計画は既存の正式PDFを自動再発行する許可を意味しません。公開済み版と次の改訂内容は区別します。

## Windows認証・業務認可・現地受入

Active Directoryは本人認証と信頼できるログイン名の取得に使用し、**操作・部署・行単位の業務権限は既存DB台帳を都度参照**します。Cookieやcircuitの継続、Razorの表示内容だけでは許可しません。権限失効、`xmin`競合、監査不能、FDWの原子性が保証できない更新は安全に拒否します。実際のパスワード・接続文字列・鍵をコミットしません。

Windows Server 2025・IIS・AD/Kerberos・FDW・Data Protection鍵・負荷・復旧の検証には別途許可と実機証跡が必要です。[現地実機受入計画](docs/course-v3/on-site-acceptance-plan.md)ではSITE-01〜15の試験を管理し、未実施はNOT_RUNとします。学習アプリと正式PDFの公開は、業務システムの本番運用承認ではありません。

## 参考資料と参加方法

[ドキュメント目次](docs/README.md)、[実装対応ハンドブック](docs/implementation-handbook.md)、[設計書](docs/architecture.md)、[日本語学習ロードマップ](docs/learning-path.md)、[セキュリティ](SECURITY.md)、[参加方法](CONTRIBUTING.md)を参照してください。
