// FitnessTracker.TelegramBot/Infrastructure/TelegramBotAdapter.cs
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using TelegramChatAction = Telegram.Bot.Types.Enums.ChatAction; // Алиас для Telegram

namespace FitnessTracker.TelegramBot.Infrastructure;

/// <summary>
/// Адаптер для Telegram API (единственное место с Telegram.Bot)
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
        CancellationToken cancellationToken = default)
    {
        var message = await _botClient.SendMessage(
            chatId: userId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: ToTelegramKeyboard(keyboard),
            cancellationToken: cancellationToken
        );

        return message.MessageId;
    }

    public async Task<int> EditMessageAsync(
        long userId,
        int messageId,
        string text,
        Keyboard? keyboard = null,
        CancellationToken cancellationToken = default)
    {
        var message = await _botClient.EditMessageText(
            chatId: userId,
            messageId: messageId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: ToTelegramKeyboard(keyboard),
            cancellationToken: cancellationToken
        );

        return message.MessageId;
    }

    public async Task AnswerCallbackAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        CancellationToken cancellationToken = default)
    {
        await _botClient.AnswerCallbackQuery(
            callbackQueryId: callbackQueryId,
            text: text,
            showAlert: showAlert,
            cancellationToken: cancellationToken
        );
    }

    public async Task DeleteMessageAsync(
        long userId,
        int messageId,
        CancellationToken cancellationToken = default)
    {
        await _botClient.DeleteMessage(
            chatId: userId,
            messageId: messageId,
            cancellationToken: cancellationToken
        );
    }

    public async Task SendChatActionAsync(
        long userId,
        BotChatAction action,
        CancellationToken cancellationToken = default)
    {
        var telegramAction = action switch
        {
            BotChatAction.Typing => TelegramChatAction.Typing,
            BotChatAction.UploadPhoto => TelegramChatAction.UploadPhoto,
            BotChatAction.UploadVideo => TelegramChatAction.UploadVideo,
            BotChatAction.UploadDocument => TelegramChatAction.UploadDocument,
            _ => TelegramChatAction.Typing
        };

        await _botClient.SendChatAction(
            userId,
            telegramAction,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Конвертирует нашу модель Keyboard в Telegram InlineKeyboardMarkup
    /// </summary>
    private InlineKeyboardMarkup? ToTelegramKeyboard(Keyboard? keyboard)
    {
        if (keyboard == null || keyboard.Buttons.Count == 0)
            return null;

        var rows = new List<InlineKeyboardButton[]>();

        foreach (var row in keyboard.Buttons)
        {
            var buttons = new List<InlineKeyboardButton>();
            foreach (var btn in row)
            {
                if (string.IsNullOrEmpty(btn.Text))
                    continue;

                buttons.Add(InlineKeyboardButton.WithCallbackData(btn.Text, btn.CallbackData));
            }

            if (buttons.Any())
                rows.Add(buttons.ToArray());
        }

        return rows.Any() ? new InlineKeyboardMarkup(rows) : null;
    }
}