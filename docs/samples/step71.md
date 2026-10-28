# 学習順96：既存台帳のキー正規化・重複拒否と索引

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L096DirectoryNormalizationPage.razor)。画面の正規URLは `/learn/096` です。

**第8段階・第20組／学習順096・正規URL /learn/096**

## 到達目標

ADが返す `DOMAIN\user` を、既存台帳・外部テーブルのログイン名と正しく比較し、表記揺れ・重複・改名時の拒否を実装します。対象は1万人以上の既存利用者ですが、同時接続者数とは分けて考えます。

## 操作して確認する

1. 学習画面の「現在の台帳照合を確認」を押します。認証のない環境では拒否されます。
2. 開発用PostgreSQL 18の架空台帳に大小の違う同名を2行用意し、`ambiguous` となることを試験します。
3. 古い名前を改名した後、旧名が `unknown`、新名が `allowed` になることを試験します。
4. 正本が無効・閲覧不可・DB停止のとき、画面が業務データを返さないことを確認します。

## コードとSQLのしくみ

WindowsUserAccessServiceは、Windows認証が発行したドメイン付き名前だけを取得して大文字正規化します。EF Core 10の読み取り専用ビューから `Where(x => x.LoginName == login).Take(2)` で検索し、0件は未登録、2件は重複として拒否します。リスト全体を読み込みません。

開発専用ビューの要点は次のSQLです。

```sql
CREATE INDEX IF NOT EXISTS learning_existing_user_ci_login_upper_idx
    ON learning_existing_user_ci (upper(login_name));
CREATE OR REPLACE VIEW learning_user_access_v AS
SELECT upper(login_name) AS login_name,
       display_name, is_active, can_read, can_edit, unit_code, authorization_version
FROM learning_existing_user_ci;
```

式索引は**この架空のローカルテーブルだけ**に対応します。本番ではデータ正本の一意制約、照合規則、FDWでの述語pushdown、`EXPLAIN (ANALYZE, BUFFERS)` の計画と実測をDB管理者が検証します。索引の存在だけで高速性や外部DBへの条件pushdownは保証されません。

## 実運用の重要な境界

- ログイン名が再利用される場合、**ADのユーザー名だけでは別人物を区別できません**。正本での名前再利用禁止、または不変IDの別契約が必須です。
- ADからグループや属性を取得しない方針は維持します。権限と部署範囲は既存DBが正本です。
- CIの仮Principalは実AD/IIS/Kerberosの受入ではありません。外部表の遅延・切断時はフェイルクローズを確認します。
- 利用者の登録、所属変更、ロール付与UIは実装しません。

## 確認問題と解答

**Q1. 台帳のログイン名をすべてメモリに取り込み検索する？** いいえ。WHERE条件と最大2件の結果に絞ります。

**Q2. 元表に文字列のPRIMARY KEYがあれば大小文字違いの重複は防げる？** その制約だけでは不十分です。正規化後のキーでも重複を拒否します。

**Q3. ADの `DOMAIN\USER` を再利用して別の職員へ割り当てても判別できる？** いいえ。ユーザー名以外の不変IDまたは非再利用の正本契約が必要です。

## 学習順096で確認する実装の責務

Windows認証で検証された主体からDOMAIN\userを得る経路と、既存台帳から閲覧権限・部署範囲を求める経路を分離します。画面の名前入力欄、HTTPヘッダーの独自ユーザー名、DB接続文字列から本人を決めません。`WindowsUserAccessService.ReadTrustedLoginName`が本人名を取り出せなければ台帳照合へ進まない構造です。取得できた場合も、台帳にその人物が存在することとは別の確認が必要です。

正規化を`ToUpperInvariant`だけに任せる場合、AD・ローカルDB・外部正本でUnicode文字の照合規則が一致するとは限りません。実際の契約はドメイン表記、区切り文字、許容文字集合、大文字変換、改名、名前再利用の処理を明記します。CIの架空データはその全境界を代表しないため、本番DB管理者が比較規則と制約を受け入れます。`upper(login_name)`の式索引は元表の設計を変更できるときの候補です。FDWでは外部側の索引利用・フィルタpushdownを`EXPLAIN`と実測で確認します。

### 実コード例1：認証済み本人の安全な抽出

次の部分は実装の`ReadCurrentAsync`から責務の順序を抜粋したものです。例外処理や結果のDTO組立ては原本を参照してください。

```csharp
var principal = (await authenticationState.GetAuthenticationStateAsync()).User;
var login = WindowsUserAccessService.ReadTrustedLoginName(principal);
if (login is null)
    return new WindowsAccessSnapshot(null, "未認証または本人名が不正です。");
// WindowsUserAccessServiceでは正規化したloginを読み取り専用ビューへ渡す。
// ここではClaimからcan_editや部署を読み取らない。
```

認証主体の受入検査では、偽Principal、匿名、ドメイン違い、形式違い、空文字、区切り文字の重複をそれぞれ拒否できるか確認します。受理後に必要なのは業務操作単位の再照会です。`ReadCurrentAsync`の結果をcircuit単位で永続的な許可としてキャッシュしません。

### 実コード例2：2件まで検索して一意性を判断

```csharp
var matches = await db.ExistingUserAccess
    .AsNoTracking()
    .Where(x => x.LoginName == login)
    .Take(2)
    .ToListAsync(cancellationToken);
if (matches.Count != 1)
    return Deny(matches.Count == 0 ? "unknown" : "ambiguous");
if (!matches[0].IsActive || !matches[0].CanRead)
    return Deny("inactive_or_forbidden");
```

この抜粋に含む`Deny`は判定を読みやすく示す教材表記であり、実装の戻り値生成は原本で確認します。2件取得する理由は「2件目以降を無視して1件だけ採用する」ためではなく、重複を明示的に拒否するためです。業務行数が増えても`ToListAsync`で台帳全件をロードしません。`Where`が生成するSQL、正規化済み列への条件、実際の索引利用が一致することを確認します。

| 判断対象 | WinForms＋Dapperでの実装 | Blazor＋EF Coreと正本の設計 |
| --- | --- | --- |
| 本人名 | WindowsIdentityの検証済み結果を取得し、自己申告を禁止 | 認証済みPrincipalをサービスが検証 |
| 重複 | ExecuteScalarで件数確認し1件以外を拒否 | Take(2)で0件と複数件を区別 |
| 絞り込み | SQLのWHERE条件をパラメータ化 | Whereと読み取り専用ビューを組合せ |
| 索引 | 実際のEXPLAINとDB管理者の確認 | CI索引を本番FDWの保証としない |

### 操作手順と障害の切り分け

1. `/learn/096`を開き、認証のない開発環境で「現在の台帳照合を確認」を押して拒否文言を記録します。画面が表示できても業務の閲覧許可ではありません。
2. PostgreSQL 18のCI用`learning_existing_user_ci`に大小文字違いの重複を置き、正規化されたビューから同じキーが2件になることをSQLで検査します。本番の既存台帳は変更しません。
3. 改名後の旧名0件、新名1件、無効ユーザー、閲覧権限なし、DB停止を分けて確認します。旧名の再利用を認める運用なら、不変IDの導入契約が必要です。
4. 同じ検索について`EXPLAIN (ANALYZE, BUFFERS)`を実行し、正規化式と索引式が一致するか確認します。外部テーブルならリモートへの条件委譲と往復回数も計測します。

| 現象 | 原因候補 | 確認・対応 |
| --- | --- | --- |
| 登録済みなのにunknown | 名前形式やドメイン表記の差 | 正本の契約と正規化前後を照合 |
| ambiguous | 大小文字違いの二重登録 | 重複のまま許可せず正本管理者に訂正依頼 |
| 台帳照合が遅い | 外部側索引なし、FDW全件転送 | EXPLAINと接続先の実計画を確認 |
| DB停止なのに許可 | 旧許可のキャッシュを使用 | キャッシュを権限正本にせず安全拒否 |

### 演習と詳しい解答

**演習1．** 同じ`DOMAIN\USER01`が大小文字の違いで2行あり、それぞれ異なる部署へ所属します。どちらか1行を採用して一覧を表示してもよいでしょうか。

**詳しい解答1．** 表示できません。同じ照合キーに2行対応すると部署や権限が一意に定まりません。`Take(2)`で複数件を検出して拒否します。正本管理者がキーの一意性を修復し、テストで再照合するまで許可を出しません。

**演習2．** CIのローカル索引検索が速ければ、1万人超のFDW正本も同じ応答時間で運用できますか。

**詳しい解答2．** できません。ローカルの式索引と外部の索引、リモート条件pushdown、ネットワーク遅延、接続プールを独立に計測します。また1万人は登録人数であり同時利用circuit数ではありません。本番受入には期限とSLOを記録した実測が必要です。

## 次へ

学習順097では、Windows認証を用いた専用HTTPログイン経路と短命Cookieの設計・安全試験へ進みます。
