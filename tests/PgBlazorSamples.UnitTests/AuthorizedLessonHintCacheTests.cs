using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>Step77〜79：キャッシュは本人/部署/権限版/有効期限をまたがない。</summary>
public sealed class AuthorizedLessonHintCacheTests
{
    private sealed class ManualClock : TimeProvider
    {
        public DateTimeOffset Utc { get; set; } = new(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Utc;
    }

    [Fact]
    public void ScopedHint_IdentityUnitAndVersionIsolation()
    {
        var clock = new ManualClock();
        var cache = new AuthorizedLessonHintCache(clock);
        Assert.False(cache.TryGet("DOM\\A", "U1", 2, out _));
        cache.Store("DOM\\A", "U1", 2, "学習用ヒント");
        Assert.True(cache.TryGet("DOM\\A", "U1", 2, out var message));
        Assert.Equal("学習用ヒント", message);
        Assert.False(cache.TryGet("DOM\\B", "U1", 2, out _));
        cache.Store("DOM\\A", "U1", 2, "学習用ヒント");
        Assert.False(cache.TryGet("DOM\\A", "U2", 2, out _));
        cache.Store("DOM\\A", "U1", 2, "学習用ヒント");
        Assert.False(cache.TryGet("DOM\\A", "U1", 3, out _));
    }

    [Fact]
    public void ScopedHint_ExpiresAndClears()
    {
        var clock = new ManualClock();
        var cache = new AuthorizedLessonHintCache(clock);
        cache.Store("DOM\\A", "U1", 1, "公開ヒント");
        clock.Utc += TimeSpan.FromSeconds(30);
        Assert.False(cache.TryGet("DOM\\A", "U1", 1, out _));
        cache.Store("DOM\\A", "U1", 1, "公開ヒント");
        cache.Clear();
        Assert.False(cache.TryGet("DOM\\A", "U1", 1, out _));
    }

    [Fact]
    public void ScopedHint_RejectsUntrustedKey()
    {
        var cache = new AuthorizedLessonHintCache(new ManualClock());
        Assert.Throws<ArgumentException>(() => cache.Store("", "U1", 0, "ヒント"));
        Assert.Throws<ArgumentException>(() => cache.Store("DOM\\A", "U1", -1, "ヒント"));
    }
}
