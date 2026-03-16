// FitnessTracker.TelegramBot/Abstractions/Models/MessageContext.cs
namespace FitnessTracker.TelegramBot.Abstractions;

/// <summary>
/// Контекст входящего сообщения
/// </summary>
public record MessageContext
{
    /// <summary>
    /// ID пользователя в Telegram
    /// </summary>
    public required long UserId { get; init; }

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// ID сообщения в Telegram
    /// </summary>
    public required int MessageId { get; init; }

    /// <summary>
    /// Состояние пользователя (если есть)
    /// </summary>
    public UserState? State { get; init; }
}