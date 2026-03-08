// FitnessTracker.AI.Recognition/Classifiers/DirectionClassifier.cs

using FitnessTracker.AI.Core.Base;
using FitnessTracker.AI.Core.Registry;
using FitnessTracker.AI.Recognition.Recognizers;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FitnessTracker.AI.Recognition.Classifiers;

/// <summary>
/// Классификатор, который определяет направление и поднаправление
/// </summary>
public class DirectionClassifier
{
    private readonly DirectionRegistry _registry;
    private readonly GigaChatEntityRecognizer _gigaChat;
    private readonly ILogger<DirectionClassifier> _logger;

    public DirectionClassifier(
        DirectionRegistry registry,
        GigaChatEntityRecognizer gigaChat,
        ILogger<DirectionClassifier> logger)
    {
        _registry = registry;
        _gigaChat = gigaChat;
        _logger = logger;
    }

    /// <summary>
    /// Определить направление по сообщению
    /// </summary>
    public async Task<DirectionBase?> ClassifyDirectionAsync(string message)
    {
        var directions = _registry.GetAllDirections();
        if (!directions.Any())
        {
            _logger.LogWarning("Нет зарегистрированных направлений");
            return null;
        }

        // Формируем промпт для нейросети
        var prompt = BuildDirectionPrompt(message, directions);

        // Отправляем в GigaChat
        var result = await _gigaChat.AskAsync(prompt);

        // Парсим ответ
        var directionName = ParseDirectionResponse(result);

        var direction = _registry.GetDirection(directionName);

        if (direction != null)
        {
            _logger.LogInformation("Определено направление: {Name}", direction.Name);
        }

        return direction;
    }

    /// <summary>
    /// Определить поднаправление с учетом контекста направления
    /// </summary>
    public async Task<SubDirectionBase?> ClassifySubDirectionAsync(string message, DirectionBase direction)
    {
        if (direction.SubDirections.Count == 0)
        {
            _logger.LogWarning("У направления {Name} нет поднаправлений", direction.Name);
            return null;
        }

        var prompt = BuildSubDirectionPrompt(message, direction);
        var result = await _gigaChat.AskAsync(prompt);
        var subDirectionName = ParseSubDirectionResponse(result);

        var subDirection = direction.SubDirections
            .FirstOrDefault(s => s.Name.Equals(subDirectionName, StringComparison.OrdinalIgnoreCase));

        if (subDirection != null)
        {
            _logger.LogInformation("Определено поднаправление: {Name}", subDirection.Name);
        }

        return subDirection;
    }

    private string BuildDirectionPrompt(string message, List<DirectionBase> directions)
    {
        var directionsList = string.Join("\n", directions.Select(d =>
            $"- {d.Name}: {d.Description} (фразы: {string.Join(", ", d.TrainingPhrases.Take(3))})"));

        return $@"
Определи направление запроса из списка:

{directionsList}

Сообщение: {message}

Верни ТОЛЬКО название направления в формате: {{ ""direction"": ""название"" }}";
    }

    private string BuildSubDirectionPrompt(string message, DirectionBase direction)
    {
        var subDirectionsList = string.Join("\n", direction.SubDirections.Select(s =>
            $"- {s.Name}: {s.Description} (фразы: {string.Join(", ", s.TrainingPhrases.Take(3))})"));

        return $@"
Определи поднаправление запроса в рамках направления '{direction.Name}':

{subDirectionsList}

Сообщение: {message}

Верни ТОЛЬКО название поднаправления в формате: {{ ""subDirection"": ""название"" }}";
    }

    private string? ParseDirectionResponse(string jsonResponse)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement.GetProperty("direction").GetString();
        }
        catch
        {
            return null;
        }
    }

    private string? ParseSubDirectionResponse(string jsonResponse)
    {
        try
        {
            var doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement.GetProperty("subDirection").GetString();
        }
        catch
        {
            return null;
        }
    }
}