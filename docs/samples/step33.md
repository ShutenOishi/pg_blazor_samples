# 学習順049：EF Core 10をWebへ登録する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L049EfSetupPage.razor)。画面の正規URLは `/learn/049` です。

## 到達目標・前提知識
WebプロジェクトのProgram.csにNpgsql.EntityFrameworkCore.PostgreSQL 10.0.3を使うDbContextFactoryを登録します。WinFormsのフォームごとに接続文字列を組み立てる構成と異なり、ASP.NET CoreのConfigurationから起動時に設定を取得します。画面ではDB接続先が設定されているかを表示しますが、表示内容は実ネットワーク接続の成功を保証しません。
別のDomain/Infrastructureプロジェクトは増やさず、Program.csで登録し、Data/LearningDbContext.csでマッピングし、Services/Learning/LearningDbService.csから呼びます。Razorに資格情報やSQLの責務を置きません。
前提は第4段階で学んだRazorイベント、DI、MudBlazorの日本語UIです。この章は既存80ステップのURLを維持したまま、全110本の学習順に従って読みます。必要に応じて開発用PostgreSQL 18を使用します。

## 仕組み・処理の流れ
ConnectionStrings__PostgreSQLの二重アンダースコアは階層設定の区切りです。接続先未設定の場合もProgram.csでは開発用の代替文字列でFactoryを構成しますが、LearningDbService.ConfiguredがfalseならDB操作を開始しません。configured=trueでも、ホストの名前解決、ポート、ロール、パスワード、SQLの存在が正しいかは実際の操作まで分かりません。
NpgsqlConnectionStringBuilderでApplicationNameをPgBlazorSamples.Webに固定します。利用者ごとに接続文字列を変えてプールを分断しないための設定ですが、個人別監査記録の代用にはなりません。接続資格情報は環境変数等で管理し、画面やログへ漏らしません。
OnModelCreatingではToTable、HasColumnName、HasColumnType、HasMaxLength、IsRowVersionなどを使用します。DDLに存在しない列をマッピングすれば実クエリが失敗します。またWindowsAuthentication有効時は教材用の無認可DB経路を使用できないようConfiguredをfalseにしています。業務アクセスは専用のSecuredLearningServiceで都度認可します。

## WinForms＋Dapperとの具体的な比較
| 比較の観点 | 従来の実装 | Web＋EF Core | 判断する点 |
| --- | --- | --- | --- |
| 接続文字列 | フォームで接続生成しがち | 起動時にConfigurationから取得 | Gitへ資格情報を保存しない |
| モデル | SELECTとPOCOを対応 | OnModelCreatingに列の対応を設定 | 実DBの定義と照合 |
| 寿命管理 | 操作ごとにusing connection | FactoryをDIから受け取る | DbContextは次の画面で都度生成 |

## 実装に対応するコード例1
```csharp
// Program.csの実装抜粋
var pgConnection = builder.Configuration.GetConnectionString("PostgreSQL");
var pgApplicationConnection = new Npgsql.NpgsqlConnectionStringBuilder(
    string.IsNullOrWhiteSpace(pgConnection)
        ? "Host=127.0.0.1;Database=pg_blazor_learning;Username=pg_blazor_learning;Timeout=2"
        : pgConnection) { ApplicationName = "PgBlazorSamples.Web" };
builder.Services.AddDbContextFactory<LearningDbContext>(options =>
    options.UseNpgsql(pgApplicationConnection.ConnectionString));
builder.Services.AddScoped<LearningDbService>();
```
このコードは実装の一部分、または比較用と明記した参考例です。画面がどこまで動作するかを区別して確認します。

## 実装に対応するコード例2
```csharp
// Data/LearningDbContext.csの主なマッピング
var task = modelBuilder.Entity<LearningTask>();
task.ToTable("learning_task");
task.HasKey(x => x.Id);
task.Property(x => x.Title).HasColumnName("title")
    .HasMaxLength(100).IsRequired();
task.Property(x => x.DueDate).HasColumnName("due_date")
    .HasColumnType("date");
task.Property(x => x.Version).IsRowVersion(); // xmin
```
表示用DTO、SQLの型、短命DbContext、DI登録などの役割は異なります。コードが示す操作の対象、寿命と副作用を説明してから次へ進んでください。

## Webに移ったときの基本的な境界
この教材ではBlazor Interactive Serverを使います。画面に表示されるHTMLやクリック操作の入口はブラウザーにありますが、RazorイベントのC#、DBサービス、EF Core、NpgsqlはWebサーバーで動きます。WinFormsではフォームのイベントが利用者PC上のプロセスで実行されていました。Webでは複数利用者の操作がサーバーに集まるため、画面に保持した状態とDB接続の寿命を同一視してはいけません。画面で固定の架空データが表示できたこと、DIの設定が読み取れたこと、SQLのテーブル定義が表示できたことは、それぞれ実DBへの接続が成功した証拠ではありません。
業務用のデータアクセスには別の条件があります。ADは本人認証とドメイン付きユーザー名の取得に用い、操作対象の部署・行範囲・権限は事前に整備されたDBまたは外部台帳を正本として毎操作サーバー側で判定します。データベース接続に付けるapplication_nameはアプリケーションの識別補助であって利用者別監査の代替ではありません。監査ができない更新、競合している更新、権限失効を確認できない操作は安全に拒否します。学習用の匿名データサービスを業務認可の済んだサービスとして流用しません。実AD、IIS、Kerberos、FDW、1万人以上の実機負荷は教材の模擬試験と分けて評価します。
開発環境ではVisual Studio 2026、.NET 10、EF Core 10とNpgsql、専用PostgreSQL 18を用います。DBが必要な操作だけ専用環境を構成し、業務DBや実在利用者データを練習対象にしないでください。接続情報にパスワードが含まれる場合はGitへ記録しません。コード例は既存実装の主要部分を抜粋したものと、比較のためだけの参考例を区別します。

## 実際の画面操作手順
1. /learn/049で接続設定の有無を確認する。
2. 開発用にConnectionStrings__PostgreSQLを設定して再起動する。
3. 設定済みという表示と実DBへの接続は別と説明する。
4. Program.cs、Data、Servicesの3か所を読み、資格情報をGitに含めないと確認する。
正常系では説明用の結果と実DB操作の結果を分けて記録します。失敗例では設定未入力、サーバーからDBへの接続不能、DDL不一致、業務認可拒否、更新競合を同じエラーと見なさず、現象から原因を分類してください。

## 障害の切り分け表
| 現象 | 確認する場所 | 対処 |
| --- | --- | --- |
| DB未設定と表示 | ConnectionStrings__PostgreSQLが空 | 開発用設定を確認 |
| 設定済みだが接続不能 | ホスト、ポート、DB、ロール、DDL | /learn/050で実操作 |
| 列の存在エラー | OnModelCreatingとDDL不一致 | 実DBの列定義と照合 |

## 演習問題と詳しい解答
**問1：** Configured=trueならSQLが実行されたか。**詳しい解答：** いいえ。設定の有無を表すだけです。**問2：** application_nameに個人名を入れれば監査が成立するか。**詳しい解答：** いいえ。接続プールの分断を招き得るうえ、本人・対象・操作・結果を保証しません。**問3：** DI登録を画面ごとに行うか。**詳しい解答：** 起動時に登録し、各サービスがFactoryを受け取ります。

## 次のステップへの接続
次の050でFactoryからDbContextを生成して実DBの検索を試します。接続設定の表示だけを成功の証拠にしない姿勢を継続してください。
