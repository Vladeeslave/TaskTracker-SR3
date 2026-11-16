using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>UC-06. Звіт «завантаження виконавців».</summary>
public sealed class WorkloadReportService
{
    private const decimal PercentScale = 100m;

    private readonly ITaskRepository _tasks;
    private readonly IDirectory _directory;

    /// <summary>Створює сервіс звіту.</summary>
    /// <param name="tasks">Сховище завдань.</param>
    /// <param name="directory">Довідники команди.</param>
    public WorkloadReportService(ITaskRepository tasks, IDirectory directory)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    /// <summary>Будує звіт: один рядок на кожного учасника команди.</summary>
    /// <returns>Рядки за спаданням завантаження, за рівної — за іменем.</returns>
    public IReadOnlyList<WorkloadRow> Build()
    {
        IReadOnlyList<WorkTask> tasks = _tasks.GetAll();
        return _directory.Members
            .Select(member => RowFor(member, tasks))
            .OrderByDescending(row => row.LoadPercent)
            .ThenBy(row => row.MemberName, StringComparer.Ordinal)
            .ToList();
    }

    private static WorkloadRow RowFor(TeamMember member, IReadOnlyList<WorkTask> tasks)
    {
        List<WorkTask> own = tasks.Where(task => task.AssigneeId == member.Id).ToList();
        int inProgress = own.Count(task => task.Status == WorkTaskStatus.InProgress);
        return new WorkloadRow
        {
            MemberName = member.Name,
            InProgress = inProgress,
            Review = own.Count(task => task.Status == WorkTaskStatus.Review),
            OpenEstimateHours = own
                .Where(task => IsOpen(task.Status))
                .Sum(task => task.EstimateHours),
            LoggedHours = own.Sum(task => task.LoggedHours),
            LoadPercent = PercentOfLimit(inProgress),
        };
    }

    private static int PercentOfLimit(int inProgress)
    {
        decimal percent = PercentScale * inProgress / WipPolicy.WipLimit;
        return (int)decimal.Round(percent, 0, MidpointRounding.AwayFromZero);
    }

    private static bool IsOpen(WorkTaskStatus status) =>
        status is WorkTaskStatus.New or WorkTaskStatus.InProgress or WorkTaskStatus.Review;
}
