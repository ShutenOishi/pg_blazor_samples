# 第9組の復習：ブラウザー機能と安全な操作

## 第4段階の最後の5ステップを結び付ける
学習順041〜045では、MudBlazorで作った日本語UIを実際に使ううえで必要な、ブラウザーとサーバーの境界を確認しました。第7組では入力・検証と一覧を、第8組では検索・確認・通知・表示状態を学びました。第9組はそれらの画面から見落としがちな「ファイル入出力」「ブラウザーAPI」「失敗時の状態」「キーボードと拡大」を整理する復習です。WinForms＋Dapperの経験を使いながら、C#がどこで動くか、利用者から届く値をどこで検証するかを説明できることを目標にします。

まず5本の役割を思い出してください。041ではC#のcount更新とJavaScriptのlang取得を比べます。042ではInputFileから受け取る.txt/.csvを64 KiBに制限してSHA-256を表示します。043では固定の架空データをCSVと英字PDFとしてブラウザーへ渡します。044ではErrorBoundaryの例外表示とCancellationTokenの取消を試します。045ではlabel、role=status、フォーカス、200%拡大を確認します。どのサンプルにも実AD認証や業務DB更新はありません。

## 5機能の比較表
| 学習順 | 実装で使う部品 | 実行場所・入力 | 安全上の境界 |
| --- | --- | --- | --- |
| 041 JS interop | IJSRuntime、IJSObjectReference、JSモジュール | C#イベントはサーバー、documentはブラウザー | DOMの値を認可の正本にしない |
| 042 アップロード | InputFile、IBrowserFile、OpenReadStream | 選択はブラウザー、検査はサーバー | acceptは補助。拡張子と容量をサーバーで確認 |
| 043 ダウンロード | MemoryStream、DotNetStreamReference、Blob | 生成はサーバー、保存開始はブラウザー | 実データなら権限・範囲・件数を都度認可 |
| 044 失敗と取消 | ErrorBoundary、CancellationTokenSource | UI状態と非同期処理を区別 | RecoverはDBロールバックではない |
| 045 操作性 | label、input、role=status、:focus-visible | HTMLの意味とブラウザー表示 | 色だけに依存せず文字とキーボードで伝える |

WinFormsのOpenFileDialogやSaveFileDialogを知っている人は、同じ「ファイル操作」という名前でも役割が変わる点に注目します。WinFormsではプログラムが利用者PC上のファイルシステムにアクセスできます。一方、Interactive ServerのC#が動くのはサーバー側です。アップロードはブラウザーからサーバーへ内容を渡し、ダウンロードはサーバーから渡した内容をブラウザーの保存機能に引き継ぎます。クライアントPCの任意の場所をサーバーのパスとして開く仕組みではありません。

## 実コードを使った横断確認
最初の確認では「JSを使わずに済む処理」を選びます。回数を増やすだけならRazorの@onclickとC#で十分です。documentにあるlang属性が必要な場合だけ、モジュールからreadLanguageを呼びます。次のコードは041で動作する内容の中心部分です。

```csharp
private void Increase() => count++;

private async Task ReadLanguage()
{
    module ??= await JS.InvokeAsync<IJSObjectReference>(
        "import", "./js/browser-learning.js");
    var language = await module.InvokeAsync<string>("readLanguage");
    message = "HTMLの言語設定：" + language;
}
```

この抜粋では例外処理を省いています。実際の041画面はJSDisconnectedExceptionとJSExceptionを個別に扱い、利用者に内部例外文を見せません。JSは認証と認可を代行できず、サービス側の業務判定が必要です。

次にアップロードを見ます。拡張子や申告サイズを先に確認したうえで、読み取りストリーム自体にも同じ上限を設定する理由を説明してください。hashの計算は受信したバイトを調べるためであり、ファイルが無害だと保証するためではありません。

```csharp
const long maxBytes = 64 * 1024;
if (Path.GetExtension(file.Name) is not ".txt" and not ".csv")
{
    // 許可していない拡張子なので拒否する。
    return;
}
if (file.Size > maxBytes) return;
await using var input = file.OpenReadStream(maxAllowedSize: maxBytes);
using var output = new MemoryStream();
await input.CopyToAsync(output);
var digest = Convert.ToHexString(SHA256.HashData(output.ToArray()));
```

上記は教材の理解用に示した抜粋であり、実際の042画面では拒否文と例外処理、digest表示の初期化が加わります。業務で保存する場合はさらに本人・対象・操作の認可、検疫、保存先の分離が必要です。ファイルのNameをそのまま保存先パスに使ってはいけません。

ダウンロードではDotNetStreamReferenceで.NETのストリームをJSに渡し、Blobと一時URLを使います。保存ボタンを押しても、サーバーのCドライブに書き込むことがクライアントへの配布にはなりません。

```csharp
using var stream = new MemoryStream(bytes);
using var reference = new DotNetStreamReference(stream);
await module.InvokeVoidAsync("downloadStream", fileName, mediaType, reference);
```

ダウンロード開始の通知と実際に利用者がファイルを開けた事実も分けます。データが架空の2行でもCSVの文字コード・引用符・数式注入を学び、PDFが1ページの英字技術見本であることを確認します。日本語フォントを含む正式な業務帳票は、別途実装と確認が必要です。

## 5画面を連続して操作する実習
1. /learn/041でC#だけのボタンを押し、回数が1になることを確認します。次にJavaScriptのボタンを押し、HTMLのlangが表示されることを確認します。
2. /learn/042で小さい.txtを選んでSHA-256を確認します。次に.pngなど許可外拡張子を選び、拒否と古いハッシュ表示の消去を確認します。さらに64 KiB超のファイルで容量拒否を確かめます。
3. /learn/043でCSVを保存して日本語のヘッダーと架空の2行を確認し、PDFを開いて英字の1ページであることを確認します。ダウンロード先はブラウザー側が扱います。
4. /learn/044で意図した例外を起こし、一般的な案内へ切り替わることを確認します。Recoverで復旧した後、別の学習用非同期処理を開始して途中取消を試し、終了メッセージを確認します。
5. /learn/045でTabだけで入力と2つの切替ボタンを操作し、拡大を200%にします。結果が色だけでなく文字でも確認できるかを調べます。
6. /learn/column/browser-45に戻り、5つの処理場所を説明し、次の学習順046へ進む準備をします。第5段階ではこれらのUIをDBへ接続する前提を学びます。

操作の記録には「どのボタンを押したか」「画面のどの文言が変わったか」「ブラウザーとサーバーのどちらを確認したか」を含めると、動作したつもりという誤認を減らせます。失敗が起きた場合は単に再読込みするのではなく、実行された範囲を切り分けてください。

## 故障時の切り分け表
| 発生した問題 | 最初に調べるもの | 後から確認するもの |
| --- | --- | --- |
| JSでlangを取得できない | モジュールのNetworkとConsole | Blazorの回線切断やJS例外 |
| ファイルを選んでも結果が出ない | InputFileのOnChangeと接続 | 拡張子・容量・読込み上限 |
| CSVが読めない | UTF-8 BOMと出力バイト列 | 表計算ソフトの読込み設定 |
| ダウンロードできない | JSモジュール、保存設定 | Blob生成、回線、ブラウザー制限 |
| ErrorBoundaryから戻れない | Recover、子コンポーネントの再例外 | 根本原因とサーバーログ |
| Cancel後も外部処理が続く | Tokenを実処理に渡したか | 完了済み副作用とトランザクション |
| キーボード操作が途切れる | フォーカス、DOM順序、disabled | 狭い幅・200%ズーム・読上げ |

## 演習と詳しい解答
**問1：JS interopの採否。** 「ボタンを押して回数を増やす」と「HTMLのlang属性を読む」は両方JSで統一する方が簡単ですか。**詳しい解答：** 統一する必要はありません。回数はBlazorのサーバー状態としてC#で更新できます。lang属性はブラウザーのDOMにあるのでJSが必要です。必要のないJS呼出しを増やすと接続エラーや状態不一致への考慮が増えます。

**問2：アップロードの防御。** accept=.csvと拡張子検査を設ければ、どのCSVファイルも安全に保存できますか。**詳しい解答：** できません。acceptは選択補助で、拡張子はファイル名に基づく判断です。サーバーでOpenReadStreamの容量上限を設け、内容形式・マルウェア・保存先・本人認可を別々に確認します。学習画面は保存しません。

**問3：出力の認可。** DataGridに見える範囲だけが権限範囲なら、CSVボタンで取得する全件SELECTの結果も自動的に守られますか。**詳しい解答：** 守られません。表示UIとサーバーのクエリは別です。CSV、集計、Count、詳細、更新のそれぞれで認証済み本人と既存DB/外部台帳に基づき認可し、行範囲を強制する必要があります。DapperでもEF Coreでもこの条件は変わりません。

**問4：取消の意味。** 通信断後にErrorBoundaryをRecoverして保存ボタンを押し直せば、確実に未保存の処理だけが実行されますか。**詳しい解答：** 保証されません。応答だけが失われても保存処理は完了済みかもしれません。画面復旧とDBの更新状態を混同せず、要求ID、業務キー、監査、トランザクションで結果を確認する設計が必要です。

**問5：利用者の操作。** 成功時の背景色を緑にしたので、読み上げや拡大表示を確認する必要はありませんか。**詳しい解答：** 必要です。色だけでは結果が伝わらない場合があるため、状態を文字で示し、label、Tab順、:focus-visible、role=status、200%拡大を実機で確かめます。自動Chrome試験だけで全支援技術の受入完了とはしません。

## 第4段階の完成条件と第5段階への接続
第4段階の学習順031〜045では、MudBlazorと日本語翻訳、フォーム、一覧、ダイアログ、通知、画面状態、JS、ファイル操作、エラー、アクセシビリティを扱いました。各画面で「表示のための制御」と「実際の安全条件」を区別できれば次へ進めます。ここでのファイル・ユーザー・一覧は学習用です。実AD/IIS/Kerberos/FDWや1万人規模の負荷が、この演習で検証されたわけではありません。

次の第5段階はDapper経験を土台にEF Core 10とPostgreSQL 18へ進みます。ブラウザーからの入力をDTOとサーバー側検証へ渡し、短命DbContextでアクセスし、一覧取得やCSV出力、認可、監査の境界を後続で確認します。画面部品が整っていても業務権限を保証するわけではないという原則を持ち越してください。

## 最終確認
- [ ] 5画面すべての正常動作と少なくとも2種類の拒否・取消を確認した。
- [ ] WinFormsのローカル処理とInteractive Serverの通信を説明できる。
- [ ] JS・アップロード・ダウンロードの各処理がどこで実行されるか説明できる。
- [ ] ErrorBoundaryとDBトランザクションの違いを説明できる。
- [ ] 200%拡大とキーボード操作を試した。

準備ができたら[学習順046：DapperとEF Coreの役割・選定を比較する](../lessons/db-bridge-01.md)へ進みます。