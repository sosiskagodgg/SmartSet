using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FitnessTracker.TelegramBot.Infrastructure;

/// <summary>
/// Адаптер для Telegram API (единственное место, где используется Telegram.Bot)
/// </summary>
public class TelegramBotAdapter : ITelegramBotAdapter
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotAdapter> _logger;

    public TelegramBotAdapter(
        ITelegramBotClient botClient,
        ILogger<TelegramBotAdapter> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<int> SendMessageAsync(
        long userId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Sending message to user {UserId}", userId);

            var message = await _botClient.SendMessage(
                chatId: userId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: ToTelegramKeyboard(keyboard),
                cancellationToken: ct
            );

            return message.MessageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> EditMessageAsync(
        long userId,
        int messageId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Editing message {MessageId} for user {UserId}", messageId, userId);

            var message = await _botClient.EditMessageText(
                chatId: userId,
                messageId: messageId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: ToTelegramKeyboard(keyboard),
                cancellationToken: ct
            );

            return message.MessageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to edit message {MessageId} for user {UserId}", messageId, userId);
            throw;
        }
    }

    public async Task AnswerCallbackAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Answering callback {CallbackId}", callbackQueryId);

            await _botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQueryId,
                text: text,
                showAlert: showAlert,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to answer callback {CallbackId}", callbackQueryId);
            throw;
        }
    }

    public async Task DeleteMessageAsync(
        long userId,
        int messageId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Deleting message {MessageId} for user {UserId}", messageId, userId);

            await _botClient.DeleteMessage(
                chatId: userId,
                messageId: messageId,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete message {MessageId} for user {UserId}", messageId, userId);
            throw;
        }
    }

    /// <summary>
    /// Конвертирует нашу Keyboard в Telegram InlineKeyboardMarkup
    /// </summary>
    private InlineKeyboardMarkup? ToTelegramKeyboard(Keyboard? keyboard)
    {
        if (keyboard == null || keyboard.Buttons.Count == 0)
            return null;

        var buttons = keyboard.Buttons.Select(row =>
            row.Select(btn => InlineKeyboardButton.WithCallbackData(btn.Text, btn.CallbackData)).ToArray()
        ).ToArray();

        return new InlineKeyboardMarkup(buttons);
    }
}