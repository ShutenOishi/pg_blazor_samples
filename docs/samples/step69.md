# 学習順094：保存直前の権限とxminを再確認

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step69/SecuredEditPage.razor)。画面の正規URLは `/learn/094` です。

**第7段階・第19組／学習順094・旧URL /learn/094**

## 到達目標

保存直前の権限とxminを再確認を、実RazorとSecuredLearningServiceのC#、PostgreSQL18の架空台帳で確認します。WinForms＋Dapperから移る際にも、画面とサーバーの責任範囲を正しく分け、拒否する条件を説明できるようにします。

## 更新要求は読込時の許可だけで実行しない

ユーザーが編集画面を開いてから保存するまでに、既存台帳のcan_editがfalseへ変わる場合があります。また別の利用者が同じ課題を更新した場合は、古い画面の値で上書きしてはいけません。学習順094 は操作権限、対象部署、xminを保存の直前に再検証します。画面には項目番号の入力、詳細取得、タイトル編集、保存があり、対象行が取得できない場合は保存ボタンを表示しません。ただし画面上の表示条件はサーバー検証の代用ではありません。

SecuredLearningService.SaveAsync は最初に AuthorizeAsync(forEdit:true) を実行し、編集権限とトランザクション連動・監査の構成を要求します。次にId、Version、Titleの妥当性を確かめ、ReadCommittedのトランザクションを開始します。同一DBで行ロック可能な台帳ビューなら CheckLockedEditAsync が SELECT ... FOR UPDATE で本人の台帳行をロックし、IsActive、CanRead、CanEdit、部署一致を確認します。FDWや別DBでこのロック契約が成立しない場合、保存を許可せず拒否します。

Visible(db, unit) を通したId検索で対象行が見つからなければ、他部署の行か存在しない行として処理します。VersionはPostgreSQLのxminによる競合検出に使います。入力時点のVersionとDB内の現在値が異なる場合はConflictです。監査INSERTと課題の SaveChangesAsync、CommitAsyncを同じトランザクションにまとめ、監査が失敗した更新だけを確定する設計を避けています。

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
// SaveAsync の主要ゲートを実ソースから抜粋
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
```

上記コードの判定結果だけをブラウザーのフィールドへ保存して次の操作へ流用しないでください。実サービスの公開操作ごとに本人、部署、権限、監査条件を確認するのがこの段階の規則です。

## 実コード例2：別の入口と出力の境界

実Razor・C#実装に基づく抜粋です。学習の際には対象ファイルを開き、条件分岐の前後で返す値と画面への表示を追ってください。

```csharp
// 一括操作でも全件を確認し、一つのトランザクションで確定
var expected = selections.ToDictionary(s => s.Id, s => s.Version);
var ids = expected.Keys.ToArray();
var rows = await Visible(db, permit.Unit)
    .Where(t => ids.Contains(t.Id)).ToListAsync(ct);
if (rows.Count != selections.Count ||
    rows.Any(t => expected[t.Id] != t.Version))
    return new(LearningEditStatus.Conflict,
        "権限外・削除・競合があるため全件取り消しました。");
foreach (var row in rows) row.IsDone = true;
await db.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```

サーバーから返す「許可」「拒否」「0件」「競合」は、画面を操作した本人の自己申告値だけで決めません。表示上の成功と、正本台帳および業務テーブルに対する正しい検査の成立を混同しないことが必要です。

## 一括更新の厳格な条件

CompleteSelectedAsync は1〜50件で、Id重複なし、Version非ゼロの入力しか受け付けません。対象が一件でも部署外、削除済み、古い版なら、見つかった行だけを途中確定しません。実装では1行ごとの監査へ共通 correlationId を渡し、追跡できるようにしています。監査の成否をブラウザー側で後から埋め合わせる設計ではありません。

xminで競合を検出したときは「再読込してください。」と伝えます。自動的に最新値へ上書きする処理はありません。業務権限の変更が別DBで行われる環境ではトランザクションとロックの共有可能性を証明する必要があり、構成フラグをtrueにするだけでFDWの原子性が確保されるわけではありません。
## 操作手順

1. Visual Studio 2026で学習用の起動設定を選び、実際の業務DBや実ADに接続していないことを確認します。利用者の権限を試すために本番台帳へ架空行を登録しません。
2. Chromeで /learn/094 を直接開き、見出しが「保存直前の権限とxminを再確認」に対応していることと、初期状態の案内を確認します。旧URLを新URLへ付け替えることはしません。
3. 画面のメインボタンを押し、匿名の環境では許可されない状態を確認します。結果が空欄なのか、操作前なのか、拒否なのかをstatus領域のメッセージで区別してください。
4. ブラウザーの開発者ツールで画面操作を確認し、C#のブレークポイントを実業務サービスの公開メソッドに置いて、どの入口でAuthorizeAsyncが呼ばれるか調べます。
5. PostgreSQL18の隔離fixtureに限定して、未登録・無効・閲覧のみ・編集可能・他部署のデータを設定し、上記コードの分岐を比較します。実台帳を改変しないことが前提です。
6. 読み取った版のまま本人の編集権限を失効させて保存を試す。 期待される応答と戻り値を記録し、操作が失敗しても権限を緩めないことを確認します。
7. can_editなしでForbidden、古いxminでConflict、部署外のID混入では一括更新全体を拒否する。 サーバーの監査が正常に記録されたときだけ業務結果を受け入れ、現地受入未了の項目を完了と誤記しません。
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

台帳行をロック可能か、監査と業務更新を同じトランザクションへ入れたか確認する。 ログに認証秘密やSQL接続文字列を出さず、画面へスタックトレースを送らないようにします。監査と障害診断を区別し、接続プールのapplication_nameだけで個人の操作証跡を作ったと主張しないでください。

## 演習と詳しい解答

**演習1．** 読み取った版のまま本人の編集権限を失効させて保存を試す。 読込時と実行時の権限が違ったら、どの判定を優先しますか。

**詳しい解答1．** 現在のサーバーが操作時に取得した既存台帳の状態を優先します。画面に残った許可メッセージや、WinFormsのボタンEnabled、Blazorのcircuit内の状態は証拠になりません。許可のない場合はサービス入口で拒否し、業務行やCSVを返しません。

**演習2．** can_editなしでForbidden、古いxminでConflict、部署外のID混入では一括更新全体を拒否する。 正常系だけでなく、他部署・監査不可・DB切断の組合せで何を確認しますか。

**詳しい解答2．** 正常系では業務サービスの戻り値と監査の保存を確認し、他部署や権限失効では結果を空または拒否にします。DB照合や監査が失敗しても、古い許可や一部の更新を成功に変更しません。更新ではxminと台帳ロックの成立条件をさらに調べます。

**演習3．** 台帳行をロック可能か、監査と業務更新を同じトランザクションへ入れたか確認する。 学習用xUnitとChrome E2Eで成功すれば、実AD/IIS/Kerberos/FDWも受入済みですか。

**詳しい解答3．** いいえ。CIでは架空台帳とテスト用Principal、コンテナ上のPostgreSQL18などを使用します。現地のIIS設定、SPN/Kerberos、信頼されたWindows本人情報、FDW接続障害、監査保存先、同時接続の負荷は別の受入表と実測記録が必要です。特に1万人超の利用者登録数と同時circuit数は同じ試験ではありません。

## 次へ

学習順095へ進みます。
