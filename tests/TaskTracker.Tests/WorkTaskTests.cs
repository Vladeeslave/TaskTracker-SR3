using TaskTracker.Domain;

namespace TaskTracker.Tests;

public class WorkTaskTests
{
    [Theory]
    [InlineData(0, "Назва", "TT-01", 2)]
    [InlineData(1, " ", "TT-01", 2)]
    [InlineData(1, "Назва", " ", 2)]
    [InlineData(1, "Назва", "TT-01", 0)]
    [InlineData(1, "Назва", "TT-01", 201)]
    public void Create_InvalidArguments_Throws(int id, string title, string project, int estimate)
    {
        // Act and Assert
        Assert.ThrowsAny<ArgumentException>(
            () => WorkTask.Create(id, title, project, estimate));
    }

    [Fact]
    public void Create_ValidArguments_StartsInNewWithoutAssignee()
    {
        // Act
        WorkTask task = WorkTask.Create(7, "Підготувати звіт", "TT-01", 3m);

        // Assert
        Assert.Equal(WorkTaskStatus.New, task.Status);
        Assert.Null(task.AssigneeId);
    }

    [Fact]
    public void Move_StartWithoutAssignee_ThrowsNoAssigneeRule()
    {
        // Arrange
        WorkTask task = Samples.Task(1, WorkTaskStatus.New);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => task.Move(TaskEvent.Start, 0));

        // Assert
        Assert.Equal("wip.no-assignee", ex.Rule);
        Assert.Equal(WorkTaskStatus.New, task.Status);
    }

    [Fact]
    public void Move_ReturnFromReviewAtLimit_ThrowsAndKeepsStatus()
    {
        // Arrange
        WorkTask task = Samples.Task(1, WorkTaskStatus.Review, assignee: 1);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => task.Move(TaskEvent.Return, WipPolicy.WipLimit));

        // Assert
        Assert.Equal("wip.limit", ex.Rule);
        Assert.Equal(WorkTaskStatus.Review, task.Status);
    }

    [Fact]
    public void Assign_NewTask_SetsAssignee()
    {
        // Arrange
        WorkTask task = Samples.Task(1, WorkTaskStatus.New);

        // Act
        task.Assign(2);

        // Assert
        Assert.Equal(2, task.AssigneeId);
    }

    [Fact]
    public void Assign_InProgressTask_ThrowsAssignStatusRule()
    {
        // Arrange
        WorkTask task = Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => task.Assign(2));

        // Assert
        Assert.Equal("task.assign-status", ex.Rule);
    }

    [Fact]
    public void LogTime_ClosedTask_ThrowsClosedRule()
    {
        // Arrange
        WorkTask task = Samples.Task(1, WorkTaskStatus.Done, assignee: 1);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => task.LogTime(new TimeEntry(1, 2m, new DateOnly(2026, 11, 20))));

        // Assert
        Assert.Equal("task.closed", ex.Rule);
    }

    [Fact]
    public void LogTime_OtherMember_ThrowsTimeMemberRule()
    {
        // Arrange
        WorkTask task = Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => task.LogTime(new TimeEntry(2, 2m, new DateOnly(2026, 11, 20))));

        // Assert
        Assert.Equal("task.time-member", ex.Rule);
    }

    [Fact]
    public void Snapshot_RoundTrip_PreservesState()
    {
        // Arrange
        WorkTask task = Samples.Task(5, WorkTaskStatus.InProgress, assignee: 3, estimate: 6m);
        task.LogTime(new TimeEntry(3, 1.5m, new DateOnly(2026, 11, 20)));

        // Act
        WorkTask restored = WorkTask.Restore(task.ToSnapshot());

        // Assert
        Assert.Equal(task.Id, restored.Id);
        Assert.Equal(task.Status, restored.Status);
        Assert.Equal(task.AssigneeId, restored.AssigneeId);
        Assert.Equal(1.5m, restored.LoggedHours);
    }
}
