// FitnessTracker.Application/Interfaces/IUserParametersService.cs
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

/// <summary>
/// Сервис для работы с параметрами пользователя
/// </summary>
public interface IUserParametersService : IApplicationService
{
    /// <summary>
    /// Получить параметры пользователя по TelegramId
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Параметры пользователя или null, если не найдены</returns>
    Task<UserParameters?> GetUserParametersAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать или обновить параметры пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="height">Рост в см (опционально)</param>
    /// <param name="weight">Вес в кг (опционально)</param>
    /// <param name="bodyFat">Процент жира (опционально)</param>
    /// <param name="experience">Уровень опыта (опционально)</param>
    /// <param name="goals">Цели (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленные параметры пользователя</returns>
    Task<UserParameters> CreateOrUpdateUserParametersAsync(
        long telegramId,
        int? height = null,
        decimal? weight = null,
        decimal? bodyFat = null,
        string? experience = null,
        string? goals = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить рост пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="height">Рост в см</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateHeightAsync(long telegramId, int height, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить вес пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="weight">Вес в кг</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateWeightAsync(long telegramId, decimal weight, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить процент жира
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="bodyFat">Процент жира</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateBodyFatAsync(long telegramId, decimal bodyFat, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить уровень опыта
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="experience">Уровень опыта (beginner/intermediate/advanced)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateExperienceAsync(long telegramId, string experience, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить цели пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="goals">Цели в свободном формате</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateGoalsAsync(long telegramId, string goals, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить параметры пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteUserParametersAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить наличие параметров у пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если параметры существуют</returns>
    Task<bool> UserParametersExistsAsync(long telegramId, CancellationToken cancellationToken = default);
}