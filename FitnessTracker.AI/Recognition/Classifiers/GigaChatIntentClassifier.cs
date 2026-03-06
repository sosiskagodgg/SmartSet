using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Configuration;
using Newtonsoft.Json.Linq;

namespace FitnessTracker.AI.Recognition.Classifiers;

public class GigaChatIntentClassifier : IIntentClassifier
{
    private readonly ILogger<GigaChatIntentClassifier> _logger;
    private readonly GigaChatConfig _config;
    private readonly IGigaChatTokenService _tokenService;
    private readonly IServiceProvider _serviceProvider;

    public GigaChatIntentClassifier(
        IGigaChatTokenService tokenService,
        IOptions<GigaChatConfig> config,
        IServiceProvider serviceProvider,
        ILogger<GigaChatIntentClassifier> logger)
    {
        _tokenService = tokenService;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }
    public async Task<Intent?> ClassifyAsync(string message, List<ICommand> commands, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!commands.Any())
            {
                return new Intent(IntentType.Unknown, 0.1);
            }

            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var systemPrompt = BuildPromptFromCommands(commands); // Только переданные команды

            var requestBody = new
            {
                model = "GigaChat-2",
                messages = new[]
                {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = message }
            },
                temperature = 0.3,
                max_tokens = 500,
                stream = false
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GigaChat error: {StatusCode} - {Error}", response.StatusCode, responseBody);
                return null;
            }

            var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(responseBody);
            var resultJson = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? "{}";

            if (resultJson.Contains("```"))
            {
                var start = resultJson.IndexOf('{');
                var end = resultJson.LastIndexOf('}');
                if (start >= 0 && end >= 0)
                {
                    resultJson = resultJson.Substring(start, end - start + 1);
                }
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<GigaChatResult>(resultJson);

            if (result?.intent != null)
            {
                var command = commands.FirstOrDefault(c =>
                    c.Name.Equals(result.intent, StringComparison.OrdinalIgnoreCase));

                if (command != null)
                {
                    var intent = new Intent(IntentType.Custom, result.confidence)
                    {
                        CustomIntentName = command.Name
                    };

                    if (result.entities != null && result.entities.Any())
                    {
                        intent.Metadata["entities"] = result.entities.Select(e => new Entity
                        {
                            Type = e.type,
                            Value = e.value,
                            Confidence = 1.0
                        }).ToList();
                    }

                    return intent;
                }

                if (Enum.TryParse<IntentType>(result.intent, true, out var intentType))
                {
                    return new Intent(intentType, result.confidence);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GigaChat with commands");
            return null;
        }
    }
    public async Task<Intent?> ClassifyAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<ICommandRegistry>();
            var commands = registry.GetAllCommands().ToList();

            // Логируем все зарегистрированные команды
            _logger.LogInformation("=== ЗАРЕГИСТРИРОВАННЫЕ КОМАНДЫ ===");
            _logger.LogInformation("Всего команд в реестре: {Count}", commands.Count);
            foreach (var cmd in commands)
            {
                _logger.LogInformation("Команда: {Name}, Фразы: {Phrases}",
                    cmd.Name,
                    string.Join(", ", cmd.TrainingPhrases));
            }
            _logger.LogInformation("===================================");

            if (!commands.Any())
            {
                return new Intent(IntentType.Help, 0.8)
                {
                    Metadata = { ["reason"] = "Функционал пока не добавлен. Система ожидает регистрации команд." }
                };
            }

            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);

            // СОЗДАЕМ КЛИЕНТ С SSL ВРУЧНУЮ
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var systemPrompt = BuildPromptFromCommands(commands);

            // 🔥 ПОЛНЫЙ ДЕБАГ ПРОМТА
            _logger.LogInformation("========== ПОЛНЫЙ PROMPT ДЛЯ GIGACHAT ==========");
            _logger.LogInformation(systemPrompt);
            _logger.LogInformation("=================================================");
            _logger.LogInformation("СООБЩЕНИЕ ПОЛЬЗОВАТЕЛЯ: {Message}", message);

            var requestBody = new
            {
                model = "GigaChat-2",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = message }
                },
                temperature = 0.3,
                max_tokens = 500,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync( // ИСПОЛЬЗУЕМ client, НЕ _httpClient
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // 🔥 ПОЛНЫЙ ДЕБАГ ОТВЕТА
            _logger.LogInformation("========== ПОЛНЫЙ ОТВЕТ ОТ GIGACHAT ==========");
            _logger.LogInformation(responseBody);
            _logger.LogInformation("================================================");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GigaChat error: {StatusCode} - {Error}", response.StatusCode, responseBody);
                return FallbackToUnknown(message);
            }

            var jsonResponse = JObject.Parse(responseBody);
            var resultJson = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? "{}";

            // Clean markdown if present
            if (resultJson.Contains("```"))
            {
                var start = resultJson.IndexOf('{');
                var end = resultJson.LastIndexOf('}');
                if (start >= 0 && end >= 0)
                {
                    resultJson = resultJson.Substring(start, end - start + 1);
                }
            }

            _logger.LogInformation("ИЗВЛЕЧЕННЫЙ JSON: {ResultJson}", resultJson);

            var result = JsonSerializer.Deserialize<GigaChatResult>(resultJson);

            if (result?.intent != null)
            {
                _logger.LogInformation("РАСПОЗНАН INTENT: {Intent} с уверенностью {Confidence}",
                    result.intent, result.confidence);

                // Проверяем, является ли это кастомной командой
                var command = commands.FirstOrDefault(c =>
                    c.Name.Equals(result.intent, StringComparison.OrdinalIgnoreCase));

                if (command != null)
                {
                    var intent = new Intent(IntentType.Custom, result.confidence)
                    {
                        CustomIntentName = command.Name
                    };

                    if (result.entities != null && result.entities.Any())
                    {
                        intent.Metadata["entities"] = result.entities.Select(e => new Entity
                        {
                            Type = e.type,
                            Value = e.value,
                            Confidence = 1.0
                        }).ToList();
                    }

                    return intent;
                }

                // Проверяем стандартные интенты
                if (Enum.TryParse<IntentType>(result.intent, true, out var intentType))
                {
                    return new Intent(intentType, result.confidence);
                }
            }

            _logger.LogWarning("НЕ УДАЛОСЬ РАСПОЗНАТЬ INTENT");
            return FallbackToUnknown(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GigaChat");
            return FallbackToUnknown(message);
        }
    }

    private Intent? FallbackToUnknown(string message)
    {
        // Если GigaChat не работает, пробуем найти команду по ключевым словам
        if (message.Contains("помощь") || message.Contains("help") || message.Contains("что ты умеешь"))
        {
            return new Intent(IntentType.Help, 0.7);
        }

        if (message.Contains("отмена") || message.Contains("назад") || message.Contains("cancel"))
        {
            return new Intent(IntentType.Cancel, 0.7);
        }

        return new Intent(IntentType.Unknown, 0.1);
    }

    private string BuildPromptFromCommands(List<ICommand> commands)
    {
        var intentDescriptions = new List<string>();
        var entityDescriptions = new HashSet<string>();

        // Сначала добавляем стандартные интенты
        intentDescriptions.Add("- Help: Показать список доступных команд");
        intentDescriptions.Add("  Примеры: помощь, help, что ты умеешь, команды, справка");
        intentDescriptions.Add("- Cancel: Отменить текущее действие");
        intentDescriptions.Add("  Примеры: отмена, назад, cancel, отменить");

        foreach (var command in commands.Where(c => c.Name != "Help" && c.Name != "Cancel"))
        {
            // Добавляем описание команды и примеры фраз
            intentDescriptions.Add($"- {command.Name}: {command.Description}");

            // Добавляем примеры фраз для обучения
            if (command.TrainingPhrases.Any())
            {
                intentDescriptions.Add($"  Примеры: {string.Join(", ", command.TrainingPhrases.Take(3))}");
            }

            // Добавляем сущности, которые нужны команде
            foreach (var entity in command.RequiredEntities)
            {
                entityDescriptions.Add($"- {entity.Type}: {entity.Description}" +
                    (entity.PossibleValues != null ? $" (например: {string.Join(", ", entity.PossibleValues)})" : ""));
            }
        }

        return $@"Ты - AI ассистент для фитнес трекера. 
Определи ЧТО хочет пользователь из списка доступных команд:

{string.Join("\n", intentDescriptions)}

Типы сущностей, которые нужно извлекать:
{string.Join("\n", entityDescriptions)}

Верни ТОЛЬКО JSON в формате:
{{
    ""intent"": ""название_команды_или_интента"",
    ""confidence"": 0.0-1.0,
    ""entities"": [
        {{
            ""type"": ""тип_сущности"",
            ""value"": ""значение""
        }}
    ]
}}";
    }

    private class GigaChatResult
    {
        public string intent { get; set; } = string.Empty;
        public double confidence { get; set; }
        public List<GigaChatEntity>? entities { get; set; }
    }

    private class GigaChatEntity
    {
        public string type { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}