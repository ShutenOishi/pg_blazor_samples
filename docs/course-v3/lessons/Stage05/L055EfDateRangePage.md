# 学習順055：DateOnly・TimeOnly・Rangeの型変換

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage05/L055EfDateRangePage.razor)。画面の正規URLは `/learn/055` です。

## 到達目標と前提
MudDatePickerはDateTime?、学習用テーブルのdateはDateOnly?、time without time zoneはTimeOnly?、daterangeはNpgsqlRange<DateTime>?で扱います。EfDateRangePageではja-JP形式で日付を選び、DateOnly.FromDateTimeで日だけに変換します。期間は開始日を含み、翌日の開始を含まない[当日,翌日)です。NpgsqlRange<DateTime>(start,true,endExclusive,false)の第4引数をfalseにするのが重要です。2引数形式で上限を含むと、PostgreSQLの日付範囲が正規化されて終端がずれる場合があります。DateOnly.MaxValueの翌日は作れないためAddDaysの前に拒否します。空の日付も拒否します。ここでは値の表示だけでDBに保存せず、datemultirangeや無限端の実DB検証とも分けます。
学習順055は第5段階・第11組に含まれます。互換URLは変更せず、画面は `/learn/055` です。Visual Studio 2026と.NET 10、EF Core 10・Npgsql・PostgreSQL 18の構成を前提にします。前のページで学んだFactoryの役割を使い、処理がブラウザー、Webサーバー、DBのどこで行われるのかを説明してください。

## WinForms＋Dapperと実装を比較する
| 観点 | WinForms＋Dapper | Blazor Server＋EF Core | 注意すること |
| --- | --- | --- | --- |
| date | DateをSQLへ指定 | DateOnly?の列 | 日時を混同しない |
| time | time without time zone | TimeOnly? | 時刻だけの値 |
| daterange | [開始,終了)の境界 | NpgsqlRangeのinclusive指定 | 上限exclusive |
| multirange | 集合演算のSQL | この画面では未実行 | 実DBは別試験 |

## ソースコード例1：中心の処理
```csharp
if (_day is null) { _result = "日付を選択してください。"; return; }
DateOnly date = DateOnly.FromDateTime(_day.Value);
if (date == DateOnly.MaxValue) {
    _result = "終了日の翌日を作れません。"; return;
}
var period = new NpgsqlRange<DateTime>(
    date.ToDateTime(TimeOnly.MinValue), true,
    date.AddDays(1).ToDateTime(TimeOnly.MinValue), false);
```
上記は実装の主要部分を抜き出した例です。補助的なConfigured判定、入力検証や例外処理もソース全体で確認します。接続しているコードと、メモリ上の状態またはSQL文字列だけを扱うコードを分けてください。

## ソースコード例2：画面／対応機構とDapperの違い
```csharp
// LearningDbContext.csとLearningTask.csのモデル
public DateOnly? DueDate { get; set; }
public TimeOnly? StartTime { get; set; }
public NpgsqlRange<DateTime>? AvailableDates { get; set; }
// OnModelCreating
task.Property(x => x.DueDate).HasColumnType("date");
task.Property(x => x.StartTime).HasColumnType("time without time zone");
task.Property(x => x.AvailableDates).HasColumnType("daterange");
```
実際の構成ではRazor画面が入力とイベントを担当し、サービスが操作を実装します。比較用に示したコードが学習画面で実行されるとは限りません。どのメソッドを呼んだか、DBへの副作用があるかを必ず確認します。

## 実行場所・アプリケーション境界
WinFormsでボタンを押すとフォームイベントのC#が通常は利用者PC上で動き、Dapperに渡したSQLはアプリからDBへ送られます。Blazor Interactive ServerではRazorのC#イベントとEF Coreの処理はWebサーバーで動きます。ブラウザーはユーザー操作とHTML表示を受け持ち、直接PostgreSQLへ接続するわけではありません。NetworkタブにWebSocket通信が見えても、その通信がDB SQLを公開しているわけではありません。UIに表示された文字列はサービスが返した結果であり、成功の根拠をDB操作の有無までさかのぼって区別する必要があります。

この教材はWebプロジェクトのData/とServices/だけを使います。学習のためだけにDomain・Infrastructure・Repositoryの別プロジェクトを増設しません。IDbContextFactoryから必要な操作ごとにDbContextを作り、await using等で破棄します。DbContextをBlazorのcircuit全体で長期間共有しません。Npgsqlの物理接続プールとEF CoreのDbContextオブジェクトは寿命が別であることも意識してください。読み取りと変更追跡と保存はそれぞれ独立した概念です。SQLを生成しただけで検索実行を保証することも、追跡状態がAddedになっただけでINSERT完了と扱うこともできません。

## 安全条件と検証範囲
教材でDBを使うときは開発専用PostgreSQL 18、架空データ、専用DDLを使用します。ConnectionStrings__PostgreSQLの値や認証情報はGitに書かず、業務DB・実在利用者情報を演習へ持ち込みません。接続先の設定があることは実DB接続成功ではなく、SQLが生成できることもデータ取得成功ではありません。未設定と正常0件、入力不備、接続障害、更新競合、権限不足は別々の状況です。エラーの詳細はサーバーログから調べ、機密の接続文字列やスタックトレースを利用者へ返しません。

ADからは本人認証とドメイン付きユーザー名だけを取得する前提です。実業務の認可は、事前整備済みDB台帳または外部ビューの最新情報で、各業務操作・部署・対象行ごとにサーバー側で判定します。検索一覧とCount、詳細、CSV、更新のすべてで適用し、画面でボタンを隠すだけにしません。監査記録、権限行ロック、更新競合を同一トランザクションで保証できない操作は安全側に拒否します。FDWなどで原子性が証明できなければ更新を禁止します。application_nameは接続識別の補助であって利用者別の監査証跡ではありません。ここでのLearningDbServiceは認可を組み込んだ本番業務サービスではありません。実AD・IIS・Kerberos・FDW・1万人規模実機の成功を学習用CIから推測しないでください。

## 理解を確かめる確認の順番
最初に画面URLと初期状態を確認し、次に入力とイベント、続けて呼ばれるサービス、DbContextの生成・破棄、DBコマンドの有無、最終的な画面メッセージの順に調べます。問題が起きたら原因を飛ばして再実行するのではなく、未設定・入力検証・SQL生成・接続・DDL・権限・競合のどこまで進んだかを記録します。WinForms＋Dapperの経験を使い、SQLと処理場所を対比することで、同じC#でもWebサーバーと利用者端末では実行位置が違うことを確認してください。

## 実際の操作手順
1. /learn/055で日本語日付を選ぶ。
2. 『DB用に変換』をクリックする。
3. DateOnly表示と[開始,翌日)を確かめる。
4. 最大日付・nullで拒否する理由を説明する。
各手順の完了後には日本語の状態表示を読み、未接続・未設定の状態を成功として扱わないでください。WinFormsのダイアログに慣れていても、サーバーログやDB統合試験とブラウザー画面の証跡を混ぜないことが重要です。

## 障害の切り分け表
| 現象 | 調べる項目 | 判断と対応 |
| --- | --- | --- |
| 日付が選択されていない | _dayがnull | 変換を拒否 |
| 翌日を作れない | DateOnly.MaxValue | AddDays前に拒否 |
| 終端が一日進む | UpperBoundIsInclusive | falseを明示 |
| DBに保存されない | 画面は変換だけ | 正常、DB別試験 |

## 演習と詳しい解答
**問1：翌日を含む範囲か**

**詳しい解答：** 違います。上限を含まない半開区間です。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問2：MaxValueへAddDays(1)を行うか**

**詳しい解答：** 事前に拒否してから計算を止めます。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

**問3：画面で変換できればDBへの書込み成功か**

**詳しい解答：** 違います。SaveChangesがなく、保存試験は別です。 この結論を画面、サービス、PostgreSQLのどこで確認するかも答え、正常な拒否と操作成功を分けて記録してください。

## 次へ進む前の確認
本教材だけを読んでも、入力、処理場所、戻り値、保存の有無、失敗時の対応を説明できるようにしてください。次は `/learn/column/db-bridge-55` に進みます。現在はPDFの生成・再生成・Preview/正式Release更新を停止しており、このMarkdownとRazor、索引ソースだけが更新対象です。
