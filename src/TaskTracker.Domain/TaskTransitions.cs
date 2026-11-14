namespace TaskTracker.Domain;

/// <summary>Таблиця переходів між колонками дошки.</summary>
public static class TaskTransitions
{
    private static Dictionary<(WorkTaskStatus Status, TaskEvent Trigger), WorkTaskStatus> Table { get; } =
       new()
       {
           [(WorkTaskStatus.New, TaskEvent.Start)] = WorkTaskStatus.InProgress,
           [(WorkTaskStatus.New, TaskEvent.Cancel)] = WorkTaskStatus.Cancelled,
           [(WorkTaskStatus.InProgress, TaskEvent.SendToReview)] = WorkTaskStatus.Review,
           [(WorkTaskStatus.InProgress, TaskEvent.Cancel)] = WorkTaskStatus.Cancelled,
           [(WorkTaskStatus.Review, TaskEvent.Return)] = WorkTaskStatus.InProgress,
           [(WorkTaskStatus.Review, TaskEvent.Complete)] = WorkTaskStatus.Done,
           [(WorkTaskStatus.Review, TaskEvent.Cancel)] = WorkTaskStatus.Cancelled,
       };

    /// <summary>Перевіряє, чи дозволено подію в поточній колонці.</summary>
    /// <param name="current">Поточна колонка.</param>
    /// <param name="trigger">Подія.</param>
    /// <returns>True, якщо перехід дозволено.</returns>
    public static bool IsAllowed(WorkTaskStatus current, TaskEvent trigger) =>
        Table.ContainsKey((current, trigger));

    /// <summary>Повертає колонку, у яку веде подія.</summary>
    /// <param name="current">Поточна колонка.</param>
    /// <param name="trigger">Подія.</param>
    /// <returns>Нова колонка.</returns>
    /// <exception cref="InvalidTransitionException">Перехід заборонено таблицею.</exception>
    public static WorkTaskStatus Next(WorkTaskStatus current, TaskEvent trigger)
    {
        if (!Table.TryGetValue((current, trigger), out WorkTaskStatus next))
        {
            throw new InvalidTransitionException(current, trigger);
        }

        return next;
    }
}
