// FitnessTracker.Application/Interfaces/IUserParametersService.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IUserParametersService : IService
{
    /// <summary>
    /// Получить параметры пользователя по TelegramId
    /// </summary>
    Task<UserParameters?> GetUserParametersAsync(long telegramId, CancellationToken ct = default);

    /// <summary>
    /// Создать или обновить параметры пользователя
    /// </summary>
    Task<UserParameters> CreateOrUpdateUserParametersAsync(
        long telegramId,
        int? height = null,
        decimal? weight = null,
        decimal? bodyFat = null,
        string? experience = null,
        string? goals = null,
        CancellationToken ct = default);

    /// <summary>
    /// Обновить конкретное поле параметров
    /// </summary>
    Task UpdateHeightAsync(long telegramId, int height, CancellationToken ct = default);
    Task UpdateWeightAsync(long telegramId, decimal weight, CancellationToken ct = default);
    Task UpdateBodyFatAsync(long telegramId, decimal bodyFat, CancellationToken ct = default);
    Task UpdateExperienceAsync(long telegramId, string experience, CancellationToken ct = default);
    Task UpdateGoalsAsync(long telegramId, string goals, CancellationToken ct = default);

    /// <summary>
    /// Проверить, есть ли параметры у пользователя
    /// </summary>
    Task<bool> UserParametersExistsAsync(long telegramId, CancellationToken ct = default);

    /// <summary>
    /// Удалить параметры пользователя
    /// </summary>
    Task DeleteUserParametersAsync(long telegramId, CancellationToken ct = default);
}