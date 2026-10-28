namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// 学習順084：Program.csから実行する起動時構成ガード。
/// 実キーや接続文字列を読み取らず、設定条件だけを判断する。
/// ファイル保存先が指定されても、ACL/保存時暗号化/復旧の検証は別工程。
/// </summary>
public static class L084StartupPolicy
{
    public static void Validate(
        bool windowsAuthenticationEnabled,
        bool explicitLoginEnabled,
        bool isDevelopment,
        string? dataProtectionKeysPath)
    {
        if (explicitLoginEnabled && !windowsAuthenticationEnabled)
            throw new InvalidOperationException("明示ログインにはWindows認証の有効化が必要です。");

        if (windowsAuthenticationEnabled && explicitLoginEnabled
            && !isDevelopment && string.IsNullOrWhiteSpace(dataProtectionKeysPath))
            throw new InvalidOperationException("明示ログインの本番利用にはDataProtectionKeysPathが必要です。");
    }

    /// <summary>
    /// 画面の固定架空値で同じガードを試す。実環境のSecret/KeyPathは受け取らない。
    /// </summary>
    public static string EvaluateLearningScenario(
        bool windowsAuthenticationEnabled,
        bool explicitLoginEnabled,
        bool isDevelopment,
        bool pretendKeyPathConfigured)
    {
        try
        {
            Validate(windowsAuthenticationEnabled, explicitLoginEnabled,
                isDevelopment, pretendKeyPathConfigured ? "架空の保存先" : null);
            return "架空条件の起動ガードは通過。鍵の共有・暗号化・権限・復旧・実IISは未検証です。";
        }
        catch (InvalidOperationException)
        {
            return "架空条件で起動拒否。認証方式または鍵保存先の条件が不足しています。";
        }
    }
}
