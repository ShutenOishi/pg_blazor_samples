# ステップ15：@injectとコンストラクター注入

## できるようになること
Razor画面と通常のC#サービスで、DIから部品を受け取る方法が違うことを理解します。

## 画面を操作する
1. `/learn/15`を開く。
2. 名前欄が「練習太郎」のとき、「練習太郎さん、こんにちは。」または「練習太郎さん、おはようございます。」と表示される。
3. 名前を「練習花子」に変更して、表示だけが変わることを確認する。

## 実装コードを読む
```razor
@inject GreetingFormatter Formatter
```

```csharp
public sealed class GreetingFormatter(GreetingService greeting)
{
    public string Format(string name) =>
        $"{name}さん、{greeting.GetMessage()}。";
}
```

`@inject`はコンポーネントで使う指定です。一方、通常のC#クラスはコンストラクターで依存サービスを受け取ります。`GreetingFormatter` と `GreetingService` の組み立てはDIコンテナーが行います。

## よくある間違い
- `@inject`を使えば、サービス登録が不要になるわけではありません。
- DIは「何でもSingletonにする」ことではありません。必要な生存期間を設計します。
- ここでは画面内表示のみで、ユーザー認証やDB保存は行いません。

## 演習と理解度チェック
- [ ] 名前を変更すると挨拶が変わる
- [ ] `@inject` とコンストラクター注入を区別できる
- [ ] DIの登録場所と利用場所を説明できる

復習：[コラム03：DIを整理する](../columns/column11-15.md)
