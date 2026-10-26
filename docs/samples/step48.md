# ステップ48：描画仮想化

## できるようになること

メモリ上のItemsをVirtualizeで部分描画する。Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor 9、EF Core 10を使用します。

## 前提と実行環境

Step41〜45のServerDataとEF Coreの基礎を終えてから進みます。このステップはDBなしで演習できます。

## 画面で操作する
1. ステップ48を開きます。実DBは不要です。
2. 表の中をスクロールして、後半の行が表示されることを確認します。
3. 500件が最初からメモリへ保持されている点に注目します。

## 重要な実装
~~~razor
<MudDataGrid T="LearningTaskRow" Items="@_rows"
    Virtualize="true" Height="360px" ItemSize="50">
~~~

Virtualizeは「画面へ描画する行数」の制限です。DBで取得する行数を減らす機能ではありません。行高が変わるレイアウトや明示されないHeightはスクロール計算を不安定にします。

## 比較
| 表示方式 | DB問合せ | 描画対象 |
| --- | --- | --- |
| 普通のItems | 読込時に全件 | 通常は表内全行 |
| Virtualize付きItems | 読込時に全件 | 可視領域付近 |
| VirtualizeServerData | 表示範囲の都度 | 可視領域付近 |

## 確認問題と答え
**Q. DB100万件をItemsへ全部読み込みVirtualizeすればよい？** いいえ。描画は減ってもメモリ・転送量は減りません。

## 完了条件
- [ ] 500件という値と部分描画の意味を区別した
- [ ] HeightとItemSizeの目的を説明できる


## 次に学ぶ内容

Step49で新しい一覧設計を学びます。
