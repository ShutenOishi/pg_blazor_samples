# 学習順043：CSV・PDFファイルのダウンロード

**実装ソース：** [対応するRazor実装](../../../src/PgBlazorSamples.Web/Components/Lessons/Stage04/L043DownloadPage.razor)。画面の正規URLは `/learn/043` です。

## 到達目標
学習順042ではファイルをブラウザーからサーバーへ送りました。ここでは方向を逆にして、サーバーで作った内容をブラウザーへ渡して保存します。Windows FormsならFile.WriteAllBytesなどでアプリを実行しているPC上に書き込めますが、Blazor Interactive ServerのC#はサーバー側で動くため、そのコードだけでは利用者のPCにファイルを保存できません。ブラウザーのダウンロード機能を使います。

この画面では日本語の架空データを含むCSVと、英字だけの1ページのPDF技術見本を用意しています。受け取ったバイト列をDotNetStreamReference経由でJSへ渡し、Blob・一時URL・ダウンロードリンクを使ってブラウザー側で保存を始めます。利用者の個人情報や実業務のDBデータは扱いません。到達条件は、生成、送信、保存開始、URL解放の4段階を説明でき、実データ出力時に必要な都度認可とCSV数式対策を挙げられることです。

## WinForms＋Dapperからの対応表
| これまでの実装 | 今回のWeb実装 | 業務アプリで追加する検討 |
| --- | --- | --- |
| SaveFileDialogで保存先を選ぶ | ブラウザーがダウンロード先を決める | サーバー側からクライアントの任意パスを直接指定しない |
| StreamWriterでCSVを書く | UTF-8 BOM付きCSVをbyte[]へ変換する | 文字コード、引用符、改行、数式注入を検査 |
| File.WriteAllBytesでPDFを書く | DotNetStreamReferenceとJSで渡す | 大きさ・通信切断・ブラウザー保存設定に注意 |
| SELECTした全レコードを出力する | この教材は固定2行のみを出力する | サーバー側で件数と行範囲を制限して認可 |
| ボタンのEnabledで制限する | ボタンの状態はあくまでUI | APIやサービスでも本人と対象を認可 |

ダウンロードの許可は、ボタンを隠しただけでは保証できません。実業務では出力直前に認証済みの本人と既存DB/外部台帳を照合し、部署や対象行の範囲をサーバーで確定する必要があります。たとえばCountだけ権限範囲を適用し、CSV出力では全件を読み取ってしまう実装は誤りです。学習順043は出力の仕組みを学ぶため、業務用の認可処理は含みません。

## 実際のC#コードで見るCSV
画面の「学習用CSVを保存する」は、固定の2行をサーバー側で生成します。先頭のFEFFはUTF-8のBOMです。日本語を含むCSVを表計算ソフトで開いたときの文字化けを避ける意図ですが、ソフトや設定によって扱いは異なるので確認が必要です。

```csharp
private async Task SaveCsv()
{
    // 認可が必要な実データではなく、固定の架空データのみを出力する。
    const string content =
        "\uFEFF番号,課題名,完了\r\n" +
        "1,画面を確認する,いいえ\r\n" +
        "2,復習する,はい\r\n";
    await SendFile("blazor_practice.csv",
        "text/csv;charset=utf-8",
        Encoding.UTF8.GetBytes(content));
}
```

この固定データにはカンマや改行を含む値がありません。実業務で任意の文字列を書き出すときはCSVの引用符によるエスケープが必要です。また表計算ソフトは先頭が=、+、-、@などの文字列を数式として解釈する場合があります。値を引用符で囲むだけでは対策として十分とは限らないため、出力先ソフトの挙動を確認し、ポリシーに応じた無害化とテストを実施します。利用者が入力したセルをそのまま連結する実装は避けます。

## 今回の補強：生成を分離し、実バイトを検査する

固定の架空データの生成を [L043DownloadFileFactory](../../../src/PgBlazorSamples.Web/Services/Learning/L043DownloadFileFactory.cs) に分離しました。[xUnit](../../../tests/PgBlazorSamples.UnitTests/L043DownloadFileFactoryTests.cs) はCSVの先頭3バイト`EF BB BF`、CRLF、CSV引用符、先頭の`= + - @`、先頭空白の後に数式記号が来る値、PDFの1ページ宣言とxrefで指定した実バイト位置を検査します。[Chrome実操作](../../../scripts/browser-smoke.cjs)でも、保存イベント名だけでなくダウンロードされたファイルの内容を実際に読み、同様の構造を照合します。

サンプルのCSVはすべてのセルを引用符で囲み、数式として解釈され得る値には先頭に単一引用符を追加します。ただしこれは例示した入力パターンの対処であり、Excel・LibreOffice等の各製品で実際にどのように解釈されるかの試験ではありません。全ソフトでの無害性や任意入力の安全性を保証しません。業務用のCSVでは、対象アプリケーション・利用時の操作・組織のルールを個別に試験し、都度認可・出力上限・監査を実装します。PDFも今回の検査は基本構造とxrefであり、すべてのPDF閲覧ソフトでの表示互換性を証明しません。

現在のRazorは`L043DownloadFileFactory.CreateCsv()`と`CreateOnePagePdf()`で固定バイトを生成します。以降の説明にある固定CSVやPDF生成関数は処理の概念例であり、実際の生成コードは上記サービスファイルを参照してください。

## DotNetStreamReferenceとJavaScriptモジュール
現在のRazor/C#実装はファイル種別とバイト列をSendFileへ渡し、初回にJSモジュールをimportします。MemoryStreamとDotNetStreamReferenceの寿命はusingで制御し、JSの呼び出しが完了するまで参照を保持します。JSDisconnectedExceptionとJSExceptionでは表示する失敗理由を分けています。

```csharp
private async Task SendFile(string fileName, string mediaType, byte[] bytes)
{
    try
    {
        module ??= await JS.InvokeAsync<IJSObjectReference>(
            "import", "./js/browser-learning.js");
        using var stream = new MemoryStream(bytes);
        using var reference = new DotNetStreamReference(stream);
        await module.InvokeVoidAsync("downloadStream", fileName, mediaType, reference);
        message = fileName + " の保存を開始しました。";
    }
    catch (JSDisconnectedException)
    {
        message = "接続が切れたため保存できませんでした。";
    }
    catch (JSException)
    {
        message = "ブラウザーでファイルを保存できませんでした。";
    }
}
```

次は実際のbrowser-learning.jsの関数です。JSが.NET側のストリームをarrayBufferとして受け取り、ブラウザーのBlobから一時URLを作り、a要素にdownload属性を設定してクリックします。リンクを削除した後も一時URLは別に解放する必要があるので、現行コードは30秒後にrevokeObjectURLを呼びます。これは小さな教材ファイル用の構成であり、大容量出力の方式まで一般化するものではありません。

```javascript
export async function downloadStream(fileName, mediaType, streamReference) {
    const bytes = await streamReference.arrayBuffer();
    const objectUrl = URL.createObjectURL(new Blob([bytes], { type: mediaType }));
    const link = document.createElement("a");
    link.href = objectUrl;
    link.download = fileName;
    document.body.appendChild(link);
    try {
        link.click();
    } finally {
        link.remove();
        setTimeout(() => URL.revokeObjectURL(objectUrl), 30000);
    }
}
```

PDFのボタンではMakeOnePagePdfが最小限のPDFオブジェクトを文字列で組み立て、ASCIIでbyte[]へ変換します。表示する文章は「Blazor PDF sample」で、英字の技術見本です。日本語PDFを正しく出力するには、日本語フォント・埋め込み・文字コードなどの追加実装が必要です。教材全体の日本語PDF作成処理とは別の、ファイル送信機構を示す画面だと理解してください。

## 操作手順
1. /learn/043を開き、ブラウザーのダウンロード許可設定を確認します。
2. 「学習用CSVを保存する」を押し、保存したblazor_practice.csvをテキストエディターでも開きます。ヘッダーが「番号,課題名,完了」であること、行末が改行で区切られていること、架空データ2行であることを調べます。
3. 表計算ソフトでCSVを開き、日本語の項目名が読めることを確認します。文字化けした場合はファイルのバイト列、BOM、ソフトの読込み設定を切り分けます。
4. 「学習用PDFを保存する」を押し、blazor_practice.pdfが保存され、1ページで開けることを確認します。英字見本であり、日本語帳票ではありません。
5. 画面にはファイル名を含む「保存を開始しました。」が表示されます。このメッセージが意味するのはブラウザーへ保存を要求したことまでです。利用者が保存先で実際に開けたかどうかまでサーバーが保証するわけではありません。
6. 開発者ツールのConsoleでJSの例外を、Networkでモジュール配信と通信状態を確認します。回線を断った場合の失敗と、単に保存場所が分からない場合は別の問題です。

## 障害時の切り分け
| 症状 | 調査先 | 判断・対処 |
| --- | --- | --- |
| ボタンを押しても何も起こらない | Interactive Server接続、サーバーログ | イベントが届きSendFileに入ったかを確認 |
| 「ブラウザーで保存できませんでした」 | Console、JS例外、モジュールパス | importやBlob作成の失敗を確認 |
| 「接続が切れたため保存できません」 | circuitの接続、通信状態 | ダウンロードを無制限に自動再送しない |
| CSVが文字化けする | BOM、UTF-8、表計算ソフトの設定 | 保存機能の失敗と文字コードを区別 |
| PDFが開けない | ダウンロードした実バイト、PDFビューア | 通信が中断されたかファイルが破損したか確認 |
| 保存できたが件数がおかしい | 元データの絞込みと出力件数 | 本番では認可済みのCountと行範囲を照合 |
| 一時URLが解放されない | JSのfinallyとrevokeObjectURL | リンク削除とBlob URL解放を別々に確認 |

## 安全な業務出力への接続
サーバーの業務サービスで認可済みデータを取得する設計では、一覧UIの現在の表示だけを信じず、出力対象をサーバーで再計算します。DapperのSQLはパラメーターを使い、部署・案件・有効期間などの条件を強制します。大量出力では全件をbyte[]へ載せることを避け、認可済みストリーミングや専用のダウンロードエンドポイント、ジョブ化と期限付き出力を検討します。その場合も認証・認可・監査を省略しません。

## 演習と詳しい解答
**問1：** WinFormsと同じようにサーバーのC:\ExportsへFile.WriteAllBytesすれば、利用者はそのファイルを受け取れますか。**解答：** いいえ。サーバーのディスクへの保存であり、ブラウザーへのダウンロードではありません。現在の教材はDotNetStreamReferenceでバイト列をブラウザーへ渡し、JSがBlobとdownload属性を使います。

**問2：** UIからCSVボタンを消せば未認可データの出力を防げますか。**解答：** 防げません。サーバー側で操作の都度、認証済み本人と既存台帳を使って出力権限と対象行を検査します。ボタンは操作性のための表示であり、認可境界ではありません。

**問3：** CSVのセルをダブルクォートで囲めば数式注入は必ず防げますか。**解答：** 必ずしも防げません。CSVパーサーによるフィールド区切りの処理と、表計算ソフトがセルを数式として解釈する処理は別です。値の危険な先頭文字、エスケープ方法、出力先の解釈を組み合わせて検証します。

**問4：** 「保存を開始しました。」と表示されたらダウンロードの成功をサーバーが完全に保証したと言えますか。**解答：** いいえ。JSの処理が保存開始を要求した事実であり、ブラウザーが選んだ保存先で利用者がファイルを開いたことの確認までは含みません。

## 完了確認
- [ ] CSVとPDFの両方を保存し、中身を開いて確認した。
- [ ] C#、DotNetStreamReference、JS、Blob、download属性の役割を説明できる。
- [ ] CSVの文字コード・数式注入と業務データの再認可を説明できる。
- [ ] 大量データには別の設計が必要な理由を説明できる。

続く[学習順044：ErrorBoundary・取消・再試行](browser-04.md)では、非同期の操作が失敗したときの画面と処理の状態を区別します。