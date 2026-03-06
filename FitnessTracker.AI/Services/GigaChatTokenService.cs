using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FitnessTracker.AI.Configuration;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Services;

public class GigaChatTokenService : IGigaChatTokenService
{
    private readonly ILogger<GigaChatTokenService> _logger;
    private readonly GigaChatConfig _config;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public GigaChatTokenService(
        IOptions<GigaChatConfig> config,
        ILogger<GigaChatTokenService> logger)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Если токен еще валиден (30 минут), возвращаем его
        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
            return _accessToken;

        try
        {
            _logger.LogInformation("Getting new GigaChat token");

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.AuthorizationKey}");

            var content = new StringContent(
                $"scope={_config.Scope}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            var response = await client.PostAsync(
                "https://ngw.devices.sberbank.ru:9443/api/v2/oauth",
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                throw new Exception($"Failed to get GigaChat token: {response.StatusCode}");
            }

            var json = JsonDocument.Parse(responseBody);
            _accessToken = json.RootElement.GetProperty("access_token").GetString();

            // Токен живет 30 минут
            _tokenExpiry = DateTime.UtcNow.AddMinutes(30);

            _logger.LogInformation("GigaChat token obtained successfully");
            return _accessToken!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GigaChat token");
            throw;
        }
    }
}