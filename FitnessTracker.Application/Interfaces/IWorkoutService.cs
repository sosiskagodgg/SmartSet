// FitnessTracker.Application/Interfaces/IWorkoutService.cs
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;

namespace FitnessTracker.Application.Interfaces;

/// <summary>
/// Сервис для работы с ежедневными тренировками.
/// Предоставляет методы для создания, получения, обновления и удаления тренировок.
/// </summary>
public interface IWorkoutService : IApplicationService
{
    /// <summary>
    /// Получить тренировку по дате
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Тренировка или null, если не найдена</returns>
    Task<Workout?> GetWorkoutByDateAsync(
        long telegramId,
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сегодняшнюю тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Тренировка или null, если нет тренировки на сегодня</returns>
    Task<Workout?> GetTodayWorkoutAsync(
        long telegramId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все тренировки пользователя с ограничением по количеству
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="limit">Максимальное количество записей (по умолчанию 50)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список тренировок</returns>
    Task<IReadOnlyList<Workout>> GetUserWorkoutsAsync(
        long telegramId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить тренировки за указанный период
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="startDate">Начальная дата (включительно)</param>
    /// <param name="endDate">Конечная дата (включительно)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список тренировок за период</returns>
    Task<IReadOnlyList<Workout>> GetWorkoutsByDateRangeAsync(
        long telegramId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать новую тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="exercises">Список упражнений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданная тренировка</returns>
    /// <exception cref="UserNotFoundException">Пользователь не найден</exception>
    /// <exception cref="WorkoutAlreadyExistsException">Тренировка на эту дату уже существует</exception>
    Task<Workout> CreateWorkoutAsync(
        long telegramId,
        DateTime date,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить существующую тренировку (полная замена упражнений)
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="exercises">Новый список упражнений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленная тренировка</returns>
    /// <exception cref="WorkoutNotFoundException">Тренировка не найдена</exception>
    Task<Workout> UpdateWorkoutAsync(
        long telegramId,
        DateTime date,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="date">Дата тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="WorkoutNotFoundException">Тренировка не найдена</exception>
    Task DeleteWorkoutAsync(
        long telegramId,
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить наличие тренировки на сегодня
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если тренировка на сегодня существует</returns>
    Task<bool> HasWorkoutTodayAsync(
        long telegramId,
        CancellationToken cancellationToken = default);
}