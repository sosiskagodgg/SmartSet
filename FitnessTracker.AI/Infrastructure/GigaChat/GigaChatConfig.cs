// FitnessTracker.AI/Infrastructure/GigaChat/GigaChatConfig.cs
namespace FitnessTracker.AI.Infrastructure.GigaChat;

/// <summary>
/// Конфигурация для подключения к GigaChat.
/// Загружается из appsettings.json.
/// </summary>
public class GigaChatConfig
{
    /// <summary>
    /// Ключ авторизации (получается в кабинете разработчика Сбера).
    /// </summary>
    public string AuthorizationKey { get; set; } = string.Empty;

    /// <summary>
    /// Область доступа (по умолчанию GIGACHAT_API_PERS).
    /// </summary>
    public string Scope { get; set; } = "GIGACHAT_API_PERS";

    /// <summary>
    /// Название модели (например, "GigaChat:latest").
    /// </summary>
    public string Model { get; set; } = "GigaChat:latest";

    /// <summary>
    /// Системный промпт по умолчанию.
    /// </summary>
    public string DefaultSystemPrompt { get; set; } = "Ты — профессиональный фитнес-ассистент. Отвечай кратко и по делу.";

    /// <summary>
    /// Температура по умолчанию.
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// Максимальное количество токенов по умолчанию.
    /// </summary>
    public int MaxTokens { get; set; } = 1000;
}