using TaskTracker.Application;

namespace TaskTracker.Infrastructure;

/// <summary>Годинник, що повертає системний час.</summary>
public sealed class SystemClock : IClock
{
    /// <summary>Поточна дата.</summary>
    /// <returns>Сьогоднішня дата.</returns>
    public DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>Поточні дата й час зі зсувом.</summary>
    /// <returns>Поточний момент.</returns>
    public DateTimeOffset Now() => DateTimeOffset.Now;
}
