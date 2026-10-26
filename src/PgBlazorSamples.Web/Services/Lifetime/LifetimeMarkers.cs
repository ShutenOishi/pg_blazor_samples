namespace PgBlazorSamples.Web.Services.Lifetime;

/// <summary>アプリケーション全体で共有する識別子の観察用です。</summary>
public sealed class SingletonMarker
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
}

/// <summary>Interactive Serverの接続単位に関連する識別子の観察用です。</summary>
public sealed class ScopedMarker
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
}

/// <summary>DIで要求するたびに新規生成される識別子の観察用です。</summary>
public sealed class TransientMarker
{
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];
}
