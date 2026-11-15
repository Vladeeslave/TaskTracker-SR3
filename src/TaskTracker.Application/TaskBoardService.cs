using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>Сценарії роботи з дошкою: створити, перемістити, призначити, записати час.</summary>
public sealed class TaskBoardService
{
    private const string CreateScenario = "UC-01";
    private const string MoveScenario = "UC-02";
    private const string AssignScenario = "UC-03";
    private const string TimeScenario = "UC-04";

    private readonly ITaskRepository _tasks;
    private readonly IDirectory _directory;
    private readonly IClock _clock;
    private readonly IAppLog _log;

    /// <summary>Створює сервіс із чотирма залежностями.</summary>
    /// <param name="tasks">Сховище завдань.</param>
    /// <param name="directory">Довідники команди й проєктів.</param>
    /// <param name="clock">Джерело часу.</param>
    /// <param name="log">Журнал застосунку.</param>
    public TaskBoardService(ITaskRepository tasks, IDirectory directory, IClock clock, IAppLog log)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    /// <summary>UC-01. Створює завдання за введеними даними.</summary>
    /// <param name="request">Сирі дані форми.</param>
    /// <returns>Створене завдання або пояснення помилки вводу.</returns>
    public Result<WorkTask> CreateTask(CreateTaskRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _log.Information(CreateScenario, "Початок");
        string? error = Validate(request, out decimal estimate);
        if (error is not null)
        {
            return Reject(CreateScenario, error);
        }

        string projectCode = request.ProjectCode!.Trim();
        string title = request.Title!.Trim();
        WorkTask task = WorkTask.Create(_tasks.NextId(), title, projectCode, estimate);
        _tasks.Add(task);
        _tasks.Save();
        _log.Information(CreateScenario, $"Створено завдання {task.Id}");
        return Result.Ok(task);
    }

    /// <summary>UC-02. Переміщає завдання дошкою за таблицею переходів і правилом WIP.</summary>
    /// <param name="taskId">Номер завдання.</param>
    /// <param name="trigger">Подія переходу.</param>
    /// <returns>Переміщене завдання або пояснення, що завдання не знайдено.</returns>
    /// <exception cref="DomainRuleException">Перехід заборонено або порушено WIP-ліміт.</exception>
    public Result<WorkTask> MoveTask(int taskId, TaskEvent trigger)
    {
        _log.Information(MoveScenario, $"Початок: завдання {taskId}, подія {trigger}");
        WorkTask? task = _tasks.GetById(taskId);
        if (task is null)
        {
            return Reject(MoveScenario, $"Завдання {taskId} не знайдено.");
        }

        Apply(MoveScenario, () => task.Move(trigger, CountInProgress(task.AssigneeId)));
        _tasks.Save();
        _log.Information(MoveScenario, $"Завдання {taskId} тепер у колонці {task.Status}");
        return Result.Ok(task);
    }

    /// <summary>UC-03. Призначає виконавця завданню в колонці New.</summary>
    /// <param name="taskId">Номер завдання.</param>
    /// <param name="memberId">Номер учасника команди.</param>
    /// <returns>Завдання або пояснення, що завдання чи учасника не знайдено.</returns>
    /// <exception cref="DomainRuleException">Завдання вже не в колонці New.</exception>
    public Result<WorkTask> AssignTask(int taskId, int memberId)
    {
        _log.Information(AssignScenario, "Початок");
        WorkTask? task = _tasks.GetById(taskId);
        if (task is null)
        {
            return Reject(AssignScenario, $"Завдання {taskId} не знайдено.");
        }

        if (_directory.FindMember(memberId) is null)
        {
            return Reject(AssignScenario, $"Учасника {memberId} не знайдено.");
        }

        Apply(AssignScenario, () => task.Assign(memberId));
        _tasks.Save();
        _log.Information(AssignScenario, $"Завдання {taskId} призначено учаснику {memberId}");
        return Result.Ok(task);
    }

    /// <summary>UC-04. Записує витрачений час від імені виконавця завдання.</summary>
    /// <param name="taskId">Номер завдання.</param>
    /// <param name="hoursText">Години текстом.</param>
    /// <returns>Завдання або пояснення помилки вводу.</returns>
    /// <exception cref="DomainRuleException">Завдання закрите.</exception>
    public Result<WorkTask> LogTime(int taskId, string? hoursText)
    {
        _log.Information(TimeScenario, "Початок");
        Result<decimal> hours = InputRules.ParseHours(hoursText, TaskLimits.MaxEntryHours);
        if (!hours.IsSuccess)
        {
            return Reject(TimeScenario, hours.Error);
        }

        WorkTask? task = _tasks.GetById(taskId);
        if (task is null)
        {
            return Reject(TimeScenario, $"Завдання {taskId} не знайдено.");
        }

        if (task.AssigneeId is not int assignee)
        {
            return Reject(TimeScenario, $"У завдання {taskId} немає виконавця.");
        }

        TimeEntry entry = new(assignee, hours.Value, _clock.Today());
        Apply(TimeScenario, () => task.LogTime(entry));
        _tasks.Save();
        _log.Information(TimeScenario, $"До завдання {taskId} записано {hours.Value} год");
        return Result.Ok(task);
    }

    private string? Validate(CreateTaskRequest request, out decimal estimate)
    {
        estimate = 0m;
        string? titleError = InputRules.CheckTitle(request.Title);
        if (titleError is not null)
        {
            return titleError;
        }

        string? projectError = CheckProject(request.ProjectCode);
        if (projectError is not null)
        {
            return projectError;
        }

        Result<decimal> parsed =
            InputRules.ParseHours(request.EstimateText, TaskLimits.MaxEstimateHours);
        estimate = parsed.Value;
        return parsed.IsSuccess ? null : parsed.Error;
    }

    private string? CheckProject(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Код проєкту не може бути порожнім.";
        }

        string trimmed = code.Trim();
        return _directory.ProjectExists(trimmed) ? null : $"Проєкту з кодом {trimmed} немає.";
    }

    private int CountInProgress(int? assigneeId) =>
        assigneeId is null
            ? 0
            : _tasks.GetAll().Count(task =>
                task.AssigneeId == assigneeId && task.Status == WorkTaskStatus.InProgress);

    private Result<WorkTask> Reject(string scenario, string message)
    {
        _log.Warning(scenario, $"Відхилено: {message}");
        return Result.Fail<WorkTask>(message);
    }

    private void Apply(string scenario, Action action)
    {
        try
        {
            action();
        }
        catch (DomainRuleException ex)
        {
            _log.Warning(scenario, $"Відхилено правилом {ex.Rule}");
            throw;
        }
    }
}
