using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Configuration;

namespace FitnessTracker.AI.Recognition.Recognizers;

public class GigaChatEntityRecognizer : IEntityRecognizer
{
    private readonly ILogger<GigaChatEntityRecognizer> _logger;
    private readonly GigaChatConfig _config;
    private readonly IGigaChatTokenService _tokenService;
    private readonly ICommandRegistry _commandRegistry;

    public int Priority => 100;

    public GigaChatEntityRecognizer(
        IGigaChatTokenService tokenService,
        IOptions<GigaChatConfig> config,
        ICommandRegistry commandRegistry,
        ILogger<GigaChatEntityRecognizer> logger)
    {
        _tokenService = tokenService;
        _commandRegistry = commandRegistry;
        _logger = logger;
        _config = config.Value;
    }

    public async Task<List<Entity>> RecognizeAsync(string message, Intent? intent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Если нет intent или это Unknown, не тратим токены
            if (intent == null || intent.Type == IntentType.Unknown)
            {
                return new List<Entity>();
            }

            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);

            // СОЗДАЕМ КЛИЕНТ С SSL ВРУЧНУЮ
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true // ИГНОРИРУЕМ SSL
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            // Динамически строим список сущностей из всех команд
            var entityTypes = BuildEntityTypesFromCommands();

            var prompt = $@"
Извлеки из сообщения пользователя все сущности, связанные с тренировками.

Сообщение: '{message}'

Верни ТОЛЬКО JSON массив в формате:
[
    {{
        ""type"": ""тип_сущности"",
        ""value"": ""значение""
    }}
]

Типы сущностей которые нужно извлекать:
{entityTypes}

Если сущность не найдена, верни пустой массив [].
";

            var requestBody = new
            {
                model = "GigaChat-2",
                messages = new[]
                {
                new { role = "system", content = "Ты - AI для извлечения данных из сообщений о тренировках. Отвечай только JSON." },
                new { role = "user", content = prompt }
            },
                temperature = 0.1,
                max_tokens = 300
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync( // ИСПОЛЬЗУЕМ client, НЕ _httpClient
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("GigaChat entity recognition error: {StatusCode} - {Error}", response.StatusCode, error);
                return new List<Entity>();
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var gigaResponse = JsonSerializer.Deserialize<GigaChatResponse>(responseJson);

            var entitiesJson = gigaResponse?.Choices?[0]?.Message?.Content ?? "[]";
            entitiesJson = CleanJsonResponse(entitiesJson);

            var extractedEntities = JsonSerializer.Deserialize<List<GigaChatEntity>>(entitiesJson) ?? new();

            return extractedEntities.Select(e => new Entity
            {
                Type = e.type,
                Value = e.value,
                Confidence = 0.9,
                Metadata = new Dictionary<string, object> { ["source"] = "gigachat" }
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting entities with GigaChat");
            return new List<Entity>();
        }
    }

    private string BuildEntityTypesFromCommands()
    {
        var commands = _commandRegistry.GetAllCommands().ToList();

        if (!commands.Any())
        {
            return "- entity (базовая сущность)";
        }

        var entityDescriptions = new HashSet<string>();

        foreach (var command in commands)
        {
            foreach (var entity in command.RequiredEntities)
            {
                var description = $"- {entity.Type}: {entity.Description}";
                if (entity.PossibleValues != null && entity.PossibleValues.Any())
                {
                    description += $" (например: {string.Join(", ", entity.PossibleValues)})";
                }
                entityDescriptions.Add(description);
            }
        }

        return string.Join("\n", entityDescriptions);
    }

    private string CleanJsonResponse(string json)
    {
        if (json.Contains("```"))
        {
            var start = json.IndexOf('[');
            var end = json.LastIndexOf(']');
            if (start >= 0 && end >= 0)
            {
                return json.Substring(start, end - start + 1);
            }
        }
        return json;
    }

    private class GigaChatEntity
    {
        public string type { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    private class GigaChatResponse
    {
        public GigaChatChoice[]? Choices { get; set; }
    }

    private class GigaChatChoice
    {
        public GigaChatMessage? Message { get; set; }
    }

    private class GigaChatMessage
    {
        public string? Content { get; set; }
    }
}