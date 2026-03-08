using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using FitnessTracker.TelegramBot.Abstractions;

namespace FitnessTracker.TelegramBot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly UpdateRouter _router;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        UpdateRouter router,
        ILogger<UpdateHandler> logger)
    {
        _router = router;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
            {
                await _router.RouteMessageAsync(
                    userId: update.Message.Chat.Id,
                    text: update.Message.Text,
                    messageId: update.Message.MessageId,
                    ct: cancellationToken);
            }
            else if (update.CallbackQuery != null)
            {
                await _router.RouteCallbackAsync(
                    userId: update.CallbackQuery.From.Id,
                    callbackData: update.CallbackQuery.Data!,
                    messageId: update.CallbackQuery.Message!.MessageId,
                    callbackQueryId: update.CallbackQuery.Id,
                    ct: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error in bot");
        return Task.CompletedTask;
    }
}