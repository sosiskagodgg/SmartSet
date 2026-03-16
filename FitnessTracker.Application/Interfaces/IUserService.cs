// FitnessTracker.Application/Interfaces/IUserService.cs
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

/// <summary>
/// Сервис для работы с пользователями
/// </summary>
public interface IUserService : IApplicationService
{
    /// <summary>
    /// Получить пользователя по TelegramId
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Пользователь или null, если не найден</returns>
    Task<User?> GetUserByIdAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по username
    /// </summary>
    /// <param name="username">Username пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Пользователь или null, если не найден</returns>
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    /// <param name="limit">Максимальное количество записей</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список пользователей</returns>
    Task<IReadOnlyList<User>> GetAllUsersAsync(int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="name">Имя пользователя</param>
    /// <param name="username">Username (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданный пользователь</returns>
    Task<User> CreateUserAsync(long telegramId, string name, string? username = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить пользователя
    /// </summary>
    /// <param name="user">Пользователь с обновленными данными</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteUserAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если пользователь существует</returns>
    Task<bool> UserExistsAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Активировать подписку пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="endDate">Дата окончания подписки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task ActivateSubscriptionAsync(long telegramId, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Деактивировать подписку пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeactivateSubscriptionAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить активность подписки
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если подписка активна</returns>
    Task<bool> HasActiveSubscriptionAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить всех активных подписчиков
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список пользователей с активной подпиской</returns>
    Task<IReadOnlyList<User>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Зарегистрировать пользователя (создать если не существует)
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="name">Имя пользователя</param>
    /// <param name="username">Username (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Пользователь</returns>
    Task<User> RegisterIfNotExistsAsync(long telegramId, string name, string? username = null, CancellationToken cancellationToken = default);
}