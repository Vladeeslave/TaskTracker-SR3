using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>Сховище завдань: відповідає лише за зберігання й пошук за номером.</summary>
public interface ITaskRepository
{
    /// <summary>Шукає завдання за номером.</summary>
    /// <param name="id">Номер завдання.</param>
    /// <returns>Завдання або null, якщо такого немає.</returns>
    WorkTask? GetById(int id);

    /// <summary>Повертає всі завдання.</summary>
    /// <returns>Копія списку завдань.</returns>
    IReadOnlyList<WorkTask> GetAll();

    /// <summary>Повертає наступний вільний номер завдання.</summary>
    /// <returns>Номер, більший за нуль.</returns>
    int NextId();

    /// <summary>Додає нове завдання до сховища.</summary>
    /// <param name="task">Завдання.</param>
    void Add(WorkTask task);

    /// <summary>Зберігає зміни у постійне сховище.</summary>
    void Save();
}
