using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace FitnessTracker.AI.Recognition.Classifiers;

public class GroupClassifier : IGroupClassifier
{
    private readonly ILogger<GroupClassifier> _logger;
    private readonly IGigaChatTokenService _tokenService;
    private readonly ICommandRegistry _commandRegistry;
    private readonly HttpClient _httpClient;

    public GroupClassifier(
        IGigaChatTokenService tokenService,
        ICommandRegistry commandRegistry,
        ILogger<GroupClassifier> logger,
        HttpClient httpClient)
    {
        _tokenService = tokenService;
        _commandRegistry = commandRegistry;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string?> ClassifyGroupAsync(string message)
    {
        try
        {
            var groups = _commandRegistry.GetAllCommands()
                .Select(c => c.Group)
                .Distinct()
                .ToList();

            if (!groups.Any())
            {
                _logger.LogWarning("No groups found in command registry");
                return null;
            }

            _logger.LogInformation("Available groups: {Groups}", string.Join(", ", groups));

            var token = await _tokenService.GetAccessTokenAsync();

            // СОЗДАЕМ КЛИЕНТ С SSL ИГНОРИРОВАНИЕМ
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var prompt = $@"Ты - классификатор запросов для фитнес-бота.

Доступные группы:
{string.Join("\n", groups.Select(g => $"- {g}"))}

Определи, к какой группе относится запрос пользователя.

Сообщение: '{message}'

Верни ТОЛЬКО название группы в JSON формате. Никакого дополнительного текста.
Пример ответа: {{ ""group"": ""Profile"" }}";

            _logger.LogDebug("Group classification prompt: {Prompt}", prompt);

            var requestBody = new
            {
                model = "GigaChat-2",
                messages = new[]
                {
                new { role = "system", content = "Ты - классификатор. Отвечай только JSON." },
                new { role = "user", content = prompt }
            },
                temperature = 0.1,
                max_tokens = 50
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Group classification error: {StatusCode} - {Error}", response.StatusCode, responseBody);
                return null;
            }

            var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(responseBody);
            var resultJson = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString() ?? "{}";

            _logger.LogDebug("Group classification raw response: {Response}", resultJson);

            // Очищаем от markdown если есть
            if (resultJson.Contains("```"))
            {
                var start = resultJson.IndexOf('{');
                var end = resultJson.LastIndexOf('}');
                if (start >= 0 && end >= 0)
                {
                    resultJson = resultJson.Substring(start, end - start + 1);
                }
            }

            var result = JsonSerializer.Deserialize<GroupResult>(resultJson);

            if (result?.group != null)
            {
                // Нормализуем группу
                var normalizedGroup = groups.FirstOrDefault(g =>
                    g.Equals(result.group, StringComparison.OrdinalIgnoreCase));

                if (normalizedGroup != null)
                {
                    _logger.LogInformation("Message classified as group: {Group}", normalizedGroup);
                    return normalizedGroup;
                }
            }

            _logger.LogWarning("Could not classify group for message: {Message}", message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in group classification");
            return null;
        }
    }

    private class GroupResult
    {
        public string? group { get; set; }
    }
}