namespace TaskTracker.Domain;

/// <summary>Проєкт, до якого належать завдання команди.</summary>
public sealed class Project
{
    /// <summary>Створює проєкт та перевіряє його дані.</summary>
    /// <param name="code">Код проєкту, не порожній.</param>
    /// <param name="title">Назва проєкту, не порожня.</param>
    public Project(string code, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Code = code;
        Title = title;
    }

    /// <summary>Код проєкту.</summary>
    public string Code { get; }

    /// <summary>Назва проєкту.</summary>
    public string Title { get; }
}
