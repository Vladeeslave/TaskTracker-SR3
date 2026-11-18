# Архітектура TaskTracker

## Шари

| Шар | Проєкт | Що містить | Що йому заборонено |
|---|---|---|---|
| Домен | TaskTracker.Domain | завдання, колонки, події, таблиця переходів, правило WIP, власні винятки | знати про файли, консоль, JSON та інші шари |
| Прикладний | TaskTracker.Application | сценарії UC-01…UC-06, інтерфейси сховищ, годинника й журналу, барикада вводу, Result | знати про формат зберігання й про консоль |
| Інфраструктура | TaskTracker.Infrastructure | сховище JSON, системний годинник, файловий журнал, генератор умовних даних | містити правила домену |
| Застосунок | TaskTracker.Cli | меню, читання вводу, форматування виводу, композиційний корінь | містити розрахунки й правила |

Тести лежать окремо (`tests/TaskTracker.Tests`) і посилаються на всі три бібліотеки.

## Схема залежностей (рисунок 1)

```mermaid
flowchart LR
    Cli[TaskTracker.Cli] --> App[TaskTracker.Application]
    Cli --> Infra[TaskTracker.Infrastructure]
    Infra --> App
    App --> Domain[TaskTracker.Domain]
```

- Cli → Application: меню викликає сценарії, а не правила.
- Cli → Infrastructure: лише композиційний корінь створює конкретні реалізації.
- Infrastructure → Application: сховище реалізує інтерфейс, оголошений у прикладному шарі.
- Application → Domain: сценарії користуються сутностями й правилами домену.
- До Domain не веде жодна стрілка, і файл проєкту Domain не містить жодного посилання.

## Правило залежностей і як воно закріплене

Залежності спрямовані всередину. Домен не має посилань на інші проєкти, тому компілятор не дозволить використати в ньому щось із Application чи Infrastructure. Додатково `ArchitectureTests` перевіряють, що збірки Domain і Application не посилаються на зайве.

## Композиційний корінь

Композиційний корінь — єдине місце, де створюються конкретні реалізації; це файл `src/TaskTracker.Cli/Program.cs`:

```csharp
SystemClock clock = new();
FileAppLog log = new(Path.Combine("logs", "app.log"), clock);
JsonBoardStore store = JsonBoardStore.Open(dataPath);
AppServices services = new(
    new TaskBoardService(store, store, clock, log),
    new TaskSearchService(store),
    new WorkloadReportService(store, store));
new MenuLoop(Console.In, Console.Out, services).Run();
```

Сервіси отримують залежності через конструктор і знають лише інтерфейси (`ITaskRepository`, `IDirectory`, `IClock`, `IAppLog`). Якби об’єкти створювались у різних місцях, підміна реалізації (файл на пам’ять, системний годинник на фіксований) потребувала б правок у кількох місцях; зібрані в одному, вони підмінюються в одному рядку, а в тестах просто створюються інші об’єкти.


## Точки розширення

| Передбачувана зміна | Механізм | Що змінюється |
|---|---|---|
| новий дозволений перехід між колонками | таблиця переходів TaskTransitions | один рядок у таблиці |
| інше сховище (CSV, база даних) | інтерфейс ITaskRepository | нова реалізація в Infrastructure; сервіси не змінюються |
| інший спосіб ведення журналу | інтерфейс IAppLog | нова реалізація в Infrastructure |
