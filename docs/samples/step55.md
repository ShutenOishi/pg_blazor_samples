# 学習順075：詳細DTOとN+1の防止

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage06/L075DetailProjectionPage.razor)。画面の正規URLは `/learn/075` です。

**学習順075・第6段階／第15組**

## 目標：一覧と詳細で取得する列を変える

一覧の表示に必要な最低限の列だけを先に読み、利用者が詳細を選択して初めてGetDetailAsyncを呼びます。サービスはAsNoTrackingとSelectによって分類名、期限、開始時刻、完了状態、VersionをLearningTaskDetailへ射影し、SingleOrDefaultAsyncで選んだ一行だけ取得します。Categoryがない場合は未分類と表示します。行数分だけ分類名を別々に問い合わせるN+1を避けることが目的です。RazorはDateOnlyをyyyy/MM/dd、TimeOnlyをHH:mmとして日本語表示し、未設定は文字で区別します。

## 学習環境と処理場所

Visual Studio 2026、.NET 10、Blazor Interactive Server、MudBlazor 9、EF Core 10、Npgsql、開発用PostgreSQL 18を前提にします。WinFormsとDapperを経験した学習者は、イベント処理から接続を開きSQLを実行する構成を想像しやすいでしょう。Interactive ServerではRazorイベントのC#がWebサーバー側で動きます。ブラウザーの表示とサーバーの保存結果を別々に考えてください。対話回線の切断、二重操作、複数人の同時編集を含めて設計します。

## 画面からDBまでの確認手順

1. 開発用のPostgreSQL 18に接続できる場合だけ架空課題を用意します。本番DBの接続先や台帳を学習用画面へ転用しません。

2. 学習順のURLを開き、表示と状態メッセージ、一覧の件数と現在版を観察します。DB設定なしでは未設定と表示されるのが正常な安全応答です。

3. Razorのイベントからサービスメソッドへ何を渡すかを確認します。DTO、Id、Version、取消条件は画面の状態です。利用者が送る値だけを認可の根拠にしません。

4. サービスがIDbContextFactoryから操作ごとにDbContextを作ることを確認します。追跡中Entityを対話状態に保持すると長命化や並列処理、古い読み取りの原因になります。

5. 正常系と異常系を両方実行し、DBの確定有無と利用者への日本語案内を突き合わせます。取消、入力不正、削除済み、競合、設定なし、接続障害は異なる結果です。

6. 同じ行を二画面から操作した場合、どの場面でxminを照合するか説明します。保存前のVersion比較だけでなくSaveChanges時の競合も処理します。

## WinForms＋Dapperとの比較

| 観点 | WinForms＋Dapper | Blazor＋EF Core 10 |

| --- | --- | --- |

| UI状態 | TextBoxとDataGridViewの表示値 | RazorのDTO・選択Id・Version |

| C#の実行場所 | クライアント端末のイベントからSQLを呼ぶ構成もある | Interactive ServerのイベントはWebサーバーで実行 |

| 接続寿命 | using NpgsqlConnectionを操作単位で開閉 | IDbContextFactoryから短命DbContextを生成 |

| 入力検証 | フォーム検証とSQL実行直前の再検証 | EditFormに加えてサービスでも再検証 |

| 更新の排他 | WHERE句の版条件・更新件数を確認 | xmin、DbUpdateConcurrencyExceptionを確認 |

| 業務認可 | 既存台帳の操作権限と行範囲をSQLごとに照合 | 同じく保存・読込の都度サーバーで照合 |

| 障害通知 | SQL結果と例外を分類して表示 | StatusとMessageを分けて分類する |

## 障害の切り分け

| 現象 | 原因として確認すること | 修復・判断 |

| --- | --- | --- |

| 読込直後に未設定 | 学習用DBが構成されていない | 開発用設定を確認し、成功と扱わない |

| 一覧に対象がない | 削除済み・検索範囲の違い | 再読込し、Missingと空結果を区別 |

| 入力欄に警告 | RequiredとStringLength(100) | UI検証と保存時検証の両方を確認 |

| 他者のタイトルが表示 | 読込時VersionよりDBのxminが進んでいる | Conflictを通知し、勝手に上書きしない |

| 例外で保存不可 | DB接続・タイムアウト・書込障害 | Errorを成功に変換しない |

| 選択した一部が変わった | 削除や並列更新が発生 | 一括処理は全件のVersionを再検査 |

## 業務適用前の境界

ここで扱うLearningEditServiceは架空の開発用DB専用であり、AD本人認証や業務台帳認可をこの画面に実装済みだと判断しないでください。実業務ではADから本人のユーザー名だけを取り、あらかじめ整備された既存DB台帳又は外部テーブルを権限の正本とします。アクセスのたびに、操作種類と対象行、利用者の有効状態を確認する必要があります。認可済み行だけを一覧・Count・詳細・一括処理・CSVに含め、更新直前にも権限行を再検証します。権限失効、xminの更新競合、監査永続化失敗、FDWの原子性不明といった条件は安全拒否にします。ボタンが非表示であることは権限の証明ではありません。

## 深く理解するための確認

画面の成功通知はDBの確定を保証しません。実際にサービスが成功を返した理由を追い、保存の前に検証した条件と、保存後に通知した条件が一致するかを説明してください。二重送信、取消、行削除、権限失効、DB障害を同じメッセージで済ませないことが重要です。xUnitや実DB試験が成功しても、実AD/IIS/Kerberos/FDWと1万人超の実機負荷・監査要件を完了とみなしません。

## 演習2と詳しい解答

**演習2：** 入力画面を開いたまま別ウィンドウで同じ行が更新されました。古いDTOを無条件で保存してよいか、また本番業務で画面のボタンを隠せば認可が完了するか説明してください。

**詳しい解答2：** 無条件保存は許容できません。Versionと現在xminを照合して競合なら拒否し、最新値を読み直した上で利用者に再編集を求めます。画面の表示制御は業務認可ではないため、保存・表示・一括処理の各操作に既存台帳に基づく行単位認可を実装します。監査に失敗した場合も成功として通知しません。

## 実装の処理順と実コード

この画面では、表示用DTOを読み込んだ時点と、データベースへ結果を確定した時点を区別します。コード中の状態やCancellationTokenの受け渡しを確認し、失敗時に成功の通知を行わないことを確認してください。

### 実コード例1：サービス側又は親コンポーネント

```csharp
public async Task<LearningDetailResult> GetDetailAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0) return new(null, "正しい番号を指定してください。");
        if (!settings.Configured) return new(null, LearningDbService.NotConfigured);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            // AsNoTracking + Selectで必要な列だけをDBでJOIN。行ごとに追加SQLを発行しない。
            var detail = await db.Tasks.AsNoTracking().Where(t => t.Id == id)
                .Select(t => new LearningTaskDetail(t.Id, t.Title,
                    t.Category == null ? "未分類" : t.Category.Name,
                    t.DueDate, t.StartTime, t.IsDone, t.Version))
                .SingleOrDefaultAsync(ct);
            return detail is null
                ? new(null, "詳細対象が削除されています。")
                : new(detail, "詳細を1回の問い合わせで取得しました。");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "学習用の詳細取得失敗");
            return new(null, "詳細を取得できませんでした。");
        }
    }
```

### 実コード例2：関連する保存・画面処理

```csharp
private async Task ShowAsync(long id)
    {
        var result = await Edits.GetDetailAsync(id);
        _detail = result.Item;
        _message = result.Message;
    }
```

## この画面で確かめること

一覧の表示に必要な最低限の列だけを先に読み、利用者が詳細を選択して初めてGetDetailAsyncを呼びます。サービスはAsNoTrackingとSelectによって分類名、期限、開始時刻、完了状態、VersionをLearningTaskDetailへ射影し、SingleOrDefaultAsyncで選んだ一行だけ取得します。Categoryがない場合は未分類と表示します。行数分だけ分類名を別々に問い合わせるN+1を避けることが目的です。RazorはDateOnlyをyyyy/MM/dd、TimeOnlyをHH:mmとして日本語表示し、未設定は文字で区別します。

## 操作手順と否定経路

一覧で「詳細を見る」を選び、分類・期限・開始時刻・完了・版番号を確認します。DB未設定時は案内が出ることを確認します。行が削除された場合の表示が接続エラーと混同されていないか確認します。

正常に実行できたときだけ更新結果を確認します。DB未設定・取消・入力エラーが発生したら、成功時の画面とどのように違うかを書き出してください。処理の途中に他者の更新が挟まる可能性も確認します。

## 演習1と詳しい解答

**演習1：** このステップの画面で正常経路と該当する拒否経路を確認し、サービスが返す状態とDBの変更有無を説明してください。

**詳しい解答1：** 詳細取得は対象のIdをWhereに入れ、必要な列をSelectでDTOへ射影するため行単位の個別SQLを避けられます。業務では詳細を見る操作にも行単位の認可が必要です。

## 完了条件

- [ ] /learn/075の旧URLを維持し、画面とサービスの対応を説明できる

- [ ] 2種類の実コードを読んでVersionや確定時点を確認した

- [ ] 比較表・障害切り分け表を使い正常と拒否を区別できる

- [ ] 実AD・実業務台帳での認可は未受入だと説明できる

## 次に学ぶ内容

第15組の復習を行い、次は学習順076へ進みます。
