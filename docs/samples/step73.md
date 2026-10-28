# 学習順98：権限行ロックと更新の原子性

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L098TransactionalAuthorizationPage.razor)。画面の正規URLは `/learn/098` です。

**第8段階・第20組／学習順098・正規URL /learn/098**

## 到達目標

画面表示時に編集可能でも、その後の所属・権限変更により保存が禁止される場合を理解します。ReadCommittedトランザクションと行ロックで、正本が同一DBにある場合だけ一貫した保存を行います。

## 動かし方

1. 学習順098画面から「更新可能性を照合する」を押し、匿名・不正な認証では拒否されることを確認します。
2. 管理者がローカル正本との行ロックを保証できると判断した開発DBでのみ、`Authorization:TransactionalLedgerConfirmed=true` を設定します。
3. CIの架空台帳で先に `can_edit=false` の変更トランザクションを保持し、その間に保存を試みます。変更がコミットされた後も更新を拒否する試験をします。
4. 別の利用者が先に業務更新を行った場合の `xmin` 競合も併せて検査します。

## EF CoreとPostgreSQL

次のイメージで権限行をロックします。パラメータは `FromSqlInterpolated` で送ります。

```sql
BEGIN;
SELECT *
FROM learning_user_access_v
WHERE login_name = 'TRAINING\USER00001'
FOR UPDATE;
-- 同じトランザクションで部署限定の対象行を読み、xminと入力を比較して更新
COMMIT;
```

実際のアプリは `SaveAsync` と `CompleteSelectedAsync` で短命DbContextを作成し、行ロック取得から更新コミットまで同一DBトランザクションを維持します。台帳に同じ名前が複数ある場合、読み取りは2件まで取得して拒否します。権限のない行は書き換えません。

## 安全境界

- `SELECT FOR UPDATE` が許されるDBロール・単純ビューと一意索引を前提にします。キーなしビューが外部テーブル・複雑ビューである場合、ロック可能性・権限・同時失効保証を別途検証します。
- **FDWの原本とアプリDBの更新が単一の原子的トランザクションに属するとは限りません。** 保証できない場合、独自に強行せず書き込みを拒否し、原本側の正規トランザクションAPIなどへ設計変更します。
- ログイン名の再利用は禁止するか、変わらない本人識別子を正本と合意します。
- これは実AD/IISの統合受入ではありません。CIのWindows偽Principalは検証用です。

## 確認問題と解答

**Q1. 読込時だけ台帳にcan_editがあれば十分？** いいえ。保存が確定する直前まで有効な権限を保証する必要があります。

**Q2. 認可と業務更新が別々のDbContextなら原子的？** いいえ。ロックと更新を同じトランザクションへ置きます。

**Q3. FDW正本でもFOR UPDATEのSQLが成功すれば原子性まで確定？** いいえ。リモート原本の権限変更とローカル更新の境界を個別に受け入れる必要があります。

## 学習順098で扱う競合の時系列

認可を画面表示時に一度確認してから利用者が入力している間に、別の管理処理が`can_edit=false`へ変更することがあります。保存時に再チェックするだけでも、チェック直後に権限が失効すれば競合します。同一PostgreSQL内で正本の行ロックを取得し、そのトランザクションで業務行の更新・監査INSERTを確定する契約が必要です。`SecuredLearningService.SaveAsync`は短命DbContextと`BeginTransactionAsync(IsolationLevel.ReadCommitted)`を使い、`CheckLockedEditAsync`で正本を照合します。

`Authorization:TransactionalLedgerConfirmed`は「設定すれば正本が安全になる」スイッチではありません。ロック可能な正本、ロール、単純ビュー、更新競合の再現試験を運用者が確かめた後に初めて真にする受入宣言です。FDWや別DBではローカルDBのBEGINだけで分散更新の原子性を保証できません。失効と更新の順序が保証できない場合には保存を拒否します。

### 実コード例1：行ロックの対象と再照合

```csharp
var rows = await db.ExistingUserAccess
    .FromSqlInterpolated(
        $"SELECT * FROM learning_user_access_v WHERE login_name = {login} FOR UPDATE")
    .AsNoTracking()
    .Take(2).ToListAsync(ct);
if (rows.Count != 1 || !rows[0].CanEdit ||
    !rows[0].IsActive || rows[0].UnitCode != unit)
    return false;
```

SQL補間はパラメータとして扱われる`FromSqlInterpolated`を使用し、ログイン名を文字列結合で埋め込みません。照合結果が0件または重複なら拒否します。実装のモデル名・判定の詳細は`CheckLockedEditAsync`の原本を参照してください。ロック可能性と孤立した外部トランザクションの問題は別に検証します。

### 実コード例2：業務更新と監査を同じ確定単位にする

```csharp
await using var db = await factory.CreateDbContextAsync(ct);
await using var tx = await db.Database.BeginTransactionAsync(
    IsolationLevel.ReadCommitted, ct);
if (!await CheckLockedEditAsync(db, permit.Unit, ct))
    return new(LearningEditStatus.Forbidden, "権限を確認できません。");
// Visibleで部署範囲を制限し、現在のxminと入力版を比較する。
await audit.WriteRequiredAsync(
    db, "task.update", permit.Unit, input.Id, 1, "success", ct);
await db.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```

省略した`Visible`による検索と`xmin`の版比較を飛ばして使う完成コードではありません。監査INSERTの呼出しに失敗すれば`CommitAsync`に到達しないこと、破棄されたトランザクションがロールバックされることを確認してください。一括操作は全行の部署・版を検査し、1件でも不一致なら部分成功にしません。

| 条件 | WinForms＋Dapper | Blazor＋EF Core |
| --- | --- | --- |
| 保存前の権限 | SELECTを毎保存時に実行し、ロック順序を統一 | AuthorizeAsyncとCheckLockedEditAsyncを分けて使用 |
| 本人・部署 | AD検証済み名前と正本の範囲で絞る | 画面値で部署を上書きしないVisible |
| 競合 | UPDATEの行数/版を確認 | xminを旧版と比較してConflict |
| 監査 | 同一IDbTransactionでINSERT | 同じDbContextとTransactionで確定 |
| 外部正本 | 分散確定契約がなければ更新禁止 | FDW原子性を推測せず安全拒否 |

### 操作手順と障害の切り分け

1. 開発専用DBで`/learn/098`を開き、デフォルト設定が更新を拒否することを確認します。運用フラグを本番で推測して有効にしません。
2. CI用の架空利用者の台帳行を別セッションで`FOR UPDATE`ロックし、トランザクションを保持したまま編集要求を行います。ロック待機とタイムアウトを記録します。
3. 先に権限剥奪をコミットした場合に更新が拒否されることを調べます。逆順なら業務確定後に失効が適用されるか時系列で確認します。
4. 別利用者が同じ業務行を更新して`xmin`が変わった状態と、監査INSERTが失敗する状態を独立に試します。どちらも後続の保存で古い内容を上書きしないことを確認します。

| 症状 | 調査 | 安全な結果 |
| --- | --- | --- |
| ロック取得不可 | ビュー種別・DBロール・FDW仕様 | TransactionalLedgerConfirmedを有効にしない |
| 保存時403 | 台帳の編集権限・部署・失効時刻 | データ未変更 |
| 競合発生 | 入力xminとDBの最新xmin | 画面へ再読込を案内 |
| 監査失敗 | INSERT権限・接続と例外処理 | 更新はロールバック |

### 演習と詳しい解答

**演習1．** 書込み前に`can_edit=true`を読み取った直後、管理者が権限を剥奪しました。再照会だけで失効との競合を防げますか。

**詳しい解答1．** 再照会だけではその直後の変更を防げません。正本ロックを取得し、同じトランザクション内で業務更新と監査を確定する順序が必要です。同一DBのロックが実証できないFDWなら書込みを拒否します。

**演習2．** 一括更新10件のうち最後の1件だけ他部署、かつ監査書込みが失敗しました。9件を保存できますか。

**詳しい解答2．** 保存できません。全行が部署・権限・xminの条件を満たすことを事前検証し、監査を含め同一トランザクションで確定します。一件でも不正・監査障害なら全件をロールバックします。

## 次へ

学習順099で永続的な監査の保存と、監査失敗時の書込み拒否を学びます。
