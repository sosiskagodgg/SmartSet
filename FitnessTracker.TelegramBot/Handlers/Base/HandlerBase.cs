using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Models;
using FitnessTracker.TelegramBot.Services;

namespace FitnessTracker.TelegramBot.Handlers.Base;

public abstract class HandlerBase
{
    protected readonly ITelegramBotAdapter _telegram;
    protected readonly ILogger _logger;
    protected readonly UserStateService _stateService;

    protected HandlerBase(
        ITelegramBotAdapter telegram,
        ILogger logger,
        UserStateService stateService)
    {
        _telegram = telegram;
        _logger = logger;
        _stateService = stateService;
    }

    protected Task<int> SendMessage(long userId, string text, Keyboard? keyboard = null, CancellationToken ct = default)
        => _telegram.SendMessageAsync(userId, text, keyboard, ct);

    protected Task<int> EditMessage(long userId, int messageId, string text, Keyboard? keyboard = null, CancellationToken ct = default)
        => _telegram.EditMessageAsync(userId, messageId, text, keyboard, ct);

    protected Task AnswerCallback(string callbackQueryId, string? text = null, bool showAlert = false, CancellationToken ct = default)
        => _telegram.AnswerCallbackAsync(callbackQueryId, text, showAlert, ct);

    protected Task DeleteMessage(long userId, int messageId, CancellationToken ct = default)
        => _telegram.DeleteMessageAsync(userId, messageId, ct);

    protected Task SetState(long userId, string? state, Dictionary<string, object>? data = null)
        => _stateService.SetStateAsync(userId, state, data);

    protected Task<UserState> GetState(long userId)
        => _stateService.GetStateAsync(userId);

    protected Task ClearState(long userId)
        => _stateService.ClearStateAsync(userId);
}