using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Services;

namespace FitnessTracker.TelegramBot.Handlers;

public class WaitingNameHandler : HandlerBase, IStateHandler
{
    public string StateType => "waiting_name";

    private static class Text
    {
        public const string Greeting = "Привет, {0}!";
    }

    public WaitingNameHandler(
        ITelegramBotAdapter telegram,
        ILogger<WaitingNameHandler> logger,
        UserStateService stateService)
        : base(telegram, logger, stateService)
    {
    }

    public async Task HandleAsync(
        long userId,
        string messageText,
        int messageId,
        Dictionary<string, object> stateData,
        CancellationToken ct)
    {
        await SendMessage(userId, string.Format(Text.Greeting, messageText), ct: ct);
        await ClearState(userId);
    }
}