# ステップ16：Optionsで設定を受け取る

## このステップでできるようになること
WinFormsで設定ファイルの値をフォームから直接取得していた場合を思い出してください。ASP.NET Coreでは、設定を型付きのオブジェクトとしてDIから受け取る**Optionsパターン**を利用できます。設定を読む処理と画面表示を分離し、後続の課題管理でも同じ設定を使う方法を学びます。

この画面のC#はBlazor Interactive Serverの**サーバー側**で動作し、ブラウザーへは教材名と課題上限の表示結果が送られます。ブラウザーがサーバーの`appsettings.json`を直接読むわけではありません。

## 前提・環境・操作
Step11〜15のDI登録と`@inject`の知識が前提です。Webアプリを起動して`/learn/16`を開きます。DB・ADは不要です。

1. 「教材名：Blazorステップアップ学習」と「課題の上限：5件」を確認する。
2. 「設定とDIの関係を確認」を押すと、`Program.cs`で登録し`IOptions<LearningOptions>`から受け取る説明が表示される。
3. もう一度押すと説明が隠れる。これは画面の`showCode`という状態を切り替えるイベントです。

## コードを順番に読む
設定ファイルの学習部分は次の値です。

```json
"Learning": {
  "CourseTitle": "Blazorステップアップ学習",
  "MaxTasks": 5
}
```

`LearningOptions`はこの2項目の受け皿です（Webプロジェクト内の`Services/Learning`にあります）。

```csharp
public sealed class LearningOptions
{
    public string CourseTitle { get; set; } = "Blazorステップアップ学習";
    public int MaxTasks { get; set; } = 5;
}
```

`Program.cs`では設定の`Learning`節を、ASP.NET Coreが用意するOptionsに結び付けます。

```csharp
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
```

Razor画面は設定ファイルのパスを知らず、型として受け取った値を表示します。

```razor
@page "/learn/16"
@using Microsoft.Extensions.Options
@inject IOptions<LearningOptions> Settings

<p>教材名：@Settings.Value.CourseTitle</p>
<p>課題の上限：@Settings.Value.MaxTasks 件</p>
<button @onclick="() => showCode = !showCode">設定とDIの関係を確認</button>
@if (showCode)
{
    <p>Program.csで設定を登録し、IOptionsから受け取ります。</p>
}
@code {
    private bool showCode;
}
```

これは説明に必要な箇所の抜粋です。実際の画面には日本語見出しと学習ナビゲーションも含まれます。`LearningOptions`の名前空間は`PgBlazorSamples.Web.Services.Learning`です。

## 操作が表示に届く流れ
1. アプリの起動処理で設定ソースを構成し、`LearningOptions`のバインドをDIへ登録する。
2. `/learn/16`のコンポーネントが`IOptions<LearningOptions>`を受け取る。
3. `Settings.Value`から`CourseTitle`と`MaxTasks`を取得して表示する。
4. ブラウザーでボタンを押すとサーバー上の`showCode`が反転し、表示する段落が変わる。

設定表示にDB問い合わせはなく、ボタンを押しても設定値を再ロードしません。

## IOptions・IOptionsSnapshot・IOptionsMonitorの違い
`IOptions<T>`は基本的な型付き設定の利用方法です。設定変更を即時反映したい場合は、`IOptionsSnapshot<T>`や`IOptionsMonitor<T>`などを用途に応じて選びます。ただし、**Blazor Interactive ServerのScopedが通常の1回のHTTP要求とは異なる**ため、「Snapshotならクリックごとに更新」とは考えません。この教材では動的変更を実装せず、`IOptions<T>`を用います。

`MaxTasks=5`と画面に表示するだけでは件数の保証になりません。Step20では`InMemoryTodoService`が設定値を読み、追加処理の側で上限を確認します。

## よくある間違いと切り分け
- `GetSection("Learning")`の名前がJSONと異なると、想定した値にバインドできません。
- `IOptions<T>`の利用には`Microsoft.Extensions.Options`の名前空間が必要です。
- Optionsを使うと設定内容が自動的に安全になるわけではありません。資格情報を画面へ表示しないでください。
- 利用者別の業務権限を`appsettings.json`だけで判定する設計にはしません。後続の認可編ではDBの既存台帳を正本とします。

## 演習と解答
**演習1：** 設定の課題上限を5から3へ変更し、アプリを再起動すると画面は何を表示しますか。

**解答：** 変更した`Learning:MaxTasks`が正しく読み込まれれば「3件」です。画面は変更しなくても、Optionsから取得した値を使います。

**演習2：** 最大件数を画面に表示しているのに、サービス側でも確認する必要があるのはなぜですか。

**解答：** 表示上の制限は処理の実行を禁止する保証にはなりません。保存や追加を行うサービス自身が検査する必要があります。

## 完了確認
- [ ] JSON、`LearningOptions`、DI登録、Razorの対応を追える。
- [ ] Optionsがサーバー側で使われることを説明できる。
- [ ] 表示制御とサービス側の上限検査を区別できる。

次は[ステップ17：非同期処理・取消・ログ](step17.md)へ進みます。
