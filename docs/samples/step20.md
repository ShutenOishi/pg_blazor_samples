# ステップ20：DIで課題管理を作る

## できるようになること
画面は契約を使い、保存方法はDIで差し替えられる形にして、課題追加・完了切替を実装します。

## 実際に操作する
1. `/learn/20`を開き、初期課題が2件あることを確かめる。
2. 空欄で「課題を追加する」を押す。「課題名は1〜40文字で入力してください。」が表示される。
3. 「DIを復習する」と入力し追加する。登録件数は3件になる。
4. チェックボックスで課題の完了状態を切り替える。
5. 上限5件を超える追加を試み、拒否されることを確認する。

## 仕組み
`ITodoService`と`TodoItem`はApplicationプロジェクトにあります。`InMemoryTodoService`はWebプロジェクトに置きます。

```csharp
builder.Services.AddScoped<ITodoService, InMemoryTodoService>();
```

画面は`@inject ITodoService Tasks`で機能を受け取ります。`InMemoryTodoService`内部は`IOptions<LearningOptions>`をDIで受け取って上限を確認します。このように**ライブラリ由来のDIと自作DIを組み合わせられます**。

## 重要
- 今はメモリ上の練習用データのみ。DBには保存しません。
- ScopedはInteractive Serverの対話回線単位に関連します。ブラウザー接続の切断や再起動でデータが消える場合があります。
- 複数利用者で共有するSingletonに課題を保存しません。

## チェック
- [ ] 空欄を拒否できた
- [ ] 追加・完了切替ができた
- [ ] インターフェイスと保存実装の位置を説明できる

復習：[コラム04](../columns/column16-20.md)／[特別コラム05：ライブラリのDI](../columns/column-library-di.md)
