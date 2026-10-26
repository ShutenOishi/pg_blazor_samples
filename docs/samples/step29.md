# ステップ29：ダークモードとアクセシビリティ

## できるようになること
共通MudThemeProviderとcircuitごとのScopedサービスの役割を理解し、日本語UIをキーボードでも操作します。

## 実際に操作する
1. 「/learn/29」を開いて表示モードを確認。
2. 「ダークモードを有効にする」を切り替え、全体の色が変化することを確認。
3. Tabキーで入力欄と確認ボタンへ進み、Enterキーで確認します。
4. 画面を拡大し、レイアウトの折り返しと焦点の視認性を確認。

## 重要なコード
\`\`\`csharp
// Circuitごとの状態。別の利用者の設定をSingletonへ入れない。
builder.Services.AddScoped<ThemePreferenceService>();
\`\`\`

\`\`\`razor
<MudThemeProvider IsDarkMode="@ThemePreference.IsDarkMode" />
\`\`\`
\`ThemePreferenceService.Changed\`を共通レイアウトが受け取って再描画します。実装は「画面→Scopedの設定サービス→レイアウト→MudThemeProvider」です。

## よくある間違い
ライト/ダークの変更だけでWCAG対応完了と思い込むこと。読み上げ、フォーカス、拡大、色以外の状態提示、キーボードの順番も確認が必要です。

## 演習
Tab/Shift+Tab/Enter操作を行い、ブラウザーで拡大表示と配色を比較します。

## 完了条件
- [ ] テーマ変更と表示状態を確認できる
- [ ] Shared LayoutとScoped DIの役割を説明できる
- [ ] キーボードで入力・確認ができる
