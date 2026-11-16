using TaskTracker.Application;

namespace TaskTracker.Infrastructure;

/// <summary>Журнал застосунку у текстовому файлі: час, рівень, сценарій, повідомлення.</summary>
public sealed class FileAppLog : IAppLog
{
    private readonly string _path;
    private readonly IClock _clock;

    /// <summary>Створює журнал і підготовлює теку для файлу.</summary>
    /// <param name="path">Шлях до файлу журналу.</param>
    /// <param name="clock">Джерело часу для рядків журналу.</param>
    public FileAppLog(string path, IClock clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _path = path;
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    /// <summary>Записує рядок рівня Information.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Повідомлення без персональних даних.</param>
    public void Information(string scenario, string message) => Write("INFO", scenario, message);

    /// <summary>Записує рядок рівня Warning.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Причина відхилення.</param>
    public void Warning(string scenario, string message) => Write("WARN", scenario, message);

    /// <summary>Записує рядок рівня Error із типом і текстом винятку.</summary>
    /// <param name="scenario">Ідентифікатор сценарію.</param>
    /// <param name="message">Що виконувалось.</param>
    /// <param name="exception">Виняток.</param>
    public void Error(string scenario, string message, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Write("ERROR", scenario, $"{message} ({exception.GetType().Name}: {exception.Message})");
    }

    private void Write(string level, string scenario, string message)
    {
        string line = $"{_clock.Now():O} {level} {scenario} {message}{Environment.NewLine}";

        // TODO(TD-03): немає ротації журналу; файл logs/app.log необмежено зростає.
        File.AppendAllText(_path, line);
    }
}
