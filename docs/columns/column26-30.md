# 学習コラム07：MudBlazorの画面部品を実務の設計へつなぐ

## 5ステップの関係
Step26のDataGridは必要な行を見つけるための入口、Step27のDialogは操作の確認、Step28のSnackbarは結果通知、Step29のテーマ/キーボードは誰もが使うための土台、Step30はそれらを統合した架空の業務画面です。部品を置くだけでなく、**入力・処理・結果の責務を分ける**ことが重要です。

## 実務での選択
| 要件 | 画面部品・方式 | 変更・留意点 |
| --- | --- | --- |
| 件数が少なく上限のある固定リスト | MudTable/MudDataGrid Items | 画面に全件を渡してよい |
| データベースに大量の行がある | MudDataGrid ServerData | WHERE/ORDER BY/ページング/CountをDBで実行 |
| 連続スクロール主体 | VirtualizeServerData | 可視範囲の取得とキャンセルを制御 |
| 更新前に確認が必要 | DialogService | 同意後もサーバー側認可が必要 |
| 処理結果を短く知らせる | Snackbarと本文の状態 | 色だけでなく文字と読み上げ |
| 利用者固有の表示設定 | Scopedの状態管理 | Blazor Interactive Serverのcircuit寿命を理解する |

## 間違いを比較
- **誤り：** QuickFilterで検索できたからDB検索も完成。**正解：** Items上でのフィルターです。
- **誤り：** 確認ダイアログで「実行」を押したら認可成功。**正解：** 業務サービスで認可する必要があります。
- **誤り：** すべてのサービスはSingletonが便利。**正解：** 利用者ごとの状態は適切なScopedなどで分離します。
- **誤り：** Snackbarに通知したので重要な操作結果は本文へ不要。**正解：** 後から確認できる状態表示も残します。

## 確認問題と解答
**Q1. 100万件のDBをItemsへすべて読み込んでページングしてよい？**  
A. いいえ。ServerData/検索用サービスでDB側のページングを行います。

**Q2. ダークモードだけでアクセシビリティは十分？**  
A. いいえ。キーボードの焦点、文字と背景のコントラスト、読み上げ、拡大時の配置なども確認します。

**Q3. EF CoreへつなぐときMudDataGridコンポーネントにDbContextを直接注入する？**  
A. 基本的にはしません。Applicationサービスの契約を経由し、InfrastructureでIDbContextFactoryから操作単位のDbContextを生成・破棄します。

## 次へ
Step31〜40ではEF Core 10とNpgsqlの役割・型・DbContextFactoryを学びます。Step41〜60では[一覧の10パターンの判断基準](../data-grid-patterns.md)を実装して比較します。
