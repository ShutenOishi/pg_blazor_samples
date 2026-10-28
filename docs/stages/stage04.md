# 補足資料：PostgreSQLとEF Core

## この段階の位置づけ
実DBを利用し、モデル・DbContextFactory・LINQ・更新・トランザクション・競合を理解できる。

## 学習前に確認すること
第3段階の入力画面。PostgreSQL 18の開発専用DBが必要。

## WinForms・Dapperの経験とのつながり
DapperでSQLとDTOを直接扱う方式に対し、EF CoreではモデルとLINQからSQLを生成できる。SQLの理解は引き続き必要。

## 初めに知る用語
Npgsql、DbContextFactory、AsNoTracking、SaveChanges、DateOnly、Range、トランザクション、xmin。

## 学ぶ順番（学習順047〜40）
学習順047〜35でモデル・DB・DI登録・検索を学びコラム08を読む。学習順054〜40でCRUD・型・トランザクション・排他・実DB確認を行い、コラム09と用途別MudBlazor特別コラムへ進む。

## 手を動かして確かめること
同じ検索をSQLとLINQで説明し、更新後の競合を意図的に起こして結果を確認する。

## 次の段階への準備
実DBの一覧を必要な件数だけ安全に取得する。

詳しくは[学習ロードマップ](../learning-path.md)と[ステップ一覧](../sample-catalog.md)を参照してください。
