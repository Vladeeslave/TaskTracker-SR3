using TaskTracker.Domain;

namespace TaskTracker.Tests;

public class TaskTransitionsTests
{
    [Theory]
    [InlineData(WorkTaskStatus.New, TaskEvent.Start, WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.New, TaskEvent.Cancel, WorkTaskStatus.Cancelled)]
    [InlineData(WorkTaskStatus.InProgress, TaskEvent.SendToReview, WorkTaskStatus.Review)]
    [InlineData(WorkTaskStatus.InProgress, TaskEvent.Cancel, WorkTaskStatus.Cancelled)]
    [InlineData(WorkTaskStatus.Review, TaskEvent.Return, WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.Review, TaskEvent.Complete, WorkTaskStatus.Done)]
    [InlineData(WorkTaskStatus.Review, TaskEvent.Cancel, WorkTaskStatus.Cancelled)]
    public void Next_AllowedTransition_ReturnsTargetColumn(
        WorkTaskStatus current, TaskEvent trigger, WorkTaskStatus expected)
    {
        // Act
        WorkTaskStatus actual = TaskTransitions.Next(current, trigger);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(WorkTaskStatus.New, TaskEvent.Complete)]
    [InlineData(WorkTaskStatus.New, TaskEvent.SendToReview)]
    [InlineData(WorkTaskStatus.InProgress, TaskEvent.Start)]
    [InlineData(WorkTaskStatus.InProgress, TaskEvent.Complete)]
    [InlineData(WorkTaskStatus.Review, TaskEvent.Start)]
    [InlineData(WorkTaskStatus.Done, TaskEvent.Return)]
    [InlineData(WorkTaskStatus.Done, TaskEvent.Cancel)]
    [InlineData(WorkTaskStatus.Cancelled, TaskEvent.Start)]
    public void Next_ForbiddenTransition_ThrowsInvalidTransition(
        WorkTaskStatus current, TaskEvent trigger)
    {
        // Act
        InvalidTransitionException ex = Assert.Throws<InvalidTransitionException>(
            () => TaskTransitions.Next(current, trigger));

        // Assert
        Assert.Equal(current, ex.From);
        Assert.Equal(trigger, ex.Trigger);
    }

    [Fact]
    public void IsAllowed_KnownAndUnknownPair_ReturnsTrueAndFalse()
    {
        // Act and Assert
        Assert.True(TaskTransitions.IsAllowed(WorkTaskStatus.New, TaskEvent.Start));
        Assert.False(TaskTransitions.IsAllowed(WorkTaskStatus.Done, TaskEvent.Start));
    }
}
