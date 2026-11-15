namespace TaskTracker.Domain;

/// <summary>Учасник команди, який може бути виконавцем завдання.</summary>
public sealed class TeamMember
{
    /// <summary>Створює учасника команди та перевіряє його дані.</summary>
    /// <param name="id">Номер учасника, більший за нуль.</param>
    /// <param name="name">Повне ім’я, не порожнє.</param>
    public TeamMember(int id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }

    /// <summary>Номер учасника.</summary>
    public int Id { get; }

    /// <summary>Повне ім’я учасника.</summary>
    public string Name { get; }
}
