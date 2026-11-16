using TaskTracker.Domain;

namespace TaskTracker.Infrastructure;

/// <summary>Генератор умовних даних: вигадані імена, проєкти й завдання.</summary>
public static class SampleDataGenerator
{
    private const int MemberCount = 24;
    private const int ProjectCount = 6;
    private const int TaskCount = 120;
    private const int InProgressMembers = 8;
    private const int EstimateBase = 2;
    private const int EstimateSpread = 8;
    private const int SampleYear = 2026;
    private const int SampleMonth = 11;
    private const int SampleDay = 2;
    private const decimal SampleEntryHours = 2m;

    private static string[] FirstNames { get; } =
        {
            "Олена", "Петро", "Ірина", "Андрій", "Марта", "Богдан"
        };

    private static string[] LastNames { get; } = { "Коваль", "Шевчук", "Бондар", "Мельник" };

    private static string[] ProjectTitles { get; } =
    {
        "Мобільний застосунок", "Вебпортал", "Облік складу", "Звітність", "Інтеграції", "Підтримка",
    };

    private static string[] Verbs { get; } =
    {
        "Підготувати", "Перевірити", "Оновити", "Описати",
        "Спроєктувати", "Виправити", "Налаштувати",
    };

    private static string[] Subjects { get; } =
    {
        "звіт", "меню", "форму входу", "імпорт даних", "журнал подій", "резервне копіювання",
    };

    private static WorkTaskStatus[] Pattern { get; } =
    {
        WorkTaskStatus.New,
        WorkTaskStatus.InProgress,
        WorkTaskStatus.Review,
        WorkTaskStatus.Done,
        WorkTaskStatus.Done,
        WorkTaskStatus.Cancelled,
        WorkTaskStatus.New,
        WorkTaskStatus.Done,
    };

    /// <summary>Створює повний набір умовних даних: учасників, проєкти й завдання.</summary>
    /// <returns>Дані для запису у файл.</returns>
    public static BoardFile Create() => new()
    {
        Members = BuildMembers(),
        Projects = BuildProjects(),
        Tasks = BuildTasks(),
    };

    private static List<TeamMember> BuildMembers()
    {
        List<TeamMember> members = new();
        for (int id = 1; id <= MemberCount; id++)
        {
            string first = FirstNames[(id - 1) % FirstNames.Length];
            string last = LastNames[((id - 1) / FirstNames.Length) % LastNames.Length];
            members.Add(new TeamMember(id, $"{first} {last}"));
        }

        return members;
    }

    private static List<Project> BuildProjects()
    {
        List<Project> projects = new();
        for (int number = 1; number <= ProjectCount; number++)
        {
            string title = ProjectTitles[(number - 1) % ProjectTitles.Length];
            projects.Add(new Project($"TT-{number:00}", title));
        }

        return projects;
    }

    private static List<WorkTaskSnapshot> BuildTasks()
    {
        List<WorkTaskSnapshot> tasks = new();
        for (int index = 1; index <= TaskCount; index++)
        {
            tasks.Add(BuildTask(index));
        }

        return tasks;
    }

    private static WorkTaskSnapshot BuildTask(int index)
    {
        WorkTaskStatus status = Pattern[(index - 1) % Pattern.Length];
        int? assignee = AssigneeFor(index, status);
        return new WorkTaskSnapshot
        {
            Id = index,
            Title = TitleFor(index),
            ProjectCode = $"TT-{(index % ProjectCount) + 1:00}",
            AssigneeId = assignee,
            EstimateHours = EstimateBase + (index % EstimateSpread),
            Status = status,
            Entries = EntriesFor(assignee, status),
        };
    }

    private static string TitleFor(int index)
    {
        string verb = Verbs[index % Verbs.Length];
        string subject = Subjects[(index / Verbs.Length) % Subjects.Length];
        return $"{verb} {subject}";
    }

    private static int? AssigneeFor(int index, WorkTaskStatus status)
    {
        if (status == WorkTaskStatus.New)
        {
            return null;
        }

        if (status == WorkTaskStatus.InProgress)
        {
            return ((index / Pattern.Length) % InProgressMembers) + 1;
        }

        return (index % MemberCount) + 1;
    }

    private static List<TimeEntry> EntriesFor(int? assignee, WorkTaskStatus status)
    {
        bool hasWork = status is WorkTaskStatus.Review or WorkTaskStatus.Done;
        if (assignee is null || !hasWork)
        {
            return new List<TimeEntry>();
        }

        DateOnly date = new(SampleYear, SampleMonth, SampleDay);
        return new List<TimeEntry> { new TimeEntry(assignee.Value, SampleEntryHours, date) };
    }
}
