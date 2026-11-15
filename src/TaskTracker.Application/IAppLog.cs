using System.Diagnostics.CodeAnalysis;

namespace TaskTracker.Application;

/// <summary>Журнал застосунку з трьома рівнями: Information, Warning, Error.</summary>
public interface IAppLog
{
    /// <summary>Записує перебіг сценарію: початок і завершення.</summary>
    /// <param name="scenario">Ідентифікатор сценарію, наприклад UC-02.</param>
    /// <param name="message">Повідомлення без персональних даних.</param>
    void Information(string scenario, string message);

    /// <summary>Записує відхилену операцію.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Причина відхилення.</param>
    void Warning(string scenario, string message);

    /// <summary>Записує виняток із контекстом.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Що виконувалось.</param>
    /// <param name="exception">Виняток.</param>
    [SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "C# only project, Error is the log level name.")]
    void Error(string scenario, string message, Exception exception);
}
