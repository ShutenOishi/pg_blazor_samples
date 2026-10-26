# ステップ29：ダークモードとキーボード操作を確認する

## できるようになること
利用者が使いやすい業務画面にするため、テーマ変更だけでなく**キーボード操作・フォーカス・読み上げ・拡大時のレイアウト**を確認します。WinFormsで`TabIndex`を管理するのと同じく、Webでも操作順序と見える焦点が大切です。

## 前提と操作
Step21〜28のMudBlazor部品を理解してから`/learn/29`を開きます。DB・ADは不要です。

1. `MudSwitch`で「ダークモードを有効にする」を切り替える。
2. 「現在の表示：ダークモード」または「ライトモード」が変わることを確認する。
3. Tab・Shift+Tab・Enter・スペースで切替、入力、確認を試す。
4. 入力欄を空で確認すると「入力が空です。」、架空の文字列を入力すると「入力された文字： 」に続いて内容が表示される。
5. ブラウザーを拡大し、画面要素の折り返しと焦点の視認性を確認する。

## Scopedのテーマサービス
`Program.cs`では利用者の対話回線ごとの状態として登録します。

```csharp
builder.Services.AddScoped<ThemePreferenceService>();
```

サービスは現在の値と変更通知のイベントを持ちます。

```csharp
public sealed class ThemePreferenceService
{
    public bool IsDarkMode { get; private set; }
    public event Action? Changed;

    public void SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode) return;
        IsDarkMode = isDarkMode;
        Changed?.Invoke();
    }
}
```

共通レイアウトの`MainLayout.razor`はこのサービスを注入し、`MudThemeProvider`へ現在の値を渡します。

```razor
<MudThemeProvider IsDarkMode="@ThemePreference.IsDarkMode" />
```

レイアウトは`ThemePreference.Changed`を購読して`StateHasChanged`を呼びます。破棄時に購読を解除します。切り替えを画面内のメッセージだけで完結させず、アプリ全体のテーマ表示にも反映する構造です。

## ページ側の操作コード

```razor
@inject ThemePreferenceService ThemePreference

<MudSwitch T="bool" Value="@ThemePreference.IsDarkMode"
           ValueChanged="ChangeTheme"
           Label="ダークモードを有効にする" />
<p role="status" aria-live="polite">
    現在の表示：@(ThemePreference.IsDarkMode ? "ダークモード" : "ライトモード")
</p>
<MudTextField T="string" Label="操作確認の入力欄" @bind-Value="_name" />
<MudButton OnClick="CheckKeyboard">入力状態を確認する</MudButton>
<p role="status">@_state</p>

@code {
    private string _name = "";
    private string _state = "まだ操作していません。";

    private void ChangeTheme(bool value)
        => ThemePreference.SetDarkMode(value);

    private void CheckKeyboard()
        => _state = string.IsNullOrWhiteSpace(_name)
            ? "入力が空です。"
            : "入力された文字： " + _name.Trim();
}
```

上記は`AccessibilityPage.razor`の操作部分の抜粋です。色変更自体はOSの設定を変更せず、このアプリのScopedテーマサービスへ反映します。

## 実際の処理順
1. ブラウザーの`MudSwitch`操作をサーバーの`ChangeTheme`が受け取る。
2. `ThemePreference.SetDarkMode`が`IsDarkMode`を変更する。
3. `Changed`イベントを購読する共通レイアウトが再描画される。
4. `MudThemeProvider`の`IsDarkMode`へ反映され、ブラウザーの表示が変わる。

## 重要な注意
- ダークモードにしただけでアクセシビリティ対応完了にはなりません。
- 小さい文字、低コントラスト、キーボードで届かない操作、見えない焦点、拡大時の横スクロールを個別に確認します。
- `Scoped`にしたから永続保存されるわけではありません。circuitが失われると設定が消えることがあります。
- 色だけでエラー・成功を示さないでください。

## 演習と解答
**問：** テーマ変更を全利用者共通のSingletonに保存してよいですか。

**答え：** 利用者別の可変状態としては適切ではありません。この教材では対話回線に関連するScopedサービスに保持しています。

## 完了確認
- [ ] テーマ切替と本文メッセージを確認した。
- [ ] キーボードで入力と確認を操作できた。
- [ ] Scopedと共有レイアウトの再描画通知の関係を説明できる。

次は[ステップ30：総合演習](step30.md)です。
