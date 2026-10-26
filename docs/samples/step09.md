# ステップ09：ライフサイクル・非同期初期化・破棄

**実装：** [親画面](../../src/PgBlazorSamples.Web/Components/Samples/Step09/LifecyclePage.razor)／[観察用子部品](../../src/PgBlazorSamples.Web/Components/Samples/Step09/LifecycleTracker.razor)

## 到達目標
`OnInitializedAsync`、`OnParametersSetAsync`、`IDisposable.Dispose`がそれぞれ異なる役割を持つことを理解する。

## 操作手順
1. `/learn/09`を開き、子コンポーネントのカウンターを読む。
2. 「テーマを変更する」を押し、テーマとパラメーター設定回数の変化を見る。
3. 初期化回数は対話型の**同じインスタンス**では通常1回のままであることを確認する。
4. 「子を非表示にする」を押し、破棄の案内を確認する。
5. 「子を再表示する」を押し、新しい子インスタンスで再び初期化されることを確認する。

## 実装コード
~~~csharp
protected override async Task OnInitializedAsync()
{
    initializationCount++;
    try
    {
        await Task.Delay(50, lifetimeCancellation.Token);
    }
    catch (OperationCanceledException)
    {
        // ページ移動などによる破棄時のキャンセルは想定内です。
    }
}

protected override Task OnParametersSetAsync()
{
    parameterSetCount++;
    return Task.CompletedTask;
}

public void Dispose()
{
    lifetimeCancellation.Cancel();
    lifetimeCancellation.Dispose();
}
~~~

### 注意：事前レンダリングとカウント
Interactive Serverの事前レンダリングを有効にすると、静的HTMLを作るインスタンスと、対話を処理するインスタンスは別になります。このため、`OnInitializedAsync`は**同一ページ表示でも総計2回になることがあります**。画面上の回数は表示中のインスタンスのカウンターであり、サーバー上の総呼出回数を示しません。`OnParametersSetAsync`も親の再描画などにより複数回呼ばれます。

### 実務では
通信・タイマー・イベント購読を開始した場合は、不要な処理のキャンセルや購読解除を考える。コンポーネントの破棄中に`StateHasChanged`を呼ばない。

## 確認問題
- [ ] 初期化とパラメーター変更の役割が異なると説明できる
- [ ] 再表示で新しいインスタンスができることを確認できる
- [ ] キャンセルトークンを破棄時に取り消す理由を説明できる

前：[Step08](step08.md)／次：[Step10](step10.md)