# 学習順39：ダークモードとキーボード操作を確認する

**実装ソース：** [対応するRazor実装](../../../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L039AccessibilityPage.razor)。画面の正規URLは `/learn/039` です。

**実習URL：`/learn/039`。** 第4段階・第8組の4本目です。ここで扱う架空データ・模擬処理は業務DBに保存されません。

## できるようになること
利用者が使いやすい業務画面にするため、テーマ変更だけでなく**キーボード操作・フォーカス・読み上げ・拡大時のレイアウト**を確認します。WinFormsで`TabIndex`を管理するのと同じく、Webでも操作順序と見える焦点が大切です。

## 前提と操作
学習順031〜28のMudBlazor部品を理解してから`/learn/039`を開きます。DB・ADは不要です。

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


## WinForms＋Dapper経験者のための対応表

| 確認点 | WinForms | Blazor Interactive Server | 実務で見る場所 |
| --- | --- | --- | --- |
| 表示テーマ | Formの色・設定 | MudThemeProviderとScoped | 共有レイアウト |
| 入力フォーカス | TabIndex・Focus | HTMLの自然な順序とTab | ブラウザー上の焦点 |
| 状態の寿命 | フォームの生存期間 | circuit内のScoped | 再接続・再読込 |
| 結果の伝達 | Label・MessageBox | role=statusの本文 | 読み上げと画面 |
| データ取得 | DapperのSQL | この画面では未使用 | DB操作は後の段階 |

この教材の`ThemePreferenceService`は`SetDarkMode`で値が変化した場合だけ`Changed`イベントを発火します。共通レイアウトはその通知を購読し、`MudThemeProvider`を更新します。ページだけで色を管理すると別ページへ移るたびに状態が別々になりやすいため、対話回線内で共有するサービスをDIから受け取ります。ただしScopedは永続保存ではありません。ブラウザーの再読込やcircuitの再作成で初期値へ戻る可能性があることも実験で確かめてください。

## キーボードと表示の実習

1. `/learn/039`を開き、最初のライトモード表示を確認する。
2. マウスでスイッチを操作し、状態の日本語表示が変わることを確認する。
3. Tabで入力欄へ移動し、キーボードで文字を入力する。
4. Tabから確認ボタンへ移動し、Enterで実行して本文のメッセージを読む。
5. Shift+Tabで戻り、フォーカスがどこにあるかを視認できるか確認する。
6. ブラウザーを200%程度に拡大し、ラベルやボタンが見切れないか確認する。倍率は観察条件の例であり、画面が200%で自動試験済みという意味ではありません。

| 症状 | 切り分ける場所 | 対応 |
| --- | --- | --- |
| ダークモードが反映されない | Changedとレイアウトの購読 | 値変更通知を確認する |
| ページ移動後に色が戻る | Scopedの寿命と回線 | 同一circuitか確認する |
| Enterで操作できない | フォーカス位置 | Tab順を確認する |
| 入力の読み上げが分からない | Labelとrole=status | 項目名・結果の存在を確認する |
| 拡大で横にはみ出す | コンポーネント幅 | レスポンシブ配置を見直す |

## 演習の詳しい解答

**演習2：** `ThemePreferenceService`をSingletonにしたら何が問題ですか。

**解答：** Singletonは全利用者の回線で可変状態を共有し得るため、ある利用者のテーマ変更が別の利用者にも影響する設計になります。利用者固有の表示設定をSingletonのグローバル状態で管理することは避けます。今回のScopedは対話回線に沿う一時状態の例であり、保存先ではありません。

**演習3：** スイッチをマウスで切り替えられたので、キーボード操作試験を省略できますか。

**解答：** 省略できません。マウスで操作できても、Tabで到達しない、焦点が見えない、読み上げで項目名を判別できない問題が残り得ます。視力や操作環境によって利用できる入力方法が変わるので、操作順・焦点・ラベル・結果通知・拡大表示を別々の観点で確認します。

## 演習と解答
**問：** テーマ変更を全利用者共通のSingletonに保存してよいですか。

**答え：** 利用者別の可変状態としては適切ではありません。この教材では対話回線に関連するScopedサービスに保持しています。

## 完了確認
- [ ] テーマ切替と本文メッセージを確認した。
- [ ] キーボードで入力と確認を操作できた。
- [ ] Scopedと共有レイアウトの再描画通知の関係を説明できる。

次は[学習順040：総合演習](L040UiPracticePage.md)です。
