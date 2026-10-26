# 特別コラム：用途から選ぶMudBlazorコンポーネント事典

**学習順序：Step40 → この特別コラム → Step41。** 業務要件・利用者の操作・データ件数・アクセシビリティから選びます。全部のコンポーネントを覚える必要はありません。

## 一覧・表・階層・集計

| やりたいこと | 推奨コンポーネント | 比較・注意 |
| --- | --- | --- |
| 数行の静的な表 | MudSimpleTable | 追加のソート・ページング不要 |
| 一般的な業務の一覧 | MudTable | 単純な列・絞り込み・ページャー向き |
| 列フィルター・複数ソート・編集 | MudDataGrid | 多機能。簡易表には過剰なことも |
| 少数の確定済みデータ | Items | DB全件取得とは意味が違う |
| DB側の検索・ページング | ServerData | 総件数とItemsは別、認可と安定ソート必須 |
| 画面に見える範囲だけDB取得 | VirtualizeServerData | Height、行高、キャンセル、読み上げを評価 |
| 深いページの連続読み取り | カーソル/キーセットの独自UI | 任意のページジャンプが難しい |
| 展開・選択できる木構造 | MudTreeView | 必要な子を遅延取得できる |
| 詳細を折りたたむ | MudExpansionPanels | 少量の補足・FAQ・行の詳細 |
| 部署別の数値・時系列 | MudChart | 正確な値を確認できる表も用意 |
| KPI・概要を示す | MudCard、MudPaper、MudText | 数値と単位を明示 |
| ドラッグで項目を分類 | MudDropZone | キーボード操作の代替を用意 |

**重要：MudSimpleTable、MudTable、MudDataGridは別の役割**です。描画だけのVirtualizeと、DB取得自体を制限するVirtualizeServerDataも混同しません。[詳しい選定ガイド](../data-grid-patterns.md)。

## 入力・検索・選択

| 目的 | 使うもの | 注意 |
| --- | --- | --- |
| 文字列、検索語 | MudTextField | 長さ・禁止文字・DB側検索条件 |
| 長い備考 | MudTextField（複数行） | 最大長と読みやすさ |
| 数量・金額 | MudNumericField | 桁・NULL・丸めと単位 |
| 候補の少ない選択 | MudSelect | 大量候補を一度に読まない |
| 候補が多い/候補を検索 | MudAutocomplete | 非同期検索、件数上限、取消・デバウンス |
| 少数の排他的選択 | MudRadioGroup | 選択肢を比較しやすい |
| 独立した選択/同意 | MudCheckBox | 同意済みかどうかを明確に |
| 設定のオン/オフ | MudSwitch | 保存のタイミングを明確に |
| スライダーで連続量調整 | MudSlider | 正確な値はMudNumericFieldも用意 |
| 1つの日付 | MudDatePicker | UIはDateTime?、DBのDateOnlyと変換 |
| 日付範囲 | MudDateRangePicker | DateRangeとPostgreSQL daterangeの境界が異なる |
| 時刻だけ | MudTimePicker | TimeOnlyとの対応 |
| 添付ファイル | MudFileUpload | 容量・拡張子・内容・アクセス権を検査 |
| 複数入力と検証 | MudForm | 画面検証・業務検証・DB制約を区別 |

## 画面配置・移動・ナビゲーション

| 目的 | 使うもの | 選び方 |
| --- | --- | --- |
| アプリの上部・横メニュー | MudAppBar、MudDrawer、MudNavMenu | 共通レイアウト向け |
| レスポンシブな段組み | MudGrid + MudItem | 画面幅で列数を切り替える |
| 行・列方向のシンプルな配置 | MudStack | 小さな画面ならGridより容易 |
| 余白・区切り・表面 | MudContainer、MudPaper、MudDivider | 視線と情報のまとまり |
| タブによる内容切替 | MudTabs、MudTabPanel | タブが多すぎないようにする |
| 手順を段階表示 | MudStepper | 途中入力・戻る・保存の扱い |
| 階層の現在地 | MudBreadcrumbs、MudNavLink | 深い画面で迷わない |
| メニューから操作 | MudMenu | 主操作を隠しすぎない |
| 少数の状態表示 | MudChip、MudBadge | 色以外に日本語ラベルを用意 |

## 処理結果・確認・待機・アクセシビリティ

| 状況 | 候補 | 使い分け |
| --- | --- | --- |
| 操作を開始 | MudButton、MudIconButton | 操作名と無効状態、キーボード操作 |
| 間違えると困る操作の確認 | MudDialog/MessageBox | 実行と中止の結果を分ける |
| 一時的な完了通知 | MudSnackbar | 重要な結果は本文にも残す |
| 継続して読む警告・エラー | MudAlert | エラーの理由・再試行方法 |
| 待ち時間の進行 | MudProgressLinear、MudProgressCircular | キャンセル可否・再試行を明示 |
| 読み込み中のプレースホルダー | MudSkeleton | 動的な読込領域を示す |
| 補助的な説明 | MudTooltip | 重要な情報をTooltipだけに置かない |
| 配色・文字 | MudThemeProvider、MudText | ダークモード、拡大、コントラスト |

## 選択するときの6つの質問

1. 件数の上限が確定している？ 不明なら全件Itemsを既定にしない。
2. 利用者は「選ぶ」「検索する」「編集する」「詳細を見る」のどれをする？
3. 値は文字、数、日付、時刻、日付Range、ファイル、階層のどれ？
4. 検索・ソート・ページングはDB側で行う必要がある？
5. キーボード・読み上げ・表示倍率変更でも操作できる？
6. 権限・競合・入力検証を画面だけに任せていない？

## 典型的な組合せ

- 小規模申請一覧：MudTextField + MudTable + MudDialog + MudSnackbar
- 大規模な業務データ：MudDateRangePicker + MudDataGrid ServerData + EF Core + Npgsql
- 組織・部署の階層：MudTreeView + MudAutocomplete + MudTable
- 集計・日次指標：MudDatePicker + MudCard + MudChart + 確認用の表
- 登録画面：MudForm + MudTextField + MudSelect + MudButton + MudAlert

## よくある誤解と解答

**Q. 高機能なMudDataGridを毎回使えばよい？** A. いいえ。単純な一覧にはMudTableが理解しやすく、静的な表にはMudSimpleTableが向きます。

**Q. ダークモードと日本語化でアクセシビリティ完成？** A. いいえ。焦点、拡大、読み上げ、色以外の状態提示を確認します。

**Q. MudDatePickerがDateOnlyを返す？** A. いいえ。DateTime?を受け、サービス境界で変換します。

**Q. 一覧が10万件でもVirtualize=trueだけでDB負荷は小さい？** A. いいえ。Itemsで全件取得済みなら取得負荷は残ります。

**Q. 確認ダイアログで「実行」と押したのでサーバー認可は不要？** A. いいえ。画面UXと業務権限は別です。

## 公式資料

- [MudBlazor全体と用途別カテゴリ](https://mudblazor.com/docs/overview)
- [コンポーネントAPI](https://mudblazor.com/api/)
- [MudTable](https://mudblazor.com/components/table)
- [MudDataGrid](https://mudblazor.com/api/MudDataGrid)
- [MudDateRangePicker](https://mudblazor.com/components/daterangepicker)

次はStep41から、要件に合った部品をEF Core 10とPostgreSQLに接続します。
