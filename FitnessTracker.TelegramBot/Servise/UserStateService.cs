using FitnessTracker.TelegramBot.Models;

namespace FitnessTracker.TelegramBot.Services;

/// <summary>
/// Сервис для хранения состояний пользователей
/// </summary>
public class UserStateService
{
    private readonly Dictionary<long, UserState> _states = new();
    private readonly ILogger<UserStateService> _logger;

    public UserStateService(ILogger<UserStateService> logger)
    {
        _logger = logger;
    }

    public Task<UserState> GetStateAsync(long userId)
    {
        if (!_states.TryGetValue(userId, out var state))
        {
            state = new UserState();
            _states[userId] = state;
            _logger.LogDebug("Created new state for user {UserId}", userId);
        }

        return Task.FromResult(state);
    }

    public Task SaveStateAsync(long userId, UserState state)
    {
        _states[userId] = state;
        _logger.LogDebug("Saved state for user {UserId}: {State}", userId, state.CurrentState);
        return Task.CompletedTask;
    }

    public Task ClearStateAsync(long userId)
    {
        if (_states.Remove(userId))
        {
            _logger.LogDebug("Cleared state for user {UserId}", userId);
        }
        return Task.CompletedTask;
    }

    public Task SetStateAsync(long userId, string? stateType, Dictionary<string, object>? data = null)
    {
        var state = new UserState
        {
            CurrentState = stateType,
            Data = data ?? new()
        };

        _states[userId] = state;
        _logger.LogDebug("Set state for user {UserId} to {StateType}", userId, stateType);

        return Task.CompletedTask;
    }
}