// FitnessTracker.AI/Core/Models/MessageContext.cs
namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Контекст входящего сообщения
/// </summary>
public record MessageContext
{
    /// <summary>
    /// Telegram ID пользователя
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Дополнительные данные сессии
    /// </summary>
    public Dictionary<string, object> SessionData { get; init; } = new();

    /// <summary>
    /// Время получения сообщения
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}