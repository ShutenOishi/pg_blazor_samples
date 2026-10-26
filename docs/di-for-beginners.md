# 初めてのDI — 依存性注入とプロジェクト分割

## DI（依存性注入）とは

ある処理が利用する部品を、**その処理の中で直接作るのではなく、外側で組み立てて渡す**仕組みです。最初は難しい設計を覚える必要はありません。「時計を固定したテストができるようになる」を目的にします。

- **インターフェイス**: 何ができるかを宣言した約束
- **実装クラス**: 約束を具体的に実行する部品
- **DIコンテナー**: 必要な部品を登録し、組み立てるASP.NET Coreの仕組み
- **コンストラクター注入**: 必要な部品を生成時に受け取ること
- **Composition Root**: 部品の登録・組立を行う起動地点（通常はWebの`Program.cs`）

## 最小サンプル（ステップ11〜15で実装済み）

```csharp
public interface IClock
{
    DateTimeOffset GetNow();
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset GetNow() => DateTimeOffset.Now;
}

// このサービスはWebプロジェクト内に実装しています。
public sealed class GreetingService(IClock clock)
{
    public string GetMessage()
        => clock.GetNow().Hour < 12 ? "おはようございます" : "こんにちは";
}
```

```csharp
// Program.cs：部品の作り方を登録する
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<FixedClock>();
builder.Services.AddTransient<GreetingService>();
```

```razor
@page "/learn/11"
@inject GreetingService Greeting

<h1>DIの練習</h1>
<p>@Greeting.GetMessage()</p>
```

練習: ステップ12で実際の時計と固定時計（午前8時）を切り替える。アプリ全体で実装を交換する場合はDI登録の`IClock`の実装先を変更する。

## ライフタイムを必ず体験する

| 種類 | 登録例 | 重要な注意 |
| --- | --- | --- |
| Singleton | `AddSingleton` | 全利用者が共有。ユーザー固有の状態を置かない |
| Scoped | `AddScoped` | Interactive ServerではHTTP要求単位ではなく**circuitに紐づき得る** |
| Transient | `AddTransient` | 解決の都度生成。破棄とコストも考慮 |

学習ページで別ブラウザー/別タブを開き、状態がどう変わるかを比較します。長寿命のScopedサービスでNpgsql接続やlinq2dbの変更可能な状態を共有しません。

## プロジェクト分割は順番に覚える

| いつ | 作るもの | 初学者に伝えること |
| --- | --- | --- |
| ステップ01 | `PgBlazorSamples.Web` | まず画面・イベント・状態を知る |
| ステップ11〜12 | Web内の`Services`と`IClock` | DIとインターフェイスを体験する |
| ステップ18〜20 | Applicationライブラリ＋単体テスト | 業務処理を画面から分ける理由を体験する |
| ステップ31〜33 | DomainとInfrastructure | 業務ルールとDB実装を分離する |
| ステップ40以降 | DB結合テスト | 実環境依存の試験と単体テストを区別する |

構成の目標は`Web → Application → Domain`、`Infrastructure → Application/Domain`です。Webが起動時にInfrastructureを登録し、ApplicationはInfrastructureの実装を参照しません。詳細は[アーキテクチャ](architecture.md)。

## DIから学ぶセキュリティの注意

- DIで認証済み利用者をSingletonに保存してはいけない。
- DIにDB接続やユーザーのWindowsトークンを無制限に保管しない。
- 「学習用の固定利用者」と「Windows統合認証済み利用者」を同一視しない。
- 本番環境で学習用の偽認証を有効化しない。

## 合格基準

- [ ] インターフェイスと実装クラスの違いを言える
- [ ] DIの登録と`@inject`で利用できる
- [ ] Singleton/Scoped/Transientの違いを動作で確認できる
- [ ] 固定時計によるテストが書ける
- [ ] Web/Application/Domain/Infrastructureを分ける理由を説明できる

参照: [BlazorのDI公式文書](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0)
