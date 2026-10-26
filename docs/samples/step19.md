# ステップ19：画面からApplicationの計算処理を分離する

## このステップでできるようになること
WinFormsでフォーム内に置いていた計算や業務ルールを、画面と無関係なC#クラスへ移す考え方を学びます。教材ではその効果を見せるため、**一度だけ`PgBlazorSamples.Application`というクラスライブラリを使います**。以後のEF Core編でDomain・Infrastructureなどの層別プロジェクトを増やすことを要求するものではありません。

## 前提と期待結果
Step11〜18のサービスとテストを理解してください。DB・ADは不要です。`/learn/19`へ移動します。

1. 最初は「0ステップを学習済み。残り60ステップです。」と表示される。
2. 「5ステップ学習した」を押すと学習済み5、残り55に変わる。
3. 続けて押すと5ずつ増えるが、60を超えない。
4. 「最初から確認する」で0へ戻る。

**60という数字はこの練習用サービスの固定条件です。** このプロジェクト全体には既に80ステップありますが、画面と計算を分離する教材の仕様として60を使用しています。カリキュラムの最新版の総数を表すものではありません。

## Application側に置いた契約と実装
`IStepSummaryService`は表示内容を文章にする操作の約束です。

```csharp
public interface IStepSummaryService
{
    string Describe(int completedSteps);
}
```

実装では、範囲外の値を明示的に拒否してから文章を返します。

```csharp
public sealed class StepSummaryService : IStepSummaryService
{
    public string Describe(int completedSteps)
    {
        if (completedSteps is < 0 or > 60)
            throw new ArgumentOutOfRangeException(
                nameof(completedSteps), "学習済み数は0〜60の範囲です.");

        return completedSteps == 60
            ? "全60ステップを修了しました。"
            : $"{completedSteps}ステップを学習済み。残り{60 - completedSteps}ステップです。";
    }
}
```

`Program.cs`の登録とRazor側の受け取りは次の組み合わせです。

```csharp
builder.Services.AddTransient<IStepSummaryService, StepSummaryService>();
```

```razor
@page "/learn/19"
@inject IStepSummaryService Summary

<p>学習済み：@completed ステップ</p>
<p role="status">@Summary.Describe(completed)</p>
<button @onclick="() => completed = Math.Min(60, completed + 5)">
    5ステップ学習した
</button>
<button @onclick="() => completed = 0">最初から確認する</button>

@code {
    private int completed;
}
```

これは実画面から計算・操作部分を抜き出したものです。`Application`プロジェクトは`Web`のRazor画面を参照せず、**Web → Application**の一方向参照です。

## ボタン操作から画面表示まで
1. ブラウザーのクリック通知をRazorのイベントが受け取る。
2. `Math.Min(60, completed + 5)`により画面内の学習済み数を更新する。
3. 表示時に`Summary.Describe(completed)`を呼ぶ。
4. `StepSummaryService`が0〜60の範囲を検査して日本語の文章を返す。
5. Razorが文字列を描画しブラウザーへ反映する。DBには保存しない。

## なぜ分離するのか
画面レイアウトを変更しても、残数の計算と範囲の検査は変えずに済みます。テストも`new StepSummaryService().Describe(5)`で実行でき、ブラウザーを起動する必要はありません。実際に`GreetingServiceTests`には`Describe(5)`と`Describe(60)`、範囲外の`Describe(61)`を確認する試験があります。

一方、**すべてのサービスを別プロジェクトへ置く必要はありません**。後続のEF Core学習では、小さなWebプロジェクト内の`Data`と`Services`を使い、必要以上の参照関係を増やしません。

## 間違いやすいこと
- インターフェイスだけ定義しても呼び出し先はありません。実装とDI登録も必要です。
- ApplicationをWebへ依存させると、計算処理がRazorやUI技術に引きずられます。
- ボタン側は`Math.Min`で60に抑え、サービス側は範囲外を例外で拒否します。画面の制限とサービス側の条件は役割が異なります。

## 演習と解答
**演習1：** `StepSummaryService.Describe(61)`を直接呼ぶとどうなりますか。

**解答：** `ArgumentOutOfRangeException`が発生します。画面ボタンは61へ進めませんが、サービスは外部から不正な値を渡されても拒否します。

**演習2：** 現時点でEF Coreのためにさらに複数の層別プロジェクトを作る必要はありますか。

**解答：** この教材の方針では必要ありません。まずWeb内のサービスやDataで責務を分けます。

## 完了確認
- [ ] WebからApplicationへの参照方向を説明できる。
- [ ] 表示と計算を別々に変更・テストできる理由が分かる。
- [ ] この画面の60は固定の練習条件であることを理解した。

次は[ステップ20：課題管理のDI総合演習](step20.md)です。
