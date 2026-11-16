using System.Text;
using TaskTracker.Application;

namespace TaskTracker.Cli;

/// <summary>Форматує звіт про завантаження виконавців у текстову таблицю.</summary>
internal static class WorkloadReportFormatter
{
    /// <summary>Будує текст таблиці зі шапкою й одним рядком на виконавця.</summary>
    /// <param name="rows">Рядки звіту.</param>
    /// <returns>Текст для виведення в консоль.</returns>
    public static string Format(IReadOnlyList<WorkloadRow> rows)
    {
        StringBuilder text = new();
        text.AppendLine(
            $"{"Виконавець",-22}{"У роботі",9}{"Перевірка",11}"
            + $"{"Оцінка, г",11}{"Витрачено",11}{"Навантаж.",11}");
        foreach (WorkloadRow row in rows)
        {
            text.AppendLine(
                $"{row.MemberName,-22}{row.InProgress,9}{row.Review,11}"
                + $"{row.OpenEstimateHours,11:0.0}{row.LoggedHours,11:0.0}"
                + $"{row.LoadPercent + "%",11}");
        }

        return text.ToString();
    }
}
