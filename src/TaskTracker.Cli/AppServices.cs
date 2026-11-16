using TaskTracker.Application;

namespace TaskTracker.Cli;

/// <summary>Набір прикладних сервісів, який композиційний корінь передає меню.</summary>
/// <param name="Board">Сценарії роботи з дошкою.</param>
/// <param name="Search">Пошук завдань.</param>
/// <param name="Report">Звіт про завантаження виконавців.</param>
internal sealed record AppServices(
    TaskBoardService Board,
    TaskSearchService Search,
    WorkloadReportService Report);
