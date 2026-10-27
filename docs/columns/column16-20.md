# 第6組の復習（学習順026〜030）：設定・非同期・テスト・サービス分離

**対象：学習順026〜030（旧Step16〜20）。** Step11〜15で学んだDIの基本を使い、Webアプリの画面とサービスを動かした後に読む復習コラムです。

## 5ステップで一つの仕組みを作った
| ステップ | 新しく学んだこと | 次とのつながり |
| --- | --- | --- |
| 16 | `IOptions<LearningOptions>`で設定を型付きで受け取る | 課題の登録上限をサービスに渡す |
| 17 | `async/await`・`CancellationToken`・`ILogger<T>` | 通信や保存処理の中断、診断へ応用する |
| 18 | `IClock`を固定実装へ交換してxUnitで確認 | 画面に依存せずサービスを試験する |
| 19 | `Application`へ進捗計算サービスを分離 | 画面から機能の契約を呼ぶ |
| 20 | `ITodoService`と`InMemoryTodoService`を組み合わせる | 設定・DI・入力・サービス制約を一画面で利用する |

WinFormsでフォームの中に設定読み込み、`Task.Delay`、ログ記録、計算、課題リストをすべて書くより、画面とサービスそれぞれの役割が分かりやすくなります。インターフェイスや別プロジェクトを作ること自体を目的にしないでください。

## 課題追加の流れを最初から説明する
1. `appsettings.json`にある`Learning:MaxTasks=5`を、`Program.cs`の`Configure<LearningOptions>`で登録する。
2. `ITodoService`を`InMemoryTodoService`としてScoped登録する。
3. ブラウザーから`/learn/20`を開くと、画面はDIで`ITodoService`を受け取る。
4. `InMemoryTodoService`はコンストラクターで`IOptions<LearningOptions>`を受け取り、上限を1〜20件に収める。
5. 入力欄の`oninput`が新しい課題名をRazorの状態に反映する。
6. 追加ボタンが`TryAdd`を実行し、サービスで前後空白、1〜40文字、上限件数を検査する。
7. 成功なら一覧に登録し、失敗ならメッセージを表示する。データベースには保存されない。

これは**ブラウザー → サーバー上のイベント → サービス → メモリの一覧 → Razor再描画**です。後続のDB編では、途中に短命DbContextとSQLの処理が追加されます。

## 登録と利用を見比べる

```csharp
// Program.cs：フレームワークのOptionsと自作の課題サービス
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
builder.Services.AddScoped<ITodoService, InMemoryTodoService>();
```

```razor
@inject ITodoService Tasks
@code {
    private void Add()
    {
        var success = Tasks.TryAdd(newTitle, out message);
        if (success) newTitle = "";
    }
}
```

`InMemoryTodoService`は`IOptions<LearningOptions>`をコンストラクターで受け取ります。**標準ライブラリのDIと自作したサービスのDIを組み合わせる**ときも、画面が依存先の実装を組み立てる必要はありません。

## 似ているが別の機能
| 混同しやすいもの | 違い |
| --- | --- |
| 入力欄の`maxlength`と`TryAdd`での文字数検査 | 前者はブラウザーの入力補助、後者はサービス側の拒否 |
| ScopedとDB保存 | Scopedのメモリ状態はサーバー再起動やcircuit消失に耐える永続保存ではない |
| `CancellationToken`とトランザクションのロールバック | 取消通知だけでは実DB更新の取り消しを保証しない |
| 単体テストと画面の動作確認 | `GreetingServiceTests`はC#の戻り値、ブラウザー試験は画面操作とイベントを確認 |
| インターフェイスとクラスライブラリ | インターフェイスは約束、クラスライブラリへの分割は配置の選択 |

Step19の残り60ステップは**学習用の固定条件**で、現在の全80ステップの収録数とは異なります。教材の総数を表示しているわけではありません。

## 統合演習：実務で発生する5つの場面

**問1：** 初期課題2件、`MaxTasks=2`で3件目を追加しようとした場合はどうなりますか。

**解答：** `InMemoryTodoService`は現在数2を確認し、`TryAdd`を`false`にして「課題は最大2件までです。」を返します。画面の`maxlength`だけで制限しているわけではありません。

**問2：** `Task.Delay(800, current.Token)`からトークンを外すとどうなりますか。

**解答：** 待機に取消が伝わらず、取消表示の後に正常終了の処理が動く可能性があります。時間のかかる処理のAPI自体に取消通知を渡す必要があります。

**問3：** 時刻が正午かどうかをテストするのに、実際のサーバーが正午になるまで待つ必要がありますか。

**解答：** ありません。`IClock`を固定時計へ交換し、`TestClock(12)`などの値を渡して試験します。

**問4：** `StepSummaryService.Describe(61)`を呼ぶとどうなりますか。

**解答：** 範囲外のため`ArgumentOutOfRangeException`が発生します。画面のボタンで上限を超えないようにしていても、サービス自身が入力を検査します。

**問5：** `ITodoService`を将来DB実装へ変えた場合、保存方法を変えるだけで認可や監査まで保証できますか。

**解答：** できません。契約を維持して実装を交換することはできますが、DB側の同時更新、本人認可、監査の保存は別の設計と試験が必要です。

## 次へ進む前に
「画面の入力」「DIの登録」「サービスの制約」「保存する場所」「試験の種類」の違いを説明できれば、この段階の中心事項を理解できています。

続く[特別コラム05：ライブラリのDI](column-library-di.md)では、**自作のDIの仕組みを知ることに加え、既存ライブラリが用意したDIを読み、利用する方法**を学びます。その後のMudBlazorで実際に使います。

## 第6組の比較・診断と実践課題

| 学習順 | DIから受け取るもの | WinForms＋Dapperでの対応 | 確認する場所 |
| --- | --- | --- | --- |
| 026 | `IOptions<LearningOptions>` | 型付き設定の取得 | `/learn/16` |
| 027 | `ILogger<AsyncLoggingPage>` | 非同期・取消・ログ | `/learn/17` |
| 028 | `IClock` | 時刻依存のテスト容易化 | `/learn/18` |
| 029 | `IStepSummaryService` | Formから計算を分離 | `/learn/19` |
| 030 | `ITodoService` | 入力画面と処理を分離 | `/learn/20` |

```csharp
builder.Services.Configure<LearningOptions>(
    builder.Configuration.GetSection("Learning"));
builder.Services.AddTransient<IStepSummaryService, StepSummaryService>();
builder.Services.AddScoped<ITodoService, InMemoryTodoService>();
```

```razor
@inject ITodoService Tasks
@code {
    private void Add()
    {
        var success = Tasks.TryAdd(newTitle, out message);
        if (success) newTitle = "";
    }
}
```

| 症状 | 切り分けの場所 | 確認の基準 |
| --- | --- | --- |
| 設定上限の表示と追加結果が違う | `IOptions`と`Math.Clamp` | 画面は生の設定値、サービスは1〜20 |
| 取消のあと完了表示になる | `Task.Delay`と`ReferenceEquals` | 現行処理以外が状態を変更しない |
| 12時のテストが失敗 | `GreetingService` | 12時は午後の挨拶 |
| 65ステップへ進める | `Math.Min`と`Describe` | 練習上限は60 |
| 6件目を登録できる | `TryAdd` | 初期2件＋追加3件で上限5 |

**統合演習6：** `ILogger<T>`を利用するため、アプリごとに自分でロガー実装を作り`AddScoped`する必要がありますか。

**詳しい解答：** 必要ありません。標準のDIから`ILogger<T>`を取得できます。一方、`ITodoService`と`InMemoryTodoService`の対応はアプリ側で登録しています。ライブラリが何を登録してくれるのかを確認する点が重要です。

**統合演習7：** `InMemoryTodoService`がScopedだからDB保存の代わりになりますか。

**詳しい解答：** なりません。これはメモリ上の練習データであり、永続化、認証、認可、監査、更新競合の処理は実装されていません。実業務のDBへ移すには別途検証が必要です。

復習画面`/learn/column/20`の後は、`/learn/column/library-di`へ進みます。ライブラリのDI登録やProviderを読み取る練習をしてから、MudBlazorを扱う学習順031〜035へ進んでください。
