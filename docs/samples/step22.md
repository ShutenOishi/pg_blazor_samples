# ステップ22：MudBlazorでレイアウトを作る

## このステップでできるようになること
WinFormsのフォーム上へToolStripやPanelを置くように、Blazorではレイアウト用のコンポーネントを組み合わせます。ここでは上部の`MudAppBar`、開閉式の`MudDrawer`、本文の`MudMainContent`、画面幅に応じた`MudGrid`を学びます。

## 前提と完成画面
Step21を修了してから`/learn/22`を開きます。DB・ADは不要です。画面内に「練習用管理画面」とメニューボタン、左右2枚の情報カードが表示されます。

1. 最初は「メニューを表示しています。」と表示される。
2. メニューボタン「学習メニューを開閉」を押すと「メニューを閉じています。」に変わる。
3. もう一度押すとメニューが戻る。
4. ブラウザーの幅を狭め、幅に応じて左右のカードが縦並びになることを確認する。

## 画面の主なコード

```razor
<MudPaper Class="pa-2" Elevation="1">
  <MudLayout Style="min-height: 380px; position:relative;">
    <MudAppBar Fixed="false" Color="Color.Primary">
      <MudIconButton Icon="@Icons.Material.Filled.Menu"
                     aria-label="学習メニューを開閉"
                     OnClick="ToggleMenu" />
      <MudText Typo="Typo.h6">練習用管理画面</MudText>
    </MudAppBar>
    <MudDrawer @bind-Open="_menuOpen"
               Variant="DrawerVariant.Persistent"
               ClipMode="DrawerClipMode.Docked"
               Fixed="false" Overlay="false">
      <MudNavMenu>
        <MudNavLink Href="/learn/21">導入と翻訳</MudNavLink>
        <MudNavLink Href="/learn/23">入力部品</MudNavLink>
      </MudNavMenu>
    </MudDrawer>
    <MudMainContent Class="pa-3">
      <p>@(_menuOpen ? "メニューを表示しています。" : "メニューを閉じています。")</p>
      <MudGrid>
        <MudItem xs="12" sm="6"><MudPaper>左側の業務情報</MudPaper></MudItem>
        <MudItem xs="12" sm="6"><MudPaper>右側の補助情報</MudPaper></MudItem>
      </MudGrid>
    </MudMainContent>
  </MudLayout>
</MudPaper>
@code {
    private bool _menuOpen = true;
    private void ToggleMenu() => _menuOpen = !_menuOpen;
}
```

上記は属性を一部省いた概念の抜粋です。実装画面では`MudItem`の余白、`MudIconButton`の色、`MudGrid`の間隔も指定しています。`xs="12"`は狭い画面では12列分、`sm="6"`は指定の幅以上で6列分を使うため、カード2枚が横並びになります。

## ボタンの操作順
1. ブラウザーのクリックがサーバーの`ToggleMenu`を呼ぶ。
2. `_menuOpen`が`true`から`false`へ変わる。
3. `@bind-Open`で結び付けた`MudDrawer`に変更が渡る。
4. 本文内のメッセージとメニューの表示が更新される。

この操作で利用者の権限が変化するわけではありません。**メニューを閉じることと機能へのアクセスを禁止することは異なります**。

## アプリ全体のレイアウトとの区別
このステップは`/learn/22`ページの**内側**に練習用`MudLayout`を置きます。`MainLayout.razor`は別に存在し、アプリ全体のヘッダー、ナビゲーション、本文、MudBlazorのProviderを扱います。ページ内だけでレイアウトの動作を試すため`Fixed="false"`としており、画面全体を覆う固定メニューの例ではありません。

## つまずいたら
- メニューボタンで開閉しない場合は`OnClick`・`_menuOpen`・`@bind-Open`を確認する。
- 画面幅を変えても見た目が変わらない場合は、実際のブラウザー幅がブレークポイントをまたいだかを調べる。
- 表示が変わったから権限が適用されたと考えない。認可は後続のサービス側で扱う。

## 演習と解答
**問：** メニューの開閉によって`_menuOpen`が変化しても、業務の閲覧権限は変わりますか。

**答え：** 変わりません。これは画面の表示状態です。機能の利用許可は別途本人と権限を確認します。

## 完了確認
- [ ] Drawerの開閉と状態の変化を確認した。
- [ ] `MudGrid`の`xs`と`sm`の意味を説明できる。
- [ ] 共通レイアウトとページ内の練習レイアウトを区別できる。

次は[ステップ23：入力コンポーネント](step23.md)です。
