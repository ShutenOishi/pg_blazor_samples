# 学習順26：Optionsで設定を受け取る

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage03/L026OptionsPatternPage.razor)。画面の正規URLは `/learn/026` です。

対象画面は`/learn/026`です。WinForms＋Dapperの経験と結び付けて理解します。

## このステップでできるようになること
WinFormsで設定ファイルの値をフォームから直接取得していた場合を思い出してください。ASP.NET Coreでは、設定を型付きのオブジェクトとしてDIから受け取る**Optionsパターン**を利用できます。設定を読む処理と画面表示を分離し、後続の課題管理でも同じ設定を使う方法を学びます。

この画面のC#はBlazor Interactive Serverの**サーバー側**で動作し、ブラウザーへは教材名と課題上限の表示結果が送られます。ブラウザーがサーバーの`appsettings.json`を直接読むわけではありません。

## 前提・環境・操作
学習順021〜15のDI登録と`@inject`の知識が前提です。Webアプリを起動して`/learn/026`を開きます。DB・ADは不要です。

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
@page "/learn/026"
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
2. `/learn/026`のコンポーネントが`IOptions<LearningOptions>`を受け取る。
3. `Settings.Value`から`CourseTitle`と`MaxTasks`を取得して表示する。
4. ブラウザーでボタンを押すとサーバー上の`showCode`が反転し、表示する段落が変わる。

設定表示にDB問い合わせはなく、ボタンを押しても設定値を再ロードしません。

## IOptions・IOptionsSnapshot・IOptionsMonitorの違い
`IOptions<T>`は基本的な型付き設定の利用方法です。設定変更を即時反映したい場合は、`IOptionsSnapshot<T>`や`IOptionsMonitor<T>`などを用途に応じて選びます。ただし、**Blazor Interactive ServerのScopedが通常の1回のHTTP要求とは異なる**ため、「Snapshotならクリックごとに更新」とは考えません。この教材では動的変更を実装せず、`IOptions<T>`を用います。

`MaxTasks=5`と画面に表示するだけでは件数の保証になりません。学習順030では`InMemoryTodoService`が設定値を読み、追加処理の側で上限を確認します。

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

## 学習順026：WinForms＋Dapperから設定を読む場所を変える
`/learn/026`ではブラウザーが設定ファイルを取得するのではなく、サーバー上のRazorが`IOptions<LearningOptions>`から教材名と上限を取り出します。WinFormsでは`ConfigurationManager`などをフォームから直接呼ぶこともありますが、DIへ設定を登録することで、表示と設定の取得方法を分けられます。プログラム起動時の`Configure<LearningOptions>`が設定節を対応させ、学習順030で同じ設定を使うサービスにも渡せます。Dapperの接続文字列や認証情報を画面へ表示する例と混同してはいけません。

| 比較 | WinForms＋Dapper | Blazorでの対応 | 読み取る場所 |
| --- | --- | --- | --- |
| 設定値 | フォームで設定のキーを読むことがある | `IOptions<LearningOptions>`を注入 | サーバーC# |
| 設定形式 | キーと文字列を個別に扱う | `CourseTitle`と`MaxTasks`の型付き値 | `LearningOptions` |
| 入力制約 | TextBox側と保存処理で検査 | 表示と`TryAdd`を分けて検査 | 学習順030のサービス |
| SQL | Dapperの接続設定が別途必要 | この画面はDBを使わない | DB演習は後続 |

`IOptions<T>`で受け取る値が常に正しいとは限りません。たとえば`Learning:MaxTasks`を100に設定すると、画面は100件と表示しますが、`InMemoryTodoService`では`Math.Clamp(100, 1, 20)`により実際の上限が20件になります。表示値とサービスで採用した値が異なることはバグとは限りませんが、利用者に何を表示するかは別途設計する必要があります。`IOptionsSnapshot<T>`はHTTP要求単位のScopedという基本を持ち、Interactive Serverのクリックごとに設定を読み直す意味ではありません。このサンプルは自動再読込を実装していません。

| 症状 | 切り分けの確認先 | 対応 |
| --- | --- | --- |
| 値が既定値のまま | `Learning`節と`GetSection("Learning")` | キーの綴りを揃える |
| 再起動せず変更が見えない | `IOptions<T>`とプロセス起動 | 演習ではアプリ再起動で比較 |
| 表示上限より早く拒否される | `Math.Clamp`と`TryAdd` | サービス上限の1〜20を確認 |
| 利用者別の権限が表示されると思った | 業務認可の取得元 | 権限は既存DB台帳から毎操作検査 |

### 追加演習と解答
**演習3：** 上限を0にしたとき、学習順026の表示と学習順030の追加許可は同じですか。

**解答：** 学習順026は0を表示しますが、学習順030のサービスは上限1へ補正します。初期課題が2件あるため新規追加はできません。画面の表示を検査の代わりにはできません。

**演習4：** ライブラリが提供する`IOptions<T>`を自作のクラスに置き換える必要はありますか。

**解答：** ありません。標準のサービス登録を読み、型・寿命・変更時の振る舞いを理解したうえで利用する方が、余計な自作コードを増やさずに済みます。続く学習順027の`ILogger<T>`も標準DIの利用例です。

## 完了確認
- [ ] JSON、`LearningOptions`、DI登録、Razorの対応を追える。
- [ ] Optionsがサーバー側で使われることを説明できる。
- [ ] 表示制御とサービス側の上限検査を区別できる。

次は[学習順027：非同期処理・取消・ログ](step17.md)へ進みます。
