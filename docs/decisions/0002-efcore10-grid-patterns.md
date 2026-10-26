# ADR-0002：データアクセスはEF Core 10を標準とし、一覧データ連携を重点教材にする

- 日付：2026-10-26
- 状態：採用（実装・統合テストは後続PR）
- 対象：Step31〜60（認証・運用の計画番号を61〜80へ移動）
- 背景：Blazor Interactive Server + MudBlazor + PostgreSQL 18の学習と、社内業務アプリの保守性を重視する。

## 決定

1. **ORM：EF Core 10**。Npgsql.EntityFrameworkCore.PostgreSQLの互換する10系を採用。DateOnly、TimeOnly、Range/Multirange、LINQ、変更追跡、`xmin`競合とトランザクションを標準教材とする。
2. **寿命：`IDbContextFactory<AppDbContext>`を原則**とし、Blazorの長寿命circuitへDbContextをScoped注入して共有しない。Npgsql物理接続プールは標準利用、EF Core DbContextプールは負荷計測後に検討。
3. **一覧UI：用途による選択**。Items、MudTable ServerData、MudDataGrid ServerData、Virtualize、VirtualizeServerData、キーセット、行編集/ダイアログ、集計、CSVを同一の検索契約から段階的に学ぶ。
4. **境界：画面へDbContextやIQueryableを渡さない**。サーバー側Applicationサービスが検索条件・権限・最大件数・ソート列を検証し、DTOを返す。
5. **例外：linq2dbは任意比較対象**。複雑SQLやバルク性能など、実測上の利点が確認できるときに限定して導入する。
6. **教材の順序：80ステップ**。Step01〜25は維持、26〜30はUI、31〜40はDBの基礎、41〜60はDB連携を20ステップ、61〜70はAD、71〜80は運用。通常16コラム＋DI特別編1。

## 理由

- PostgreSQL固有RangeとLINQ翻訳の公式サポート、および多人数によるCRUD/競合処理の一貫性がある。
- .NETの公式DI・DbContextFactory・EF Core文書から学習を進めやすい。
- MudBlazorはORMを要求しないため、UI状態とデータ取得方法の選定を独立したスキルとして教えられる。
- ORM併用を最初から要求すると、接続寿命・トランザクション・変更追跡の境界が初学者に伝わりにくい。

## トレードオフと留意点

- 複雑SQLや巨大バルクの最適化ではEF Core以外の選択が必要になる可能性がある。
- MudDataGridの全フィルター演算が自動的にEF Coreへ翻訳されるわけではない。明示的な変換・検証が必要。
- `Items`や`Virtualize`だけではDBからの全件取得を抑制できない。`ServerData`はCountとItemsが別クエリであることも理解する。
- 後続の実装・CI・PostgreSQL 18接続を実施するまで、ソースが対応していると主張しない。

## 実装の参照

- [EF Core 10とNpgsql](../efcore10-data-access.md)
- [一覧表示の10パターン](../data-grid-patterns.md)
- [全80ステップ](../sample-catalog.md)
- [公式：BlazorとEF Core](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0)
- [公式：MudDataGrid API](https://mudblazor.com/api/MudDataGrid)
