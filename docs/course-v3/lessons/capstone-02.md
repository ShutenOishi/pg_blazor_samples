# 学習順107：PostgreSQL検索とページング・生成SQL

## 到達目標
生成SQLの検査と、実際のDB取得を区別し、認可された部署のCountとItemsを同じ条件で取得する。

## 前提と対象業務
第1〜第8段階の内容を使い、架空の課題管理業務を通しで考えます。開発環境はVisual Studio 2026、.NET 10、Blazor Interactive Server、EF Core 10、PostgreSQL 18です。実際のAD/IISは別の実機受入を行います。

## WinForms＋Dapperからの考え方
ToQueryStringはEF CoreがSQLへ変換した結果の表示です。実データを取得するのは認可・監査を含む業務サービスです。全件取得からMudDataGridへ流すのでなく、DBで絞り込みとページングを行います。

## 実習の操作手順
アプリの `/learn/capstone-02` を開きます。「生成SQLだけを確認する」ボタンでSQLを表示した後、「権限付き一覧を取得する」を押します。匿名状態では表示件数0件となり拒否表示になることを確認します。実台帳の認可が成立する環境では許可範囲内の最大10件だけ取得します。

## コードと処理の場所
```csharp
// 実SQLは確認だけでDBに接続しない。
_sql = db.Tasks.AsNoTracking()
    .Where(x => x.UnitCode == "DEMO" && x.Title.Contains(keyword))
    .OrderBy(x => x.Id).Take(10)
    .Select(x => new { x.Id, x.Title }).ToQueryString();
// DB操作は認可・監査を持つ共通サービスに委譲する。
var result = await Business.LoadPageAsync(new(0, 10, keyword));
```

この抜粋は対応するRazor画面と業務サービスの責任を示しています。Razorのイベントはサーバーで実行されます。**業務データは必ず共通の認可済みサービスでのみアクセス**します。ブラウザーの入力や表示状態を認可の根拠にしてはいけません。

## 拒否・失敗時の確認
匿名時の空表示はDBの0件と意味が異なります。無認可ではTotalCount=0、Rowsも空、拒否メッセージを表示します。SQLは自動生成されても本人認可は自動で行われません。

## 確認問題
生成SQLを表示できたら、その利用者は業務データのSELECTを許されたことになりますか？

## 解答
なりません。SQLの変換と権限付き実行は別です。業務サービスが毎操作本人・部署・監査を検査します。

## 完了条件
- [ ] Web画面で操作し、許可/拒否または自己点検の結果を確認した。
- [ ] 認証・DB台帳認可・監査・トランザクションの担当を説明できる。
- [ ] CIの検証済み範囲と、実AD/IIS・本番運用の未確認事項を区別できる。
