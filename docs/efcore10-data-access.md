> **簡素化方針：** Step31〜40はWebプロジェクトのData/とServices/だけで実装。新しいDomain、Infrastructure、Repositoryプロジェクトは不要です。実DB未設定時には日本語の案内を表示します。

# EF Core 10・Npgsql・Blazor Interactive Server データアクセス学習ガイド

> **位置付け：Step31〜40・Step41〜60の実装設計資料。2026-10-26時点でこの章のDB連携ソースと実接続は未実装です。** 以下のコードは教材化する際の設計例であり、現行サンプルで実行できることを意味しません。

## この教材で目指すこと

Web画面から直接データベースを操作するのではなく、MudBlazorのUI状態 → Web内のLearningDbService → EF Core 10（Npgsqlプロバイダー） → PostgreSQL 18、という境界を説明・実装・テストできるようにします。[一覧・グリッドの選択ガイド](data-grid-patterns.md)と一体で学びます。

## 1. EF Core・Npgsql・PostgreSQLの役割

| 部品 | 担当 | 担当しないもの |
| --- | --- | --- |
| MudBlazor | 入力・表示・イベントとページ状態 | ORM、DB接続・権限判定 |
| Application | 画面用DTO、ユースケース、入力・権限確認、検索条件、取引境界 | コンポーネント固有型、SQL接続の長期保管 |
| EF Core 10 | エンティティマッピング、LINQ→SQL、変更追跡、トランザクション、競合検出 | 物理接続プールの実体 |
| Npgsql / EF Core PostgreSQL Provider | PostgreSQL型、SQLプロバイダー、物理接続・接続プール | 業務上の認可の判断 |
| PostgreSQL 18 | 制約・索引・SQL実行・整合性・ロック | MudDataGrid上の画面状態 |

NuGetの基本候補：`Npgsql.EntityFrameworkCore.PostgreSQL` の **10系**（EF Core 10と組み合わせる）。`Microsoft.EntityFrameworkCore.Design`はスキャフォールド/移行作業時に必要性を判断。`Npgsql`はプロバイダー依存版との整合を確認して決める。正確な採用パッチは実装PRで復元・ビルド・統合テストし、[バージョン表](versions.md)を確定更新します。linq2dbは**任意の上級比較**で、必須導入しません。

## 2. Blazor ServerでDbContextFactoryを使う理由

Interactive ServerのDIの`Scoped`はHTTP 1要求で終わらずcircuit単位で存続します。DbContextはスレッドセーフではなく、別コンポーネント/イベントの同時利用に適しません。

- 標準は`IDbContextFactory<LearningDbContext>`で**1操作1DbContext**。`await using`で必ず破棄します。
- 読み取り画面では`AsNoTracking`＋必要列のみ`Select`でDTOに射影します。
- 更新は**新しいDbContext**で対象を読み直し、入力値と権限を検証して`SaveChangesAsync`します。
- DbContext、`IQueryable`、`DbDataReader`、遅延列挙をRazorコンポーネントへ返しません。コンポーネント内へ接続文字列を置きません。
- キャンセルされたリクエスト・破棄された画面・二重クリックを学習し、保存系は二重実行防止や冪等性も確認します。

### 実装予定の最小登録例

```csharp
// Program.cs（計画例、まだ導入していません）
builder.Services.AddDbContextFactory<LearningDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgreSQL")
        ?? throw new InvalidOperationException("PostgreSQLの接続設定が必要です。")));
builder.Services.AddScoped<ILearningDbService, LearningDbService>();
```

```csharp
// Web/Servicesのサービス内（計画例）
public sealed class LearningDbService(
    IDbContextFactory<LearningDbContext> factory) : ILearningDbService
{
    public async Task<List<EmployeeRow>> GetRecentAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Employees
            .AsNoTracking()
            .OrderBy(x => x.Id) // 必ず安定した順序を指定する
            .Take(20)
            .Select(x => new EmployeeRow(x.Id, x.Name))
            .ToListAsync(ct);
    }
}
```

例中の`EmployeeRow`などの型は後続ステップで定義します。サービス契約はApplication、EF Coreの具象実装はWeb/Servicesに置きます。説明のためにWebへEF Core依存を持ち込まず、DIの登録場所のみWebのComposition Rootに置きます。

## 3. 接続プールとDbContextプールは別

| 項目 | Npgsql接続プール | EF Core DbContextプール |
| --- | --- | --- |
| 再利用するもの | PostgreSQLへの物理接続 | DbContextインスタンス |
| 管理 | NpgsqlDataSource等 | `AddPooledDbContextFactory`など |
| 標準 | 有効。接続を閉じても通常はプールへ戻る | **初期教材では使わない** |
| 注意 | DB側`max_connections`、アプリサーバー数、プール上限、接続リーク | 可変のテナント/ユーザー状態をコンテキストに残さない |

1ユーザー1DB接続の設計にしません。複数Webプロセス・DB接続文字列ごとに物理接続上限を見積もります。プールを利用しても長いトランザクション、開いたReader、未破棄Contextを放置してよいわけではありません。

**演習**：単一サーバー・複数サーバー・多人数操作について、Npgsql側のアクティブ接続数と待ち、DbContext生成回数、P95応答時間を分けて観測する。必要になる前にDbContextプーリングを導入しない。

## 4. 日付・時刻・期間の境界

| PostgreSQL | .NET側の基本表現 | ポイント |
| --- | --- | --- |
| `date` | `DateOnly` | タイムゾーン変換しない |
| `time` | `TimeOnly` | 日付を持たない |
| `timestamp without time zone` | `DateTime`（Unspecified） | 業務上の時間帯を別途説明 |
| `timestamptz` | `DateTime`（UTC） | 地域のタイムゾーンは保存されない |
| `daterange` | `NpgsqlRange<T>`（実採用Tとマッピングを統合試験で確定） | 境界・空範囲・無限端を検証 |
| `tstzrange` | `NpgsqlRange<DateTime>`（UTC） | 時刻と境界の解釈 |
| `datemultirange`等 | NpgsqlのRange集合 | 集合演算・翻訳を実DBで確認 |

Npgsql 10の通常の`date`/`time`読み取りの既定は`DateOnly`/`TimeOnly`です。**Range型の`T`は単独の`date`列と同じだと推測しない**こと。公式のRangeマッピングとバージョンを照合し、`daterange`と`tstzrange`をそれぞれ実行する教材にします。

MudDatePickerは`DateTime?`で受け取り、サービス境界で`DateOnly?`と変換します。

```csharp
// MudDatePicker（DateTime?）→ 業務日付（DateOnly?）
DateOnly? businessDate = selectedDate is { } d
    ? DateOnly.FromDateTime(d) : null;

// 業務日付 → MudDatePicker
DateTime? selectedDate = businessDate is { } d
    ? d.ToDateTime(TimeOnly.MinValue) : null;
```

MudDateRangePickerの`DateRange`とPostgreSQLのRangeは**別の型**。画面の両端を含む期間を、DBの`[下限, 上限)`（上限を含まない）へ正規化する。終了日の翌日を作れない`DateOnly.MaxValue`、null/未指定、夏時間、`DateTimeKind`、空範囲・無限境界は明示してテストします。

## 5. LINQ検索とパフォーマンス

- 一覧取得は`AsNoTracking`、必要な列だけの`Select`、DB側の`Where`/`OrderBy`/`Skip`/`Take`を既定とする。
- `OrderBy`に主キー等を追加して**完全に一意な順序**にする。順序なしのページングは禁止。
- `ToListAsync`する前に件数制限。画面上の絞り込みをサーバー実行だと偽らない。
- JOIN/N+1/集約では必要に応じて射影や`AsSplitQuery`の利害を比較し、生成SQLと`EXPLAIN (ANALYZE, BUFFERS)`を確認する。
- 何度も同じ問い合わせを実行する場合に限りCompiled Query/キャッシュを計測して採用する。
- 検索条件・ソート列は**許可リスト**に変換する。画面の式や列名を動的SQLに連結しない。
- SQLを直接書く場合はパラメーター化、権限・ロギングを確認し、採用理由を記録する。

## 6. 変更追跡・競合・トランザクション

| 場面 | 推奨 |
| --- | --- |
| 読み取りのみ | `AsNoTracking`、表示DTO |
| 単一行更新 | 新DbContextで読み直して更新、`SaveChangesAsync` |
| 複数テーブルを原子的に更新 | 明示的な`BeginTransactionAsync`、例外・ロールバック試験 |
| 複数ユーザーの競合 | PostgreSQL `xmin`をConcurrency Tokenに設定、`DbUpdateConcurrencyException`を説明 |
| 再読み込みと再試行 | 競合したデータを勝手に上書きせず、差分と再編集の選択肢を表示 |
| 複数画面・長い入力 | DBトランザクションを保持せず、画面DTOを維持 |
| 大量変更 | `ExecuteUpdateAsync`/`ExecuteDeleteAsync`の追跡・監査・競合上の違いを学習 |

外部通知とDB更新をまたぐ場合は、DBトランザクションのみでは通知の完全一致を保証できません。必要ならOutboxなどのパターンを別途紹介します。

## 7. スキーマとテスト

初期設計に基づくSQLをレビューし、開発・結合テスト専用DBへ適用する。既存本番スキーマへのMigration自動適用は行わない。DBファーストのスキャフォールドは使ってよいが、ドメイン/DTOとの責務の違いを説明する。

テストは、DateOnly、`timestamptz`、Range包含・重複、`xmin`競合、トランザクション中断、二重保存、プール枯渇、キャンセル、権限拒否、SQL実行計画を含める。**InMemoryプロバイダーやモックだけでPostgreSQL固有SQLの正しさを証明しない**。

## 8. linq2dbの位置付け（任意の上級編）

SQL表現や一括処理に明確な利点があるとき、linq2dbとEF Coreの生成SQL・実行計画・メモリ/処理時間を**同一DB・同一データ**で比較する。原則はEF Core単体で完結させ、必要性が実測できて初めて併用を判断する。標準CRUD、一般的なDI、MudBlazor連携にlinq2dbは要求しない。

## 公式参考

- [Microsoft: BlazorとEF Core](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0)
- [Microsoft: EF Coreのページング](https://learn.microsoft.com/ja-jp/ef/core/querying/pagination)
- [Npgsql: 日付・時刻](https://www.npgsql.org/doc/types/datetime.html)
- [Npgsql: RangeとMultirange](https://www.npgsql.org/efcore/mapping/range.html)
- [Npgsql: 接続プール](https://www.npgsql.org/doc/connection-string-parameters.html)

この章の説明と後続ソースの違いは、各ステップ実装PRでテスト結果とともに修正します。
