using TaskTracker.Domain;

namespace TaskTracker.Infrastructure;

/// <summary>Вміст файлу даних: довідники й завдання.</summary>
public sealed class BoardFile
{
    /// <summary>Учасники команди.</summary>
    public IReadOnlyList<TeamMember> Members { get; init; } = Array.Empty<TeamMember>();

    /// <summary>Проєкти.</summary>
    public IReadOnlyList<Project> Projects { get; init; } = Array.Empty<Project>();

    /// <summary>Знімки завдань.</summary>
    public IReadOnlyList<WorkTaskSnapshot> Tasks { get; init; } = Array.Empty<WorkTaskSnapshot>();
}
