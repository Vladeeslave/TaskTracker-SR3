using TaskTracker.Application;
using TaskTracker.Domain;

namespace TaskTracker.Tests;

public class SearchAndReportTests
{
    [Fact]
    public void Find_EmptyStore_ReturnsEmptyList()
    {
        // Arrange
        TaskSearchService search = new(new FakeTaskRepository());

        // Act
        IReadOnlyList<WorkTask> found = search.Find(new TaskFilter());

        // Assert
        Assert.Empty(found);
    }

    [Fact]
    public void Find_ByStatus_ReturnsSingleMatch()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.New));
        tasks.Add(Samples.Task(2, WorkTaskStatus.Review, assignee: 1));
        TaskSearchService search = new(tasks);

        TaskFilter filter = new() { Status = WorkTaskStatus.Review };

        // Act
        IReadOnlyList<WorkTask> found = search.Find(filter);

        // Assert
        WorkTask only = Assert.Single(found);
        Assert.Equal(2, only.Id);
    }

    [Fact]
    public void Find_NoCriteria_ReturnsAllOrderedById()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(3, WorkTaskStatus.New));
        tasks.Add(Samples.Task(1, WorkTaskStatus.New));
        tasks.Add(Samples.Task(2, WorkTaskStatus.New));
        TaskSearchService search = new(tasks);

        // Act
        IReadOnlyList<WorkTask> found = search.Find(new TaskFilter());

        // Assert
        int[] expected = { 1, 2, 3 };
        Assert.Equal(expected, found.Select(task => task.Id));
    }

    [Fact]
    public void Find_StatusAndAssignee_ReturnsOnlyTasksMatchingBoth()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1));
        tasks.Add(Samples.Task(2, WorkTaskStatus.InProgress, assignee: 2));
        tasks.Add(Samples.Task(3, WorkTaskStatus.Review, assignee: 1));
        TaskSearchService search = new(tasks);
        TaskFilter filter = new() { Status = WorkTaskStatus.InProgress, AssigneeId = 1 };

        // Act
        IReadOnlyList<WorkTask> found = search.Find(filter);

        // Assert
        int[] expected = { 1 };
        Assert.Equal(expected, found.Select(task => task.Id));
    }

    [Fact]
    public void Find_TitleFragmentIgnoringCase_ReturnsMatches()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.New));
        TaskSearchService search = new(tasks);

        // Act
        IReadOnlyList<WorkTask> found = search.Find(new TaskFilter { TitleText = "ЗАВДАННЯ 1" });

        // Assert
        Assert.Single(found);
    }

    [Fact]
    public void Find_NullFilter_ThrowsArgumentNull()
    {
        // Arrange
        TaskSearchService search = new(new FakeTaskRepository());

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => search.Find(null!));
    }

    [Fact]
    public void Build_SeveralMembers_OrdersByLoadThenName()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1, estimate: 5m));
        tasks.Add(Samples.Task(2, WorkTaskStatus.InProgress, assignee: 1, estimate: 3m));
        tasks.Add(Samples.Task(3, WorkTaskStatus.Review, assignee: 1, estimate: 2m));
        tasks.Add(Samples.Task(4, WorkTaskStatus.InProgress, assignee: 2, estimate: 4m));
        tasks.Add(Samples.Task(5, WorkTaskStatus.Done, assignee: 2, estimate: 6m));
        WorkloadReportService report = new(tasks, Samples.TeamDirectory());

        // Act
        IReadOnlyList<WorkloadRow> rows = report.Build();

        // Assert
        string[] expected = { "Олена Коваль", "Петро Шевчук", "Ірина Бондар" };
        Assert.Equal(expected, rows.Select(row => row.MemberName));
    }

    [Fact]
    public void Build_MemberWithTasks_CalculatesCountsHoursAndLoad()
    {
        // Arrange
        FakeTaskRepository tasks = new();
        tasks.Add(Samples.Task(1, WorkTaskStatus.InProgress, assignee: 1, estimate: 5m));
        tasks.Add(Samples.Task(2, WorkTaskStatus.InProgress, assignee: 1, estimate: 3m));
        tasks.Add(Samples.Task(3, WorkTaskStatus.Review, assignee: 1, estimate: 2m));
        tasks.Add(Samples.Task(4, WorkTaskStatus.Done, assignee: 1, estimate: 9m));
        WorkloadReportService report = new(tasks, Samples.TeamDirectory());

        // Act
        WorkloadRow row = report.Build().Single(item => item.MemberName == "Олена Коваль");

        // Assert
        Assert.Equal(2, row.InProgress);
        Assert.Equal(1, row.Review);
        Assert.Equal(10m, row.OpenEstimateHours);
        Assert.Equal(67, row.LoadPercent);
    }

    [Fact]
    public void Build_MemberWithoutTasks_HasZeroLoad()
    {
        // Arrange
        WorkloadReportService report = new(new FakeTaskRepository(), Samples.TeamDirectory());

        // Act
        IReadOnlyList<WorkloadRow> rows = report.Build();

        // Assert
        Assert.Equal(3, rows.Count);
        Assert.All(rows, row => Assert.Equal(0, row.LoadPercent));
    }
}
