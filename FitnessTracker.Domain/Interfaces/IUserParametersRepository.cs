// FitnessTracker.Domain/Interfaces/IUserParametersRepository.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

/// <summary>
/// Репозиторий для работы с параметрами пользователя
/// </summary>
public interface IUserParametersRepository : IBaseRepository<UserParameters, long>
{
    /// <summary>
    /// Получить параметры по TelegramId
    /// </summary>
    Task<UserParameters?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}