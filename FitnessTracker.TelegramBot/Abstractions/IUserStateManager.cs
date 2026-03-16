// FitnessTracker.TelegramBot/Abstractions/IUserStateManager.cs
namespace FitnessTracker.TelegramBot.Abstractions;

/// <summary>
/// Интерфейс для управления состояниями пользователей
/// </summary>
public interface IUserStateManager
{
    /// <summary>
    /// Установить состояние для пользователя
    /// </summary>
    void SetState(long userId, UserState state);

    /// <summary>
    /// Получить состояние пользователя
    /// </summary>
    UserState? GetState(long userId);

    /// <summary>
    /// Очистить состояние пользователя
    /// </summary>
    void ClearState(long userId);
}