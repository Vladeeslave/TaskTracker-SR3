using TaskTracker.Application;
using TaskTracker.Domain;

namespace TaskTracker.Cli;

/// <summary>Текстове меню: читає ввід, викликає сценарії й друкує результат.</summary>
internal sealed class MenuLoop
{
    private const string ExitChoice = "0";
    private const int PageSize = 20;
    private const string TriggerPrompt =
        "Подія: 1 Start, 2 SendToReview, 3 Return, 4 Complete, 5 Cancel";

    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly AppServices _services;
    private readonly Dictionary<string, Action> _commands;

    /// <summary>Створює меню над заданими потоками вводу й виводу.</summary>
    /// <param name="input">Потік вводу.</param>
    /// <param name="output">Потік виводу.</param>
    /// <param name="services">Прикладні сервіси.</param>
    public MenuLoop(TextReader input, TextWriter output, AppServices services)
    {
        _input = input;
        _output = output;
        _services = services;
        _commands = new Dictionary<string, Action>
        {
            ["1"] = CreateTask,
            ["2"] = MoveTask,
            ["3"] = AssignTask,
            ["4"] = LogTime,
            ["5"] = FindTasks,
            ["6"] = ShowReport,
        };
    }

    private static TaskEvent[] Triggers { get; } =
    {
        TaskEvent.Start,
        TaskEvent.SendToReview,
        TaskEvent.Return,
        TaskEvent.Complete,
        TaskEvent.Cancel,
    };

    /// <summary>Виконує меню, доки користувач не обере вихід або не закінчиться ввід.</summary>
    public void Run()
    {
        string? choice = ShowMenuAndRead();
        while (choice is not null && choice != ExitChoice)
        {
            Execute(choice);
            choice = ShowMenuAndRead();
        }
    }

    private static WorkTaskStatus? ParseStatus(string? text)
    {
        bool parsed = Enum.TryParse(text, true, out WorkTaskStatus status);
        return parsed && Enum.IsDefined(status) ? status : null;
    }

    private string? ShowMenuAndRead()
    {
        _output.WriteLine();
        _output.WriteLine("1 Створити завдання   2 Перемістити   3 Призначити виконавця");
        _output.WriteLine("4 Записати час        5 Знайти        6 Звіт   0 Вихід");
        return Ask("Пункт меню");
    }

    private void Execute(string choice)
    {
        if (!_commands.TryGetValue(choice, out Action? command))
        {
            _output.WriteLine("Невідомий пункт меню.");
            return;
        }

        try
        {
            command();
        }
        catch (DomainRuleException ex)
        {
            _output.WriteLine($"Відхилено: {ex.Message}");
        }
    }

    private string? Ask(string prompt)
    {
        _output.Write($"{prompt}: ");
        return _input.ReadLine();
    }

    private bool TryAskNumber(string prompt, out int number)
    {
        number = 0;
        if (int.TryParse(Ask(prompt), out number))
        {
            return true;
        }

        _output.WriteLine("Потрібне ціле число.");
        return false;
    }

    private void CreateTask()
    {
        string? title = Ask("Назва");
        string? project = Ask("Код проєкту, наприклад TT-01");
        string? estimate = Ask("Оцінка, годин");
        Print(_services.Board.CreateTask(new CreateTaskRequest(title, project, estimate)));
    }

    private void MoveTask()
    {
        if (!TryAskNumber("Номер завдання", out int taskId))
        {
            return;
        }

        bool known = TryAskNumber(TriggerPrompt, out int code);
        if (!known || code < 1 || code > Triggers.Length)
        {
            _output.WriteLine("Невідома подія.");
            return;
        }

        Print(_services.Board.MoveTask(taskId, Triggers[code - 1]));
    }

    private void AssignTask()
    {
        bool ready = TryAskNumber("Номер завдання", out int taskId);
        if (ready && TryAskNumber("Номер виконавця", out int memberId))
        {
            Print(_services.Board.AssignTask(taskId, memberId));
        }
    }

    private void LogTime()
    {
        if (TryAskNumber("Номер завдання", out int taskId))
        {
            Print(_services.Board.LogTime(taskId, Ask("Години, наприклад 2,5")));
        }
    }

    private void FindTasks()
    {
        string? status = Ask("Колонка (New, InProgress, Review, Done, Cancelled) або Enter");
        string? assignee = Ask("Номер виконавця або Enter");

        // TODO(TD-02): некоректний критерій пошуку мовчки ігнорується, а не пояснюється.
        TaskFilter filter = new()
        {
            Status = ParseStatus(status),
            AssigneeId = int.TryParse(assignee, out int parsedAssignee) ? parsedAssignee : null,
            TitleText = Ask("Фрагмент назви або Enter"),
        };
        IReadOnlyList<WorkTask> found = _services.Search.Find(filter);
        _output.WriteLine($"Знайдено: {found.Count}");
        foreach (WorkTask task in found.Take(PageSize))
        {
            string owner = task.AssigneeId?.ToString() ?? "—";
            _output.WriteLine($"#{task.Id} [{task.Status}] {task.Title} (виконавець: {owner})");
        }
    }

    private void ShowReport() =>
        _output.Write(WorkloadReportFormatter.Format(_services.Report.Build()));

    private void Print(Result<WorkTask> result)
    {
        if (!result.IsSuccess)
        {
            _output.WriteLine($"Помилка: {result.Error}");
            return;
        }

        WorkTask task = result.Value!;
        _output.WriteLine($"Готово: завдання {task.Id} у колонці {task.Status}.");
    }
}
