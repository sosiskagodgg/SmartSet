// FitnessTracker.Domain/Interfaces/IUserRepository.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

/// <summary>
/// Репозиторий для работы с пользователями
/// </summary>
public interface IUserRepository : IBaseRepository<User, long>
{
    /// <summary>
    /// Получить пользователя по TelegramId
    /// </summary>
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить всех активных подписчиков
    /// </summary>
    Task<IReadOnlyList<User>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование пользователя по TelegramId
    /// </summary>
    Task<bool> ExistsByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}