using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Models;

namespace FitnessTracker.TelegramBot.Services;

public class UpdateRouter
{
    private readonly Dictionary<string, ICallbackHandler> _callbackHandlers;
    private readonly List<IMessageHandler> _messageHandlers;
    private readonly Dictionary<string, IStateHandler> _stateHandlers;
    private readonly UserStateService _stateService;

    public UpdateRouter(
        IEnumerable<ICallbackHandler> callbackHandlers,
        IEnumerable<IMessageHandler> messageHandlers,
        IEnumerable<IStateHandler> stateHandlers,
        UserStateService stateService)
    {
        _callbackHandlers = callbackHandlers.ToDictionary(h => h.CallbackPrefix);
        _messageHandlers = messageHandlers.ToList();
        _stateHandlers = stateHandlers.ToDictionary(h => h.StateType);
        _stateService = stateService;
    }

    public async Task RouteCallbackAsync(
        long userId,
        string callbackData,
        int messageId,
        string callbackQueryId,
        CancellationToken ct)
    {
        var parts = callbackData.Split(':');
        if (parts.Length < 3 || parts[0] != "ft")
            return;

        var callback = new CallbackInfo
        {
            Prefix = parts[1],
            Action = parts[2],
            Data = parts.Skip(3).ToArray(),
            FullData = callbackData
        };

        if (_callbackHandlers.TryGetValue(callback.Prefix, out var handler))
        {
            await handler.HandleAsync(userId, callback, messageId, callbackQueryId, ct);
        }
    }

    public async Task RouteMessageAsync(
        long userId,
        string text,
        int messageId,
        CancellationToken ct)
    {
        // Проверяем, ждет ли пользователь ввода
        var state = await _stateService.GetStateAsync(userId);

        if (!string.IsNullOrEmpty(state.CurrentState))
        {
            // Ищем обработчик состояния
            if (_stateHandlers.TryGetValue(state.CurrentState, out var stateHandler))
            {
                await stateHandler.HandleAsync(userId, text, messageId, state.Data, ct);
                return;
            }
        }

        // Ищем обработчик сообщения
        foreach (var handler in _messageHandlers)
        {
            if (handler.CanHandle(text))
            {
                await handler.HandleAsync(userId, text, messageId, ct);
                return;
            }
        }
    }
}