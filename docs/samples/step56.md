# 学習順076：日本語日付範囲とDB検索

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step56/RangeSearchPage.razor)。画面の正規URLは `/learn/076` です。

**学習順076・第6段階／第16組・旧URL `/learn/076`**

## 学習目標と実行環境

WinForms＋Dapperで開発してきた学習者が、初めてWebのBlazor Interactive Serverで操作する想定です。Visual Studio 2026、.NET 10、MudBlazor、EF Core 10、Npgsql、開発用PostgreSQL18を前提にします。今回の主題は日付境界・Overlapsです。ブラウザーの入力とWebサーバーのC#、SQLが実行されるPostgreSQLの責任を切り分け、サーバー側の結果だけを根拠に表示を更新します。操作を繰り返す場合の二重送信、回線切断、取消も設計に含めます。

## 実Razor/C#/SQLでの処理を追う

MudDateRangePickerのDateRange.Start/EndをDateOnlyへ変換し、画面で両端を含む指定をPostgreSQL daterangeの半開区間へ直します。10月1日から3日なら上端は10月4日を含まない値です。これを10月3日00:00未満にすると最終日を検索できません。DueDateは単一の日付、AvailableDatesはdaterangeであり、検索するのは後者です。

CreateInclusiveRangeは逆転する日付、DateOnly.MaxValue、差が366日を超える指定を拒否します。FindOverlapsAsyncはNULL期間を除外してOverlapsで検索し、同じ条件でCountAsyncを実行し、Id順に最大20行を取得します。21件一致しても表示は20件で総件数は21件です。設定がないDBを空結果や成功と混同しません。

RangeSearchPageは日付の未選択時に検索を始めず、入力エラーなら表示行を空にします。実業務でRowLimitを設けても、CountとItemsに同じ本人・部署・操作・行単位の認可Whereが必要です。開始日と終了日の検査は画面だけに頼らずサービス側でも実施し、境界テストを開発用DBで行います。

サービスと画面を照合するときは、UIの成功表示から逆方向に追うのではなく、入力値を受けた順に確認してください。Razorのイベントハンドラー、サービスが検査する境界値、IDbContextFactoryで生成する短命DbContext、DBへ送信する絞り込み、返却するDTO、画面の状態メッセージを記録します。複数の利用者が同時にアクセスできるWebでは、ブラウザー側の状態を業務上の正本と扱わず、サーバーで必要な検証を繰り返します。

MudDateRangePickerのDateRange.Start/EndをDateOnlyへ変換し、画面で両端を含む指定をPostgreSQL daterangeの半開区間へ直します。10月1日から3日なら上端は10月4日を含まない値です。これを10月3日00:00未満にすると最終日を検索できません。DueDateは単一の日付、AvailableDatesはdaterangeであり、検索するのは後者です。

同じ画面を別タブで開いて操作した場合、古い結果や取得済みの項目が残ることがあります。操作を続ける前に対象データの状態を確認します。Web画面を正しく描画できたこと、データを読み込めたこと、権限を持つこと、更新が確定したことを同じ成功条件にまとめないでください。エラー表示の文面だけを見ず、サービスの戻り値とログを照合します。

## WinForms＋Dapperとの比較

| 観点 | WinForms＋Dapper | Blazor＋EF Core 10 |
| --- | --- | --- |
| イベント処理 | 端末のフォームイベント | Interactive ServerのサーバーC# |
| 入力 | TextBoxやDateTimePicker | Razorで双方向バインドしたMudBlazor |
| 検索 | パラメーター付きSQLを明示する | LINQからSQLへ翻訳し必要な列をDTOへ射影 |
| DB接続 | NpgsqlConnectionをusingで保持 | IDbContextFactoryから短命DbContext |
| この教材 | 日付境界・OverlapsをSQLと画面で表現 | 学習用サービスとRazorで処理を分離 |
| 大量データ | DataGridViewへ全件読み込まない工夫 | Count・Itemsと表示上限、取得方式を区別 |
| 認可 | 既存台帳の操作・行単位のSQL条件 | サービスの全経路で同じ台帳を都度照合 |
| エラー | 例外、取消、空結果を区別 | 状態メッセージとDBの確定を区別 |

## 実コード例1：日付境界・Overlapsの主要処理

```csharp
public static NpgsqlRange<DateTime> CreateInclusiveRange(DateOnly from, DateOnly to)
{
    if (from > to) throw new ArgumentException("開始日は終了日以前にしてください。");
    if (to == DateOnly.MaxValue) throw new ArgumentOutOfRangeException(nameof(to));
    if (to.DayNumber - from.DayNumber > 366)
        throw new ArgumentOutOfRangeException(nameof(to));
    return new NpgsqlRange<DateTime>(from.ToDateTime(TimeOnly.MinValue), true,
        to.AddDays(1).ToDateTime(TimeOnly.MinValue), false);
}
```

上のコードは既存サービス／Razorの処理を学習用に抜き出しています。入力検査は画面で一回行えば足りるものではなく、サービス側でも拒否条件を再確認します。UIの値が正しい形式でもアクセスしてよい対象とは限りません。実業務では行・操作の認可をDB台帳で検査する処理を別途実装する必要があります。

## 実コード例2：問い合わせと結果の判定

```csharp
var q = db.Tasks.AsNoTracking().Where(t => t.AvailableDates.HasValue
    && t.AvailableDates.Value.Overlaps(range));
var count = await q.CountAsync(ct);
var rows = await q.OrderBy(t => t.Id).Take(SearchRowLimit)
    .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
        t.StartTime, t.IsDone, t.Version)).ToListAsync(ct);
```

取得件数と表示件数、検索の成功と空結果、入力拒否とDB障害を分けて説明してください。SQLが生成される箇所とC#のメモリ内で処理される箇所を区別し、クエリの実行時点も確認します。例外を成功件数に変換したり、前の操作で得た結果を失敗後も最新として表示したりしないことが条件です。

## 操作手順と確認方法

1. まず学習用PostgreSQL18の設定状態を確認し、実運用DBや本番台帳を接続先として使っていないことを確かめます。
2. `/learn/076` を開き、入力欄、選択肢、操作ボタン、状態表示を確認します。ホームの学習順076からも遷移できることを確かめます。
3. 正常な架空データを用意して操作を一度実行し、Razorから呼び出されたメソッドと戻り値を記録します。表示された行や件数の意味を確認します。
4. 同じ操作を繰り返し、重複クリックや以前の表示結果が残る条件を調べます。結果の取り消しや再実行が必要かを検討します。
5. CreateInclusiveRangeは逆転する日付、DateOnly.MaxValue、差が366日を超える指定を拒否します。FindOverlapsAsyncはNULL期間を除外してOverlapsで検索し、同じ条件でCountAsyncを実行し、Id順に最大20行を取得します。21件一致しても表示は20件で総件数は21件です。設定がないDBを空結果や成功と混同しません。
6. 例外と設定なしを実行し、成功扱いしない日本語案内を確認します。ステップの最後に対応するC#サービスの返却値と照合します。

## 障害の切り分け

| 症状と否定試験 | 確認する箇所 | 適切な判断 |
| --- | --- | --- |
| 開始・終了日のどちらかがNULL | 入力と形式検査 | 原因を切り分け成功と拒否を区別する |
| 開始日が終了日より後 | Razorが保持する状態 | 原因を切り分け成功と拒否を区別する |
| DateOnly.MaxValueで翌日を作れない | サービスの事前条件 | 原因を切り分け成功と拒否を区別する |
| AvailableDatesのNULL行 | LINQとSQLのWHERE・件数 | 原因を切り分け成功と拒否を区別する |
| 境界に隣接するだけの期間 | キャッシュ・版・索引・接続 | 原因を切り分け成功と拒否を区別する |
| 21件一致して表示上限は20行 | 障害ログと利用者への応答 | 原因を切り分け成功と拒否を区別する |
| 学習用DBが未設定 | LearningDbService.Configured | NotConfiguredを成功とみなさない |
| DB接続に失敗 | サービスの警告ログと結果 | 空結果として隠さず失敗を表示 |

診断は画面の文字だけで結論を出さず、サービスの正常系・拒否系を分けて調査します。旧URLを維持した状態でも、データ・アクセス権・通信状況が変わると同じ操作の結果が変わります。失敗した操作を無条件で再試行すると二重更新や古い表示の問題を引き起こすため、DBで確定した内容を確認してから復旧を判断します。

## 演習と詳しい解答

**演習1：** 10月1日から10月3日まで検索した場合、10月4日から開始するだけの期間は一致しますか。また25件一致した場合の総件数と画面の行数はそれぞれいくつですか。

**詳しい解答1：** 検索範囲は10月4日を含まないので、10月4日開始の期間と内部が重ならなければ一致しません。総件数は25、表示行は最大20です。単一日付のDueDateと期間型のAvailableDatesを区別する必要があります。

**演習2：** この学習用の画面から正常な結果が得られたことを理由に、実業務でのAD本人認証、既存台帳による行認可、監査まで実装済みと判断してよいでしょうか。

**詳しい解答2：** 判断できません。ADは本人とユーザー名を確認し、業務操作の許可と対象行の可視性は既存DB/外部台帳で都度判定します。Count、Items、詳細、CSV、更新の各経路を通じて同じ権限条件を強制します。更新前に権限・xminを再検証し、監査に失敗した場合やFDWの原子性が不明な場合は安全拒否します。ボタンが非表示であることは権限がないことの証明にも、権限があることの証明にもなりません。

## 本番業務への適用前に必要な判断

これは開発用の架空データを使う学習画面です。認証済みのADユーザー名を得ることと業務上の行・操作の許可を得ることは別です。利用者登録用の新しい表を勝手に作って認可の正本にせず、既存DBまたは外部台帳の有効利用者・操作権限・対象行をサービス側で都度照合します。表示・Count・詳細・CSV・更新で同じ権限条件を適用します。権限失効、xmin競合、監査の永続化失敗、FDWや別DBの原子性が証明できない場合は安全拒否します。画面のボタン非表示やフィルターは認可ではありません。

Windows Server 2025/IIS、実AD/Kerberos/SPN、証明書・鍵共有、接続切断、FDWの原子性、監査永続化、1万人超の実機負荷の受入は開発用CIとは別です。PostgreSQL18、xUnit、Chromeが成功しても現地の運用条件を満たすとは限りません。例外時に成功を返さない設計と、失敗を追跡できるログを保持し、機密データを学習用のログやキャッシュへ残さないことも確認します。

## 完了条件

- [ ] 旧URLと学習順076で同じ教材画面を開ける
- [ ] WinForms＋Dapperとの違いを比較表から説明できる
- [ ] 実コード2例の入力・SQL・結果・拒否を説明できる
- [ ] 正常・空結果・DB未設定・認可未実装を区別できる
- [ ] 開発用CIと実AD・IISの実機受入を混同しない

次は学習順077へ進みます。
