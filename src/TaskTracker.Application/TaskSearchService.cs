using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>UC-05. Пошук завдань за колонкою, виконавцем і фрагментом назви.</summary>
public sealed class TaskSearchService
{
    private readonly ITaskRepository _tasks;

    /// <summary>Створює сервіс пошуку.</summary>
    /// <param name="tasks">Сховище завдань.</param>
    public TaskSearchService(ITaskRepository tasks)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
    }

    /// <summary>Знаходить завдання, що відповідають усім заданим критеріям.</summary>
    /// <param name="filter">Критерії пошуку.</param>
    /// <returns>Завдання за номером; порожній список, якщо нічого не знайдено.</returns>
    public IReadOnlyList<WorkTask> Find(TaskFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return _tasks.GetAll()
            .Where(task => Matches(task, filter))
            .OrderBy(task => task.Id)
            .ToList();
    }

    private static bool Matches(WorkTask task, TaskFilter filter)
    {
        bool statusMatches = filter.Status is null || task.Status == filter.Status;
        bool assigneeMatches = filter.AssigneeId is null || task.AssigneeId == filter.AssigneeId;
        bool titleMatches = string.IsNullOrWhiteSpace(filter.TitleText)
            || task.Title.Contains(filter.TitleText.Trim(), StringComparison.OrdinalIgnoreCase);
        return statusMatches && assigneeMatches && titleMatches;
    }
}
