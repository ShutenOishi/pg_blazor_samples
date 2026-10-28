# 学習順093：集計とCSVにも認可を適用

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L093ScopedExportPage.razor)。画面の正規URLは `/learn/093` です。

**第7段階・第19組／学習順093・正規URL /learn/093**

## 到達目標

集計とCSVにも認可を適用を、実RazorとSecuredLearningServiceのC#、PostgreSQL18の架空台帳で確認します。WinForms＋Dapperから移る際にも、画面とサーバーの責任範囲を正しく分け、拒否する条件を説明できるようにします。

## ダウンロードを別の抜け道にしない

画面の一覧を正しく部署条件で絞っても、集計画面が全社のGROUP BYを使っていれば件数が漏れます。CSVは画面表示を経由しないデータ持ち出し経路になるため、出力範囲をサーバー側で同じように制限します。SecuredLearningService.CountByDoneAsync と ExportCsvAsync は両方とも、認可で取得した Unit を Visible(db, unit) に渡しています。本人・操作の許可を閲覧後にキャッシュした値だけで代用することはありません。

GroupBy(IsDone) はDB側で集計し、行を全部メモリに持ってから分けません。集計後に audit.WriteRequiredAsync が呼ばれ、永続監査が失敗した場合に成功応答を返さないようにします。CSVは Id の昇順で MaxCsvRows+1、すなわち101行を取得し、上限100件を超えたらダウンロードデータそのものを返しません。100件だけ切り出して「全件出力」と説明する誤りを防ぎます。

文字列列のエスケープには LearningRangeExportService.EscapeCsv を使い、番号、課題名、完了をUTF-8 BOM付きで出力します。表計算ソフトへ取り込む利用形態を念頭に、改行・ダブルクォート・式として解釈され得る入力への対応をサービスで統一します。CSVをURLで共有する方式や、他のページがBase64を再利用する方式は、このサンプルでは完成した運用設計として扱いません。

## 学習環境と既存システムとの境界

想定環境は Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor、EF Core 10、Npgsql、PostgreSQL 18、Windows Server 2025 と IIS です。開発用の架空課題・既存台帳の試験データだけを使います。CIの認証済みPrincipalはテスト用に構成したもので、現地ADに接続した証跡ではありません。実AD、IIS、Kerberos、外部DB/FDW、1万人超の実機負荷試験は未完了です。

ADが担うのは本人の認証と、認証ハンドラーが検証済みの DOMAIN\user 形式の名前を渡すことです。本人がログイン済みという事実を、業務一覧の閲覧、課題の保存、CSVの出力、他部署のデータ取得の許可へ直結させません。操作ごとの許可は管理者が管理する既存DBの利用者台帳を投影した読み取り専用ビューが正本です。アプリに利用者登録画面や権限付与画面を追加しません。既存台帳の該当行がゼロ件、複数件、無効、権限なし、読取障害の場合は拒否します。

Razorの表示状態やボタンの有効化は操作性のための情報です。複数タブ、画面再接続、権限剥奪、HTTPの別経路からの呼出しを考慮し、実データを返すサーバーの公開メソッドで都度認可します。学習順047〜60の学習用DBサービスは WindowsAuthentication:Enabled が true の構成で停止します。認可ゲートが確認できなければデータを空にするだけでなく、監査の永続化が失敗する場合も成功と扱わない実装です。

| 比較点 | WinForms＋Dapperで注意する点 | Blazor＋EF Coreで確認する点 |
| --- | --- | --- |
| ユーザー識別 | フォーム入力や接続文字列を本人と見なさない | 認証済みPrincipalの信頼された方式と名前を確認 |
| 画面操作 | Enabled=false は安全の証明にならない | ボタン非表示を認可ゲートの代用にしない |
| 権限参照 | SQL実行前に既存台帳で都度確定 | WindowsUserAccessServiceと業務サービスで再照合 |
| データ範囲 | SQLのWHEREへ本人の許可部署を渡す | LINQのIQueryableへ部署条件を先に置く |
| 失効・障害 | 古い許可情報を使い回さない | DB照合・監査障害なら安全に拒否 |
| 試験 | DBと現地Windows環境を分ける | xUnit/Chrome/PG18と実IIS受入を区別 |

## 実コード例1：サーバー側の判定を読む

次はリポジトリの実装から学習範囲を抜粋したものです。省略した例外処理やDI登録は原本のSecuredLearningService.csと対応させてください。単なる図の疑似処理ではありません。

```csharp
// 認可済み部署内で行うGROUP BY の実処理
var permit = await AuthorizeAsync(ct: ct);
if (permit.Unit is null) return new([], permit.Message);
await using var db = await factory.CreateDbContextAsync(ct);
var groups = await Visible(db, permit.Unit).AsNoTracking()
    .GroupBy(t => t.IsDone)
    .Select(g => new LearningCountRow(g.Key, g.Count())).ToListAsync(ct);
await audit.WriteRequiredAsync(db, "task.summary", permit.Unit, 0,
    groups.Sum(g => g.Count), "success", ct);
```

上記コードの判定結果だけをブラウザーのフィールドへ保存して次の操作へ流用しないでください。実サービスの公開操作ごとに本人、部署、権限、監査条件を確認するのがこの段階の規則です。

## 実コード例2：別の入口と出力の境界

実Razor・C#実装に基づく抜粋です。学習の際には対象ファイルを開き、条件分岐の前後で返す値と画面への表示を追ってください。

```csharp
// CSVの100件上限を検証する実コード抜粋
var rows = await Visible(db, permit.Unit).AsNoTracking()
    .OrderBy(t => t.Id).Take(MaxCsvRows + 1)
    .Select(t => new { t.Id, t.Title, t.IsDone }).ToListAsync(ct);
if (rows.Count > MaxCsvRows)
{
    await audit.WriteRequiredAsync(db, "task.csv", permit.Unit, 0,
        rows.Count, "limit_rejected", ct);
    return new(null, 0, "101件以上は出力しません。条件を絞ってください。");
}
var csv = new StringBuilder("\uFEFF番号,課題名,完了\r\n");
foreach (var t in rows)
    csv.Append(t.Id).Append(',').Append(LearningRangeExportService.EscapeCsv(t.Title))
        .Append(',').Append(t.IsDone ? "はい" : "いいえ").Append("\r\n");
```

サーバーから返す「許可」「拒否」「0件」「競合」は、画面を操作した本人の自己申告値だけで決めません。表示上の成功と、正本台帳および業務テーブルに対する正しい検査の成立を混同しないことが必要です。

## 失敗時の画面状態も見直す

ScopedExportPage は「認可付き集計」と「認可付きCSV」を別ボタンで実行します。集計を押したとき _csv=null、CSVを押したとき _summary=null とするため、直前の別操作の古い結果を残しにくい構造です。Base64 が null のときはダウンロードリンクを生成しません。ブラウザーのリンクを非表示にすることだけに依存せず、サービスがデータを返さないことまで試験します。

実DB試験では架空課題のタイトルを自部署と他部署で区別し、Base64を復号した内容に他部署のタイトルが含まれないことを確かめます。一覧と集計の行数が食い違う場合、検索キーワードの適用範囲と同一時点性を確認します。CountByDoneAsync は全許可行、ExportCsvAsync は任意キーワードという契約の違いがあるので、単純に別条件の件数同士を比較して誤判定しないでください。
## 操作手順

1. Visual Studio 2026で学習用の起動設定を選び、実際の業務DBや実ADに接続していないことを確認します。利用者の権限を試すために本番台帳へ架空行を登録しません。
2. Chromeで /learn/093 を直接開き、見出しが「集計とCSVにも認可を適用」に対応していることと、初期状態の案内を確認します。互換URLを新URLへ付け替えることはしません。
3. 画面のメインボタンを押し、匿名の環境では許可されない状態を確認します。結果が空欄なのか、操作前なのか、拒否なのかをstatus領域のメッセージで区別してください。
4. ブラウザーの開発者ツールで画面操作を確認し、C#のブレークポイントを実業務サービスの公開メソッドに置いて、どの入口でAuthorizeAsyncが呼ばれるか調べます。
5. PostgreSQL18の隔離fixtureに限定して、未登録・無効・閲覧のみ・編集可能・他部署のデータを設定し、上記コードの分岐を比較します。実台帳を改変しないことが前提です。
6. 部署A/Bを混在させたデータで集計とCSVの結果を比較する。 期待される応答と戻り値を記録し、操作が失敗しても権限を緩めないことを確認します。
7. CSVは100件なら生成し101件ならBase64=nullで明示拒否、他部署のタイトルは含まれない。 サーバーの監査が正常に記録されたときだけ業務結果を受け入れ、現地受入未了の項目を完了と誤記しません。
8. 第19組の復習ページに進み、五つの操作入口に同じ台帳と監査の契約が適用されているか説明します。

## 障害の切り分け

| 観測した状態 | 確認する場所 | 安全な判断 |
| --- | --- | --- |
| anonymous／no-login | 信頼済み認証ハンドラーとDOMAIN付きの名前 | 本人を推定せず拒否 |
| unknown／ambiguous | 既存台帳ビューの照合件数0/2と正規化 | 便宜的に別ユーザーを選ばない |
| inactive／forbidden | is_active、can_read、can_edit と要求操作 | 許可を再利用せず拒否 |
| DB/FDWの失敗 | Npgsql、台帳ビュー、接続権限、遅延 | 以前の許可を返さない |
| 監査が保存できない | DurableAuditConfirmedと監査テーブル | 業務結果を公開・確定しない |
| 部署不一致／対象なし | VisibleのWHEREと業務行のunit_code | 他部署の存在を明示しない |
| 編集競合・原子性未確認 | xmin、行ロック、同一DBトランザクション | 保存を拒否して再読込 |
| 実機の認証と負荷 | IIS、Kerberos、FDWと同時circuit | CI合格と区別して未受入 |

集計・エクスポートをそれぞれ操作する際、違う操作の古いダウンロードリンクが残らないか調べる。 ログに認証秘密やSQL接続文字列を出さず、画面へスタックトレースを送らないようにします。監査と障害診断を区別し、接続プールのapplication_nameだけで個人の操作証跡を作ったと主張しないでください。

## 演習と詳しい解答

**演習1．** 部署A/Bを混在させたデータで集計とCSVの結果を比較する。 読込時と実行時の権限が違ったら、どの判定を優先しますか。

**詳しい解答1．** 現在のサーバーが操作時に取得した既存台帳の状態を優先します。画面に残った許可メッセージや、WinFormsのボタンEnabled、Blazorのcircuit内の状態は証拠になりません。許可のない場合はサービス入口で拒否し、業務行やCSVを返しません。

**演習2．** CSVは100件なら生成し101件ならBase64=nullで明示拒否、他部署のタイトルは含まれない。 正常系だけでなく、他部署・監査不可・DB切断の組合せで何を確認しますか。

**詳しい解答2．** 正常系では業務サービスの戻り値と監査の保存を確認し、他部署や権限失効では結果を空または拒否にします。DB照合や監査が失敗しても、古い許可や一部の更新を成功に変更しません。更新ではxminと台帳ロックの成立条件をさらに調べます。

**演習3．** 集計・エクスポートをそれぞれ操作する際、違う操作の古いダウンロードリンクが残らないか調べる。 学習用xUnitとChrome E2Eで成功すれば、実AD/IIS/Kerberos/FDWも受入済みですか。

**詳しい解答3．** いいえ。CIでは架空台帳とテスト用Principal、コンテナ上のPostgreSQL18などを使用します。現地のIIS設定、SPN/Kerberos、信頼されたWindows本人情報、FDW接続障害、監査保存先、同時接続の負荷は別の受入表と実測記録が必要です。特に1万人超の利用者登録数と同時circuit数は同じ試験ではありません。

## 次へ

学習順094へ進みます。
