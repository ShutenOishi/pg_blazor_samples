namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step77：画面用の非機密ヒントだけを短時間保持する教材。
/// 業務行・CSV・閲覧許可をキャッシュしない。表示前のDB認可・永続監査は常に別途必要。
/// Scoped登録によりBlazor Serverのcircuit外へ結果を共有しない。
/// </summary>
public sealed class AuthorizedLessonHintCache(TimeProvider clock)
{
    public static readonly TimeSpan LifeTime = TimeSpan.FromSeconds(30);
    private (string Login, string Unit, long Version, DateTimeOffset Expiry, string Message)? _entry;

    public bool TryGet(string login, string unit, long version, out string message)
    {
        var e = _entry;
        if (e is { } hit && hit.Login == login && hit.Unit == unit
            && hit.Version == version && hit.Expiry > clock.GetUtcNow())
        {
            message = hit.Message;
            return true;
        }
        _entry = null;
        message = "";
        return false;
    }

    public void Store(string login, string unit, long version, string message)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(unit) || version < 0)
            throw new ArgumentException("認証済みキーと権限版が必要です。");
        _entry = (login, unit, version, clock.GetUtcNow() + LifeTime, message);
    }

    public void Clear() => _entry = null;
}
