# 学習順32：MudBlazorのレイアウトを学ぶ

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Samples/Step22/MudLayoutPage.razor)。画面の正規URLは `/learn/032` です。

## 学ぶ前に
この教材はWinForms＋Dapperを使ってきた開発者が、WebのUIとサーバー側C#の境界を理解するためのものです。学習用の架空データで試し、認証・認可や実業務の保存が成立したとは見なしません。対象画面は /learn/032 です。

## WinForms＋Dapperとの比較
| 観点 | デスクトップでの経験 | MudBlazorでの対応 |
| --- | --- | --- |
| 上部バー | ToolStrip | MudAppBar |
| メニュー | Panel | MudDrawerとMudNavMenu |
| 配置 | Dock／Anchor | MudGridのxsとsm |
| 権限 | 別の認可制御 | メニューの開閉では認可しない |

## 不具合時の切り分け
| 症状 | 確認する場所 |
| --- | --- |
| ボタンが動かない | ToggleMenuと_menuOpenの変化 |
| 配置が変化しない | ブラウザー幅とブレークポイント |
| 全画面を覆う | Fixed・Overlayの設定 |
| リンクを直接開ける | 非表示と権限の違い |

## アプリ共通部分とページ内の練習領域
ここではMudLayoutを学習ページの内側に置いています。アプリ全体のMainLayoutには別のナビゲーションやProviderがあります。練習用のMudAppBarとMudDrawerはFixed=false、Overlay=falseで固定表示を避けます。WinFormsのPanelとDockをそのまま持ち込まず、CSS上の表示領域を確認してください。Drawerの開閉はUI状態であり、利用者の閲覧権限や機能へのアクセス可否を設定する仕組みではありません。

```razor
<MudDrawer @bind-Open="_menuOpen" Variant="DrawerVariant.Persistent"
           ClipMode="DrawerClipMode.Docked" Fixed="false" Overlay="false">
    <MudNavMenu><MudNavLink Href="/learn/031">導入と翻訳</MudNavLink></MudNavMenu>
</MudDrawer>
```

```csharp
private bool _menuOpen = true;
private void ToggleMenu() => _menuOpen = !_menuOpen;
```

xs=12は狭い画面で列をすべて使い、sm=6は幅が広がると半分を使います。2枚のカードが縦並びと横並びになる様子を確認してください。データの値が変わったわけではなく、表示領域の大きさによって配置だけが変わります。

**演習と詳しい解答：** 開閉ボタンを2回押して状態を説明してください。初期true、1回目false、2回目trueとなり、@bind-Openの状態と説明文が一致します。別のブラウザーで直接URLを指定すればメニューが非表示でも画面の要求は届きます。機密データを返すときは既存DB/外部台帳を正本とするサーバー側認可を毎回行う必要があります。Dapperで検索する場合も、表示条件だけを権限条件にしないでください。

## 実装に対応した操作・コード・詳細解答

## このステップでできるようになること
WinFormsのフォーム上へToolStripやPanelを置くように、Blazorではレイアウト用のコンポーネントを組み合わせます。ここでは上部の`MudAppBar`、開閉式の`MudDrawer`、本文の`MudMainContent`、画面幅に応じた`MudGrid`を学びます。

## 前提と完成画面
学習順031を修了してから`/learn/032`を開きます。DB・ADは不要です。画面内に「練習用管理画面」とメニューボタン、左右2枚の情報カードが表示されます。

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
        <MudNavLink Href="/learn/031">導入と翻訳</MudNavLink>
        <MudNavLink Href="/learn/033">入力部品</MudNavLink>
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
このステップは`/learn/032`ページの**内側**に練習用`MudLayout`を置きます。`MainLayout.razor`は別に存在し、アプリ全体のヘッダー、ナビゲーション、本文、MudBlazorのProviderを扱います。ページ内だけでレイアウトの動作を試すため`Fixed="false"`としており、画面全体を覆う固定メニューの例ではありません。

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

次は[学習順033：入力コンポーネント](step23.md)です。


## 学習順32の追加確認
- [ ] 画面で操作を実際に行い、状態の変化を確認した。
- [ ] 比較表と切り分け表に沿って、WinFormsとの相違点を説明できる。
- [ ] DapperによるDB検索と画面表示の処理を区別できる。
- [ ] 教材の演習に回答し、失敗時の復旧手順を説明できる。
