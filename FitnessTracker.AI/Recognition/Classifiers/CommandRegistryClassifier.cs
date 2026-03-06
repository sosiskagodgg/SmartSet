using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Recognition.Classifiers;

/// <summary>
/// Классификатор, который ищет команду по ключевым словам из TrainingPhrases
/// Работает даже без ML, просто по совпадению слов
/// </summary>
public class CommandRegistryClassifier : IIntentClassifier
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly ILogger<CommandRegistryClassifier> _logger;

    public CommandRegistryClassifier(
        ICommandRegistry commandRegistry,
        ILogger<CommandRegistryClassifier> logger)
    {
        _commandRegistry = commandRegistry;
        _logger = logger;
    }
    public async Task<Intent?> ClassifyAsync(string message, List<ICommand> commands, CancellationToken cancellationToken = default)
    {
        var lowerMessage = message.ToLowerInvariant();
        var bestMatch = FindBestMatch(lowerMessage, commands);

        if (bestMatch.HasValue)
        {
            var intent = new Intent(IntentType.Custom, bestMatch.Value.Item3)
            {
                CustomIntentName = bestMatch.Value.Item1.Name
            };
            intent.Metadata["matched_phrase"] = bestMatch.Value.Item2;

            return intent;
        }

        return null;
    }

    public Task<Intent?> ClassifyAsync(string message, CancellationToken cancellationToken = default)
    {
        var commands = _commandRegistry.GetAllCommands().ToList();

        if (!commands.Any())
        {
            _logger.LogDebug("No commands registered");
            return Task.FromResult<Intent?>(new Intent(IntentType.Help, 0.5)
            {
                Metadata = { ["reason"] = "no_commands" }
            });
        }

        var lowerMessage = message.ToLowerInvariant();
        var bestMatch = FindBestMatch(lowerMessage, commands);

        if (bestMatch.HasValue)
        {
            var intent = new Intent(IntentType.Custom, bestMatch.Value.Item3) // Confidence
            {
                CustomIntentName = bestMatch.Value.Item1.Name // Command.Name
            };
            intent.Metadata["matched_phrase"] = bestMatch.Value.Item2; // MatchedPhrase

            return Task.FromResult<Intent?>(intent);
        }

        // Если ничего не нашли, возвращаем Help с низкой уверенностью
        return Task.FromResult<Intent?>(new Intent(IntentType.Help, 0.3)
        {
            Metadata = { ["reason"] = "no_match" }
        });
    }

    private (ICommand, string, double)? FindBestMatch(
        string message,
        List<ICommand> commands)
    {
        var bestScore = 0.0;
        ICommand? bestCommand = null;
        string? bestPhrase = null;

        foreach (var command in commands)
        {
            foreach (var phrase in command.TrainingPhrases)
            {
                var score = CalculateMatchScore(message, phrase.ToLowerInvariant());

                if (score > bestScore && score >= command.ConfidenceThreshold)
                {
                    bestScore = score;
                    bestCommand = command;
                    bestPhrase = phrase;
                }
            }
        }

        if (bestCommand != null)
        {
            return (bestCommand, bestPhrase!, bestScore);
        }

        return null;
    }

    private double CalculateMatchScore(string message, string phrase)
    {
        // Простой алгоритм: считаем сколько слов из фразы есть в сообщении
        var messageWords = new HashSet<string>(message.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var phraseWords = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!phraseWords.Any()) return 0;

        var matchedWords = phraseWords.Count(w => messageWords.Contains(w));
        return (double)matchedWords / phraseWords.Length;
    }
}