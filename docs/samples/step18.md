# ステップ18：DIと単体テストを理解する

## このステップでできるようになること
実行する時刻やブラウザーの状態に左右されず、C#のサービスが正しい値を返すか確かめます。WinFormsでフォームを起動せずに業務計算クラスを試験するのと同じ考え方です。Step12で学んだ`IClock`を使い、実時刻への依存をテスト時に置き換えます。

## 前提と画面での確認
Step11〜17が前提です。PostgreSQL・ADは不要です。`/learn/18`を開きます。

1. 「午前8時：おはようございます」「午後1時：こんにちは」を確認する。
2. 「固定時計で再確認する」を押しても同じ結果になることを確認する。
3. **この画面を見ただけで単体テスト成功とは判定しません。** 次のxUnitテストを別途実行します。

## 時計と挨拶サービスの関係

```csharp
public interface IClock
{
    DateTimeOffset GetNow();
}

public sealed class GreetingService(IClock clock)
{
    public string GetMessage() => clock.GetNow().Hour < 12
        ? "おはようございます"
        : "こんにちは";
}

public sealed class FixedClock : IClock
{
    public DateTimeOffset GetNow() =>
        new(2026, 1, 1, 8, 0, 0, TimeSpan.FromHours(9));
}
```

画面の`UnitTestPage.razor`は午前の`FixedClock`と、午後1時を返すページ内の`AfternoonClock : IClock`を使います。どちらもサーバーの実時刻を問い合わせません。

```csharp
private void Compare()
{
    morningMessage = new GreetingService(new FixedClock()).GetMessage();
    afternoonMessage = new GreetingService(new AfternoonClock()).GetMessage();
}

private sealed class AfternoonClock : IClock
{
    public DateTimeOffset GetNow() =>
        new(2026, 1, 1, 13, 0, 0, TimeSpan.FromHours(9));
}
```

## 実際のxUnitテスト
`PgBlazorSamples.UnitTests`の`GreetingServiceTests.cs`には次の試験があります。

```csharp
[Fact]
public void 午前8時ならおはようございます()
{
    var service = new GreetingService(new FixedClock());
    Assert.Equal("おはようございます", service.GetMessage());
}

[Theory]
[InlineData(0, "おはようございます")]
[InlineData(11, "おはようございます")]
[InlineData(12, "こんにちは")]
[InlineData(23, "こんにちは")]
public void 時間帯で挨拶が決まる(int hour, string expected)
{
    Assert.Equal(expected,
        new GreetingService(new TestClock(hour)).GetMessage());
}
```

`TestClock`もテストファイル内で`IClock`を実装し、指定した`hour`の日時を返します。これにより、境界の11時・12時も実際の時刻に関係なく繰り返し検証できます。

## テストを実行する
Visual Studioのテストエクスプローラーで`GreetingServiceTests`を選びます。CLIならソリューションの作業ディレクトリから次を実行します。

```powershell
dotnet test tests/PgBlazorSamples.UnitTests/PgBlazorSamples.UnitTests.csproj
```

失敗時は「期待値」「実際値」「実行したテストケース」を確認します。ブラウザーを起動して結果を見直す前に、サービスとテストの条件が一致しているか調べます。

## 単体テストとブラウザー・実DBテストの境界
- **単体テスト**：`GreetingService`に固定時計を渡して返る文字列を検査する。WebサーバーやDBを使わない。
- **ブラウザー試験**：ボタン、画面の再描画、入力と結果のつながりを検査する。
- **結合テスト**：実際のPostgreSQLやEF Coreなど、複数の処理の連携を確認する。後半で扱う。
- **現地受入**：AD/IISの本当の認証や配備は別の環境と証跡で判断する。

## 誤解とトラブル
- `new`をテストで使っても、DIの設計と矛盾しません。依存先をコンストラクターから渡す設計だから自由に組み立てられます。
- DIに登録したから自動的にテストが作られるわけではありません。
- `[Theory]`で指定した値が期待と違う場合、`GetMessage`の午前判定（`Hour < 12`）を見直します。

## 演習と解答
**演習1：** 午前11時と正午12時は同じ挨拶になりますか。

**解答：** なりません。現在の実装では11時は「おはようございます」、12時は「こんにちは」です。

**演習2：** 画面の`Compare()`が動けば単体試験は不要ですか。

**解答：** 不要ではありません。コードの期待結果を独立した試験として実行・記録することに意味があります。

## 完了確認
- [ ] テスト実行と画面表示を分けて確認した。
- [ ] テスト時だけ実時刻を置き換える理由を説明できる。
- [ ] 11時・12時の境界条件を説明できる。

次は[ステップ19：Applicationへの分離](step19.md)です。
