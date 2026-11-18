namespace TaskTracker.Domain;

/// <summary>Межі значень, спільні для домену та перевірки вводу.</summary>
public static class TaskLimits
{
    /// <summary>Найбільша довжина назви завдання, символів.</summary>
            public const int MaxTitleLength = 100;

    /// <summary>Найбільша оцінка завдання, годин.</summary>
    public const decimal MaxEstimateHours = 200m;

    /// <summary>Найбільше значення одного запису часу, годин.</summary>
    public const decimal MaxEntryHours = 24m;
}
