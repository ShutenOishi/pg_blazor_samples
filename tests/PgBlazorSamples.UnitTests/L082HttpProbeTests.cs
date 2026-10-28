using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>学習順082。擬似検証器による安全拒否の単体試験。実トークンはChromeで検証。</summary>
public sealed class L082HttpProbeTests
{
    [Theory]
    [InlineData(true, StatusCodes.Status403Forbidden)]
    [InlineData(false, StatusCodes.Status400BadRequest)]
    public async Task PostAlwaysRefuses(bool valid, int expected)
    {
        var context = new DefaultHttpContext();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();
        var result = await L082HttpProbe.ValidateAndDenyAsync(context, new FakeAntiforgery(valid));
        await result.ExecuteAsync(context);
        Assert.Equal(expected, context.Response.StatusCode);
    }

    [Fact]
    public void PathIsDedicatedToThisLesson()
        => Assert.Equal("/learn/082/http-probe", L082HttpProbe.Path);

    private sealed class FakeAntiforgery(bool valid) : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext context) => throw new NotSupportedException();
        public AntiforgeryTokenSet GetTokens(HttpContext context) => throw new NotSupportedException();
        public Task<bool> IsRequestValidAsync(HttpContext context) => Task.FromResult(valid);
        public void SetCookieTokenAndHeader(HttpContext context) => throw new NotSupportedException();
        public Task ValidateRequestAsync(HttpContext context)
        {
            if (!valid) throw new AntiforgeryValidationException("検証用の無効なトークン");
            return Task.CompletedTask;
        }
    }
}
