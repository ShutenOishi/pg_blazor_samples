# 第16組の復習：学習順076〜080

## 期間・キャッシュ・集計・性能・選定を横断する

WinForms＋Dapperの経験を持つ学習者は、今回の5つのステップでWeb画面のイベント、サービス、短命DbContext、PostgreSQL18を同じ順序で追ってください。Blazor Interactive Serverの画面表示が成功しても、本番業務の本人確認と権限付与が終了したわけではありません。特に、日付境界、Scopedの生存範囲、CSVの上限と拒否、P95/P99の標本数、一覧方式の取得件数を区別します。

| 学習順 | 実装と主題 | 避ける誤解 |
| --- | --- | --- |
| 076 | RangeSearchPageとLearningRangeExportServiceのOverlaps | 終了日を含む画面と半開区間を混同 |
| 077 | CacheBoundaryPageとLearningCacheLessonService | 架空A/Bと権限版を本人認証と思う |
| 078 | StatisticsExportPageのGroupBy/CountとCSV | 101件でも100件だけ出せば全件と思う |
| 079 | PerformancePageのEXPLAIN・20回のP95/P99 | 小標本を本番SLOの保証とする |
| 080 | PatternChoicePageの10方式 | Gridの非表示を認可とする |

## 5画面を順番に試す操作手順

1. /learn/56では10月1〜3日の両端を含む指定を入力します。PostgreSQLの範囲型は10月4日を含まない上端となり、AvailableDatesの重なりで判定します。DueDateは表示用の単一日付です。20行まで表示してもCountはそれを超え得ます。
2. /learn/57で架空A版1を二回読みます。同じcircuitでは二回目がキャッシュヒットです。B版1は別キーです。権限版の増加は旧エントリの物理削除ではなく、Clearが辞書を破棄します。
3. /learn/58ではGroupByをSQL側で実行してから小さな集計結果へ射影します。CSVは最大101件目を確認し、101件なら出力を拒否します。文字列の先頭数式記号と引用符を安全に変換します。
4. /learn/59では開発用DBを20回読み取り、並び替えた19番目と20番目をP95/P99とします。EXPLAINの推定計画は同じものを実行した実測と区別し、少数測定を性能保証にしません。
5. /learn/60では50件固定のItems、大量複数列ソートのGrid ServerData、順次読込のキーセット、DB取得つき仮想化、DB側集計と制限付きCSVを選びます。技術を一つだけで完結させず、DTO、Count、安定ソート、認可を必要に応じて組み合わせます。

## WinForms＋Dapperから見た違い

| 観点 | WinForms＋Dapper | Blazor＋EF Core 10 |
| --- | --- | --- |
| イベント | クライアントの画面イベント | Webサーバー上のRazorイベント |
| 一覧 | DataGridViewへ取得結果を渡す | Items、ServerData、仮想化の取得方式を区別 |
| 検索 | SQLパラメーターを手で指定 | DateOnlyとNpgsqlRangeをLINQに渡す |
| 集計 | COUNTやGROUP BYをSQLで実行 | 同じSQLをGroupBy/Countで生成する |
| キャッシュ | フォーム内や共有フィールド | circuit内Scopedと権限版の関係を考える |
| 実行計画 | EXPLAINとStopwatch | SQL推定計画・サーバー応答・ブラウザー体感を区別 |
| 認可 | 既存業務台帳で行・操作を照合 | 同じ正本をサービスの全経路で都度照合 |

## 実コード例1：期間の重なりとCount

```csharp
var range = LearningRangeExportService.CreateInclusiveRange(from, to);
var query = db.Tasks.AsNoTracking().Where(t => t.AvailableDates.HasValue
    && t.AvailableDates.Value.Overlaps(range));
var total = await query.CountAsync(ct);
var rows = await query.OrderBy(t => t.Id).Take(20)
    .Select(t => new { t.Id, t.Title, t.DueDate }).ToListAsync(ct);
```

Countは同じ検索条件に合う件数で、rowsは20行までのDTOです。UIの終了日を含む条件は翌日を含まない上端へ変換しています。本番の一覧と件数では同じ行単位認可を適用し、未許可の件数が漏れる経路を作りません。期間がNULLなら対象外となり、DB未設定なら失敗を成功と呼ばないでください。

## 実コード例2：CSV上限と拒否

```csharp
var rows = await query.OrderBy(t => t.Id).Take(101)
    .Select(t => new { t.Id, t.Title, t.DueDate, t.IsDone })
    .ToListAsync(ct);
if (rows.Count > 100)
    return new LearningCsvResult(null, rows.Count, "101件以上のため出力を中止しました。");
// 実コードではBOM、引用符、式注入へのEscapeCsvも使用する。
```

100件だけ出して全件と見せることはしません。101件目が存在したら全件出力を中止し、条件を絞る案内を返します。小規模なdata URLと大規模な本番ダウンロードを分け、業務用の出力では既存台帳認可、監査、容量制限を実装します。前回成功したダウンロードリンクを失敗後に残さないことも確かめます。

## 障害の切り分け

| 現象 | 原因の候補 | 確認方法 |
| --- | --- | --- |
| 終了日の行が出ない | daterangeの上端が誤っている | DateOnlyと翌日上端を確認 |
| NULL期間の行がない | AvailableDates.HasValueの条件 | DueDateとは別列だと確認 |
| 別利用者の結果が出る | キャッシュキーが不適切 | 架空A/Bと版・Clearを確認 |
| CSV101件が拒否される | 100件上限の設計どおり | エラーを成功表示しない |
| 数式として開かれる | EscapeCsv不足 | 等号などと引用符を確認 |
| P99が大きい | 20標本の最大値や接続差 | SLO受入とは区別する |
| 部署外の行が非表示 | UIだけの制御 | DBの行認可Whereを確認 |

## 演習と詳しい解答

**演習1：** 数百万行の課題一覧に、期間指定、次ページへの移動、部署別件数、CSVが必要です。どの方式を組み合わせますか。

**詳しい解答1：** 日付はDateOnlyからNpgsqlRangeの半開区間へ変換し、次ページだけの移動なら一意キーによるキーセットを使います。件数は台帳で認可した範囲のDB側GroupBy/Count、CSVは容量上限と監査を備えた別経路で扱います。全件Items、深いOFFSET、画面で隠すだけの認可を避けます。CountとItemsで許可範囲が違うと件数が漏えいします。

**演習2：** A・版1を二度読むとキャッシュヒットになります。実際のADユーザーでも同じ値を本人・台帳確認なしで返してよいでしょうか。

**詳しい解答2：** 返してはいけません。A/Bは教材だけのラベルです。ADは本人とユーザー名を識別し、業務の許可は既存DB/外部台帳を正本に操作・対象行ごと都度照合します。失効や監査失敗、xmin競合、FDWの原子性が不明な場合には安全拒否します。Scopedキャッシュは業務認可ではありません。

**演習3：** 20回の測定で19番目が30ms、20番目が100msのときのP95/P99を答え、本番受入を判断してください。

**詳しい解答3：** 最近傍順位法では30msと100msです。小標本なので本番のP99保証値とは言えません。開発用PostgreSQL18とChrome/xUnitのCIだけで、実AD/IIS/Kerberos/FDW、監査、1万人超負荷は受入できません。

## 本番適用と次の学習

これは開発用の架空課題による学習です。WinForms＋Dapperで実装してきた業務認可をWebへ移す場合、AD本人認証だけに頼りません。既存DB/外部台帳を正本として、Count、Items、詳細、CSV、更新の各経路で操作と行の認可を都度判定します。権限行ロックとxmin、監査の永続化を確認し、権限失効や監査障害、FDW原子性が不明な場合は安全拒否します。Windows Server2025/IIS、Kerberos、Data Protection鍵、1万人超の性能は別の実機受入です。次は学習順081〜085でWebセキュリティの基礎を学びます。
