# ステップ16：Optionsで設定を受け取る

## できるようになること
設定値を画面で直接解析せず、ASP.NET Coreの標準ライブラリが提供するDI機能`IOptions<T>`で受け取る方法を理解します。

## 画面で試す
1. `/learn/16` を開く。
2. 教材名「Blazorステップアップ学習」と課題の上限「5件」を確認する。
3. 「設定とDIの関係を確認」を押す。登録と利用の場所を説明する文章が現れる。

## ソースコードのしくみ
`LearningOptions`に`CourseTitle`、`MaxTasks`を定義します。起動時の登録は次のとおりです。

```csharp
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
```

画面は次のように利用します。

```razor
@inject IOptions<LearningOptions> Settings
<p>@Settings.Value.CourseTitle</p>
```

`IOptions<T>`はライブラリが用意した仕組みです。自分で`OptionsService`クラスを作る必要はありません。

## 誤解しやすい点
`IOptions<T>`は設定を受け取る機能です。`IOptionsMonitor<T>`などの別の仕組みと、更新タイミングは同じではありません。また、画面に5件と表示するだけでは制限にはなりません。Step20のサービスでも上限を判定します。

## 演習
- [ ] DI登録、設定の型、画面の利用箇所を説明できる。
- [ ] 画面に設定ファイル読み込み処理が存在しないと確認できる。

次：[ステップ17](step17.md)
