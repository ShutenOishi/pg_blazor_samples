# 学習順97：明示Windowsログインと短命Cookie

**実装ソース：** [対応するRazor実装](../../src/PgBlazorSamples.Web/Components/Lessons/Stage08/L097ExplicitWindowsLoginPage.razor)。画面の正規URLは `/learn/097` です。

**第8段階・第20組／学習順097・旧URL /learn/097**

## 到達目標

普段はWindows統合認証だけで利用する業務と、本人が明示操作したときだけログイン状態を保持する業務を区別します。ADのパスワード入力フォームは作らず、ASP.NET CoreのNegotiate認証とCookie認証を組み合わせます。

## 操作方法

1. Windows認証が不要な学習環境では、確認ボタンを押すと拒否されます。
2. 管理者が両設定 `WindowsAuthentication:Enabled=true` / `WindowsAuthentication:ExplicitLoginEnabled=true` を有効にすると、ログイン経路が利用可能になります。
3. 「Windows認証を確認してログインする」へ移動します。**HTTP GETの確認画面ではCookieを発行しません**。Windows認証を確立し、同画面のフォームからPOSTを実行した場合に限り10分Cookieを発行します。
4. 記名もパスワードも入力しません。保護されたCookieを発行した後、画面へ戻ります。
5. POSTログアウトでCookieを破棄し、画面再読込後にDBの権限を再確認します。

## 実装上のポイント

- 通常は従来のNegotiate統合認証を維持し、明示モードだけCookieハンドラーを既定にします。
- `/auth/windows/confirm` と `/auth/windows/signin` だけはNegotiateスキームを強制します。
- `AntiforgeryToken` と `IAntiforgery.ValidateRequestAsync` によりログイン・ログアウトPOSTのCSRFを防止します。
- CookieチケットはASP.NET Coreが署名・暗号化して検証します。ブラウザーからの任意クレームやログイン名の自己申告は受け付けません。
- 発行Cookieは10分、HttpOnly/Secure/SameSite=Strict、SlidingExpiration=false、IsPersistent=falseです。
- 認証済みCookieでも、業務操作の都度DB側で `is_active` / `can_read` / `can_edit` / `unit_code` を再照合します。Cookieの権限Claimは使用しません。
- SignIn/SignOutはBlazor対話回線内ではなくHTTPエンドポイントから実行するため、レスポンスヘッダーの送信後エラーを避けます。

## Windows Server 2025 + IISでの受入条件

本番で明示モードを使う場合は、次を実機で確認してください。

- イントラネットIISでWindows認証と匿名接続を両方許可し、アプリのNegotiate指定エンドポイントだけで本人のWindows認証を必須化する。
- HTTPS/HSTSとKerberosのSPN・資格交渉を確認し、プロキシ接続の分離による資格取り違えを防ぐ。
- `WindowsAuthentication:DataProtectionKeysPath` を**アプリプールだけが読み書きできる保護された共有場所**に設定する。複数サーバーの鍵同期と保存時暗号化、ローテーション、復元試験を行う。開発用の一時鍵を本番へ流用しない。
- Cookieの有効期間終了・DB権限剥奪・部署変更・DB停止・SignalR再接続・タブをまたぐログアウトを否定試験する。
- CIの偽PrincipalでのClaim試験は、実ADのKerberos認証、Cookie暗号の動作、IIS経路分離の受入に代わりません。

## 確認問題と解答

**Q1. Cookieへcan_edit権限を保持してよい？** いいえ。変更が反映されず、失効直後の更新を防げません。操作時に正本を再確認します。

**Q2. Blazorのボタンクリックイベントから直接SignInAsyncすればよい？** いいえ。Cookieを書き込むHTTPレスポンスが必要です。

**Q3. Cookieに自作のsource Claimが入っていれば単独で信用できる？** いいえ。ASP.NET Core認証ハンドラーによる暗号署名と有効期限の検証が成立して初めて信用します。

## 学習順097で追跡するHTTP認証の順番

Blazor Interactive Serverのボタンイベントは長時間接続上の対話処理です。認証Cookieの`Set-Cookie`を書き込む処理は、ヘッダーをまだ送信していない通常のHTTP応答で行います。`Program.cs`では`WindowsAuthentication:Enabled`と`ExplicitLoginEnabled`を別々に読み、明示モードが有効なら専用の認証スキームを構成します。通常の統合認証では本来のNegotiate経路を維持し、すべての画面を短命Cookieへ勝手に置換しません。

明示ログインの入口は`GET /auth/windows/confirm`であり、この時点ではCookieを発行しません。認証済みWindowsユーザーを確認した上でフォームと偽造防止トークンを表示します。`POST /auth/windows/signin`はCSRFトークンを検証し、検証済み本人名に対応する短命認証チケットを発行します。`POST /auth/windows/signout`もCSRF検証を行います。一般利用者がログイン名やADパスワードを入力する画面は作りません。

### 実コード例1：Razor画面から専用HTTPへ移動

次は`ExplicitWindowsLoginPage.razor`にある動作を抜粋したもので、`AntiforgeryToken`を通常のPOSTフォームに置いています。

```razor
@if (Configuration.GetValue<bool>("WindowsAuthentication:ExplicitLoginEnabled"))
{
    <a href="/auth/windows/confirm">Windows認証を確認してログインする</a>
    <form method="post" action="/auth/windows/signout">
        <AntiforgeryToken />
        <button type="submit">業務ログインを終了する</button>
    </form>
}
```

ログイン済み表示だけを根拠に機密データを返してはいけません。`Cookie`が正常でも`ReadCurrentAsync`と業務サービスの都度認可を通ります。`can_read`、`can_edit`、部署コードをCookieの権限Claimへ保存すると、台帳側の失効や異動を直ちに反映できなくなります。

### 実コード例2：短命Cookieの設定条件

以下はASP.NET Coreの設定観点を切り出した例で、実際の認証スキーム選択、鍵保管先と失効試験は`Program.cs`を原本として確認します。

```csharp
options.Cookie.HttpOnly = true;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Strict;
options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
options.SlidingExpiration = false;
options.Cookie.IsEssential = true;
```

ブラウザーを閉じずに10分経過しても、自動延長されない設計です。認証チケットの失効だけでは、既に取得した画面の静的表示を消し去るとは限りません。実業務操作前にサーバーで認証状態を再評価し、SignalR再接続や複数タブの状態ずれも否定試験します。

| 観点 | WinForms＋Dapper | BlazorとHTTP認証 |
| --- | --- | --- |
| 本人取得 | WindowsIdentityなど信頼できるOSの結果 | IIS/Negotiateが検証したPrincipal |
| ログイン操作 | アプリ起動時の資格とセッションを管理 | 認証専用HTTP POSTでCookie発行 |
| CSRF | Webフォームの問題とは別 | 明示ログインとログアウト双方で検証 |
| 権限失効 | DB操作の直前に既存台帳を確認 | Cookie保持中も毎操作DBで再認可 |
| 多台構成 | クライアントごとの資格 | Data Protection鍵共有・復元が必要 |

### 操作手順と障害の切り分け

1. 無効構成で`/learn/097`を開き、明示ログインリンクが利用できないことを確認します。開発用環境で本番のADパスワードを入力しません。
2. 実運用担当者がWindows認証との組合せを承認した検証IISだけで明示モードを有効にします。GETでCookieが新規発行されないこと、CSRFなしのPOSTが拒否されることを確認します。
3. 検証済みWindows主体のPOST後に10分Cookieの属性を調べ、Secure・HttpOnly・SameSite・期限・永続化なしを確認します。匿名の任意Claimで本人になれないことも調べます。
4. ログアウト、ブラウザー再読込、期限切れ、別タブ、別サーバー、鍵ローテーション、SignalR再接続、DB権限剥奪の順に、都度認可が有効であることを確かめます。

| 現象 | 切り分ける場所 | 安全な対応 |
| --- | --- | --- |
| 確認GETで401 | IISのWindows認証、SPN、ブラウザー設定 | 認証経路を修正し、匿名を許可に変更しない |
| POSTで400 | Antiforgeryの発行・検証・フォーム | トークンの流れを直し、検証を無効にしない |
| 別ノードでログアウト | Data Protection鍵・アプリ名・共有ACL | 本番鍵の保護と復元を実証 |
| Cookie有効でも拒否 | DB台帳の失効・重複・監査障害 | 失効を優先し古いCookieの権限は採用しない |

### 演習と詳しい解答

**演習1．** Blazorの`OnClick`から`SignInAsync`を直接呼ぶ設計が失敗するのはなぜですか。

**詳しい解答1．** 対話処理中はHTTPの初回レスポンスヘッダーが既に送信されている場合があります。Cookieは通常HTTPエンドポイントのPOST応答で発行します。GET確認とPOST認証確定を分け、CSRF検査を行う必要があります。

**演習2．** Cookieに`can_edit=true`を保存し、台帳側で権限が削除された後も保存を許可できますか。

**詳しい解答2．** 許可できません。Cookieは本人認証を示すだけです。編集サービスは正本DBの編集権限と部署を保存直前に都度確認し、行ロックを保証できない構成や監査障害時には拒否します。

## 次へ

学習順098では、権限変更と書込み競合・外部台帳の原子性を扱います。
