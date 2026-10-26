# ステップ12：インターフェイスで実装を交換する

## できるようになること
`IClock` という共通の約束で、実際の時計と固定時計を交換できます。

## 操作手順
1. `/learn/12` を開く。
2. 「実際の時計」を押す。現在時刻に応じた挨拶を表示する。
3. 「固定時計（午前8時）」を押す。
4. 時刻が **2026/01/01 08:00**、挨拶が **おはようございます** になることを確かめる。

## 実際のコード
```csharp
public interface IClock
{
    DateTimeOffset GetNow();
}
```

```csharp
public sealed class FixedClock : IClock
{
    public DateTimeOffset GetNow() =>
        new(2026, 1, 1, 8, 0, 0, TimeSpan.FromHours(9));
}
```

画面は `IClock SelectedClock => useFixed ? FixedClock : SystemClock;` で実装を選択します。`GreetingService` が受け取る型は常に `IClock` です。

## 大切な区別
このページのボタンによる切り替えは、学習のために両方の実装を登録して選んでいます。アプリ全体の実装を切り替える場合は、DI登録先を変更して使います。

## 演習・確認
- [ ] 固定時計の結果が時間帯に左右されない
- [ ] `IClock` と `FixedClock` の役割の違いを説明できる
- [ ] `GreetingService` に時計の実装の詳細を持たせない理由を説明できる

次：[ステップ13：サービスの生存期間](step13.md)
