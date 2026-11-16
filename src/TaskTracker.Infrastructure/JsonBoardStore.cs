using System.Text.Json;
using System.Text.Json.Serialization;
using TaskTracker.Application;
using TaskTracker.Domain;

namespace TaskTracker.Infrastructure;

/// <summary>Сховище завдань і довідників в одному файлі JSON.</summary>
public sealed class JsonBoardStore : ITaskRepository, IDirectory
{
    private readonly string _path;
    private readonly List<TeamMember> _members;
    private readonly List<Project> _projects;
    private readonly List<WorkTask> _tasks;

    private JsonBoardStore(string path, BoardFile data)
    {
        _path = path;
        _members = new List<TeamMember>(data.Members);
        _projects = new List<Project>(data.Projects);
        _tasks = data.Tasks.Select(WorkTask.Restore).ToList();
    }

    /// <summary>Усі учасники команди.</summary>
    public IReadOnlyList<TeamMember> Members => _members;

    private static JsonSerializerOptions Options { get; } = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Відкриває сховище з наявного файлу.</summary>
    /// <param name="path">Шлях до файлу JSON.</param>
    /// <returns>Готове сховище.</returns>
    /// <exception cref="InvalidDataException">Файл порожній або пошкоджений.</exception>
    public static JsonBoardStore Open(string path)
    {
        string json = File.ReadAllText(path);
        BoardFile data = JsonSerializer.Deserialize<BoardFile>(json, Options)
            ?? throw new InvalidDataException($"Файл {path} порожній або пошкоджений.");
        return new JsonBoardStore(path, data);
    }

    /// <summary>Записує дані у файл JSON, створюючи теку за потреби.</summary>
    /// <param name="path">Шлях до файлу.</param>
    /// <param name="data">Дані для запису.</param>
    public static void Write(string path, BoardFile data)
    {
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
    }

    /// <summary>Шукає завдання за номером.</summary>
    /// <param name="id">Номер завдання.</param>
    /// <returns>Завдання або null.</returns>
    public WorkTask? GetById(int id) => _tasks.FirstOrDefault(task => task.Id == id);

    /// <summary>Повертає копію списку завдань.</summary>
    /// <returns>Усі завдання.</returns>
    public IReadOnlyList<WorkTask> GetAll() => _tasks.ToList();

    /// <summary>Повертає наступний вільний номер завдання.</summary>
    /// <returns>Номер, більший за нуль.</returns>
    public int NextId() => _tasks.Count == 0 ? 1 : _tasks.Max(task => task.Id) + 1;

    /// <summary>Додає завдання до пам’яті; на диск потрапляє після <see cref="Save"/>.</summary>
    /// <param name="task">Завдання.</param>
    public void Add(WorkTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        _tasks.Add(task);
    }

    /// <summary>Записує всі дані у файл.</summary>
    public void Save()
    {
        // TODO(TD-01): файл пишеться цілком; за тисяч завдань потрібен інший спосіб збереження.
        Write(_path, new BoardFile
        {
            Members = _members,
            Projects = _projects,
            Tasks = _tasks.Select(task => task.ToSnapshot()).ToList(),
        });
    }

    /// <summary>Шукає учасника команди за номером.</summary>
    /// <param name="id">Номер учасника.</param>
    /// <returns>Учасник або null.</returns>
    public TeamMember? FindMember(int id) => _members.FirstOrDefault(member => member.Id == id);

    /// <summary>Перевіряє, чи існує проєкт із таким кодом.</summary>
    /// <param name="code">Код проєкту.</param>
    /// <returns>True, якщо проєкт існує.</returns>
    public bool ProjectExists(string code) =>
        _projects.Any(project =>
            string.Equals(project.Code, code, StringComparison.OrdinalIgnoreCase));
}
