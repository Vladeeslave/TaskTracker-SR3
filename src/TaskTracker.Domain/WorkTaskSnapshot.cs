namespace TaskTracker.Domain;

/// <summary>Незмінний знімок стану завдання для збереження й відновлення.</summary>
public sealed class WorkTaskSnapshot
{
    /// <summary>Номер завдання.</summary>
    public int Id { get; init; }

    /// <summary>Назва завдання.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Код проєкту.</summary>
    public string ProjectCode { get; init; } = string.Empty;

    /// <summary>Номер виконавця або null, якщо його не призначено.</summary>
    public int? AssigneeId { get; init; }

    /// <summary>Оцінка завдання, годин.</summary>
    public decimal EstimateHours { get; init; }

    /// <summary>Поточна колонка.</summary>
    public WorkTaskStatus Status { get; init; }

    /// <summary>Записи витраченого часу.</summary>
    public IReadOnlyList<TimeEntry> Entries { get; init; } = Array.Empty<TimeEntry>();
}
