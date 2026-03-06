using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Commands.Base;

/// <summary>
/// Базовый класс для всех команд. Упрощает создание новых команд.
/// </summary>
public abstract class BaseCommand : ICommand
{
    protected readonly ILogger Logger;

    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual string Category => "General";
    public virtual string Group => "System"; // ПО УМОЛЧАНИЮ
    public virtual double ConfidenceThreshold => 0.7;

    public virtual List<string> TrainingPhrases { get; } = new();
    public virtual List<EntityDefinition> RequiredEntities { get; } = new();

    public Dictionary<string, object> Metadata { get; } = new();

    protected BaseCommand(ILogger logger)
    {
        Logger = logger;
    }

    public virtual Task<bool> CanHandleAsync(string message, CancellationToken cancellationToken = default)
    {
        // Базовая реализация - проверяем по TrainingPhrases
        var lowerMessage = message.ToLowerInvariant();

        foreach (var phrase in TrainingPhrases)
        {
            var lowerPhrase = phrase.ToLowerInvariant();

            // Простая проверка: все ключевые слова из фразы есть в сообщении
            var keywords = lowerPhrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var matchesAll = keywords.All(k => lowerMessage.Contains(k));

            if (matchesAll)
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public abstract Task<CommandResult> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);

    public virtual Task<bool> ValidateAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        // Базовая валидация - проверяем обязательные сущности
        var missingRequired = RequiredEntities
            .Where(e => e.IsRequired && !context.HasEntity(e.Type))
            .ToList();

        return Task.FromResult(!missingRequired.Any());
    }

    /// <summary>
    /// Хелпер для логирования выполнения команды
    /// </summary>
    protected void LogExecution(CommandContext context)
    {
        Logger.LogInformation(
            "Executing command {CommandName} for user {UserId}. Entities: {Entities}",
            Name,
            context.UserId,
            string.Join(", ", context.Entities.Select(e => $"{e.Type}:{e.Value}")));
    }

    /// <summary>
    /// Хелпер для создания успешного результата
    /// </summary>
    protected CommandResult Success(string message, Dictionary<string, object>? data = null)
    {
        return new CommandResult
        {
            IsSuccess = true,  // Success -> IsSuccess
            Message = message,
            Data = data ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Хелпер для создания ошибки
    /// </summary>
    protected CommandResult Error(string error)
    {
        Logger.LogWarning("Command {CommandName} failed: {Error}", Name, error);

        return new CommandResult
        {
            IsSuccess = false,  // Success -> IsSuccess
            ErrorMessage = error
        };
    }
}