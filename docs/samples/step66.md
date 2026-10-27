# 学習順091：サービス側で認可を強制する

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step66/AuthorizedEntryPage.razor)。画面の正規URLは `/learn/091` です。

**第7段階・第19組／学習順091・旧URL /learn/091**

## 到達目標

サービス側で認可を強制するを、実RazorとSecuredLearningServiceのC#、PostgreSQL18の架空台帳で確認します。WinForms＋Dapperから移る際にも、画面とサーバーの責任範囲を正しく分け、拒否する条件を説明できるようにします。

## サービスが最後に許可を判断する理由

旧来のWinFormsでは、一つの端末上で押せないボタンを設定すると、それだけで処理を止められると思いがちでした。しかしDapperを呼ぶ処理を別のメニューから実行できれば、同じSQLが走ります。Blazor Interactive Serverでも、画面に「閲覧可能」と表示することは、その後の保存やCSVを許可したことにはなりません。現在のユーザーが再接続したときにも、古いcircuitのフィールドから権限を復元してはいけません。

実ソースの SecuredLearningService.AuthorizeAsync は構成値と既存台帳の両方を見ます。WindowsAuthentication:Enabled と接続文字列が必要で、更新要求なら Authorization:TransactionalLedgerConfirmed、全操作には Authorization:DurableAuditConfirmed が必要です。その後 WindowsUserAccessService.ReadCurrentAsync を呼び、必要権限と単一の部署コードを取り出します。部署コードが正規表現に一致しない場合も拒否します。この一連の判定は画面のOnClickだけではなく、LoadPageAsync、GetDetailAsync、CountByDoneAsync、ExportCsvAsync、SaveAsync、CompleteSelectedAsync の入口から使われます。

認可に通った結果は (Unit, Message) です。Unit が null の場合は業務行を返せません。認証済みユーザー名を画面に表示して試験する設計にはしていません。監査の成立を事前に構成値で確かめても、実際の操作ごとの監査書き込みは別に必要です。設定スイッチを本番の代わりに切り替えるだけでは監査が正しく保存されたと証明できません。

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
// SecuredLearningService.AuthorizeAsync の判定条件を抜粋
if (!configuration.GetValue<bool>("WindowsAuthentication:Enabled") ||
    string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL")))
    return (null, Refused);
if (forEdit && !configuration.GetValue<bool>("Authorization:TransactionalLedgerConfirmed"))
    return (null, "権限正本のトランザクション連動を確認できないため、更新を拒否しました。");
if (!configuration.GetValue<bool>(LearningAuditService.AuditConfirmedSetting))
    return (null, "監査の永続化を確認できないため、処理を拒否しました。");
var state = await access.ReadCurrentAsync(ct);
var needed = forEdit ? "learning.edit" : WindowsUserAccessService.ReadPermission;
```

上記コードの判定結果だけをブラウザーのフィールドへ保存して次の操作へ流用しないでください。実サービスの公開操作ごとに本人、部署、権限、監査条件を確認するのがこの段階の規則です。

## 実コード例2：別の入口と出力の境界

実Razor・C#実装に基づく抜粋です。学習の際には対象ファイルを開き、条件分岐の前後で返す値と画面への表示を追ってください。

```csharp
// AuthorizedEntryPage.razor のボタンイベント
@inject SecuredLearningService Business
<MudButton Color="Color.Primary" OnClick="CheckAsync">
    業務サービスに認可を問い合わせる
</MudButton>
<p role="status" data-testid="step66-state">@_status</p>
@code {
    private string _status = "業務データはまだ読み込んでいません。";
    private async Task CheckAsync()
    {
        var permit = await Business.AuthorizeAsync();
        _status = permit.Message;
    }
}
```

サーバーから返す「許可」「拒否」「0件」「競合」は、画面を操作した本人の自己申告値だけで決めません。表示上の成功と、正本台帳および業務テーブルに対する正しい検査の成立を混同しないことが必要です。

## 入口ごとの拒否と構成の確認

許可判定をメニュー表示だけに置くと、URL直入力や別ページから呼ぶ保存処理が保護されません。したがって認可を一か所の業務サービスへ集め、それぞれのデータアクセス操作の先頭で呼びます。学習用APIを公開したくない場合には、認証を有効にした状態で旧DBサービスがConfigured=falseになることまで確かめます。これは古いサービスが権限なしで参照する抜け道を減らすためです。

認可要求時に「権限正本のトランザクション連動を確認できないため」と表示されるのは更新だけの追加ゲートです。閲覧許可と更新許可は同じではありません。管理者が can_read と can_edit の列を分けても、サービスが forEdit を渡し忘れると意味がありません。保存するメソッドの呼出し箇所と、実際に WHERE に部署条件が付く箇所の両方をレビューしてください。
## 操作手順

1. Visual Studio 2026で学習用の起動設定を選び、実際の業務DBや実ADに接続していないことを確認します。利用者の権限を試すために本番台帳へ架空行を登録しません。
2. Chromeで /learn/091 を直接開き、見出しが「サービス側で認可を強制する」に対応していることと、初期状態の案内を確認します。旧URLを新URLへ付け替えることはしません。
3. 画面のメインボタンを押し、匿名の環境では許可されない状態を確認します。結果が空欄なのか、操作前なのか、拒否なのかをstatus領域のメッセージで区別してください。
4. ブラウザーの開発者ツールで画面操作を確認し、C#のブレークポイントを実業務サービスの公開メソッドに置いて、どの入口でAuthorizeAsyncが呼ばれるか調べます。
5. PostgreSQL18の隔離fixtureに限定して、未登録・無効・閲覧のみ・編集可能・他部署のデータを設定し、上記コードの分岐を比較します。実台帳を改変しないことが前提です。
6. ボタンで許可を確認してから同じ画面でページを再読み込みする。 期待される応答と戻り値を記録し、操作が失敗しても権限を緩めないことを確認します。
7. 画面に許可メッセージが残っていても、台帳でcan_readを取り消した後のLoadPageAsyncは新しい拒否判定になることを確認する。 サーバーの監査が正常に記録されたときだけ業務結果を受け入れ、現地受入未了の項目を完了と誤記しません。
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

許可をC#のフィールドに保持し再利用してしまっていないか、AuthorizeAsyncの呼出し箇所を探す。 ログに認証秘密やSQL接続文字列を出さず、画面へスタックトレースを送らないようにします。監査と障害診断を区別し、接続プールのapplication_nameだけで個人の操作証跡を作ったと主張しないでください。

## 演習と詳しい解答

**演習1．** ボタンで許可を確認してから同じ画面でページを再読み込みする。 読込時と実行時の権限が違ったら、どの判定を優先しますか。

**詳しい解答1．** 現在のサーバーが操作時に取得した既存台帳の状態を優先します。画面に残った許可メッセージや、WinFormsのボタンEnabled、Blazorのcircuit内の状態は証拠になりません。許可のない場合はサービス入口で拒否し、業務行やCSVを返しません。

**演習2．** 画面に許可メッセージが残っていても、台帳でcan_readを取り消した後のLoadPageAsyncは新しい拒否判定になることを確認する。 正常系だけでなく、他部署・監査不可・DB切断の組合せで何を確認しますか。

**詳しい解答2．** 正常系では業務サービスの戻り値と監査の保存を確認し、他部署や権限失効では結果を空または拒否にします。DB照合や監査が失敗しても、古い許可や一部の更新を成功に変更しません。更新ではxminと台帳ロックの成立条件をさらに調べます。

**演習3．** 許可をC#のフィールドに保持し再利用してしまっていないか、AuthorizeAsyncの呼出し箇所を探す。 学習用xUnitとChrome E2Eで成功すれば、実AD/IIS/Kerberos/FDWも受入済みですか。

**詳しい解答3．** いいえ。CIでは架空台帳とテスト用Principal、コンテナ上のPostgreSQL18などを使用します。現地のIIS設定、SPN/Kerberos、信頼されたWindows本人情報、FDW接続障害、監査保存先、同時接続の負荷は別の受入表と実測記録が必要です。特に1万人超の利用者登録数と同時circuit数は同じ試験ではありません。

## 次へ

学習順092へ進みます。
