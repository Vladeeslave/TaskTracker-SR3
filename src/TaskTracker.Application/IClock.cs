namespace TaskTracker.Application;

/// <summary>Джерело поточного часу; у тестах підміняється фіксованим.</summary>
public interface IClock
{
    /// <summary>Поточна дата.</summary>
    /// <returns>Сьогоднішня дата.</returns>
    DateOnly Today();

    /// <summary>Поточні дата й час зі зсувом.</summary>
    /// <returns>Поточний момент.</returns>
    DateTimeOffset Now();
}
