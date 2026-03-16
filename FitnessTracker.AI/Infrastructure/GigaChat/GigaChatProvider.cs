// FitnessTracker.AI/Infrastructure/GigaChat/GigaChatProvider.cs
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Exceptions;

namespace FitnessTracker.AI.Infrastructure.GigaChat;

/// <summary>
/// Реализация IAiProvider для работы с GigaChat от Сбера.
/// </summary>
public class GigaChatProvider : IAiProvider
{
    private readonly ILogger<GigaChatProvider> _logger;
    private readonly GigaChatConfig _config;
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public GigaChatProvider(
        IOptions<GigaChatConfig> config,
        ILogger<GigaChatProvider> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _config = config.Value;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<AiResponse> AskAsync(string prompt, AiOptions? options = null, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            await EnsureTokenAsync(cancellationToken);

            var requestBody = BuildRequestBody(prompt, options);
            var response = await _httpClient.PostAsJsonAsync(
                "https://gigachat.devices.sberbank.ru/api/v1/chat/completions",
                requestBody,
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GigaChat error: {StatusCode} - {Error}", response.StatusCode, responseBody);
                return AiResponse.Failure($"GigaChat error: {response.StatusCode}", DateTime.UtcNow - startTime);
            }

            using var doc = JsonDocument.Parse(responseBody);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var model = doc.RootElement.TryGetProperty("model", out var modelEl) ? modelEl.GetString() : _config.Model;
            var tokens = doc.RootElement.TryGetProperty("usage", out var usage) && usage.TryGetProperty("total_tokens", out var t)
                ? t.GetInt32()
                : (int?)null;

            return AiResponse.Success(content ?? string.Empty, model, tokens, DateTime.UtcNow - startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GigaChat");
            return AiResponse.Failure(ex.Message, DateTime.UtcNow - startTime);
        }
    }

    /// <inheritdoc />
    public async Task<AiResponse<T>> AskStructuredAsync<T>(string prompt, AiOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var response = await AskAsync(prompt, options, cancellationToken);
            if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
                return AiResponse<T>.Failure(response.Error ?? "Empty response", response.Duration);

            // Очищаем ответ от markdown-обёрток (иногда GigaChat возвращает JSON в ```json ... ```)
            var cleaned = CleanJsonResponse(response.Content);
            var data = JsonSerializer.Deserialize<T>(cleaned);

            if (data == null)
                return AiResponse<T>.Failure("Failed to deserialize structured data", response.Duration);

            return AiResponse<T>.Success(data, response.Content, response.ModelUsed, response.TokensUsed, response.Duration);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse structured response");
            return AiResponse<T>.Failure($"JSON parse error: {ex.Message}", DateTime.UtcNow - startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in structured request");
            return AiResponse<T>.Failure(ex.Message, DateTime.UtcNow - startTime);
        }
    }

    /// <summary>
    /// Получение или обновление токена доступа.
    /// Токен живёт 30 минут, кешируем его.
    /// </summary>
    private async Task EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
            return;

        try
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.AuthorizationKey));
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {auth}");
            _httpClient.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());

            var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", _config.Scope) });
            var response = await _httpClient.PostAsync("https://ngw.devices.sberbank.ru:9443/api/v2/oauth", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: cancellationToken);
            _accessToken = json?.RootElement.GetProperty("access_token").GetString();
            _tokenExpiry = DateTime.UtcNow.AddMinutes(30);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain GigaChat token");
            throw new AiProviderException("GigaChat", "Token acquisition failed", "GIGACHAT_TOKEN_ERROR");
        }
    }

    private object BuildRequestBody(string prompt, AiOptions? options)
    {
        var opt = options ?? AiOptions.Default;
        var model = opt.Model ?? _config.Model;
        var systemPrompt = opt.SystemPrompt ?? _config.DefaultSystemPrompt;

        return new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
            temperature = opt.Temperature,
            max_tokens = opt.MaxTokens,
            stream = false
        };
    }

    private static string CleanJsonResponse(string json)
    {
        if (json.Contains("```"))
        {
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');
            if (start >= 0 && end >= 0)
                return json.Substring(start, end - start + 1);
        }
        return json;
    }
}