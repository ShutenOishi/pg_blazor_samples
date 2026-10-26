namespace PgBlazorSamples.Web.Data;

/// <summary>
/// Step63〜65：既存利用者台帳・外部テーブルを同じ形式で扱うための読み取り専用ビュー。
/// アプリが利用者を登録したり権限を付与したりすることはない。
/// </summary>
public sealed class ExistingUserAccessRow
{
    // DOMAIN\userなど、Windows認証で確定した値と正規化済みの一意なアカウント。
    public string LoginName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public bool IsActive { get; set; }
    public bool CanRead { get; set; }
    public bool CanEdit { get; set; }
    public string? UnitCode { get; set; }
    public long AuthorizationVersion { get; set; }
}
