using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>Критерії пошуку завдань; порожній критерій не обмежує вибірку.</summary>
public sealed class TaskFilter
{
    /// <summary>Колонка дошки або null, якщо будь-яка.</summary>
    public WorkTaskStatus? Status { get; init; }

    /// <summary>Номер виконавця або null, якщо будь-який.</summary>
    public int? AssigneeId { get; init; }

    /// <summary>Фрагмент назви без урахування регістру або null.</summary>
    public string? TitleText { get; init; }
}
