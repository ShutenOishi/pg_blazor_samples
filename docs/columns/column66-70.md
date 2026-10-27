# 第19組の復習：認可を業務サービスの全操作へ適用する

**学習順091〜095／学習順091〜70・互換URL /learn/column/70**

## この組の到達目標

Windows認証、既存台帳、閲覧・編集権限、部署単位の行条件、永続監査、xminとトランザクション、実AD/IIS/Kerberos/FDWの現地受入を、一連の業務操作として説明します。Windows認証が成功したから保存まで許可するわけではありません。認証ハンドラーが検証した本人のDOMAIN\userを受け取り、既存DB台帳を操作ごとに照会し、要求操作と行の両方が許可された場合だけ業務データへ進みます。利用者登録・権限編集はこのアプリで行わず、既存台帳が正本です。

| 学習順 | 旧Step | 実操作と利用サービス | 主な否定試験 |
| --- | --- | --- | --- |
| 091 | 66 | AuthorizeAsync と旧DBサービスの停止 | 匿名・構成不足・監査未確認 |
| 092 | 67 | LoadPageAsync／GetDetailAsync | Countだけ全社・部署外ID |
| 093 | 68 | CountByDoneAsync／ExportCsvAsync | CSV101件・他部署の漏えい |
| 094 | 69 | SaveAsync／CompleteSelectedAsync | can_edit剥奪・古いxmin・1件の権限外 |
| 095 | 70 | IIS・Kerberos・FDWと監査の現地受入 | CIの模擬成功を現地成功扱いする誤り |

## WinForms＋Dapperとの比較

| 観点 | WinForms＋Dapper | Blazor Interactive Server＋EF Core |
| --- | --- | --- |
| 本人 | OSログインだけでDB実行者を推定しない | 検証済みのClaimsPrincipalを用いる |
| 認可 | フォーム表示前だけでなくSQL実行時に確定 | 業務サービスの全公開操作で都度実行 |
| 部署 | パラメータ付きSQLのWHEREを共通化 | Visible(db, unit)を全照会の起点にする |
| 件数 | COUNTだけ全社を参照しない | TotalCountとItemsに同じ検索条件 |
| CSV | 一覧とは別のSQLにも認可を適用 | 100件上限とエスケープ、監査に失敗なら拒否 |
| 更新 | 読込時だけの許可と前回値に依存しない | can_edit、行ロック、xmin、トランザクション |
| 証跡 | 業務操作ごとの永続監査が必要 | WriteRequiredAsyncと相関IDを使う |
| 受入 | 実端末とDBの接続・障害試験 | CIに加え実IIS/AD/FDW・負荷試験が必要 |

## ケーススタディ：閲覧から保存まで

部署Aの担当者が架空課題を検索する例を考えます。ログイン済みPrincipalにWindowsの本人が含まれていても、最初に照合するのは既存DB台帳です。WindowsUserAccessService.ReadCurrentAsyncは検索名が0件ならunknown、2件以上ならambiguous、無効ならinactive、can_readがなければforbiddenを返します。正しい本人と閲覧権限が成立してから、業務サービスがUnitCodeを確認します。これを画面で入力できる部署コードへ置き換えません。

一覧と件数の双方を部署Aかつ同じ検索条件で取得します。Idを直接指定した詳細参照もVisibleに従います。完了・未完了の集計に他部署を加えず、CSVを100行で出せる場合も101行なら明示的に拒否します。画面のデータダウンロードリンクを非表示にして終わりではなく、許可がないときサービスがBase64を返さないようにします。

編集に進む場合は閲覧権限とは別のcan_editが必要です。編集画面で読み取った旧xminを保存時に指定し、サーバーで現在のxminと比べます。既存台帳が同じPostgreSQL上の行ロック可能なビューで、トランザクション連動が確認された場合だけ権限行をFOR UPDATEし、更新と監査を一回のコミットにまとめます。FDWや別DBでロックできなければ安全拒否します。1件でも他部署や古い版が混じる一括操作は全件取り消します。

## 実コード例1：照会の部署条件を共有する

以下はSecuredLearningServiceの実装の関係部分です。LINQのSQL翻訳と、取得結果の形を原本と比較してください。

```csharp
private static IQueryable<LearningTask> Visible(LearningDbContext db, string unit) =>
    db.Tasks.Where(t => t.UnitCode == unit);
var permit = await AuthorizeAsync(ct: ct);
if (permit.Unit is null) return new([], 0, permit.Message);
var query = Visible(db, permit.Unit).AsNoTracking();
if (!string.IsNullOrWhiteSpace(request.Keyword))
    query = query.Where(t => t.Title.Contains(request.Keyword.Trim()));
var total = await query.CountAsync(ct);
var rows = await query.OrderBy(t => t.Id)
    .Skip(checked(request.Page * request.PageSize)).Take(request.PageSize)
    .Select(t => new LearningTaskRow(t.Id, t.Title, t.DueDate,
        t.StartTime, t.IsDone, t.Version)).ToListAsync(ct);
```

件数だけ別クエリにして先に全テーブルを数えると、見えない行の件数を推測できます。検索条件を毎回組み立てても、認可範囲そのものは共通であることが必要です。複数ページ間で画面の検索条件が変わる場合は、同一条件で問い合わせたときの結果同士を比較してください。

## 実コード例2：編集と監査を一つの確定単位にする

次はSaveAsyncの主要判定を抜粋したものです。実装はさらに入力検証・例外処理・ログを含みます。

```csharp
var permit = await AuthorizeAsync(forEdit: true, ct: ct);
if (permit.Unit is null)
    return new(LearningEditStatus.Forbidden, permit.Message);
await using var db = await factory.CreateDbContextAsync(ct);
await using var tx = await db.Database.BeginTransactionAsync(
    IsolationLevel.ReadCommitted, ct);
if (!await CheckLockedEditAsync(db, permit.Unit, ct))
    return new(LearningEditStatus.Forbidden, Refused);
var task = await Visible(db, permit.Unit)
    .SingleOrDefaultAsync(t => t.Id == input.Id, ct);
if (task is null)
    return new(LearningEditStatus.Missing, "対象が存在しないか、編集権限がありません。");
if (task.Version != input.Version)
    return new(LearningEditStatus.Conflict, "別の利用者が更新しました。再読込してください。");
task.Title = input.Title.Trim();
await audit.WriteRequiredAsync(db, "task.update", permit.Unit, input.Id, 1, "success", ct);
await db.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```

実際のDB接続先が外部台帳の場合、SQLのロックが同一トランザクションで通るとは限りません。構成フラグの設定を証拠とせず、実際にロックと失効が同じ確定単位で制御されるか否定試験してください。監査保存だけ後から別接続で実施する方式にしていないことも確認します。

## 操作手順

1. GitHubの旧URL /learn/091〜/learn/095 と /learn/column/70 をChromeで開き、各ページの見出し・ボタン・初期状態・前後リンクを確認します。
2. 匿名の開発環境で認可確認を実行し、許可がない状態で行・Count・CSV Base64・保存成功を返さないことを確かめます。
3. PostgreSQL18実DBの隔離された架空台帳に本人、他部署、閲覧のみ、編集可能の利用者を用意します。既存の業務台帳には試験行を作成しません。
4. 読込時のcan_readとcan_editを確認し、既存台帳で権限を剥奪して次の操作を試します。画面に残った許可文言と新しい拒否結果を区別します。
5. CSVを101件以上にして、100件の一部出力を「全件」と表記せず明示拒否になることを確認します。集計がDB側で行われることも確認します。
6. 二人で同じVersionを読んでから一方を更新し、もう一方の古いxminがConflictになることを確認します。部署外Idを混ぜた一括更新の全件取消も試します。
7. 実AD/IIS/Kerberos・FDW、監査保存先、1万人超の登録と同時接続負荷については別の現地受入表に未完了として記録します。CIの緑表示を転記して合格にしません。

## 障害の切り分け

| 症状 | 切り分ける場所 | 対応 |
| --- | --- | --- |
| Windows認証なし | IIS設定、認証済みPrincipal、Negotiate | 匿名を拒否 |
| DB照合が2件 | 既存台帳の正規化と索引、一意制約 | 任意の1件を許可しない |
| Countだけ多い | VisibleとKeyword/IsDone/DueDateの適用順 | SQL範囲を統一 |
| CSVで他部署が見える | ExportCsvAsyncの起点とBase64の内容 | ダウンロードを止める |
| 101件で一部だけ出力 | Take(101)とMaxCsvRowsの判定 | 全件拒否メッセージ |
| xmin不一致 | 読込時VersionとDB現在値 | 競合として再読込 |
| 更新中の権限剥奪 | FOR UPDATE、台帳と業務DBの原子性 | 保証できなければ拒否 |
| 監査障害 | audit.WriteRequiredAsyncとDBトランザクション | 保存を確定しない |
| 本番負荷不明 | IISの同時circuitとDBプール、FDW遅延 | 未受入に残す |

## 演習と詳しい解答

**演習1．** 匿名状態でUIに「更新」ボタンが見えている場合、なぜ保存を実行してはいけないですか。

**詳しい解答1．** UIの表示が認証・認可の証明にならないためです。実サービスのSaveAsyncが必ずforEdit:trueで現在の既存台帳を確認し、編集可能な本人・部署・監査・原子性が成立しなければForbiddenにする必要があります。前回の許可文言を保持していても使いません。

**演習2．** 一覧が1件、TotalCountが150件で、他部署の149件が含まれていたら何が誤りですか。

**詳しい解答2．** CountとItemsのSQLの認可範囲が不一致です。Visible(db, permit.Unit)を両者の共通起点として検索条件を追加し、部署Aの許可行だけを数えます。DapperでもCOUNTとSELECTへ同じWHERE・パラメータを渡す必要があります。

**演習3．** CSVが101件の場合、先頭100件を返して正常終了してよいですか。

**詳しい解答3．** いいえ。現在の実装は101件目が存在するか検査し、超過した場合はBase64をnullとして返します。監査には上限拒否を記録します。検索条件を絞って再実行できるよう案内し、残りの1件を隠して全件取得と説明しません。

**演習4．** 編集画面で読み取った時点の権限は有効でしたが、保存直前にcan_editが失効しました。どちらを優先しますか。

**詳しい解答4．** 保存時の正本を優先します。AuthorizeAsync(forEdit:true)と、DBトランザクション内のCheckLockedEditAsyncで権限を確認します。行ロックが実機で成立しないFDW/別DBなら安全拒否し、設定だけで原子性を推定しません。監査の永続化にも失敗すれば業務更新は確定しません。

**演習5．** ChromeとxUnitが成功したことを根拠に1万人超のユーザーを実運用へ公開できますか。

**詳しい解答5．** できません。認証ハンドラーによる現地ADの本人確認、IIS・Kerberos/SPN、外部台帳のFDW絞り込みと失効時のロック、負荷、永続監査、障害復旧は実機で別途受け入れる必要があります。一万人の台帳登録数と同時接続人数は分けて検査します。

## 次へ

学習順096で利用者名正規化・重複拒否・索引へ進みます。旧URLと第19組の復習リンクは維持します。