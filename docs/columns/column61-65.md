# 第18組の復習：Windows本人認証と既存台帳の操作別認可

**学習順086〜090／旧URL /learn/column/65**

## 認証から業務認可までの一連の流れ

Windows端末のサインイン状態を確認しただけではWebの業務を許可できません。IISまたはASP.NET Coreの認証ハンドラーが本人を確定し、認証済みPrincipalからDOMAINとユーザー名を読み取り、既存の利用者台帳へ照合して有効状態と操作権限を確かめます。この5段階のいずれかが成立しなければ拒否します。画面に結果を表示することと、全サービスの操作・行単位の認可が完成したことは別です。実AD・IIS・Kerberos・FDWと一万人超の実機負荷試験は未完了なので、匿名のChrome試験で置き換えません。

## 5つのステップ

| 学習順 | サーバー側の確認 | 拒否する典型例 |
| --- | --- | --- |
| 086／Step61 | Negotiateなどによる認証済みPrincipal | anonymous |
| 087／Step62 | 検証済みのDOMAINとユーザー名 | no-login、未検証スキーム |
| 088／Step63 | 既存台帳ビューで一意に照合 | unknown、ambiguous |
| 089／Step64 | 有効状態と閲覧権限・DB接続 | inactive、forbidden、db-error |
| 090／Step65 | can_read・can_edit・unit_code | 操作・対象行の範囲外 |

## WinForms＋Dapperとの比較

| 観点 | WinForms＋Dapper | Blazor Interactive Server |
| --- | --- | --- |
| 本人 | プロセスの実行者と操作主体を区別 | 認証ハンドラーで確定したPrincipal |
| DB照合 | WHEREとパラメーター付きSELECT | DbContextFactory・Where・Take(2) |
| 既存権限 | DB台帳が正本 | 台帳の読取り専用ビューが正本 |
| 無効化 | 再照合してから処理 | circuitへ許可を固定せず都度照合 |
| データ | Count・CSV・更新で同じ範囲 | 画面とサービスの両方で行単位条件 |
| 障害 | DBに照合できなければ拒否 | DB・FDW・監査失敗を安全拒否 |

## 実装コードで復習する

まず認証状態と名前の形式を検証します。Blazorの画面はAuthenticationStateProviderから検証済みのPrincipalを参照し、ブラウザーのテキスト入力や任意のHTTPヘッダーで本人を選ばせません。

```csharp
var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
if (principal.Identity?.IsAuthenticated != true)
    return new("anonymous", "Windows認証されていません。業務利用は拒否します。");
var login = ReadTrustedLoginName(principal);
if (login is null) return new("no-login", "名前を確認できません。");
```

次に条件付き検索を使い、0件と2件以上のどちらも拒否します。正規化したログイン名の二重登録は、元表の主キーがあっても生じ得ます。Take(1)を使うと重複検出ができません。全利用者行をメモリへ読み込む必要もありません。

```csharp
await using var db = await factory.CreateDbContextAsync(cancellationToken);
var matches = await db.ExistingUserAccess.AsNoTracking()
    .Where(x => x.LoginName == login).Take(2).ToListAsync(cancellationToken);
if (matches.Count == 0) return new("unknown", "登録なし");
if (matches.Count != 1) return new("ambiguous", "重複あり");
if (!matches[0].IsActive) return new("inactive", "無効");
if (!matches[0].CanRead) return new("forbidden", "閲覧権限なし");
```

コードの抜粋は判定の分岐を示しています。実際に更新やCSVを出すときは、この結果を表示するだけでなく、サービス内で保存直前の権限版、対象行の部署条件、xminの競合、監査の記録を確認します。権限を持つ利用者が別タブや長いcircuitを使っていても、正本の失効を無視してはいけません。

## 操作手順

1. ブラウザーで旧URL /learn/61へ移動し、認証状態の確認ボタンを押します。匿名環境で「Windows認証されていません」が表示されることを確認します。
2. /learn/62で認証済みPrincipalとログイン名の形式を読み、USER単独、UPN形式、未検証Cookieの主体がなぜ不適切か説明します。
3. /learn/63でビューとWhere・Take(2)を確認し、該当なし、正規化後の重複、接続失敗を拒否することを確認します。
4. /learn/64で無効、権限なし、DB停止を区別し、過去に許可された場合でも現在の照合で拒否することを確認します。
5. /learn/65で閲覧権限と編集権限を分け、unit_codeによる部署範囲は後続の業務サービスで強制することを確認します。
6. 第18組の復習から特別コラムへ進み、実機のIIS/Kerberos/FDW・負荷受入がまだ必要であることを整理します。

## 障害の切り分け

| 結果 | 調べる場所 | 安全な処理 |
| --- | --- | --- |
| anonymous | IIS設定と認証スキーム | 業務利用を拒否 |
| no-login | DOMAIN形式とCookieハンドラー | 入力値で補完しない |
| unknown | ビューの照合行数0件 | 登録処理を作らない |
| ambiguous | 大文字化と正本の一意性 | 任意の一行を採用しない |
| inactive | is_active | 古い権限を使用しない |
| forbidden | can_readや操作別権限 | 業務要求を実行しない |
| db-error | SQL・FDW・DB接続障害 | 接続失敗を許可に変えない |

ChromeやGitHub CIでテスト用の匿名接続を拒否できた場合でも、現地のドメインコントローラーや複数IISワーカーでの挙動まで確認したことにはなりません。実機ではSPN、認証ハンドラー、負荷時のP95/P99、索引、FDWのWHERE条件pushdown、CookieやData Protection鍵の管理、監査と失効を別に受け入れます。

## 総合演習

**問1。** 正しいWindows認証を通過した利用者が既存台帳では無効なら、どの操作を許可できるでしょうか。

**問2。** DOMAINを省略した名前やヘッダーから受け取った名前で台帳を照合してよいでしょうか。別ドメインの同名利用者がいる場合も考えてください。

**問3。** 既存台帳で閲覧権限のみを持ち、部署コードAである利用者に、部署BのCountやCSVを表示できますか。更新ボタンを非表示にしただけで権限確認は完了するでしょうか。

**問4。** 以前はallowedだった利用者の権限が失効し、同時に台帳照合でDBエラーが発生しています。以前の結果を使えますか。xmin競合や監査失敗も含めて説明してください。

## 詳しい解答

**問1の解答。** 本人認証に成功しても操作は拒否します。Windows認証と業務権限は別々の条件であり、既存台帳のis_activeが偽ならinactiveとなります。UIのメニューが表示されていてもDBデータを返しません。

**問2の解答。** できません。DOMAINを落とすと別人を混同し、任意のヘッダーは本人確認ではありません。サーバーの認証ハンドラーが確定したDOMAINとユーザー名を検証し、正本ビューの正規化規則と一致させます。UPNへ暗黙に変換したり、重複した二行から任意の一行を選んだりしません。

**問3の解答。** できません。can_readは操作に関する許可であり、対象行の部署条件も必要です。Count、Items、詳細、CSV、集計、更新を同じ条件で保護します。can_editが偽なら編集処理は拒否です。ボタンの非表示は操作性のためで、サービスとSQLの認可にはなりません。

**問4の解答。** 使用できません。現在の台帳に照合できなければdb-errorとして拒否し、以前の許可を使いません。権限版の失効、xmin競合、監査の永続化失敗、FDW原子性不明も許可へ置き換えません。結果が確定していない更新を成功と表示することも禁止します。

## 復習後の実務への接続

実装の正本は既存の業務・人事台帳です。CI専用の10001件の架空データを持つlearning_existing_user_ciを本番に導入せず、既存のテーブルまたはFDW外部テーブルの必要列だけを読み取り専用ビューへ射影します。EF CoreのToViewとHasNoKeyはモデル上の契約であり、運用DBでのSELECT権限や正規化後の一意性は別に管理します。実機でまだ受入できていない認証・通信・負荷条件は完了と扱いません。

次は「1万人規模の認証・既存台帳認可・セッション・監査」特別コラムを読み、学習順091（Step66）で認可を業務サービスに強制する実装へ進みます。
