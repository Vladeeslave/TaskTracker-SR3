namespace TaskTracker.Domain;

/// <summary>Рішення правила про ліміт незавершеної роботи.</summary>
public enum StartDecision
{
    /// <summary>Завдання можна взяти в роботу.</summary>
    Allowed,

    /// <summary>У завдання немає виконавця.</summary>
    NoAssignee,

    /// <summary>У виконавця вже досягнуто ліміт незавершеної роботи.</summary>
    LimitReached,
}
