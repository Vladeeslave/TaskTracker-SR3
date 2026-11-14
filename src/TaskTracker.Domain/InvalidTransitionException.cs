namespace TaskTracker.Domain;

/// <summary>Спроба виконати перехід між колонками, якого таблиця переходів не дозволяє.</summary>
public sealed class InvalidTransitionException : DomainRuleException
{
    /// <summary>Створює виняток для забороненого переходу.</summary>
    /// <param name="from">Колонка, у якій перебуває завдання.</param>
    /// <param name="trigger">Подія, яку намагалися виконати.</param>
    public InvalidTransitionException(WorkTaskStatus from, TaskEvent trigger)
        : base("task.transition", $"З колонки {from} подія {trigger} заборонена.")
    {
        From = from;
        Trigger = trigger;
    }

    /// <summary>Колонка, у якій перебувало завдання.</summary>
    public WorkTaskStatus From { get; }

    /// <summary>Подія, яку не дозволено виконати.</summary>
    public TaskEvent Trigger { get; }
}
