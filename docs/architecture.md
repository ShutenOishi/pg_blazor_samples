# アーキテクチャ方針

## 達成したいこと

- **学びやすい**：01から60へ1機能ずつ増やす。既存の共通設定・画面を再利用する。
- **実務に近い**：UI、業務処理、データアクセス、認証と認可の責務を区別する。
- **増やしやすい**：ステップを追加しても大量の複製プロジェクトや設定ファイルを増やさない。
- **安全**：Windowsの本人認証だけで業務操作を許可しない。

## 想定するディレクトリ

```text
/
  README.md
  CONTRIBUTING.md
  SECURITY.md
  docs/
    README.md
    sample-catalog.md
    learning-path.md
    ...
    decisions/
  src/
    PgBlazorSamples.Web/
      Components/
        Layout/
        Pages/
        Samples/
          Step01/
          Step02/
          ...
      Features/
        Users/
        Authorization/
      Application/
      Infrastructure/
        Persistence/
        Identity/
  tests/
    PgBlazorSamples.Tests/
    PgBlazorSamples.IntegrationTests/
  database/
    schema/
    seed/
```

これは**実装予定の配置**であり、まだ存在しないファイルを完成済みと主張するものではありません。最初は1つのWebプロジェクトと必要なテストプロジェクトで構成し、責務の分割が必要と確認できた場合のみクラスライブラリを切り出します。

## 責務分担

| 層 | 行うこと | 避けること |
| --- | --- | --- |
| Components | 画面表示、入力、イベント、状態表示 | SQL直書き、ADの資格情報操作 |
| Application | 業務ルール、入力の正当性、操作単位の認可 | UI部品やSQL文字列への直接依存 |
| Infrastructure/Persistence | linq2db・Npgsql・トランザクション | 画面状態の保持 |
| Infrastructure/Identity | Windows主体の識別とDB利用者照合 | ADのパスワードをDBへ複製 |
| Database | 業務データ・利用者・ロール・監査 | 本人認証そのものの代行 |

## Interactive Serverの注意点

サーバー上でコンポーネントのイベントが実行され、ブラウザーとの接続に回線状態が影響します。切断・再接続・二重操作・コンポーネント破棄を考慮します。

- DB接続やlinq2dbのデータコンテキストは**操作単位**で生成・破棄し、アプリ全体のSingletonにしない。
- Interactive ServerのScoped寿命は一般的なHTTPリクエストとは異なるため、接続の競合や長時間保持に注意する。
- `NpgsqlDataSource`の共有と、接続・トランザクション単位の分離を区別する。
- ページ表示の非表示制御は**認可の代用にしない**。業務処理の入口でも認可する。
- 読み取り操作と更新操作を区別し、必要に応じ中断・冪等化・楽観排他を実装する。
- 事前描画と初期化処理の二重実行を検証する。

## 追加プロジェクトを作る条件

次のいずれかが明確になったときにだけ追加します。自動テストの分離が必要、ドメイン層を複数ホストで再利用する、学習ステップの前提を独立させないと壊れる、といった場合です。無条件に大規模なクリーンアーキテクチャのひな形を持ち込むことは避けます。

この判断の記録は[ADR-0001](decisions/0001-learning-app-structure.md)に残します。
