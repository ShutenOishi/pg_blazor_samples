# 特別コラム05：ライブラリが提供するDIを理解して使う

**対象：ステップ11〜20を終えた方。MudBlazorの導入前に読む特別編。**

## なぜ、この理解がBlazorを始めやすくするのか

Blazorでは、機能をすべて自作する必要はありません。ASP.NET Coreが用意する機能や、外部ライブラリの機能を、決められた手順で登録し利用できます。

多くの入門者は「DIのインターフェイスや実装クラスを自分で作る方法」を覚えた段階で難しさを感じます。しかし実務では、**すでに用意されたサービスの登録方法と受け取り方を読めること**も同じくらい重要です。MudBlazorのダイアログ・通知、ログ、設定、HTTP通信、PostgreSQL接続などを組み合わせる土台になります。

**結論：まず「部品の中身」を作る前に、「使い方の契約」を読む。**

## 登録から利用まで：3つの責任

```text
(1) ライブラリ側：機能・契約・登録用Add...メソッドを提供
                    ↓
(2) Program.cs：利用する機能をサービス群へ登録
                    ↓
(3) RazorやC#クラス：@inject／コンストラクターで受け取る
                    ↓
       必要に応じてProviderの配置や描画モードを指定
```

### 1. ライブラリのDI登録方法を探す

まず公式のインストール手順で次の項目を確認します。

- NuGetパッケージ名、対象.NETバージョン
- `using`が必要な名前空間
- `builder.Services.Add...()`等の登録メソッドと必須の設定
- 画面で注入するサービス型
- Singleton／Scoped／Transientの寿命
- Providerなど追加するRazorコンポーネントの有無
- 対話型描画、サーバー実行、認証、接続管理などの要件

`IServiceCollection`は**登録時**に使うサービス一覧です。`IServiceProvider`は**必要な部品を解決する**仕組みです。通常の画面やサービスは`IServiceProvider`から毎回手作業で取り出すのではなく、`@inject`やコンストラクターで必要な型を明示します。

### 2. ASP.NET Coreの標準機能を使う

```csharp
// 起動時：学習用設定をOptionsへ登録します。
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
```

```razor
@inject IOptions<LearningOptions> Settings
@inject ILogger<SomePage> Logger
@inject NavigationManager Navigation
```

このうち`IOptions<T>`は設定を型付きで受け取る機能、`ILogger<T>`はログ記録機能、`NavigationManager`はURL・画面移動の機能です。後者の一部はフレームワークがすでに登録しており、独自の`AddLoggerService()`などを作る必要はありません。

**実習：** `/learn/column/library-di` を開き、教材名と現在のURLが表示されることを確かめ、「ILoggerで学習用ログを書く」を押します。サービスの実装を自作せずに機能を使えます。

### 3. 外部UIライブラリ：MudBlazorの例（今後のステップで実装）

MudBlazorには、サービス登録用の拡張メソッドが用意されています。パッケージを導入したら、そのバージョンの公式手順に合わせます。

```csharp
using MudBlazor.Services;
builder.Services.AddMudServices();
```

ただし、**登録だけでは完成しません**。ダイアログや通知の利用には`MudDialogProvider`・`MudSnackbarProvider`などのProviderコンポーネント、対応するテーマ・スクリプト・スタイル、**同じ対話型描画モード**が必要です。

「ダイアログが開かない」場合、DI登録、Provider、描画モードの順に確認すると、闇雲にコンポーネントを作り直さずに原因を絞り込めます。

この教材のStep21より前にはMudBlazorはまだ導入していません。上記は公式の導入手順を読む練習です。

### 4. PostgreSQLライブラリ：Npgsqlの例（DB学習編で実装）

Npgsqlでは`NpgsqlDataSource`が接続設定・接続プールを管理します。アプリ全体で再利用するDataSourceと、操作ごとに開いて破棄する接続は、**生存期間が異なります**。

DI用拡張パッケージや登録方法は、使用するNpgsqlバージョンと構成に合わせて公式手順を確認します。`NpgsqlConnection`をSingletonに登録して使い回したり、長寿命Scopedの接続を対話回線に抱え続けたりしないでください。

この学習ページはPostgreSQLへ接続せず、接続文字列・ID・パスワードの入力も必要ありません。

### 5. 一覧で確認：自作することと利用すること

| 何を使う？ | 自分で作る部分 | 用意されている部分 |
| --- | --- | --- |
| 課題管理 | ITodoService、InMemoryTodoService | ASP.NET Core DIコンテナー |
| 設定の読み取り | LearningOptions | IOptions、設定バインド機能 |
| ログ | どの場面で何を記録するか | ILogger、ログ基盤 |
| MudBlazor | 画面構成・入力項目・認可 | UI部品、登録メソッド、Provider |
| Npgsql | データ取得・更新の業務ルール | DataSource、接続プール、接続API |

## 初学者が引っかかりやすい5つの落とし穴

1. **Add...を呼べば全部動くと考える**：Providerや描画モード、設定が必要な場合があります。
2. **NuGetパッケージの追加だけで登録されたと思う**：利用手順でDI登録を確認します。
3. **すべてをSingletonにする**：利用者別の状態、長寿命接続、破棄が必要な部品に注意します。
4. **見つからないサービスを`new`で回避する**：正しいライブラリ登録方法を確認し、根本原因を直します。
5. **画面が動けば権限も問題ないと思う**：UI表示とサーバー側の認可は別です。

## 確認問題と解答

**Q1. `ILogger<T>`が使えるのに、ログ用の実装クラスを新しく書くべき？**

A. 通常は不要です。標準のDIで受け取り、必要なログを記録します。

**Q2. MudBlazorを導入し、`AddMudServices()`を呼んだのにダイアログが表示されません。まず何を調べる？**

A. 対応するProvider配置、対話型描画モード、パッケージと必要な静的アセット、バージョンに合った公式手順です。

**Q3. NpgsqlDataSourceとNpgsqlConnectionは同じ寿命で保持する？**

A. 違います。DataSourceは共有・再利用する設計が一般的ですが、接続は利用ごとに取得・破棄します。

**Q4. ライブラリから取得したサービスがユーザー別の情報を保持する場合は？**

A. ドキュメントのライフタイム仕様を確認し、ユーザー間で共有されるSingletonへの保管を避けます。

## 次に読むもの
Step21以降でMudBlazorの導入・画面部品・Providerを実際に操作し、本コラムで覚えた「公式手順を読む → 登録する → 注入する → 画面で使う」を再確認します。

**参考資料：** [BlazorのDI（Microsoft Learn）](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0)、[MudBlazor公式インストール手順](https://mudblazor.com/getting-started/installation)、[Npgsql公式Basic Usage](https://www.npgsql.org/doc/basic-usage.html)。
