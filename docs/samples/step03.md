# ステップ03：Razor構文とC#の値表示

**対応するソース：** [RazorSyntaxPage.razor](../../src/PgBlazorSamples.Web/Components/Samples/Step03/RazorSyntaxPage.razor)

**状態：Windows/LinuxのReleaseビルドとLinuxのHTTP画面描画をCIで確認済み。実ブラウザーでの操作・読み上げ等は未確認です。**

## 到達目標

`@code`で定義されたC#の値を画面へ表示できること、日付を日本語で書式設定できること、通常の文字列表示ではHTMLエンコードが働くことを理解します。

## 前提

ステップ01〜02。DB・ADは不要です。

## 実装と説明

`RazorSyntaxPage.razor`の`@code`には次の固定値があります。

```csharp
// Step03の値は架空の固定データです。DB・ADへの接続は行いません。
private readonly string displayName = "学習者";
private readonly int stepCount = 3;
private readonly DateTime sampleDate = new(2026, 10, 26);
```

この値は画面内の`@displayName`や`@stepCount`で参照します。日付は`CultureInfo.GetCultureInfo("ja-JP")`を使って日本語表示します。

また、次の値も画面に表示します。

```csharp
// HTMLと混同されないよう、通常の文字列として表示します。
private readonly string sampleText = "<重要>これは文字列です</重要>";
```

Razorでは、この値を通常の文字列として表示すると、山括弧がHTMLタグとして動作しないようにエンコードされます。業務システムでユーザー入力を扱う場合も重要な基本です。

## 動作確認

1. `/learn/03`へアクセスします。
2. 表示名が「学習者」、表示するステップ数が「3」、日付が「2026年10月26日」であることを確認します。
3. `<重要>これは文字列です</重要>`が、画面上で文字列として表示されることを確認します。
4. `@code`内の固定値を変更し、画面を再表示して変化を確認します。

## 演習

`displayName`を架空の日本語名へ変更します。`stepCount`を4にして表示が変わることを確認します。実在の利用者情報は使用しないでください。

## よくある間違い

C#の文法エラーが出た場合はセミコロンや引用符の対応を確認します。日付表示が異なる場合は`ToString`の書式指定とカルチャーを確認してください。

## 完了条件

- [ ] 文字列・整数・日付を画面へ表示できる
- [ ] `@code`の役割を説明できる
- [ ] HTMLエンコードの目的を説明できる

次のステップ04では`@onclick`によるイベント処理を学びます。
