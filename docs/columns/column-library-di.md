# 特別コラム05：ライブラリのDIを理解して利用する

**対象：学習順021〜20の修了者。** MudBlazorを学ぶ前に、ASP.NET CoreやNuGetライブラリが用意する機能を安全に利用する方法を確認します。

## なぜライブラリのDIを学ぶのか
DIというとインターフェイス・実装クラス・登録処理をすべて自作するように見えます。しかしBlazorの実務では、**既に用意されているDI登録方法を理解して使う作業**が少なくありません。

学習順026の`IOptions<T>`、学習順027の`ILogger<T>`、Razor画面の`NavigationManager`、MudBlazorのダイアログ・通知、EF CoreのDbContextFactoryなど、利用するライブラリが登録方法や寿命を定めています。WinFormsでNuGetパッケージやコントロールを導入した経験を、**登録 → 注入 → 実行条件の確認**という順序に置き換えて考えます。

## 自作する部分とライブラリが担う部分
| 利用場面 | 自分で作るもの | ライブラリが提供するもの |
| --- | --- | --- |
| 学習用設定 | `LearningOptions`と設定の項目 | `IOptions<LearningOptions>`、構成のバインド、DI |
| 非同期処理のログ | どの操作をどのレベルで記録するか | `ILogger<T>`とログ基盤 |
| 現在URLと画面遷移 | 行き先の決定、入力・表示 | `NavigationManager` |
| MudBlazorのUI | 業務画面、入力項目、操作と認可 | コンポーネント、登録メソッド、Provider |
| EF CoreとPostgreSQL | クエリ・サービス・トランザクション上の判断 | EF Core/NpgsqlのDBアクセス基盤 |

ライブラリのDIがあるからといって、**画面固有の業務判断や権限検査まで提供されるわけではありません**。

## 登録・解決・画面表示の区別
1. NuGetパッケージと対応する.NETバージョンを確認する。
2. 必要な名前空間と`builder.Services.Add...()`などの登録方法を調べる。
3. 登録が何を追加し、Singleton・Scoped・Transientのどれを使うか公式説明で確認する。
4. Razorなら`@inject`、通常のC#サービスならコンストラクターから受け取る。
5. UIライブラリならProvider、CSS、JavaScript、描画モードなど、DI以外に必要な要素も確認する。
6. 正常動作だけでなく、未登録・寿命の不一致・必要なProviderの不足を切り分ける。

`IServiceCollection`は主として登録時に使い、`IServiceProvider`は登録済みの型を解決します。通常の業務サービスでは`IServiceProvider.GetRequiredService`を至るところで呼ぶより、必要な依存先をコンストラクターへ明示します。

## この教材で動く実例：IOptions・ILogger・NavigationManager
`/learn/column/library-di`を開いてください。教材名、現在の画面URL、ログ出力ボタンが表示されます。

```razor
@page "/learn/column/library-di"
@using Microsoft.Extensions.Options
@inject IOptions<LearningOptions> Settings
@inject ILogger<ColumnLibraryDiPage> Logger
@inject NavigationManager Navigation

<p>Optionsが返す教材名：@Settings.Value.CourseTitle</p>
<p>NavigationManagerが返す現在の画面：
   @Navigation.ToBaseRelativePath(Navigation.Uri)</p>
<button @onclick="WriteLog">ILoggerで学習用ログを書く</button>
<p role="status">@message</p>

@code {
    private string message = "ログはまだ書き込んでいません。";

    private void WriteLog()
    {
        Logger.LogInformation("ライブラリDIコラムの学習用操作を確認しました。");
        message = "学習用ログを書き込みました。";
    }
}
```

実際のページには見出し・詳細説明・ナビゲーションもあります。画面を開くと、`Navigation.ToBaseRelativePath(Navigation.Uri)`はアプリのベースURLから相対的な現在パス（通常`learn/column/library-di`）を表示します。ログボタンを押すとサーバー側の`ILogger`が実行され、画面のメッセージが変わります。ログ本文を画面に直接出しているわけではありません。

`Program.cs`では学習用Optionsを登録しています。

```csharp
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
```

`ILogger<T>`と`NavigationManager`はASP.NET Core/Blazorの仕組みで提供されるため、このサンプルのための独自`AddLoggerService()`や`AddNavigationManager()`を追加していません。

## MudBlazorの例：DI登録だけで完成しない
このリポジトリの現在のアプリには既にMudBlazorと`MudBlazor.Translations`が導入されています。**学習順としては学習順031で導入方法を初めて説明します**。現在のコードでの登録例は次のとおりです。

```csharp
using MudBlazor.Services;
using MudBlazor.Translations;

// Program.cs
builder.Services.AddMudServices();
builder.Services.AddLocalization();
builder.Services.AddMudTranslations();
```

これに加えてテーマ、CSS・静的アセット、ダイアログや通知のProvider、Interactive Serverで動作するコンポーネントの配置を確認します。例えば`MudDialogProvider`・`MudSnackbarProvider`が必要な機能もあります。追加するパッケージ・Providerの要件は導入するバージョンの公式ドキュメントと実装を確認してください。

**「AddMudServicesを呼んだのにダイアログが出ない」場合の確認順**は、パッケージのバージョン、DI登録、Provider、描画モード、必要な静的アセット、ブラウザーのConsoleとサーバーログです。原因が分からないままProviderを重複配置しないでください。

翻訳パッケージが提供する定型文の日本語化と、業務アプリの独自ラベル・入力エラーの日本語化は別です。

## データアクセスの例：EF CoreとNpgsql
後続の学習順047〜40では`IDbContextFactory<LearningDbContext>`をWeb側に登録します。DbContextは操作に応じて短時間で生成・破棄し、Blazorの長寿命circuitに接続を保持し続ける設計と区別します。

Npgsqlの接続プールや`NpgsqlDataSource`をアプリ側で共有する場合と、SQL実行時に利用する`NpgsqlConnection`やDbContextの寿命も異なります。アプリ全体で一つのDB接続インスタンスを使い回す構成は避けます。このコラムの画面自体はDBへ接続しません。

## 業務画面で考慮する必要があること
DI登録の正しさと、セキュリティの保証は異なります。業務の本人認証はADの認証結果、認可は事前整備済みのDB台帳が正本です。画面コンポーネントを隠しただけではサービスの認可になりません。ライブラリに頼る範囲と、アプリが保証する範囲を区別してください。

## 間違えやすいこと
- パッケージのインストールとDI登録を同じ作業だと思わない。
- 注入が成功しても、Providerや描画モードが不要とは限らない。
- Singletonなら何を格納してもよいと考えない。ユーザー別の可変状態やDB接続に注意する。
- ライブラリで利用できる標準機能を、理由なく別名の自作サービスへ作り直さない。
- バージョン間で公開APIが変わる可能性を考え、コードとドキュメントの対象バージョンをそろえる。

## 確認問題と解答
**問1：** `ILogger<T>`を使うために、毎画面で`new`する専用ロガーを作る必要がありますか。

**解答：** このアプリでは不要です。フレームワークがDIで提供する`ILogger<T>`を注入します。

**問2：** `AddMudServices()`を登録したのにダイアログが表示されません。DI登録をもう一度追加すればよいですか。

**解答：** 追加する前にProvider、対話型描画モード、必要なアセットとバージョンを確認します。

**問3：** `NpgsqlConnection`を一つのSingletonとして保持して全利用者でSQLを実行してよいですか。

**解答：** その設計は避けます。接続プールを利用し、操作ごとに適切に接続・DbContextを生成・破棄します。

**問4：** `IOptions<LearningOptions>`を注入できても、課題追加件数の拒否は自動的に実行されますか。

**解答：** されません。`InMemoryTodoService.TryAdd`が現在件数と設定上限を比較し、拒否します。

**問5：** ログの文字列が日本語であれば機密情報も自由に書いてよいですか。

**解答：** いけません。内容、アクセス権、保存期間などを別に設計する必要があります。

## 次のステップ
[学習順031](../samples/step21.md)でMudBlazorの登録と日本語化を学び、以降のフォーム・表・ダイアログを使いながら、**ライブラリの機能・DI登録・Providerの役割**を再確認します。

参考：[Microsoft Learn: BlazorのDI](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0)、[MudBlazor公式Installation](https://mudblazor.com/getting-started/installation)、[Npgsql Basic Usage](https://www.npgsql.org/doc/basic-usage.html)。
