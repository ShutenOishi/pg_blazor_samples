# 学習順095：Windows Server 2025の実機受入を分ける

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage07/L095DeploymentGatePage.razor)。画面の正規URLは `/learn/095` です。

**第7段階・第19組／学習順095・旧URL /learn/095**

## 到達目標

Windows Server 2025の実機受入を分けるを、実RazorとSecuredLearningServiceのC#、PostgreSQL18の架空台帳で確認します。WinForms＋Dapperから移る際にも、画面とサーバーの責任範囲を正しく分け、拒否する条件を説明できるようにします。

## CI成功と現地受入の責任範囲

Windows Server 2025上でReleaseビルドが通り、Ubuntu上でChrome E2Eが通り、PostgreSQL 18の実DB試験で架空の利用者・課題を操作できても、本番AD/IIS/Kerberosの本人確認が完了したわけではありません。学習順095では検査環境を分けて扱います。表示中の「現在の更新権限を確認」は SecuredLearningService.AuthorizeAsync(forEdit:true) を呼ぶだけで、画面が開いたことやボタンを押せたことから実機受入を宣言しません。

IISのWindows認証を有効にした場合には、サーバー側で本当にNegotiate/Windowsの検証済み主体を受け取り、DOMAIN\userを読み取れること、匿名・Cookie偽装・別ドメイン・UPN等が想定外の形で許可されないことを確認します。実ADの名前変更、ユーザー名再利用、無効化、権限失効、複数件照合を運用担当の台帳仕様に照らして試します。アプリへ新しい利用者登録機能を追加せず、既存台帳ビューの索引と一意性をDB管理者が受け入れる必要があります。

権限正本がFDWや別DBの場合、SELECT FOR UPDATEの整合性と失効タイミングを、本番に近い構成で確認します。同一PostgreSQLの単純ビューと異なり、正本側のトランザクションへ安全に参加できないケースを許可扱いしてはいけません。監査は本人、操作、対象行、結果、UTC、相関ID、保管期間、閲覧制限、監査DB停止時の拒否を要求します。application_name は接続プールの診断用であって個人の操作証明ではありません。

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
// DeploymentGatePage.razor の実画面イベント
@inject SecuredLearningService Business
<MudButton Color="Color.Primary" OnClick="CheckAsync">
    現在の更新権限を確認
</MudButton>
<p role="status" data-testid="step70-state">@_status</p>
@code {
    private string _status = "実機受入は未実施です。";
    private async Task CheckAsync()
    {
        var permit = await Business.AuthorizeAsync(forEdit: true);
        _status = permit.Message;
    }
}
```

上記コードの判定結果だけをブラウザーのフィールドへ保存して次の操作へ流用しないでください。実サービスの公開操作ごとに本人、部署、権限、監査条件を確認するのがこの段階の規則です。

## 実コード例2：別の入口と出力の境界

実Razor・C#実装に基づく抜粋です。学習の際には対象ファイルを開き、条件分岐の前後で返す値と画面への表示を追ってください。

```csharp
// 実装済みのWindowsログイン名検証条件
var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
var login = WindowsUserAccessService.ReadTrustedLoginName(principal);
if (login is null)
{
    // 実機で未認証・不正形式・偽装を否定試験する。
    return;
}
// 別の業務操作では、さらに既存DB台帳の認可確認が必要。
var permit = await business.AuthorizeAsync(forEdit: true);
if (permit.Unit is null) return;
```

サーバーから返す「許可」「拒否」「0件」「競合」は、画面を操作した本人の自己申告値だけで決めません。表示上の成功と、正本台帳および業務テーブルに対する正しい検査の成立を混同しないことが必要です。

## 1万人超の評価は登録数と同時接続数を区別する

一万人分の利用者を既存DBに登録している状態と、一万人が同時にブラウザーを開き、circuitとDB接続プールへ負荷をかける状態は違います。ログイン照合では全件をC#へ取り込まず、正規化されたログイン名で最大2件まで取得します。ただしFDW側の絞り込みpushdown、名前索引、通信遅延は接続先の実測が必要です。負荷検査では同時circuit数、必要時だけ生成するDbContext、Npgsql接続プールの枯渇、DB CPU、P95/P99、再接続と失効タイミングを監視します。

災害や障害からの復旧試験では、DB/FDWが落ちたときに古い許可で継続しないこと、監査保存が失敗した操作を成功にしないこと、更新中のトランザクションが正常に取り消されることを確認します。Windows認証専用HTTP経路と短命Cookieを導入する設計は後続の学習順097の対象で、ADのパスワードをBlazor画面に入力・保存する方式へ置き換えません。
## 操作手順

1. Visual Studio 2026で学習用の起動設定を選び、実際の業務DBや実ADに接続していないことを確認します。利用者の権限を試すために本番台帳へ架空行を登録しません。
2. Chromeで /learn/095 を直接開き、見出しが「Windows Server 2025の実機受入を分ける」に対応していることと、初期状態の案内を確認します。旧URLを新URLへ付け替えることはしません。
3. 画面のメインボタンを押し、匿名の環境では許可されない状態を確認します。結果が空欄なのか、操作前なのか、拒否なのかをstatus領域のメッセージで区別してください。
4. ブラウザーの開発者ツールで画面操作を確認し、C#のブレークポイントを実業務サービスの公開メソッドに置いて、どの入口でAuthorizeAsyncが呼ばれるか調べます。
5. PostgreSQL18の隔離fixtureに限定して、未登録・無効・閲覧のみ・編集可能・他部署のデータを設定し、上記コードの分岐を比較します。実台帳を改変しないことが前提です。
6. 開発CIとWindows Server実機で別の受入表を作る。 期待される応答と戻り値を記録し、操作が失敗しても権限を緩めないことを確認します。
7. CIでできた項目にAD/Kerberos/FDW負荷の受入チェックを付けず、現地証跡と運用担当者の承認を待つ。 サーバーの監査が正常に記録されたときだけ業務結果を受け入れ、現地受入未了の項目を完了と誤記しません。
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

障害時の拒否、1万人の登録数と同時利用、監査の保存可否を別々に測る。 ログに認証秘密やSQL接続文字列を出さず、画面へスタックトレースを送らないようにします。監査と障害診断を区別し、接続プールのapplication_nameだけで個人の操作証跡を作ったと主張しないでください。

## 演習と詳しい解答

**演習1．** 開発CIとWindows Server実機で別の受入表を作る。 読込時と実行時の権限が違ったら、どの判定を優先しますか。

**詳しい解答1．** 現在のサーバーが操作時に取得した既存台帳の状態を優先します。画面に残った許可メッセージや、WinFormsのボタンEnabled、Blazorのcircuit内の状態は証拠になりません。許可のない場合はサービス入口で拒否し、業務行やCSVを返しません。

**演習2．** CIでできた項目にAD/Kerberos/FDW負荷の受入チェックを付けず、現地証跡と運用担当者の承認を待つ。 正常系だけでなく、他部署・監査不可・DB切断の組合せで何を確認しますか。

**詳しい解答2．** 正常系では業務サービスの戻り値と監査の保存を確認し、他部署や権限失効では結果を空または拒否にします。DB照合や監査が失敗しても、古い許可や一部の更新を成功に変更しません。更新ではxminと台帳ロックの成立条件をさらに調べます。

**演習3．** 障害時の拒否、1万人の登録数と同時利用、監査の保存可否を別々に測る。 学習用xUnitとChrome E2Eで成功すれば、実AD/IIS/Kerberos/FDWも受入済みですか。

**詳しい解答3．** いいえ。CIでは架空台帳とテスト用Principal、コンテナ上のPostgreSQL18などを使用します。現地のIIS設定、SPN/Kerberos、信頼されたWindows本人情報、FDW接続障害、監査保存先、同時接続の負荷は別の受入表と実測記録が必要です。特に1万人超の利用者登録数と同時circuit数は同じ試験ではありません。

## 次へ

第19組の復習で学習順091〜70を横断して確認し、次に学習順096へ進みます。
