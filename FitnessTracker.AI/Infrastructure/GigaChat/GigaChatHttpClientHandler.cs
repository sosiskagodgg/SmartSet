// FitnessTracker.AI/Infrastructure/GigaChat/GigaChatHttpClientHandler.cs
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.AI.Infrastructure.GigaChat;

/// <summary>
/// Обработчик HTTP запросов с отключенной проверкой SSL сертификатов
/// </summary>
public class GigaChatHttpClientHandler : HttpClientHandler
{
    private readonly ILogger<GigaChatHttpClientHandler>? _logger;

    // Конструктор без логгера (для простоты)
    public GigaChatHttpClientHandler()
    {
        ConfigureHandler();
    }

    // Конструктор с логгером (для диагностики)
    public GigaChatHttpClientHandler(ILogger<GigaChatHttpClientHandler> logger)
    {
        _logger = logger;
        ConfigureHandler();
    }

    private void ConfigureHandler()
    {
        // Разрешаем все версии TLS
        SslProtocols = System.Security.Authentication.SslProtocols.Tls |
                       System.Security.Authentication.SslProtocols.Tls11 |
                       System.Security.Authentication.SslProtocols.Tls12 |
                       System.Security.Authentication.SslProtocols.Tls13;

        // Увеличиваем лимиты
        MaxConnectionsPerServer = 20;
        AllowAutoRedirect = true;
        UseCookies = true;
        CookieContainer = new System.Net.CookieContainer();

        // Полностью отключаем проверку SSL с диагностикой
        ServerCertificateCustomValidationCallback = ValidateCertificate;
    }

    private bool ValidateCertificate(
        HttpRequestMessage sender,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // Логируем информацию о сертификате если есть логгер
        _logger?.LogInformation("=== GIGACHAT SSL DEBUG ===");
        _logger?.LogInformation("SSL Policy Errors: {Errors}", sslPolicyErrors);

        if (certificate != null)
        {
            _logger?.LogInformation("Certificate Subject: {Subject}", certificate.Subject);
            _logger?.LogInformation("Certificate Issuer: {Issuer}", certificate.Issuer);
            _logger?.LogInformation("Certificate Not Before: {NotBefore}", certificate.NotBefore);
            _logger?.LogInformation("Certificate Not After: {NotAfter}", certificate.NotAfter);
        }
        else
        {
            _logger?.LogInformation("Certificate is NULL");
        }

        if (chain != null)
        {
            foreach (var status in chain.ChainStatus)
            {
                _logger?.LogInformation("Chain Status: {Status}", status.Status);
            }
        }

        // ВСЕГДА возвращаем true - игнорируем все ошибки
        return true;
    }
}