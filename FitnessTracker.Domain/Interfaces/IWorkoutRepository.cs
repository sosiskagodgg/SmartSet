// FitnessTracker.Domain/Interfaces/IWorkoutRepository.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

/// <summary>
/// Репозиторий для работы с ежедневными тренировками (выполненными).
/// Отвечает за хранение и извлечение выполненных тренировок по датам.
/// </summary>
public interface IWorkoutRepository
{
    /// <summary>
    /// Получить тренировку по составному ключу (TelegramId + Date)
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Тренировка или null, если не найдена</returns>
    Task<Workout?> GetByIdAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все тренировки пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список всех тренировок пользователя, отсортированных по дате (сначала новые)</returns>
    Task<IReadOnlyList<Workout>> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить тренировки пользователя за указанный период
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="startDate">Начальная дата (включительно)</param>
    /// <param name="endDate">Конечная дата (включительно)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список тренировок за период, отсортированных по дате</returns>
    Task<IReadOnlyList<Workout>> GetByDateRangeAsync(
        long telegramId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование тренировки в конкретную дату
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если тренировка существует, иначе false</returns>
    Task<bool> ExistsAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую тренировку
    /// </summary>
    /// <param name="workout">Тренировка для добавления</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(Workout workout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить существующую тренировку
    /// </summary>
    /// <param name="workout">Тренировка с обновленными данными</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default);
}