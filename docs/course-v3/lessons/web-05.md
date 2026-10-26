# 学習順005：Visual Studio 2026でWebアプリを起動・デバッグする

## 到達目標

ここまでに、Interactive Serverのイベントがサーバーで動くこと、初回のHTTP取得、HTMLとCSS、ブラウザーの開発者ツールを学びました。最後に、**Visual Studio 2026から.NET 10のWebアプリを起動し、サーバー側のC#をデバッグする**までを一連の操作として実施します。

WinFormsの`Form_Load`や`Button.Click`を調べるときは、通常、自分が起動したデスクトップアプリのプロセスへブレークポイントを置きます。この教材では、ブラウザーの表示プロセスとASP.NET CoreのWebサーバープロセスが分かれています。そのためC#の`BreakpointSample()`を停止させるには、Webサーバー側をデバッグ実行している必要があります。

## 1. 起動・通信・デバッグの構成

| 役割 | この教材での例 | 調べる場所 |
| --- | --- | --- |
| 開発環境 | Visual Studio 2026 | ソリューション、ブレークポイント |
| フレームワーク | .NET 10 / ASP.NET Core | ビルド・実行ログ |
| Webサーバー | Kestrel | サーバープロセスとポート |
| 画面 | Chrome/Edgeなどのブラウザー | Elements・Network・Console |
| C#イベント | `Web05Page.razor`の`BreakpointSample()` | Visual Studioのデバッガー |
| HTTPS | 開発時のTLS接続 | 証明書の信頼状態、ブラウザー警告 |
| 本番配置（別工程） | Windows Server 2025 / IIS | 実機構成・証明書・認証の受入 |

**KestrelとIISは同じものではありません。** この教材の開発中はKestrelでの実行を前提にし、Windows Server 2025のIIS構成やKerberosは後半の段階で学びます。本番のIIS受入をローカルのデバッグ成功で代用してはいけません。

## 2. 起動前に確認する

1. Visual Studio 2026でリポジトリのソリューションを開きます。
2. スタートアッププロジェクトとして`PgBlazorSamples.Web`が起動する設定になっていることを確認します。
3. Webプロジェクトの`PgBlazorSamples.Web.csproj`が`net10.0`を対象としていることを確認します。
4. デバッグ起動前にソリューションをビルドし、NuGetの復元やC#のコンパイルエラーがないことを確認します。
5. PostgreSQL/Active Directoryは、この第1段階では不要です。学習順005のカウンターはWebサーバーだけで動きます。

実装ファイルの主な設定は次のようになっています。

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>PgBlazorSamples.Web</RootNamespace>
    <AssemblyName>PgBlazorSamples.Web</AssemblyName>
  </PropertyGroup>
</Project>
```

上のXMLは、`PgBlazorSamples.Web.csproj`のWeb SDKとターゲット指定だけを抜粋したものです。実際のプロジェクトにはMudBlazor、Npgsqlなどのパッケージ参照もあります。ここではすべてを覚える必要はなく、「Webの起動対象はこのプロジェクト」という点を確認してください。

## 3. C#メソッドとブレークポイント

`Web05Page.razor`にある実装は次のとおりです。

```razor
<p>クリックでサーバー側で実行した回数：
    <strong data-testid="web05-count">@clicks</strong> 回</p>
<button type="button" @onclick="BreakpointSample">
    デバッグ対象の処理を実行する
</button>

@code {
    private int clicks;
    // Visual Studioでここにブレークポイントを置き、サーバー側実行を確認する。
    private void BreakpointSample() => clicks++;
}
```

`BreakpointSample()`の`clicks++`の行にブレークポイントを置きます。メソッドが1行の式形式の場合でも、その行にブレークポイントを置けます。Visual StudioでF5によるデバッグ実行を開始し、ブラウザーで`/learn/web-05`を開いてからボタンを押します。

停止したら、変数`clicks`の値を確認します。1回目は0から1へ、2回目は1から2へ増えます。F10で実行を進め、表示へ戻ってカウントが反映されたことを確認してください。ブレークポイントで停止している間、ブラウザーはサーバー側の処理が進むのを待つため、表示が一時的に応答待ちとなる場合があります。

## 4. Webサーバーを起動する処理を読む

ASP.NET Coreアプリは`Program.cs`でサービスを登録し、要求を処理する仕組みを構成します。以下は**実装に存在する主要設定の抜粋**です。

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 実際のProgram.csには、このほか認証、DI、日本語表示、DB等の設定がある。
```

`AddRazorComponents()`と`AddInteractiveServerComponents()`は、RazorコンポーネントとInteractive Serverの機能をサービスへ登録します。各ステップの画面は`@page`によりルーティングされます。起動プロジェクトは`PgBlazorSamples.Web`で、`Web05Page.razor`だけを個別のEXEとして実行するわけではありません。

## 5. デバッガーと開発者ツールの役割分担

| 問題 | 主に使う道具 | 確認するポイント |
| --- | --- | --- |
| C#が実行されているか | Visual Studioのブレークポイント | `BreakpointSample()`へ到達 |
| 値がどこで変わったか | 変数・呼び出し履歴 | `clicks`の現在値 |
| ボタンがHTMLに存在するか | ブラウザーのElements | `button`要素 |
| サーバーへ接続できるか | Network | 初回GETや対話型接続 |
| ブラウザー側でエラーがないか | Console | 画面の診断メッセージ |
| アプリの起動に失敗する | Visual Studioの出力・サーバーログ | .NET実行、ポート、起動例外 |

Webの学習で重要なのは、「画面が見えない」という一つの症状について、**ブラウザーへの配信の問題**と**サーバー上のC#の問題**を混ぜないことです。

## 6. HTTPSと証明書

開発時のHTTPSでは、開発証明書が信頼されていない場合にブラウザーから警告されることがあります。証明書の警告を無条件に無視して先へ進むのではなく、開発環境で証明書を信頼できるように構成されているかを確認してください。

HTTPSは通信経路の保護です。それだけで利用者の本人認証や業務の認可が完了したことにはなりません。後半のWindows統合認証では、ADで認証した名前と既存DB台帳の権限を別に確認します。今回の`web-05`は、いずれの実認証も試験していません。

## 7. 実習手順

1. Webプロジェクトをスタートアップに設定します。
2. `Web05Page.razor`の`BreakpointSample()`へブレークポイントを置きます。
3. Visual StudioでF5を押してデバッグ起動します。
4. ブラウザーで`/learn/web-05`を開き、初期値「0回」を確認します。
5. 「デバッグ対象の処理を実行する」を押して、Visual Studioで停止することを確認します。
6. `clicks`を確認した後、F10で進み、画面の表示が「1回」に変わったことを確かめます。
7. 同じ操作をもう1回行い、表示が「2回」になることを確認します。
8. 最後に第1組の復習へ進み、HTTP・HTML・C#イベント・デバッグの役割を整理します。

## 8. よくある失敗と対応

| 症状 | 確認事項 | 対応 |
| --- | --- | --- |
| ビルドが失敗する | .NET 10 SDK、NuGet、C#コンパイルエラー | 出力ウィンドウの最初のエラーを確認 |
| URLが開かない | 起動プロジェクト、待受ポート、Kestrelのログ | デバッグ実行対象を確認 |
| 404が返る | `/learn/web-05`の綴り | `@page`とURLを照合 |
| ブレークポイントが止まらない | アタッチしたWebサーバー、ビルド構成、シンボル | 正しいプロセスをF5起動しているか確認 |
| HTMLは出るがクリックに反応しない | Interactive Serverの接続 | Network・Console・サーバー側ログを確認 |
| 再読込後に0へ戻る | 一時的なコンポーネント状態の寿命 | DBへ保存したわけではないと理解 |

## 9. 演習と解答

**問1：** ボタンを押してもブレークポイントが停止しません。最初に確認することは何ですか。

**解答：** WebプロジェクトをVisual Studioでデバッグ実行しているか、`BreakpointSample()`の`clicks++`へ置いているか、対話型接続が成立しているかを確認します。

**問2：** ブレークポイントで停止しているのに、ブラウザーにはボタンが見えています。矛盾していますか。

**解答：** 矛盾しません。ブラウザーは表示済みのHTMLを保持し、サーバーC#の続きが実行されるのを待っているためです。

**問3：** ローカルでHTTPSのページが表示できれば、Windows Server 2025のKerberosや証明書も合格ですか。

**解答：** いいえ。実サーバーでのTLS・IIS・AD・SPN・鍵共有・運用条件の受入は別です。

## 完了チェック

- [ ] Visual StudioでWebアプリをF5実行できた。
- [ ] `BreakpointSample()`で停止し、`clicks`の変化を確認できた。
- [ ] Kestrel、ブラウザー、C#処理の場所を区別できた。
- [ ] 開発用HTTPSと本番用証明書の違いを説明できた。
- [ ] エラーに応じてVisual Studio・Network・Consoleのどれを開くか説明できた。

次は[第1組の復習](../reviews/review-01.md)です。復習後は[学習順006：最初のBlazor画面](../../samples/step01.md)へ進みます。
