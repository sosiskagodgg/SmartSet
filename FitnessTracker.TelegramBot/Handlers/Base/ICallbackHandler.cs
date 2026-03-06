using FitnessTracker.TelegramBot.Models;

namespace FitnessTracker.TelegramBot.Handlers.Base;

public interface ICallbackHandler
{
    string CallbackPrefix { get; }
    Task HandleAsync(long userId, CallbackInfo callback, int messageId, string callbackQueryId, CancellationToken ct);
}