# 学習コラム08：WebプロジェクトだけでEF Coreを始める

## Step31〜35のつながり
Step31でEntityとDTOを区別し、Step32でPostgreSQL型とDDLを確認し、Step33でEF Core 10プロバイダーを追加し、Step34でDbContextFactoryの寿命を扱い、Step35でAsNoTracking・Where・Selectによる検索を学びます。

## 余計な層を増やさない理由
EF CoreはWeb内のData/LearningDbContext.csに置きます。SQL実行はServices/Learning/LearningDbService.csで完結します。別のDomain、Infrastructure、Repository、抽象インターフェイスが必須というわけではありません。必要なときにだけ取り出せます。

## DIを外部ライブラリに適用する
~~~csharp
builder.Services.AddDbContextFactory<LearningDbContext>(
    options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<LearningDbService>();
~~~
Blazorのcircuitは長く続くため、DbContextは1操作ごとに生成・破棄します。Npgsql接続プールとDbContextプールは別です。

## 誤解と確認問題
**Q. 1画面ならDbContextをInjectしてずっと保持してよい？** A. 標準ではIDbContextFactoryを使い、操作単位で生成します。

**Q. DB検索の結果をすべて画面に持ってくる？** A. いいえ。検索・並べ替え・件数制限はDBに実行させます。

**Q. CRUDをするために新たな4層プロジェクトが必要？** A. いいえ。今回のWebプロジェクトだけで始めます。

次：Step36〜40の更新・競合・期間・DBテスト。
