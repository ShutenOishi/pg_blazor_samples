# ステップ19：画面からApplicationを分離する

## できるようになること
サービスの契約と業務処理をWeb画面から切り離し、クラスライブラリへ置く理由を説明できます。

## 実際に操作する
1. `/learn/19`を開く。「0ステップを学習済み。残り60ステップです。」が表示される。
2. 「5ステップ学習した」を押し、残り55になることを確認する。
3. 「最初から確認する」で元へ戻す。

## 実装
`PgBlazorSamples.Application`に`IStepSummaryService`と`StepSummaryService`を配置します。Web側で次のDI登録をします。

```csharp
builder.Services.AddTransient<IStepSummaryService, StepSummaryService>();
```

画面側は`@inject IStepSummaryService Summary`として`Summary.Describe(completed)`を呼び出すだけです。

**依存関係の向き：Web → Application**。ApplicationからWebを参照しません。

## チェック
- [ ] Applicationのサービスを使って画面が更新される
- [ ] UIの変更と計算の変更を別々に扱える理由を説明できる
- [ ] プロジェクトが増えた理由を答えられる

次：[ステップ20](step20.md)
