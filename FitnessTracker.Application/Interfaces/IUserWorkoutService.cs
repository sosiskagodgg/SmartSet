// FitnessTracker.Application/Interfaces/IUserWorkoutService.cs
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;

namespace FitnessTracker.Application.Interfaces;

/// <summary>
/// Сервис для работы с тренировками пользователя (шаблонами)
/// </summary>
public interface IUserWorkoutService : IApplicationService
{
    /// <summary>
    /// Получить тренировку по дню
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Тренировка пользователя или null, если не найдена</returns>
    Task<UserWorkout?> GetUserWorkoutAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все тренировки пользователя
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список тренировок пользователя</returns>
    Task<IReadOnlyList<UserWorkout>> GetAllUserWorkoutsAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать новую тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="name">Название тренировки</param>
    /// <param name="exercises">Список упражнений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданная тренировка</returns>
    Task<UserWorkout> CreateUserWorkoutAsync(
        long telegramId,
        int dayNumber,
        string name,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить существующую тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="name">Новое название тренировки</param>
    /// <param name="exercises">Новый список упражнений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленная тренировка</returns>
    Task<UserWorkout> UpdateUserWorkoutAsync(
        long telegramId,
        int dayNumber,
        string name,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать или обновить тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="name">Название тренировки</param>
    /// <param name="exercises">Список упражнений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Тренировка</returns>
    Task<UserWorkout> CreateOrUpdateUserWorkoutAsync(
        long telegramId,
        int dayNumber,
        string name,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить тренировку
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteUserWorkoutAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование тренировки
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя</param>
    /// <param name="dayNumber">Номер дня тренировки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>true если тренировка существует</returns>
    Task<bool> UserWorkoutExistsAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default);
}