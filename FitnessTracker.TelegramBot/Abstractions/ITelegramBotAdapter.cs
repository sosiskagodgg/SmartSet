using FitnessTracker.TelegramBot.Models;

namespace FitnessTracker.TelegramBot.Abstractions;

public interface ITelegramBotAdapter
{
    /// <summary>
    /// Отправить сообщение
    /// </summary>
    Task<int> SendMessageAsync(
        long userId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken ct = default);

    /// <summary>
    /// Отредактировать сообщение
    /// </summary>
    Task<int> EditMessageAsync(
        long userId,
        int messageId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken ct = default);

    /// <summary>
    /// Ответить на callback
    /// </summary>
    Task AnswerCallbackAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        CancellationToken ct = default);

    /// <summary>
    /// Удалить сообщение
    /// </summary>
    Task DeleteMessageAsync(
        long userId,
        int messageId,
        CancellationToken ct = default);
}