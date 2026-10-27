# 学習順047：モデルとDTOを理解する

## 到達目標・前提知識
DBの行を表すLearningTaskと、画面へ渡すLearningTaskRowを区別します。前者はEF CoreのEntityで、Id、Title、DueDate、StartTime、AvailableDates、IsDone、UnitCode、CategoryId、Category、Versionを持ちます。後者は一覧用のrecordで、Id、Title、DueDate、StartTime、IsDone、Versionに絞っています。WinForms＋DapperでSELECTの列をPOCOへ格納する経験は、そのままDTOの理解に使えます。
この章のRazorは固定のLearningTaskRowを表示するためDB接続がなくても動きます。固定サンプルが画面に表示された事実と、PostgreSQLに保存された事実を混同しません。
前提は第4段階で学んだRazorイベント、DI、MudBlazorの日本語UIです。この章は既存80ステップのURLを維持したまま、全110本の学習順に従って読みます。必要に応じて開発用PostgreSQL 18を使用します。

## 仕組み・処理の流れ
id bigintはlong、title varchar(100)はstring、dateはDateOnly?、timeはTimeOnly?、booleanはboolに対応します。daterangeはNpgsqlRange<DateTime>?で保持し、NULL許容と範囲端点の包含・除外を明示します。xminはuintのVersionとして扱い、DBが更新する競合トークンなので画面から採番しません。
DTOを分ける目的は単なる別名付けではありません。不要な内部列を返さない、一覧で必要な列をSQLで投影する、表示に使う状態とDBの追跡状態を混在させない、という具体的な効果があります。EF CoreのAsNoTrackingで読み取ったDTOを変更してもSaveChangesで自動保存されません。更新時にはサービスで対象Entityを再取得し、権限・入力・競合を判定して保存します。画面にVersionを渡しても、それ自体が認可の証明になるわけではありません。
部署キーUnitCodeを表示する必要がない場合、DTOに含めるべきではありません。一方、DTOから列を除くだけで部署外の行へのアクセスを防げるわけでもありません。クエリのWHERE条件として部署と行権限をサーバーで適用します。

## WinForms＋Dapperとの具体的な比較
| 比較の観点 | 従来の実装 | Web＋EF Core | 判断する点 |
| --- | --- | --- | --- |
| 検索行の型 | SELECT結果をPOCOへ格納 | EntityまたはDTOへ投影 | 画面には必要列だけ |
| 日時の型 | DB型をADO.NETで変換 | DateOnly?/TimeOnly?をマッピング | nullと時差を混同しない |
| 更新版 | WHEREに版条件を記述 | xminをVersionとして扱う | 競合と認可を別に判定 |

## 実装に対応するコード例1
```csharp
// Data/LearningTask.csの主要なEntity
public sealed class LearningTask {
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public DateOnly? DueDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public NpgsqlRange<DateTime>? AvailableDates { get; set; }
    public bool IsDone { get; set; }
    public string UnitCode { get; set; } = "DEMO";
    public uint Version { get; set; } // xmin
}
```
このコードは実装の一部分、または比較用と明記した参考例です。画面がどこまで動作するかを区別して確認します。

## 実装に対応するコード例2
```csharp
// 同ファイルの一覧DTOとStep31画面の固定値
public sealed record LearningTaskRow(
    long Id, string Title, DateOnly? DueDate,
    TimeOnly? StartTime, bool IsDone, uint Version);
private readonly LearningTaskRow _sample = new(
    1, "EF Coreのモデルを学ぶ", new DateOnly(2026, 10, 26),
    new TimeOnly(9, 0), false, 0);
```
表示用DTO、SQLの型、短命DbContext、DI登録などの役割は異なります。コードが示す操作の対象、寿命と副作用を説明してから次へ進んでください。

## Webに移ったときの基本的な境界
この教材ではBlazor Interactive Serverを使います。画面に表示されるHTMLやクリック操作の入口はブラウザーにありますが、RazorイベントのC#、DBサービス、EF Core、NpgsqlはWebサーバーで動きます。WinFormsではフォームのイベントが利用者PC上のプロセスで実行されていました。Webでは複数利用者の操作がサーバーに集まるため、画面に保持した状態とDB接続の寿命を同一視してはいけません。画面で固定の架空データが表示できたこと、DIの設定が読み取れたこと、SQLのテーブル定義が表示できたことは、それぞれ実DBへの接続が成功した証拠ではありません。
業務用のデータアクセスには別の条件があります。ADは本人認証とドメイン付きユーザー名の取得に用い、操作対象の部署・行範囲・権限は事前に整備されたDBまたは外部台帳を正本として毎操作サーバー側で判定します。データベース接続に付けるapplication_nameはアプリケーションの識別補助であって利用者別監査の代替ではありません。監査ができない更新、競合している更新、権限失効を確認できない操作は安全に拒否します。学習用の匿名データサービスを業務認可の済んだサービスとして流用しません。実AD、IIS、Kerberos、FDW、1万人以上の実機負荷は教材の模擬試験と分けて評価します。
開発環境ではVisual Studio 2026、.NET 10、EF Core 10とNpgsql、専用PostgreSQL 18を用います。DBが必要な操作だけ専用環境を構成し、業務DBや実在利用者データを練習対象にしないでください。接続情報にパスワードが含まれる場合はGitへ記録しません。コード例は既存実装の主要部分を抜粋したものと、比較のためだけの参考例を区別します。

## 実際の画面操作手順
1. /learn/31を開き、架空のId・課題名・日付・完了表示を見る。
2. LearningTaskとLearningTaskRowを見比べ、画面のDTOにない列を挙げる。
3. 日付は固定の2026/10/26でありDBから来たわけではないと説明する。
4. 学習順046へ戻り、次へで048のDDL説明に進む。
正常系では説明用の結果と実DB操作の結果を分けて記録します。失敗例では設定未入力、サーバーからDBへの接続不能、DDL不一致、業務認可拒否、更新競合を同じエラーと見なさず、現象から原因を分類してください。

## 障害の切り分け表
| 現象 | 確認する場所 | 対処 |
| --- | --- | --- |
| 業務日が空に見える | DueDateがnullか画面の固定サンプルか確認 | SQL結果とUI固定値を分ける |
| 列名が一致しない | HasColumnNameとDB定義を確認 | Data/LearningDbContextを参照 |
| DTOの変更が保存されない | DTOは変更追跡されない | サービスで更新対象を再取得 |

## 演習問題と詳しい解答
**問1：** DTOのIsDoneだけ書き換えるとDBが更新されるか。**詳しい解答：** 更新されません。表示recordは追跡されたEntityではなく、更新サービスでDbContextを生成しSaveChangesする必要があります。**問2：** UnitCodeをDTOから省けば部署外参照も防げるか。**詳しい解答：** いいえ。行絞り込みと操作権限をDB台帳に基づき都度判定します。**問3：** xminを業務の監査履歴番号に使えるか。**詳しい解答：** 使いません。xminはDBが管理する競合検出に用い、監査は別途永続的な記録として設計します。

## 次のステップへの接続
次の048では、上記のプロパティに対応するPostgreSQL 18のDDLを読みます。モデルと物理テーブルが一致しない場合は、画面表示だけ成功してもDB操作で失敗します。
