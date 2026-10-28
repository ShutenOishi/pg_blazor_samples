using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順026：Learning節からIOptionsへの型付き構成と既定値。</summary>
public sealed class L026LearningOptionsTests
{
    [Fact]
    public void Learning節をIOptionsの型付き値へ結び付ける()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Learning:CourseTitle"] = "試験用タイトル",
                ["Learning:MaxTasks"] = "12"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<LearningOptions>(config.GetSection("Learning"));
        using var provider = services.BuildServiceProvider();
        var settings = provider.GetRequiredService<IOptions<LearningOptions>>().Value;

        Assert.Equal("試験用タイトル", settings.CourseTitle);
        Assert.Equal(12, settings.MaxTasks);
    }

    [Fact]
    public void Learning節が空のときはモデルの初期値を使用する()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<LearningOptions>(
            new ConfigurationBuilder().Build().GetSection("Learning"));
        using var provider = services.BuildServiceProvider();
        var settings = provider.GetRequiredService<IOptions<LearningOptions>>().Value;

        Assert.Equal("Blazorステップアップ学習", settings.CourseTitle);
        Assert.Equal(5, settings.MaxTasks);
        // サービス側の上限クランプはTodoServiceTestsの担当とする。
    }
}
