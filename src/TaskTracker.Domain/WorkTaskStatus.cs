namespace TaskTracker.Domain;

/// <summary>Колонка дошки: стан завдання в його життєвому циклі.</summary>
public enum WorkTaskStatus
{
    /// <summary>Нове завдання, роботу ще не розпочато.</summary>
    New,

    /// <summary>Роботу виконує призначений виконавець.</summary>
    InProgress,

    /// <summary>Роботу виконано, результат на перевірці.</summary>
    Review,

    /// <summary>Завдання прийнято й закрито.</summary>
    Done,

    /// <summary>Завдання скасовано.</summary>
    Cancelled,
}
