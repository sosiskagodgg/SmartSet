// FitnessTracker.Domain/Interfaces/IUserWorkoutRepository.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

/// <summary>
/// Репозиторий для работы с тренировками пользователя (шаблонами).
/// Отвечает за хранение и извлечение запланированных тренировок по дням.
/// </summary>
public interface IUserWorkoutRepository
{
    /// <summary>
    /// Получить тренировку пользователя по составному ключу (TelegramId + DayNumber)
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Тренировка пользователя или null, если не найдена</returns>
    Task<UserWorkout?> GetByIdAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все тренировки пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список всех тренировок пользователя, отсортированных по дням</returns>
    Task<IReadOnlyList<UserWorkout>> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование тренировки у пользователя в конкретный день
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если тренировка существует, иначе false</returns>
    Task<bool> ExistsAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую тренировку пользователя
    /// </summary>
    /// <param name="workout">Тренировка для добавления</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(UserWorkout workout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить существующую тренировку пользователя
    /// </summary>
    /// <param name="workout">Тренировка с обновленными данными</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateAsync(UserWorkout workout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить тренировку пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default);
}