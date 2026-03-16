// FitnessTracker.AI/Core/Models/AiMessageContext.cs
namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Контекст сообщения для AI плагинов (не зависит от Telegram)
/// </summary>
public record AiMessageContext
{
    /// <summary>
    /// ID пользователя (может быть Telegram ID или любой другой)
    /// </summary>
    public required long UserId { get; init; }

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Дополнительные данные
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}