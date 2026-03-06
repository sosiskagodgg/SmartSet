using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Recognition.Classifiers;

/// <summary>
/// Классификатор, который возвращает Help только если сообщение похоже на запрос помощи
/// </summary>
public class HelpClassifier : IIntentClassifier
{
    private readonly ILogger<HelpClassifier> _logger;
    private readonly ICommandRegistry _commandRegistry;

    private readonly List<string> _helpKeywords = new()
    {
        "помощь",
        "что ты умеешь",
    };

    public HelpClassifier(
        ICommandRegistry commandRegistry,
        ILogger<HelpClassifier> logger)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
    }
    public async Task<Intent?> ClassifyAsync(string message, List<ICommand> commands, CancellationToken cancellationToken = default)
    {
        var lowerMessage = message.ToLowerInvariant();

        // Проверяем по ключевым словам
        foreach (var keyword in _helpKeywords)
        {
            if (lowerMessage.Contains(keyword))
            {
                _logger.LogDebug("Help keyword detected: {Keyword}", keyword);
                return new Intent(IntentType.Help, 0.9);
            }
        }

        return null;
    }
    public Task<Intent?> ClassifyAsync(string message, CancellationToken cancellationToken = default)
    {
        var commands = _commandRegistry.GetAllCommands().ToList();
        var lowerMessage = message.ToLowerInvariant();

        // Если есть команды, проверяем что сообщение действительно про помощь
        if (commands.Any())
        {
            // Проверяем по ключевым словам
            foreach (var keyword in _helpKeywords)
            {
                if (lowerMessage.Contains(keyword))
                {
                    _logger.LogDebug("Help keyword detected: {Keyword}", keyword);
                    return Task.FromResult<Intent?>(new Intent(IntentType.Help, 0.9));
                }
            }

            // Если не похоже на помощь, возвращаем null
            return Task.FromResult<Intent?>(null);
        }

        // Если команд нет, но сообщение похоже на помощь
        foreach (var keyword in _helpKeywords)
        {
            if (lowerMessage.Contains(keyword))
            {
                _logger.LogDebug("No commands but help requested");
                return Task.FromResult<Intent?>(new Intent(IntentType.Help, 0.8)
                {
                    Metadata = { ["reason"] = "help_requested_no_commands" }
                });
            }
        }

        // Если команд нет и это не запрос помощи - возвращаем Unknown
        _logger.LogDebug("No commands and not a help request, returning Unknown");
        return Task.FromResult<Intent?>(new Intent(IntentType.Unknown, 0.3)
        {
            Metadata = { ["reason"] = "no_commands_not_help" }
        });
    }
}