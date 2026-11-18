using TaskTracker.Application;
using TaskTracker.Domain;
using TaskTracker.Infrastructure;

namespace TaskTracker.Tests;

public class InfrastructureTests
{
    [Fact]
    public void SampleData_Create_HasExpectedSizesAndRespectsWipLimit()
    {
        // Act
        BoardFile data = SampleDataGenerator.Create();

        // Assert
        Assert.Equal(24, data.Members.Count);
        Assert.Equal(6, data.Projects.Count);
        Assert.Equal(120, data.Tasks.Count);
        int maxInProgress = data.Tasks
            .Where(task => task.Status == WorkTaskStatus.InProgress)
            .GroupBy(task => task.AssigneeId)
            .Max(group => group.Count());
        Assert.True(maxInProgress <= WipPolicy.WipLimit);
    }

    [Fact]
    public void JsonStore_WriteOpenSave_RoundTripsTasksAndDirectory()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        JsonBoardStore.Write(path, SampleDataGenerator.Create());

        try
        {
            // Act
            JsonBoardStore store = JsonBoardStore.Open(path);
            WorkTask created = WorkTask.Create(store.NextId(), "Нове завдання", "TT-01", 2m);
            store.Add(created);
            store.Save();
            JsonBoardStore reopened = JsonBoardStore.Open(path);

            // Assert
            Assert.Equal(121, created.Id);
            Assert.Equal(24, reopened.Members.Count);
            Assert.True(reopened.ProjectExists("tt-01"));
            Assert.Equal("Нове завдання", reopened.GetById(121)?.Title);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FileAppLog_WritesLevelScenarioAndMessage()
    {
        // Arrange
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        FileAppLog log = new(path, new FixedClock(new DateOnly(2026, 11, 20)));

        try
        {
            // Act
            log.Warning("UC-02", "Відхилено");
            string text = File.ReadAllText(path);

            // Assert
            Assert.Contains("WARN UC-02 Відхилено", text);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
