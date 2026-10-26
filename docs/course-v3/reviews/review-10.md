# 第10組の復習：DapperとEF Coreの入口
**対象：学習順46〜50**

## 5つのテーマ
1. DapperとEF Coreの役割
2. EntityとDTO
3. PostgreSQL DDL
4. DI登録
5. 短命DbContextFactory

## 問題
Interactive ServerのScoped DbContextを全操作で共有すべきですか？

## 解答
いいえ。Factoryから操作ごとに短命DbContextを作ります。

## 演習
画面に戻り、処理の場所とデータの保存有無を確認します。SQLを文字列結合しないこと、DbContextの寿命、業務認可と監査を別に考えることが重要です。
