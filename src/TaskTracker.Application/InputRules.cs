using System.Globalization;
using TaskTracker.Domain;

namespace TaskTracker.Application;

/// <summary>Барикада вводу: дані користувача перевіряються один раз, на вході.</summary>
public static class InputRules
{
    /// <summary>Перевіряє назву завдання.</summary>
    /// <param name="title">Введена назва.</param>
    /// <returns>Пояснення помилки або null, якщо назва коректна.</returns>
    public static string? CheckTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Назва не може бути порожньою.";
        }

        return title.Trim().Length > TaskLimits.MaxTitleLength
            ? $"Назва задовга: не більше {TaskLimits.MaxTitleLength} символів."
            : null;
    }

    /// <summary>Розбирає кількість годин, введену текстом.</summary>
    /// <param name="text">Введений текст; дробова частина через кому.</param>
    /// <param name="max">Найбільше допустиме значення.</param>
    /// <returns>Години або пояснення помилки.</returns>
    public static Result<decimal> ParseHours(string? text, decimal max)
    {
        CultureInfo ukrainian = CultureInfo.GetCultureInfo("uk-UA");
        if (!decimal.TryParse(text, NumberStyles.Number, ukrainian, out decimal hours))
        {
            return Result.Fail<decimal>("Години мають бути числом, наприклад 2,5.");
        }

        return hours > 0m && hours <= max
            ? Result.Ok(hours)
            : Result.Fail<decimal>($"Години мають бути більшими за нуль і не більшими за {max}.");
    }
}
