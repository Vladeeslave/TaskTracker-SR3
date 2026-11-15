namespace TaskTracker.Application;

/// <summary>Сирі дані форми створення завдання, як їх ввів користувач.</summary>
/// <param name="Title">Назва завдання.</param>
/// <param name="ProjectCode">Код проєкту.</param>
/// <param name="EstimateText">Оцінка в годинах текстом.</param>
public sealed record CreateTaskRequest(string? Title, string? ProjectCode, string? EstimateText);
