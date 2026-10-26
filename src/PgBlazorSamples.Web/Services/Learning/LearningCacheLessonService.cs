namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>
/// Step57：DIのScoped=circuit内の学習用キャッシュ。架空利用者だけを扱う。
/// これは業務認可ではない。実ユーザーID・権限を導入するのはStep61〜70。
/// </summary>
public sealed class LearningCacheLessonService
{
    private readonly Dictionary<(string Actor, int PermissionVersion), IReadOnlyList<string>> _cache = new();
    private static readonly string[] Items = ["共通の連絡", "部署Aの作業", "部署Bの作業"];

    public (IReadOnlyList<string> Rows, bool CacheHit) Read(string actor, int permissionVersion)
    {
        if (actor is not ("A" or "B")) throw new ArgumentException("架空利用者AかBを指定してください。");
        if (permissionVersion < 0) throw new ArgumentOutOfRangeException(nameof(permissionVersion));
        var key = (actor, permissionVersion);
        if (_cache.TryGetValue(key, out var rows)) return (rows, true);

        // 架空データへの演習用フィルター。AD連携・DB行単位認可の代わりにはならない。
        rows = Items.Where(x => x == "共通の連絡" || x.Contains("部署" + actor)).ToArray();
        _cache[key] = rows;
        return (rows, false);
    }

    public void Clear() => _cache.Clear();
}
