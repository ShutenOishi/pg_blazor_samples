# 第11組の復習：LINQ・追跡・CRUD・型
**対象：学習順51〜55**

## 5つのテーマ
1. LINQとAsNoTracking
2. SQLとLINQの対応
3. 変更追跡
4. CRUD
5. DateOnly/Range

## 問題
ToQueryStringだけでDBは更新されますか？

## 解答
いいえ。SQL生成のみではSELECTも更新も実行されません。

## 演習
画面に戻り、処理の場所とデータの保存有無を確認します。SQLを文字列結合しないこと、DbContextの寿命、業務認可と監査を別に考えることが重要です。
