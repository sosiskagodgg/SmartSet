using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Core.Orchestration;

public class CommandOrchestrator
{
    private readonly ICommandRegistry _registry;
    private readonly IIntentClassifier _intentClassifier;
    private readonly IEntityRecognizer _entityRecognizer;
    private readonly ILogger<CommandOrchestrator> _logger;

    public CommandOrchestrator(
        ICommandRegistry registry,
        IIntentClassifier intentClassifier,
        IEntityRecognizer entityRecognizer,
        ILogger<CommandOrchestrator> logger)
    {
        _registry = registry;
        _intentClassifier = intentClassifier;
        _entityRecognizer = entityRecognizer;
        _logger = logger;
    }

    public async Task<CommandResult> ProcessAsync(
        string message,
        long userId,
        Dictionary<string, object>? sessionData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing message from user {UserId}: {Message}", userId, message);

            // Шаг 1: Классифицируем intent
            var intent = await _intentClassifier.ClassifyAsync(message, cancellationToken);

            // Шаг 2: Извлекаем сущности
            var entities = await _entityRecognizer.RecognizeAsync(message, intent, cancellationToken);

            // Шаг 3: Ищем подходящую команду
            var command = await _registry.FindBestMatchAsync(message, cancellationToken);

            if (command == null)
            {
                _logger.LogWarning("No command found for message: {Message}", message);
                return CommandResult.Error("Не удалось определить команду. Напиши 'помощь' чтобы узнать что я умею.");
            }

            _logger.LogInformation("Found command: {CommandName}", command.Name);

            // Шаг 4: Создаем контекст
            var context = new CommandContext
            {
                OriginalMessage = message,
                Intent = intent,
                Entities = entities,
                UserId = userId,
                SessionData = sessionData ?? new Dictionary<string, object>()
            };

            // Шаг 5: Валидация
            if (!await command.ValidateAsync(context, cancellationToken))
            {
                var missingRequired = command.RequiredEntities
                    .Where(e => e.IsRequired && !context.HasEntity(e.Type))
                    .ToList();

                if (missingRequired.Any())
                {
                    var missingDescriptions = missingRequired.Select(e => e.Description);
                    return CommandResult.NeedMoreInfo(
                        $"Пожалуйста, укажи: {string.Join(", ", missingDescriptions)}",
                        missingRequired
                    );
                }
            }

            // Шаг 6: Выполняем команду
            var result = await command.ExecuteAsync(context, cancellationToken);

            _logger.LogInformation("Command {CommandName} executed with success: {IsSuccess}",
                command.Name, result.IsSuccess); // Success -> IsSuccess

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command for user {UserId}", userId);
            return CommandResult.Error("Произошла внутренняя ошибка. Попробуй позже.");
        }
    }
}