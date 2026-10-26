# ステップ13：Singleton・Scoped・Transientを比較する

## できるようになること
DIに登録したサービスが「いつ生成され」「どこまで共有されるか」を説明できます。

## 動作の確認
1. `/learn/13`を開く。
2. Singleton・Scoped・Transient（初回）それぞれの識別子を見る。
3. 「Transientをもう1つ取得する」を押して、新しい識別子を確認する。
4. ページを同じタブで移動して戻った場合や、新しいプライベートウィンドウで開いた場合を比較する。

## Program.csの登録
```csharp
builder.Services.AddSingleton<SingletonMarker>();
builder.Services.AddScoped<ScopedMarker>();
builder.Services.AddTransient<TransientMarker>();
```

- **Singleton**：アプリ全体で原則1つのインスタンス。別の利用者とも共有する。
- **Scoped**：スコープごと。Blazor Interactive Serverでは通常のHTTPリクエストとは異なり、対話回線（circuit）に関連して長く維持される。
- **Transient**：DIから要求されるたびに新しいインスタンス。

`Services.GetRequiredService<TransientMarker>()` で新しい部品をその都度取得することが、今回の画面の観察ポイントです。

## 間違いやすい点
Transientを「ボタン操作のたびに画面へ自動注入される」と理解しないこと。新しいインスタンスができるのは、コンテナーへ新しい要求をした時です。

利用者ごとの識別情報や可変状態をSingletonに保持してはいけません。

## 確認問題
- [ ] 3つの生存期間を区別できた
- [ ] Singletonがユーザーごとの状態に向かない理由を説明できた

次：[ステップ14：Scopedとcircuit](step14.md)
