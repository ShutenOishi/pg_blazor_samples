# 構成方針：Webプロジェクト内でEF Core 10を学ぶ

## 基本方針

**EF CoreのためにDomain・Infrastructure・Repositoryなどの別プロジェクトを増やしません。** 初めてEF Coreを使う学習者が、Blazor/MudBlazor→サービス→DbContext→PostgreSQLの呼び出しを素直に追えることを優先します。

Step19で既存のApplicationプロジェクトを使ったDIの練習を実装済みですが、**新しいDB教材ではその参照を必須にせず、既存Web内で完結**します。実務で分割が必要になれば、その段階で選択肢として説明します。

## 現在の実装

| 用途 | 置き場所 | 目的 |
| --- | --- | --- |
| 画面 | Web/Components/Samples/Step31〜40 | 日本語で動作と目的を確認 |
| Entity・DTO | Web/Data/LearningTask.cs | PostgreSQLの列と画面表示を区別 |
| EF Core DbContext | Web/Data/LearningDbContext.cs | PostgreSQLのテーブル型を設定 |
| 業務用の小さなサービス | Web/Services/Learning/LearningDbService.cs | Factoryから操作単位にDbContextを生成 |
| DIと構成 | Web/Program.cs | AddDbContextFactory・接続文字列 |
| SQL | database/schema、seed、tests | 開発用DBを準備・検証 |
| 日本語教材 | docs/samples と docs/columns | コードと画面に合わせた説明 |
| 既存のテスト | tests/PgBlazorSamples.UnitTests | 従前のDI教材などをテスト |

```text
Blazor / MudBlazor（Web）
         ↓
Web/Services/Learning/LearningDbService
         ↓ IDbContextFactory<LearningDbContext>
Web/Data/LearningDbContext（EF Core 10）
         ↓ Npgsql
PostgreSQL 18（開発専用DB）
```

## あえてしないこと

- 新たなDomain、Infrastructure、Repositoryの各プロジェクトを作らない。
- 画面ごとにRepositoryインターフェイスを増設しない。
- 画面にDbContextやIQueryableを長期間保持しない。
- 起動時に本番DBのスキーマ変更を自動実行しない。
- DB未設定でも以前のStep01〜30を学べる状態を維持する。

## Blazor Interactive Server固有の注意

Scopedの寿命はcircuitと結び付いて長くなるため、標準はIDbContextFactoryで操作ごとにコンテキストを作り、await usingで閉じます。Npgsql物理接続プールとDbContextオブジェクトのプールは別です。読み取りはAsNoTracking＋DTO＋取得上限、更新はxminによる楽観的排他と日本語の競合案内を用います。認証/認可の本格実装はStep61以降の課題です。

## ステップ40と41の間

[特別コラム：MudBlazor用途別ガイド](columns/column-mudblazor-by-purpose.md)で、TableとDataGrid、DatePicker、入力、階層、通知、ダイアログ等の使い分けを学び、Step41〜60の実DB一覧連携へ進みます。

Windows Server 2025/IISへの配備、Windows統合認証は後続の教材で扱います。

## Step41〜45のDB一覧連携

Web/Services/Learning/LearningPageService.csで、EF Core 10とPostgreSQLへフィルター・Count・ソート・Skip/Takeを委譲します。MudBlazorのGridState/TableStateはWeb側の表示部品に閉じ、Serviceには単純なLearningPageQueryだけを渡します。従来のシンプルな構成を継続します。
