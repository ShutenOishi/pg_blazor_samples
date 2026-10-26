using PgBlazorSamples.Web.Services.Learning;
using Xunit;

namespace PgBlazorSamples.UnitTests;

/// <summary>セキュリティ教材の模擬判定が、危険な入力で許可を返さないことを検査する。</summary>
public sealed class SecurityLessonPolicyTests
{
    [Fact]
    public void BusinessOperation_RequiresEveryCondition()
    {
        Assert.True(SecurityLessonPolicy.PracticeAllowsOperation(true, true, true, true, true, true, true));
        for (var missing = 0; missing < 7; missing++)
        {
            var values = Enumerable.Repeat(true, 7).ToArray();
            values[missing] = false;
            Assert.False(SecurityLessonPolicy.PracticeAllowsOperation(
                values[0], values[1], values[2], values[3], values[4], values[5], values[6]));
        }
    }

    [Theory]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, false)]
    public void Mutation_RejectsMissingBoundary(bool https, bool origin, bool token, bool authorized) =>
        Assert.False(SecurityLessonPolicy.PracticeAcceptsMutation(https, origin, token, authorized));

    [Fact]
    public void Mutation_AcceptsOnlyAllConditions() =>
        Assert.True(SecurityLessonPolicy.PracticeAcceptsMutation(true, true, true, true));

    [Fact]
    public void UntrustedHtml_IsEncodedInsteadOfExecutable()
    {
        var result = SecurityLessonPolicy.EncodedExample("<script>alert(1)</script>");
        Assert.DoesNotContain("<script>", result);
        Assert.Contains("&lt;script&gt;", result);
    }

    [Fact]
    public void Session_RejectsExpiredOrRevokedUser()
    {
        var now = new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero);
        Assert.True(SecurityLessonPolicy.PracticeSessionCurrent(now, now.AddMinutes(5), 2, 2, true));
        Assert.False(SecurityLessonPolicy.PracticeSessionCurrent(now, now.AddSeconds(-1), 2, 2, true));
        Assert.False(SecurityLessonPolicy.PracticeSessionCurrent(now, now, 2, 2, true));
        Assert.False(SecurityLessonPolicy.PracticeSessionCurrent(now, now.AddMinutes(5), 2, 3, true));
        Assert.False(SecurityLessonPolicy.PracticeSessionCurrent(now, now.AddMinutes(5), 2, 2, false));
    }

    [Fact]
    public void SecretValues_AreNeverExposed()
    {
        Assert.Contains("不足", SecurityLessonPolicy.PracticeConfigState(false, true, true));
        Assert.Contains("条件あり", SecurityLessonPolicy.PracticeConfigState(true, true, true));
    }
}
