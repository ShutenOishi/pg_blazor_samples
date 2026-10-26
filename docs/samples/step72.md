# ステップ72：明示Windowsログインと短命Cookie

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

## 次へ

Step73では、権限変更と書込み競合・外部台帳の原子性を扱います。
