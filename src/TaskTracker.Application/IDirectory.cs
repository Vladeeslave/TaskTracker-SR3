using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>Довідники команди й проєктів лише для читання.</summary>
public interface IDirectory
{
    /// <summary>Усі учасники команди.</summary>
    IReadOnlyList<TeamMember> Members { get; }

    /// <summary>Шукає учасника за номером.</summary>
    /// <param name="id">Номер учасника.</param>
    /// <returns>Учасник або null, якщо такого немає.</returns>
    TeamMember? FindMember(int id);

    /// <summary>Перевіряє, чи існує проєкт із таким кодом.</summary>
    /// <param name="code">Код проєкту.</param>
    /// <returns>True, якщо проєкт існує.</returns>
    bool ProjectExists(string code);
}
