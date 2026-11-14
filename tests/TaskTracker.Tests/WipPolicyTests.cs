using TaskTracker.Domain;

namespace TaskTracker.Tests;

public class WipPolicyTests
{
    [Theory]
    [InlineData(false, 0, StartDecision.NoAssignee)]
    [InlineData(false, 5, StartDecision.NoAssignee)]
    [InlineData(true, 0, StartDecision.Allowed)]
    [InlineData(true, 1, StartDecision.Allowed)]
    [InlineData(true, 2, StartDecision.Allowed)]
    [InlineData(true, 3, StartDecision.LimitReached)]
    [InlineData(true, 4, StartDecision.LimitReached)]
    [InlineData(true, 100, StartDecision.LimitReached)]
    public void Decide_ByDecisionTable_ReturnsExpected(
        bool hasAssignee, int inProgress, StartDecision expected)
    {
        // Act
        StartDecision actual = WipPolicy.Decide(hasAssignee, inProgress);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Decide_NegativeCount_ThrowsOutOfRange()
    {
        // Act and Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => WipPolicy.Decide(true, -1));
    }

    [Fact]
    public void EnsureCanEnter_LimitReached_ThrowsWipLimitRule()
    {
        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => WipPolicy.EnsureCanEnter(true, WipPolicy.WipLimit));

        // Assert
        Assert.Equal("wip.limit", ex.Rule);
    }

    [Fact]
    public void EnsureCanEnter_NoAssignee_ThrowsNoAssigneeRule()
    {
        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => WipPolicy.EnsureCanEnter(false, 0));

        // Assert
        Assert.Equal("wip.no-assignee", ex.Rule);
    }

    [Fact]
    public void EnsureCanEnter_BelowLimit_DoesNotThrow()
    {
        // Act
        Exception? error = Record.Exception(
            () => WipPolicy.EnsureCanEnter(true, WipPolicy.WipLimit - 1));

        // Assert
        Assert.Null(error);
    }
}
