# ステップ02：URLと画面遷移

**対応するソース：** [NavigationPage.razor](../../src/PgBlazorSamples.Web/Components/Samples/Step02/NavigationPage.razor)、[NavMenu.razor](../../src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor)、[Routes.razor](../../src/PgBlazorSamples.Web/Components/Routes.razor)

**状態：コードを作成済み。動作確認結果はCIに従って更新します。**

## 到達目標

ルーティングとは何かを理解し、`NavLink`を使ってページの遷移と現在地の表示を行うこと。

## 前提

ステップ01まで。DB・ADは不要です。

## 実装の場所

`NavigationPage.razor`には次の行があります。

```razor
@page "/learn/02"
```

そして、画面移動のための`NavLink`が配置されています。

```razor
<NavLink href="/learn/03" Match="NavLinkMatch.All">ステップ03へ進む</NavLink>
```

`href`で行先を指定し、`Match`で選択中の画面と一致するか判断します。`NavMenu.razor`はすべてのステップに共通する左側の学習メニューを表示します。

## 動作確認

1. `/learn/02`へアクセスします。
2. 「学習の目次」「ステップ01へ戻る」「ステップ03へ進む」のリンクを操作します。
3. 左側の学習メニューで選択中の項目が強調されることを確認します。
4. 存在しない`/learn/99`にアクセスし、「ページが見つかりません」の日本語画面を確認します。

## よくある間違い

`@page`に指定したルートとリンクの`href`を別の文字列にすると、期待した画面に移動できません。現在地の強調表示も`NavLink`側のURLと一致方法に依存します。

## 演習

`NavMenu.razor`のステップ02の**表示名だけ**を別の日本語に変更し、ルーティングは変わらないことを確かめてください。

## 完了条件

- [ ] 画面を移動できる
- [ ] `@page`と`href`の対応を説明できる
- [ ] 選択中のメニューと未存在ページを確認できる

前：[ステップ01](step01.md)／次：[ステップ03](step03.md)
