# 次のチャットへの引き継ぎ：Blazor Step21〜25

- 最新の認証済みGitHubから main と成功済みCI/postmergeを確認する。
- Step21〜25にMudBlazorを導入する。公式のバージョン別手順に従い、パッケージ、AddMudServices、Provider、対話型レンダーモード、CSS・JSを検証する。
- Step21：導入とサービス登録、22：レイアウト、23：入力部品、24：フォーム検証、25：テーブル。
- コラム06（ステップ21〜25）で、特別コラム05の「公式手順を読む→登録→注入→Provider配置」を実際のMudBlazorコードで振り返る。
- Stepごとの日本語教材、コラム、PDF日本語目次、Windows/Linuxビルド、UnitTests、HTTP・Chrome操作を同一PRで実装する。
- CIが全成功した後のみ保護マージし、main postmerge受入まで検証する。
- DB、AD、IISは未接続であり、実証済みと記述しない。既存の成功済み工程は再実行しない。

環境：Visual Studio 2026／.NET 10／Blazor Interactive Server。通常12本＋特別編1本のコラムを計画し、実装済みはコラム01〜05。
