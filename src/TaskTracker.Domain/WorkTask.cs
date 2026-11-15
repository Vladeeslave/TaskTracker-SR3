namespace TaskTracker.Domain;

/// <summary>Завдання на дошці: документ зі станом і записами витраченого часу.</summary>
public sealed class WorkTask
{
    private readonly List<TimeEntry> _entries;

    private WorkTask(WorkTaskSnapshot snapshot)
    {
        Id = snapshot.Id;
        Title = snapshot.Title;
        ProjectCode = snapshot.ProjectCode;
        AssigneeId = snapshot.AssigneeId;
        EstimateHours = snapshot.EstimateHours;
        Status = snapshot.Status;
        _entries = new List<TimeEntry>(snapshot.Entries);
    }

    /// <summary>Номер завдання.</summary>
    public int Id { get; }

    /// <summary>Назва завдання.</summary>
    public string Title { get; }

    /// <summary>Код проєкту, до якого належить завдання.</summary>
    public string ProjectCode { get; }

    /// <summary>Номер виконавця або null, якщо його не призначено.</summary>
    public int? AssigneeId { get; private set; }

    /// <summary>Оцінка завдання, годин.</summary>
    public decimal EstimateHours { get; }

    /// <summary>Поточна колонка дошки.</summary>
    public WorkTaskStatus Status { get; private set; }

    /// <summary>Записи витраченого часу лише для читання.</summary>
    public IReadOnlyList<TimeEntry> Entries => _entries.AsReadOnly();

    /// <summary>Сума витрачених годин за всіма записами.</summary>
    public decimal LoggedHours => _entries.Sum(entry => entry.Hours);

    /// <summary>Створює нове завдання в колонці New без виконавця.</summary>
    /// <param name="id">Номер, більший за нуль.</param>
    /// <param name="title">Назва, не порожня й не довша за межу.</param>
    /// <param name="projectCode">Код проєкту, не порожній.</param>
    /// <param name="estimateHours">Оцінка: більше нуля й не більше за межу.</param>
    /// <returns>Нове завдання.</returns>
    public static WorkTask Create(int id, string title, string projectCode, decimal estimateHours)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectCode);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(title.Length, TaskLimits.MaxTitleLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(estimateHours);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(estimateHours, TaskLimits.MaxEstimateHours);

        return new WorkTask(new WorkTaskSnapshot
        {
            Id = id,
            Title = title,
            ProjectCode = projectCode,
            EstimateHours = estimateHours,
            Status = WorkTaskStatus.New,
        });
    }

    /// <summary>Відновлює завдання зі збереженого знімка без повторної валідації.</summary>
    /// <param name="snapshot">Знімок стану.</param>
    /// <returns>Відновлене завдання.</returns>
    public static WorkTask Restore(WorkTaskSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new WorkTask(snapshot);
    }

    /// <summary>Робить знімок поточного стану для збереження.</summary>
    /// <returns>Знімок стану.</returns>
    public WorkTaskSnapshot ToSnapshot() => new()
    {
        Id = Id,
        Title = Title,
        ProjectCode = ProjectCode,
        AssigneeId = AssigneeId,
        EstimateHours = EstimateHours,
        Status = Status,
        Entries = _entries.ToList(),
    };

    /// <summary>Призначає виконавця; дозволено лише для завдання в колонці New.</summary>
    /// <param name="memberId">Номер учасника, більший за нуль.</param>
    /// <exception cref="DomainRuleException">Завдання вже не в колонці New.</exception>
    public void Assign(int memberId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memberId);
        if (Status != WorkTaskStatus.New)
        {
            throw new DomainRuleException(
                "task.assign-status",
                $"Виконавця змінюють лише в колонці New, а завдання {Id} у стані {Status}.");
        }

        AssigneeId = memberId;
    }

    /// <summary>Переміщає завдання за таблицею переходів і правилом WIP.</summary>
    /// <param name="trigger">Подія переходу.</param>
    /// <param name="assigneeInProgress">Скільки завдань виконавця вже в роботі.</param>
    /// <exception cref="InvalidTransitionException">Перехід заборонено.</exception>
    /// <exception cref="DomainRuleException">Немає виконавця або досягнуто WIP-ліміт.</exception>
    public void Move(TaskEvent trigger, int assigneeInProgress)
    {
        WorkTaskStatus next = TaskTransitions.Next(Status, trigger);
        if (next == WorkTaskStatus.InProgress)
        {
            WipPolicy.EnsureCanEnter(AssigneeId.HasValue, assigneeInProgress);
        }

        Status = next;
    }

    /// <summary>Додає запис витраченого часу до відкритого завдання виконавця.</summary>
    /// <param name="entry">Запис часу.</param>
    /// <exception cref="DomainRuleException">Завдання закрите або запис чужий.</exception>
    public void LogTime(TimeEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (Status is WorkTaskStatus.Done or WorkTaskStatus.Cancelled)
        {
            throw new DomainRuleException(
                "task.closed",
                $"Завдання {Id} закрите, час записувати не можна.");
        }

        if (AssigneeId != entry.MemberId)
        {
            throw new DomainRuleException(
                "task.time-member",
                $"Час можна записати лише від виконавця завдання {Id}.");
        }

        _entries.Add(entry);
    }
}
