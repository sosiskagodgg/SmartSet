// FitnessTracker.TelegramBot/Abstractions/Models/CallbackContext.cs
namespace FitnessTracker.TelegramBot.Abstractions;

/// <summary>
/// Контекст входящего колбэка (нажатия на кнопку)
/// </summary>
public record CallbackContext
{
    /// <summary>
    /// ID пользователя в Telegram
    /// </summary>
    public required long UserId { get; init; }

    /// <summary>
    /// ID колбэка (нужен для AnswerCallback)
    /// </summary>
    public required string CallbackQueryId { get; init; }

    /// <summary>
    /// ID сообщения, к которому привязана кнопка
    /// </summary>
    public required int MessageId { get; init; }

    /// <summary>
    /// Действие (вторая часть колбэка)
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Параметры (третья и далее части колбэка)
    /// </summary>
    public required string[] Parameters { get; init; }

    /// <summary>
    /// Полная строка колбэка
    /// </summary>
    public required string RawData { get; init; }
}