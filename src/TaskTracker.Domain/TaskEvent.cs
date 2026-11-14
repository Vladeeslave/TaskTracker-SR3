namespace TaskTracker.Domain;

/// <summary>Подія, яка переміщає завдання між колонками дошки.</summary>
public enum TaskEvent
{
    /// <summary>Узяти завдання в роботу.</summary>
    Start,

    /// <summary>Передати виконану роботу на перевірку.</summary>
    SendToReview,

    /// <summary>Повернути завдання з перевірки на доопрацювання.</summary>
    Return,

    /// <summary>Прийняти роботу й закрити завдання.</summary>
    Complete,

    /// <summary>Скасувати завдання.</summary>
    Cancel,
}
