# 学習順050：DbContextFactoryと接続プール

## 到達目標・前提知識
Blazor Interactive ServerでDbContextを安全に利用する単位を学びます。WinForms＋Dapperでクリックごとにusing var connectionとしていた考え方に近いものの、DbContextには変更追跡など独自の状態があります。Interactive ServerのScopedはcircuitの間続くことがあり、DbContextはスレッドセーフではありません。画面に長期間注入したDbContextを複数操作で共有せず、IDbContextFactoryで操作ごとに作ります。
画面の「接続して一覧を取得」はLearningDbService.SearchAsyncを呼び、Messageをrole=statusで表示します。開発用DBが未設定なら接続を試さず案内を表示し、設定済みでDBが稼働すれば最大20件の検索を実行します。
前提は第4段階で学んだRazorイベント、DI、MudBlazorの日本語UIです。この章は既存80ステップのURLを維持したまま、全110本の学習順に従って読みます。必要に応じて開発用PostgreSQL 18を使用します。

## 仕組み・処理の流れ
CreateDbContextAsyncで新しいDbContextを取得し、await usingで処理終了時に破棄します。AddDbContextFactoryは自動的にDbContextインスタンスをプールするAddPooledDbContextFactoryとは別の登録です。Npgsqlは設定に応じて物理接続をプールできるため、DbContextを破棄してもTCP接続を毎回新設するとは限りません。この違いを説明できることが重要です。
SearchAsyncはAsNoTrackingを用い、絞り込み条件があればTitle.Containsで検索し、CountAsyncで総件数、OrderBy(Id).Take(20).Select(...)で表示用の最大20行を取得します。CountAsyncとToListAsyncは別のDBコマンドで、両者の間で更新が起これば件数と一覧が完全には一致しない可能性があります。厳密な時点の一致が必要ならトランザクションの分離レベルも検討してください。
接続失敗はログへ例外を記録し、画面へは内部接続情報を隠した日本語の案内を返します。OperationCanceledExceptionは通常の接続失敗にまとめません。実業務の1万人同時利用を設計するときは、同時DB操作数、Npgsqlプール、DB側max_connections、SQL実行時間、タイムアウトを測ります。この教材のCIだけでは実機負荷を受入済みにはできません。

## WinForms＋Dapperとの具体的な比較
| 比較の観点 | 従来の実装 | Web＋EF Core | 判断する点 |
| --- | --- | --- | --- |
| 寿命 | using NpgsqlConnectionを閉じる | Factoryから操作ごとにDbContextを生成 | 長いcircuitへ保持しない |
| 接続の再利用 | Npgsql接続プールを利用可能 | Context破棄と物理接続再利用は別 | 毎回TCP接続とは限らない |
| 検索結果 | SELECTで件数を指定 | AsNoTracking/Take(20)/Select | 表示件数と総件数は別 |

## 実装に対応するコード例1
```razor
// DbFactoryPage.razorの表示とイベント
@inject PgBlazorSamples.Web.Services.Learning.LearningDbService Database
<MudButton Variant="Variant.Filled" Color="Color.Primary"
           OnClick="LoadAsync">接続して一覧を取得</MudButton>
<p role="status" data-testid="step34-result">@_message</p>
@code {
    private string _message = "まだ実行していません。";
    private async Task LoadAsync() =>
        _message = (await Database.SearchAsync()).Message;
}
```
このコードは実装の一部分、または比較用と明記した参考例です。画面がどこまで動作するかを区別して確認します。

## 実装に対応するコード例2
```csharp
// LearningDbService.SearchAsyncの主要部分
if (!Configured) return new(NotConfigured, []);
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var query = db.Tasks.AsNoTracking();
int count = await query.CountAsync(cancellationToken);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new LearningTaskRow(
        t.Id, t.Title, t.DueDate, t.StartTime, t.IsDone, t.Version))
    .ToListAsync(cancellationToken);
```
表示用DTO、SQLの型、短命DbContext、DI登録などの役割は異なります。コードが示す操作の対象、寿命と副作用を説明してから次へ進んでください。

## Webに移ったときの基本的な境界
この教材ではBlazor Interactive Serverを使います。画面に表示されるHTMLやクリック操作の入口はブラウザーにありますが、RazorイベントのC#、DBサービス、EF Core、NpgsqlはWebサーバーで動きます。WinFormsではフォームのイベントが利用者PC上のプロセスで実行されていました。Webでは複数利用者の操作がサーバーに集まるため、画面に保持した状態とDB接続の寿命を同一視してはいけません。画面で固定の架空データが表示できたこと、DIの設定が読み取れたこと、SQLのテーブル定義が表示できたことは、それぞれ実DBへの接続が成功した証拠ではありません。
業務用のデータアクセスには別の条件があります。ADは本人認証とドメイン付きユーザー名の取得に用い、操作対象の部署・行範囲・権限は事前に整備されたDBまたは外部台帳を正本として毎操作サーバー側で判定します。データベース接続に付けるapplication_nameはアプリケーションの識別補助であって利用者別監査の代替ではありません。監査ができない更新、競合している更新、権限失効を確認できない操作は安全に拒否します。学習用の匿名データサービスを業務認可の済んだサービスとして流用しません。実AD、IIS、Kerberos、FDW、1万人以上の実機負荷は教材の模擬試験と分けて評価します。
開発環境ではVisual Studio 2026、.NET 10、EF Core 10とNpgsql、専用PostgreSQL 18を用います。DBが必要な操作だけ専用環境を構成し、業務DBや実在利用者データを練習対象にしないでください。接続情報にパスワードが含まれる場合はGitへ記録しません。コード例は既存実装の主要部分を抜粋したものと、比較のためだけの参考例を区別します。

## 実際の画面操作手順
1. /learn/34を開き初期文言「まだ実行していません。」を確認する。
2. DB未設定でボタンを押し未設定の日本語案内が出ることを確認する。
3. 開発専用DBとDDLを用意した場合だけ実検索を実施する。
4. 正常なら「DBから最大20件を取得しました。」と表示され、最大20件という上限を理解する。
正常系では説明用の結果と実DB操作の結果を分けて記録します。失敗例では設定未入力、サーバーからDBへの接続不能、DDL不一致、業務認可拒否、更新競合を同じエラーと見なさず、現象から原因を分類してください。

## 障害の切り分け表
| 現象 | 確認する場所 | 対処 |
| --- | --- | --- |
| 未設定の案内 | Configured条件と環境変数 | 開発用DB設定を確認 |
| DBへ接続できません | ネットワーク・ロール・DDL・ログ | 画面に資格情報を出さない |
| 並列実行でDbContext例外 | 同じContextの共有 | Factoryで操作ごとに分離 |

## 演習問題と詳しい解答
**問1：** Scoped DbContextをcircuit中使い回してよいか。**詳しい解答：** DbContextはスレッドセーフではなく状態も残るので、操作ごとにFactoryから生成します。**問2：** await using後はTCP接続が必ず切れるか。**詳しい解答：** いいえ。Npgsqlの物理接続プールで再利用される場合があります。**問3：** 50件あるのに20件しか取得しなければ障害か。**詳しい解答：** Take(20)の仕様であり、総件数とは区別します。

## 次のステップへの接続
次は第10組の復習で、方式選定、Entity/DTO、DDL、DI、操作単位のContextを一つのデータアクセス経路として説明します。その後の051でLINQ検索を深めます。
