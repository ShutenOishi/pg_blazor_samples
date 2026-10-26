# ステップ13：Singleton・Scoped・Transientを比較する

## このステップで学ぶこと
WinFormsではアプリ全体で一つだけ保持するオブジェクトと、画面を開くたびに作るオブジェクトを明示的に管理してきました。DIには、部品をどの範囲で再利用するかを示す**生存期間（ライフタイム）**があります。`/learn/13`の識別子を使い、その違いを観察します。

## 前提と実行条件
ステップ11・12のDI登録を理解してください。DB・ADは不要です。画面には個人情報ではなく、インスタンス識別のための短いランダムな文字列が表示されます。

## まず操作する
1. `/learn/13`を開き、Singleton、Scoped、Transient（初回）の識別子を確認する。
2. 「Transientをもう1つ取得する」を押す。再取得用の識別子が表示される。
3. 繰り返して押し、新しい識別子へ変わることを確認する。
4. 別のプライベートウィンドウから同じ画面へ入り、Scopedの識別子を比較する。
5. 同じタブ内の再描画、ページ移動、全体再読込は異なる操作です。接続や事前レンダリングで生成が変わるため、観測した条件も記録します。

## Program.csと部品の構成
Webアプリでは以下の登録を使用します。

```csharp
builder.Services.AddSingleton<SingletonMarker>();
builder.Services.AddScoped<ScopedMarker>();
builder.Services.AddTransient<TransientMarker>();
```

各型は生成時に識別子を作ります。以下は共通の考え方を示す例です。

```csharp
public sealed class TransientMarker
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
}
```

`SingletonMarker`と`ScopedMarker`にも同じ`Id`プロパティがあり、各クラス自体に特別なDIコードは不要です。ライフタイムは登録時に指定します。

画面は次のように解決済みインスタンスを受け取っています。

```razor
@inject SingletonMarker Singleton
@inject ScopedMarker Scoped
@inject TransientMarker InitialTransient
@inject IServiceProvider Services

<p>Singleton：@Singleton.Id</p>
<p>Scoped：@Scoped.Id</p>
<p>Transient（初回）：@InitialTransient.Id</p>
<p>Transient（再取得）：@additionalTransientId</p>
<button @onclick="ResolveAnother">Transientをもう1つ取得する</button>

@code {
    private string additionalTransientId = "まだ取得していません";

    private void ResolveAnother()
        => additionalTransientId =
            Services.GetRequiredService<TransientMarker>().Id;
}
```

このコードは実ページの表示部分を抜き出したものです。`GetRequiredService`はDIに新しい解決を要求します。サービスロケーターを業務ロジックの標準に推奨する例ではなく、**Transientを観察するための操作**です。

## 3つの違いを整理する

| ライフタイム | 生成と共有 | 業務データへの注意 |
| --- | --- | --- |
| Singleton | アプリのサービス提供元で原則一つ。利用者間でも共有する | 本人名や利用者固有の可変状態を保存しない |
| Scoped | スコープの範囲で共有。Interactive Serverの対話中はcircuitに関連して維持される | 認証・接続・回線断や再作成との関係を確認する |
| Transient | DIに解決を要求するたびに新しく生成する | `@inject`済みの値がボタンのたびに自動交換されるわけではない |

通常のHTTP要求とBlazor Interactive Serverの継続的な操作では、Scopedの範囲の考え方が異なります。これはステップ14で具体的に確認します。

## 処理の順番
1. サーバーがRazorコンポーネントを作り、DIが三種類のサービスを注入する。
2. 画面へ識別子を表示する。識別子はサーバー側のオブジェクトが生成したものです。
3. ボタンを押すと`ResolveAnother`が実行される。
4. `IServiceProvider`へTransientを要求し、作られたインスタンスの`Id`を状態へ代入する。
5. Blazorが変更後の状態をブラウザーへ表示する。

## 失敗しやすい点
- Singletonを「同じ画面だけで共有」と考えない。別利用者にも共有され得ます。
- Transientを「イベント1回につき自動で1個作る」と考えない。`InitialTransient`は既に注入済みです。
- 自分で`new`したオブジェクトはDI登録のライフタイム管理を受けません。
- SingletonへScoped依存を直接取り込む設計は寿命の不整合を招きます。起動時のDI検証や例外を確認します。

## 演習と解答
**演習1：** ある利用者の検索条件をSingletonフィールドへ格納してよいですか。

**解答：** いけません。他の利用者にも共有される可能性があります。そもそも検索条件をどこまで保持すべきかも後続の状態管理で検討します。

**演習2：** `InitialTransient`の識別子は、ボタンを押すたびに必ず変わりますか。

**解答：** 変わりません。新たに取得する`additionalTransientId`が変化します。

## 完了確認
- [ ] 3種類の生成範囲を説明できる。
- [ ] Transientを再取得する操作とコンポーネントの再描画を区別できる。
- [ ] 利用者情報をSingletonへ格納しない理由を説明できる。

次は[ステップ14：Scopedとcircuit](step14.md)へ進みます。
