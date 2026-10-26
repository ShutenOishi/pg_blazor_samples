namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Interactive Serverのcircuitごとに画面テーマを保持する練習用サービス。
/// 全利用者で共有するSingletonに、利用者の選択状態を置かない。
/// </summary>
public sealed class ThemePreferenceService
{
    public bool IsDarkMode { get; private set; }
    public event Action? Changed;

    public void SetDarkMode(bool isDarkMode)
    {
        if (IsDarkMode == isDarkMode) return;
        IsDarkMode = isDarkMode;
        Changed?.Invoke();
    }
}
