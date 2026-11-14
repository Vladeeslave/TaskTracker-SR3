namespace TaskTracker.Domain;

/// <summary>Правило ліміту незавершеної роботи (WIP) для колонки InProgress.</summary>
public static class WipPolicy
{
    /// <summary>Найбільша кількість завдань одного виконавця в колонці InProgress.</summary>
    public const int WipLimit = 3;

    /// <summary>Вирішує, чи можна завдання перемістити в колонку InProgress.</summary>
    /// <param name="hasAssignee">Чи призначено виконавця.</param>
    /// <param name="inProgressCount">Скільки завдань виконавця вже в роботі.</param>
    /// <returns>Рішення за таблицею рішень специфікації.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Кількість від’ємна.</exception>
    public static StartDecision Decide(bool hasAssignee, int inProgressCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(inProgressCount);
        if (!hasAssignee)
        {
            return StartDecision.NoAssignee;
        }

        return inProgressCount >= WipLimit ? StartDecision.LimitReached : StartDecision.Allowed;
    }

    /// <summary>Кидає виняток, якщо завдання не можна взяти в роботу.</summary>
    /// <param name="hasAssignee">Чи призначено виконавця.</param>
    /// <param name="inProgressCount">Скільки завдань виконавця вже в роботі.</param>
    /// <exception cref="DomainRuleException">Немає виконавця або досягнуто ліміт.</exception>
    public static void EnsureCanEnter(bool hasAssignee, int inProgressCount)
    {
        StartDecision decision = Decide(hasAssignee, inProgressCount);
        if (decision == StartDecision.NoAssignee)
        {
            throw new DomainRuleException("wip.no-assignee", "У завдання немає виконавця.");
        }

        if (decision == StartDecision.LimitReached)
        {
            throw new DomainRuleException(
                "wip.limit",
                $"У виконавця вже {inProgressCount} завдань у роботі, ліміт — {WipLimit}.");
        }
    }
}
