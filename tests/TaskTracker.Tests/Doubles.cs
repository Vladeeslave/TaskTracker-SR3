using TaskTracker.Application;
using TaskTracker.Domain;

namespace TaskTracker.Tests;

internal sealed class FakeTaskRepository : ITaskRepository
{
    private readonly List<WorkTask> _items = new();

    public int SaveCalls { get; private set; }

    public WorkTask? GetById(int id) => _items.FirstOrDefault(task => task.Id == id);

    public IReadOnlyList<WorkTask> GetAll() => _items.ToList();

    public int NextId() => _items.Count == 0 ? 1 : _items.Max(task => task.Id) + 1;

    public void Add(WorkTask task) => _items.Add(task);

    public void Save() => SaveCalls++;
}

internal sealed class FakeDirectory : IDirectory
{
    private readonly List<TeamMember> _members;
    private readonly List<string> _projectCodes;

    public FakeDirectory(IEnumerable<TeamMember> members, params string[] projectCodes)
    {
        _members = members.ToList();
        _projectCodes = projectCodes.ToList();
    }

    public IReadOnlyList<TeamMember> Members => _members;

    public TeamMember? FindMember(int id) => _members.FirstOrDefault(member => member.Id == id);

    public bool ProjectExists(string code) => _projectCodes.Contains(code);
}

internal sealed class FixedClock : IClock
{
    private readonly DateOnly _today;

    public FixedClock(DateOnly today)
    {
        _today = today;
    }

    public DateOnly Today() => _today;

    public DateTimeOffset Now() => new(_today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}

internal sealed class SpyLog : IAppLog
{
    public List<string> Lines { get; } = new();

    public void Information(string scenario, string message) =>
        Lines.Add($"INFO {scenario} {message}");

    public void Warning(string scenario, string message) => Lines.Add($"WARN {scenario} {message}");

    public void Error(string scenario, string message, Exception exception) =>
        Lines.Add($"ERROR {scenario} {message}");
}

internal static class Samples
{
    public static WorkTask Task(
        int id,
        WorkTaskStatus status,
        int? assignee = null,
        decimal estimate = 4m)
    {
        return WorkTask.Restore(new WorkTaskSnapshot
        {
            Id = id,
            Title = $"Завдання {id}",
            ProjectCode = "TT-01",
            AssigneeId = assignee,
            EstimateHours = estimate,
            Status = status,
        });
    }

    public static FakeDirectory TeamDirectory() => new(
        new[]
        {
            new TeamMember(1, "Олена Коваль"),
            new TeamMember(2, "Петро Шевчук"),
            new TeamMember(3, "Ірина Бондар"),
        },
        "TT-01",
        "TT-02");

    public static TaskBoardService Service(
        FakeTaskRepository tasks,
        SpyLog log,
        FakeDirectory? directory = null) =>
        new(tasks, directory ?? TeamDirectory(), new FixedClock(new DateOnly(2026, 11, 20)), log);
}
