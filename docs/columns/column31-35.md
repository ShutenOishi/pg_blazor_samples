# 学習コラム08：WebプロジェクトだけでEF Coreを始める

## 学習順047〜35のつながり
学習順047でEntityとDTOを区別し、学習順048でPostgreSQL型とDDLを確認し、学習順049でEF Core 10プロバイダーを追加し、学習順050でDbContextFactoryの寿命を扱い、学習順051でAsNoTracking・Where・Selectによる検索を学びます。

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

次：学習順054〜40の更新・競合・期間・DBテスト。
