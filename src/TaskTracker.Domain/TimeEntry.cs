namespace TaskTracker.Domain;

/// <summary>Запис витраченого часу: хто, скільки годин і коли.</summary>
public sealed class TimeEntry
{
    /// <summary>Створює запис часу та перевіряє його дані.</summary>
    /// <param name="memberId">Номер учасника, більший за нуль.</param>
    /// <param name="hours">Години: більше нуля й не більше за межу запису.</param>
    /// <param name="date">Дата, коли виконувалась робота.</param>
    public TimeEntry(int memberId, decimal hours, DateOnly date)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memberId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hours);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hours, TaskLimits.MaxEntryHours);
        MemberId = memberId;
        Hours = hours;
        Date = date;
    }

    /// <summary>Номер учасника, який виконував роботу.</summary>
    public int MemberId { get; }

    /// <summary>Витрачені години.</summary>
    public decimal Hours { get; }

    /// <summary>Дата виконання роботи.</summary>
    public DateOnly Date { get; }
}
