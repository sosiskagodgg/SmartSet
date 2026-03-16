// FitnessTracker.TelegramBot/Abstractions/ITelegramBotAdapter.cs
using FitnessTracker.TelegramBot.Models;

namespace FitnessTracker.TelegramBot.Abstractions;

/// <summary>
/// Типы действий в чате (для SendChatActionAsync)
/// </summary>
public enum BotChatAction  // Переименовали, чтобы не конфликтовать с Telegram.Bot
{
    Typing,
    UploadPhoto,
    UploadVideo,
    UploadDocument
}

/// <summary>
/// Адаптер для Telegram API (абстракция над конкретной библиотекой)
/// </summary>
public interface ITelegramBotAdapter
{
    /// <summary>
    /// Отправить сообщение
    /// </summary>
    Task<int> SendMessageAsync(
        long userId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отредактировать существующее сообщение
    /// </summary>
    Task<int> EditMessageAsync(
        long userId,
        int messageId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ответить на колбэк (убрать "часики" на кнопке)
    /// </summary>
    Task AnswerCallbackAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить сообщение
    /// </summary>
    Task DeleteMessageAsync(
        long userId,
        int messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Показать статус "печатает..."
    /// </summary>
    Task SendChatActionAsync(
        long userId,
        BotChatAction action,  // Используем BotChatAction
        CancellationToken cancellationToken = default);
}