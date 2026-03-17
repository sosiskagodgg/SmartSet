// FitnessTracker.AI/Infrastructure/GigaChat/GigaChatProvider.cs

using System.Net.Http.Json;
using System.Text.Json;
using FitnessTracker.AI.Core.Exceptions;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessTracker.AI.Infrastructure.GigaChat;

/// <summary>
/// Реализация IAiProvider для работы с GigaChat от Сбера.
/// </summary>
public class GigaChatProvider : IAiProvider
{
    private readonly ILogger<GigaChatProvider> _logger;
    private readonly GigaChatConfig _config;
    private readonly HttpClient _tokenClient;  // для получения токена (порт 9443)
    private readonly HttpClient _apiClient;    // для API запросов
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public GigaChatProvider(
        IOptions<GigaChatConfig> config,
        ILogger<GigaChatProvider> logger,
        HttpClient tokenClient,
        HttpClient apiClient)
    {
        _logger = logger;
        _config = config.Value;
        _tokenClient = tokenClient;
        _apiClient = apiClient;

        _logger.LogInformation("=== GIGACHAT PROVIDER INITIALIZED ===");
        _logger.LogInformation("Token client base address: {BaseAddress}", _tokenClient.BaseAddress);
        _logger.LogInformation("API client base address: {BaseAddress}", _apiClient.BaseAddress);
        _logger.LogInformation("Model: {Model}", _config.Model);
    }

    /// <inheritdoc />
    public async Task<AiResponse> AskAsync(string prompt, AiOptions? options = null, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            await EnsureTokenAsync(cancellationToken);

            var requestBody = BuildRequestBody(prompt, options);

            _logger.LogDebug("Sending request to GigaChat API with prompt length: {Length}", prompt.Length);

            var response = await _apiClient.PostAsJsonAsync(
                "chat/completions",
                requestBody,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GigaChat API error: {StatusCode} - {Error}",
                    response.StatusCode, responseBody);
                return AiResponse.Failure(
                    $"GigaChat API error: {response.StatusCode}",
                    DateTime.UtcNow - startTime);
            }

            using var doc = JsonDocument.Parse(responseBody);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var model = doc.RootElement.TryGetProperty("model", out var modelEl)
                ? modelEl.GetString()
                : _config.Model;

            var tokens = doc.RootElement.TryGetProperty("usage", out var usage)
                && usage.TryGetProperty("total_tokens", out var t)
                ? t.GetInt32()
                : (int?)null;

            return AiResponse.Success(
                content ?? string.Empty,
                model,
                tokens,
                DateTime.UtcNow - startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GigaChat API");
            return AiResponse.Failure(ex.Message, DateTime.UtcNow - startTime);
        }
    }

    /// <inheritdoc />
    public async Task<AiResponse<T>> AskStructuredAsync<T>(
        string prompt,
        AiOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var response = await AskAsync(prompt, options, cancellationToken);

            if (!response.IsSuccess || string.IsNullOrEmpty(response.Content))
                return AiResponse<T>.Failure(
                    response.Error ?? "Empty response",
                    response.Duration);

            // Очищаем ответ от markdown-обёрток
            var cleaned = CleanJsonResponse(response.Content);

            try
            {
                var data = JsonSerializer.Deserialize<T>(cleaned);

                if (data == null)
                    return AiResponse<T>.Failure(
                        "Failed to deserialize structured data",
                        response.Duration);

                return AiResponse<T>.Success(
                    data,
                    response.Content,
                    response.ModelUsed,
                    response.TokensUsed,
                    response.Duration);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON response: {Response}", response.Content);
                return AiResponse<T>.Failure(
                    $"JSON parse error: {ex.Message}",
                    response.Duration);
            }
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
        _logger.LogDebug("Checking token validity...");
        _logger.LogDebug("Token exists: {HasToken}", !string.IsNullOrEmpty(_accessToken));
        _logger.LogDebug("Token expiry: {Expiry}", _tokenExpiry);
        _logger.LogDebug("Current time: {Now}", DateTime.UtcNow);

        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
        {
            _logger.LogDebug("Using existing token, valid until {Expiry}", _tokenExpiry);
            return;
        }

        _logger.LogInformation("Acquiring new GigaChat token...");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v2/oauth");

            // Добавляем Basic авторизацию
            request.Headers.Add("Authorization", $"Basic {_config.AuthorizationKey}");
            request.Headers.Add("RqUID", Guid.NewGuid().ToString());

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("scope", _config.Scope)
            });

            _logger.LogDebug("Sending token request to {Url}", _tokenClient.BaseAddress + "api/v2/oauth");

            var response = await _tokenClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("Token response status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token endpoint failed: {StatusCode} - {Error}",
                    response.StatusCode, responseBody);
                throw new HttpRequestException(
                    $"Token acquisition failed: {response.StatusCode} - {responseBody}");
            }

            using var json = JsonDocument.Parse(responseBody);
            _accessToken = json.RootElement.GetProperty("access_token").GetString();

            // Токен живёт 30 минут, но для надёжности обновляем через 25
            _tokenExpiry = DateTime.UtcNow.AddMinutes(25);

            // Устанавливаем токен для API клиента
            _apiClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogInformation("GigaChat token acquired successfully, expires at {Expiry}", _tokenExpiry);
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
        var temperature = opt.Temperature > 0 ? opt.Temperature : _config.Temperature;
        var maxTokens = opt.MaxTokens > 0 ? opt.MaxTokens : _config.MaxTokens;

        return new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
            temperature = temperature,
            max_tokens = maxTokens,
            stream = false
        };
    }

    private static string CleanJsonResponse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        // Убираем markdown-обёртки ```json ... ```
        if (json.Contains("```"))
        {
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');
            if (start >= 0 && end >= 0 && end > start)
                json = json.Substring(start, end - start + 1);
        }

        // Находим начало массива [
        var arrayStart = json.IndexOf('[');
        var arrayEnd = json.LastIndexOf(']');

        if (arrayStart >= 0 && arrayEnd >= 0 && arrayEnd > arrayStart)
        {
            json = json.Substring(arrayStart, arrayEnd - arrayStart + 1);
        }

        // ИСПРАВЛЯЕМ: удаляем лишние { перед объектами в массиве
        json = FixJsonArrayErrors(json);

        return json;
    }

    private static string FixJsonArrayErrors(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        // Удаляем лишние { { (две открывающие скобки подряд)
        json = System.Text.RegularExpressions.Regex.Replace(json, @"\{\s*\{", "{");

        // Исправляем паттерн: , { { на , {
        json = System.Text.RegularExpressions.Regex.Replace(json, @",\s*\{\s*\{", ", {");

        // Исправляем паттерн: [ { { на [ {
        json = System.Text.RegularExpressions.Regex.Replace(json, @"\[\s*\{\s*\{", "[ {");

        return json;
    }
}