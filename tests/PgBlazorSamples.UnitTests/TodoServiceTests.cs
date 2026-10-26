using Microsoft.Extensions.Options;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

public sealed class TodoServiceTests
{
    private static InMemoryTodoService Create(int limit = 5) =>
        new(Options.Create(new LearningOptions { MaxTasks = limit }));

    [Fact]
    public void 有効な課題は追加できて完了を切り替えられる()
    {
        var service = Create();
        Assert.True(service.TryAdd(" DIの確認 ", out _));
        var item = service.GetAll().Last();
        Assert.Equal("DIの確認", item.Title);
        Assert.True(service.Toggle(item.Id));
        Assert.True(service.GetAll().Last().IsComplete);
        Assert.False(service.Toggle(Guid.NewGuid()));
    }

    [Fact]
    public void 未入力と長すぎる課題を拒否する()
    {
        var service = Create();
        Assert.False(service.TryAdd(" ", out _));
        Assert.False(service.TryAdd(new string('あ', 41), out _));
        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void 課題上限を超えて追加できない()
    {
        var service = Create(2);
        Assert.False(service.TryAdd("追加したい", out var message));
        Assert.Contains("最大2件", message);
        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void 別のスコープに相当するサービスは状態が独立する()
    {
        var a = Create();
        var b = Create();
        Assert.True(a.TryAdd("Aだけ", out _));
        Assert.Equal(3, a.GetAll().Count);
        Assert.Equal(2, b.GetAll().Count);
    }
}
