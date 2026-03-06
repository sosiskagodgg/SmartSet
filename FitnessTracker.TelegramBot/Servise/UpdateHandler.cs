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

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        try
        {
            if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
            {
                await _router.RouteMessageAsync(
                    userId: update.Message.Chat.Id,
                    text: update.Message.Text,
                    messageId: update.Message.MessageId,
                    ct: ct);
            }
            else if (update.CallbackQuery != null)
            {
                await _router.RouteCallbackAsync(
                    userId: update.CallbackQuery.From.Id,
                    callbackData: update.CallbackQuery.Data!,
                    messageId: update.CallbackQuery.Message!.MessageId,
                    callbackQueryId: update.CallbackQuery.Id,
                    ct: ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        _logger.LogError(exception, "Error in bot");
        return Task.CompletedTask;
    }
}