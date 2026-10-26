namespace PgBlazorSamples.Web.Data;

// Step63〜65：本人のAD識別子と、アプリ内で許可する役割は別の情報として保存する。
// ADパスワードやWindows資格情報をPostgreSQLへ保存しない。
public sealed class AppUser
{
    public long Id { get; set; }
    public string Sid { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class AppRole
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
}

public sealed class AppPermission
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class AppUserRole
{
    public long UserId { get; set; }
    public int RoleId { get; set; }
}

public sealed class AppRolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}
