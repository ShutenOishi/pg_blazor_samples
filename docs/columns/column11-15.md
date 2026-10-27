# 第5組の復習：DIの登録・交換・生存期間・注入

学習順021〜025をまとめます。ここでは独立した五つの画面が、一つのサービス構成へどのようにつながるかを学びます。DIに登録すれば機能がすべて自動で完成するという誤解を避けるため、登録・解決・イベント・描画・保存と認可を分けて説明してください。

## 5画面をつなげて操作

| 学習順 | URL | 操作 | 説明する内容 |
| --- | --- | --- | --- |
| 021 | /learn/11 | 時刻を読み直す | 直接newと注入の差 |
| 022 | /learn/12 | 固定時計（午前8時） | 同一契約で実装を交換 |
| 023 | /learn/13 | Transientをもう1つ取得する | 新しい解決と単なる再描画 |
| 024 | /learn/14 | 親・子のボタン | 同じScopedと再描画通知 |
| 025 | /learn/15 | 練習花子を入力 | @injectとコンストラクター注入 |

## DIと画面のコードを読む

```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<FixedClock>();
builder.Services.AddTransient<GreetingService>();
builder.Services.AddTransient<GreetingFormatter>();
builder.Services.AddScoped<ScopedCounter>();
```

この登録でIClockの標準解決はSystemClockです。固定時計は具象型として取得できます。GreetingFormatterはGreetingServiceを要求し、その先でIClockが必要になります。ScopedCounterはcircuit内の状態共有を練習するための型で、業務DBの代わりに使いません。

```razor
@inject ScopedCounter Counter
<SharedCounter CurrentCount="@Counter.Count" OnChanged="Refresh" />
@code {
    private void Refresh() => StateHasChanged();
}
```

親がCurrentCountを子に渡し、子は同じScopedCounterを変更した後で親に通知します。共有されたサービスが変更されたという事実だけでは表示が自動的に最新になると保証できません。WinFormsで画面状態を共有する場合と同じように、データと表示更新の責任を分けます。

## 比較表

| 用語 | 役割 | 混同してはいけないこと |
| --- | --- | --- |
| IClock | 時刻取得という契約 | DIに自動登録される仕組みではない |
| @inject | Razorへサービスを受け取る | クリックごとの再解決ではない |
| コンストラクター | C#サービスが依存先を受け取る | 実装の具象型に固定する必要はない |
| Singleton | 提供元全体で共有する | ユーザー固有データの保存先ではない |
| Scoped | スコープ内で共有する | PostgreSQLへの永続保存ではない |
| Transient | 解決要求ごとに生成する | 描画ごとに自動生成する意味ではない |

## 症状の切り分け

| 症状 | 調べること | 解決の手掛かり |
| --- | --- | --- |
| 画面の初期化に失敗 | 登録漏れ | 最も内側の依存先IClockまで確認 |
| 再描画しても識別子が同じ | 初回注入済みサービスを使っている | 新規解決の有無を区別 |
| 親だけ古いCountを表示 | EventCallbackを送っていない | 子のOnChanged.InvokeAsyncを確認 |
| 固定時計でも現在時刻になる | SelectedClockが実時計を選択 | useFixedを確認 |
| 再接続で値が消えた | circuitと保存の寿命 | 練習用Scoped値は永続化しない |

## 実際に試す総合演習

最初に学習順021を開き、直接生成とDIの挨拶が同じ時間帯で一致することを確認します。次に022で固定時計を押して2026/01/01 08:00と『おはようございます』を確認します。このときDIへ登録されたSystemClockがFixedClockへ置き換わったわけではないことを説明してください。023へ進んで初回Transientと再取得したTransientの識別子を記録し、同じ画面の再描画と新規解決を混同しないようにします。024では親から1増加、子から1増加、0へリセットの順に操作し、親子が同じ値を表示する理由とEventCallbackの必要性を説明してください。最後に025で入力欄を『練習花子』とし、FormatterからClockまでの依存関係を声に出してたどります。

ここではWindows認証やDapperによるDB更新を行いません。画面の練習が成功した事実と、業務アプリが認可・監査付きで安全に動く事実は別です。業務処理ではADから本人とユーザー名だけを得て、既存DB/外部台帳の権限を毎回確認する必要があります。ロック、xmin、監査が同一トランザクションで保証できない更新は拒否します。

## 演習と詳しい解答

**問1：** GreetingFormatterを注入したがIClockを登録し忘れました。何が起き、どこを直しますか。

**解答1：** Formatter→GreetingService→IClockの依存連鎖の最も内側を解決できず、Formatterも作れません。Program.csでIClockの型と登録を確認します。画面の例外が出たからといって@injectを消して手動newへ戻す解決は適切ではありません。

**問2：** Transientの初回識別子がボタン後も同じなのは異常ですか。

**解答2：** 異常ではありません。InitialTransientは画面が作成された時点で受け取った同じオブジェクトです。追加解決用のボタンでGetRequiredServiceを実行した場合の識別子と比較します。

**問3：** 子のボタンを押した後、親の表示更新に必要な仕組みは何ですか。

**解答3：** 子がScopedCounter.Incrementを実行した後、OnChanged.InvokeAsyncで親を通知し、親のRefreshがStateHasChangedを呼ぶ構成です。サービスが共有されていることと通知を送ることは別の責任です。

**問4：** 固定時計への切替がテストに役立つ理由は何ですか。

**解答4：** 実際のサーバー時刻に左右されず、午前と午後の表示を再現できます。IClockという契約を通じて同じGreetingServiceへ別の時計を渡せます。

**問5：** Singletonへ利用者名と検索条件を入れてよいですか。

**解答5：** 不適切です。複数利用者が同じ提供元から参照する可能性があり、状態の混在や漏えいにつながります。利用者と業務権限は信頼できる本人情報とDB正本に照合します。

**問6：** コンストラクター注入と@injectを併用できますか。

**解答6：** できます。RazorコンポーネントでFormatterを受け取り、FormatterからGreetingService、GreetingServiceからIClockを受け取る例がこれに当たります。

## さらに考えること

WinFormsで作ったIRepositoryの実装をDIへ登録するとき、長時間生きるサービスが短命なDB接続を保持し続けるとどうなるか考えてください。ここではDB接続を実際に使っていませんが、サービスの寿命はリソースの管理に関わります。後続ではOptionsを使う設定注入、ILoggerとCancellationToken、固定時計でのxUnit、処理を分離する設計を扱います。各項目を覚える目的は、画面に直接処理を増やすことなく必要な部品を安全に利用するためです。
