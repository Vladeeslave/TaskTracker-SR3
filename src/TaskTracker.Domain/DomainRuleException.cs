namespace TaskTracker.Domain;

/// <summary>Порушення правила предметної області; містить ідентифікатор правила.</summary>
public class DomainRuleException : Exception
{
    /// <summary>Створює виняток із ідентифікатором правила та повідомленням.</summary>
    /// <param name="rule">Ідентифікатор правила, наприклад wip.limit.</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule);
        Rule = rule;
    }

    /// <summary>Ідентифікатор порушеного правила.</summary>
    public string Rule { get; }
}
