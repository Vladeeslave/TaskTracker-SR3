using TaskTracker.Application;
using TaskTracker.Domain;

namespace TaskTracker.Tests;

public class TaskBoardServiceTests
{
    private const string RangeError = "Години мають бути більшими за нуль і не більшими за 200.";

    [Theory]
    [InlineData("", "TT-01", "2", "Назва не може бути порожньою.")]
    [InlineData(null, "TT-01", "2", "Назва не може бути порожньою.")]
    [InlineData("Звіт", "", "2", "Код проєкту не може бути порожнім.")]
    [InlineData("Звіт", "XX-99", "2", "Проєкту з кодом XX-99 немає.")]
    [InlineData("Звіт", "TT-01", "abc", "Години мають бути числом, наприклад 2,5.")]
    [InlineData("Звіт", "TT-01", "-1", RangeError)]
    [InlineData("Звіт", "TT-01", "0", RangeError)]
    public void CreateTask_InvalidInput_ReturnsErrorAndStoresNothing(
        string? title, string? project, string? estimate, string expectedError)
    {
        // Arrange
        FakeTaskRepository tasks = new();
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        CreateTaskRequest request = new(title, project, estimate);

        // Act
        Result<WorkTask> result = service.CreateTask(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.Error);
        Assert.Empty(tasks.GetAll());
    }

    [Fact]
    public void CreateTask_TooLongTitle_ReturnsError()
    {
        // Arrange
        TaskBoardService service = Samples.Service(new FakeTaskRepository(), new SpyLog());
        string title = new('x', TaskLimits.MaxTitleLength + 1);

        // Act
        Result<WorkTask> result = service.CreateTask(new CreateTaskRequest(title, "TT-01", "2"));

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CreateTask_ValidInput_StoresNewTaskAndLogsInformation()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        SpyLog log = new();
        TaskBoardService service = Samples.Service(tasks, log);

        CreateTaskRequest request = new(" Підготувати звіт ", "TT-01", "2,5");

        // Act
        Result<WorkTask> result = service.CreateTask(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Підготувати звіт", result.Value!.Title);
        Assert.Equal(WorkTaskStatus.New, result.Value.Status);
        Assert.Equal(2.5m, result.Value.EstimateHours);
        Assert.Equal(1, tasks.SaveCalls);
        Assert.Contains("INFO UC-01 Створено завдання 1", log.Lines);
    }

    [Fact]
    public void MoveTask_UnknownTask_ReturnsErrorAndLogsWarning()
    {
        // Arrange
        SpyLog log = new();
        TaskBoardService service = Samples.Service(new FakeTaskRepository(), log);

        // Act
        Result<WorkTask> result = service.MoveTask(404, TaskEvent.Start);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(log.Lines, line => line.StartsWith("WARN UC-02", StringComparison.Ordinal));
    }

    [Fact]
    public void MoveTask_StartWithAssigneeBelowLimit_MovesToInProgress()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.New, assignee: 1));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.MoveTask(1, TaskEvent.Start);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(WorkTaskStatus.InProgress, result.Value!.Status);
        Assert.Equal(1, tasks.SaveCalls);
    }

    [Fact]
    public void MoveTask_StartWithTwoInProgress_MovesToInProgress()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1));
        tasks.Add(Samples.Task(2, WorkTaskStatus.InProgress, assignee: 1));
        tasks.Add(Samples.Task(3, WorkTaskStatus.New, assignee: 1));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.MoveTask(3, TaskEvent.Start);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(WorkTaskStatus.InProgress, result.Value!.Status);
    }

    [Fact]
    public void MoveTask_StartWithThreeInProgress_ThrowsAndLogsWarningWithoutSaving()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1));
        tasks.Add(Samples.Task(2, WorkTaskStatus.InProgress, assignee: 1));
        tasks.Add(Samples.Task(3, WorkTaskStatus.InProgress, assignee: 1));
        tasks.Add(Samples.Task(4, WorkTaskStatus.New, assignee: 1));
        SpyLog log = new();
        TaskBoardService service = Samples.Service(tasks, log);

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(
            () => service.MoveTask(4, TaskEvent.Start));

        // Assert
        Assert.Equal("wip.limit", ex.Rule);
        Assert.Equal(0, tasks.SaveCalls);
        Assert.Contains(log.Lines, line => line.StartsWith("WARN UC-02", StringComparison.Ordinal));
    }

    [Fact]
    public void MoveTask_ForbiddenTransition_ThrowsInvalidTransition()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.Done, assignee: 1));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act and Assert
        Assert.Throws<InvalidTransitionException>(() => service.MoveTask(1, TaskEvent.Start));
    }

    [Fact]
    public void AssignTask_NewTask_SetsAssignee()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.New));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.AssignTask(1, 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.AssigneeId);
    }

    [Fact]
    public void AssignTask_UnknownMember_ReturnsError()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.New));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.AssignTask(1, 99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Учасника 99 не знайдено.", result.Error);
    }

    [Fact]
    public void AssignTask_UnknownTask_ReturnsError()
    {
        // Arrange
        TaskBoardService service = Samples.Service(new FakeTaskRepository(), new SpyLog());

        // Act
        Result<WorkTask> result = service.AssignTask(404, 1);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void LogTime_OpenTask_AddsEntryWithClockDate()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.LogTime(1, "1,5");

        // Assert
        Assert.True(result.IsSuccess);
        TimeEntry entry = Assert.Single(result.Value!.Entries);
        Assert.Equal(new DateOnly(2026, 11, 20), entry.Date);
        Assert.Equal(1.5m, entry.Hours);
    }

    [Fact]
    public void LogTime_ClosedTask_ThrowsClosedRule()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.Done, assignee: 1));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        DomainRuleException ex = Assert.Throws<DomainRuleException>(() => service.LogTime(1, "2"));

        // Assert
        Assert.Equal("task.closed", ex.Rule);
    }

    [Fact]
    public void LogTime_TaskWithoutAssignee_ReturnsError()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.New));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.LogTime(1, "2");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("У завдання 1 немає виконавця.", result.Error);
    }

    [Fact]
    public void LogTime_InvalidHours_ReturnsError()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1));
        TaskBoardService service = Samples.Service(tasks, new SpyLog());

        // Act
        Result<WorkTask> result = service.LogTime(1, "багато");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Constructor_NullDependency_ThrowsArgumentNull()
    {
        // Act and Assert
        FixedClock clock = new(new DateOnly(2026, 1, 1));
        Assert.Throws<ArgumentNullException>(
            () => new TaskBoardService(null!, Samples.TeamDirectory(), clock, new SpyLog()));
    }
}
