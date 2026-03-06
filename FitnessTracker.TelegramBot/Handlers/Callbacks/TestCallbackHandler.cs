using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;

namespace FitnessTracker.TelegramBot.Handlers;

public class TestHandler : HandlerBase, ICallbackHandler
{
    public string CallbackPrefix => "test";

    // Константы прямо в классе
    private static class Callback
    {
        public const string Start = "start";
        public const string StartFull = "ft:test:start";
        public const string Info = "info";
        public const string InfoFull = "ft:test:info";
        public const string StartState = "start_state";
        public const string StartStateFull = "ft:test:start_state";
    }

    private static class Text
    {
        public const string Welcome = "👋 Добро пожаловать!";
        public const string EnterName = "Введи имя:";
    }

    public TestHandler(
        ITelegramBotAdapter telegram,
        ILogger<TestHandler> logger,
        UserStateService stateService)
        : base(telegram, logger, stateService)
    {
    }

    public async Task HandleAsync(
        long userId,
        CallbackInfo callback,
        int messageId,
        string callbackQueryId,
        CancellationToken ct)
    {
        switch (callback.Action)
        {
            case Callback.Start:
                await ShowMenu(userId, messageId, ct);
                break;
            case Callback.Info:
                await SendMessage(userId, "Информация", ct: ct);
                break;
            case Callback.StartState:
                await StartState(userId, messageId, ct);
                break;
        }

        await AnswerCallback(callbackQueryId, ct: ct);
    }

    private async Task ShowMenu(long userId, int messageId, CancellationToken ct)
    {
        var keyboard = Keyboard.FromRows(
            new List<Button>
            {
                Button.Create("ℹ️ Инфо", Callback.InfoFull),
                Button.Create("📝 Анкета", Callback.StartStateFull)
            }
        );

        await EditMessage(userId, messageId, Text.Welcome, keyboard, ct);
    }

    private async Task StartState(long userId, int messageId, CancellationToken ct)
    {
        await SetState(userId, "waiting_name");
        await EditMessage(userId, messageId, Text.EnterName, ct: ct);
    }
}