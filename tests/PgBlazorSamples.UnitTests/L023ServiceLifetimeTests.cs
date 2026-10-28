using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Services.Lifetime;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順023：練習用Markerを異なるDI生存期間で解決する単体試験。</summary>
public sealed class L023ServiceLifetimeTests
{
    [Fact]
    public void SingletonとScopedとTransientの解決範囲を区別する()
    {
        // Program.csの登録形態と同じ構成を試験内に隔離して再現する。
        // 本試験は本番ホストの実際の登録やcircuit寿命そのものを検証しない。
        var services = new ServiceCollection();
        services.AddSingleton<SingletonMarker>();
        services.AddScoped<ScopedMarker>();
        services.AddTransient<TransientMarker>();
        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var first = firstScope.ServiceProvider;
        var second = secondScope.ServiceProvider;

        Assert.Same(first.GetRequiredService<SingletonMarker>(), second.GetRequiredService<SingletonMarker>());
        Assert.Same(first.GetRequiredService<ScopedMarker>(), first.GetRequiredService<ScopedMarker>());
        Assert.NotSame(first.GetRequiredService<ScopedMarker>(), second.GetRequiredService<ScopedMarker>());
        Assert.NotSame(first.GetRequiredService<TransientMarker>(), first.GetRequiredService<TransientMarker>());
    }

    [Fact]
    public void 観察用識別子は8桁の16進数で保持される()
    {
        var singleton = new SingletonMarker();
        var scoped = new ScopedMarker();
        var transient = new TransientMarker();
        Assert.Matches("^[0-9a-f]{8}$", singleton.Id);
        Assert.Matches("^[0-9a-f]{8}$", scoped.Id);
        Assert.Matches("^[0-9a-f]{8}$", transient.Id);
        Assert.Equal(singleton.Id, singleton.Id);
    }
}
