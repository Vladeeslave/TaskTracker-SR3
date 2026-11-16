namespace TaskTracker.Application;

/// <summary>Рядок звіту «завантаження виконавців».</summary>
public sealed class WorkloadRow
{
    /// <summary>Ім’я виконавця.</summary>
    public string MemberName { get; init; } = string.Empty;

    /// <summary>Скільки завдань у колонці InProgress.</summary>
    public int InProgress { get; init; }

    /// <summary>Скільки завдань у колонці Review.</summary>
    public int Review { get; init; }

    /// <summary>Сума оцінок відкритих завдань, годин.</summary>
    public decimal OpenEstimateHours { get; init; }

    /// <summary>Сума витрачених годин за всіма завданнями виконавця.</summary>
    public decimal LoggedHours { get; init; }

    /// <summary>Завантаження: частка від WIP-ліміту у відсотках, округлена до цілого.</summary>
    public int LoadPercent { get; init; }
}
