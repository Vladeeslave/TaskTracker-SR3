using System.Text;
using TaskTracker.Application;
using TaskTracker.Cli;
using TaskTracker.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;
string dataPath = Path.Combine("data", "board.json");
SystemClock clock = new();
FileAppLog log = new(Path.Combine("logs", "app.log"), clock);
try
{
    if (!File.Exists(dataPath))
    {
        JsonBoardStore.Write(dataPath, SampleDataGenerator.Create());
    }

    JsonBoardStore store = JsonBoardStore.Open(dataPath);
    AppServices services = new(
        new TaskBoardService(store, store, clock, log),
        new TaskSearchService(store),
        new WorkloadReportService(store, store));
    new MenuLoop(Console.In, Console.Out, services).Run();
}
catch (Exception ex)
{
    log.Error("APP", "Непередбачений збій", ex);
    Console.WriteLine($"Збій: {ex.Message}. Подробиці в logs/app.log.");
    return 1;
}

return 0;
