// FitnessTracker.Domain/Exceptions/WorkoutExceptions.cs
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.Exceptions;

/// <summary>
/// Исключение: тренировка не найдена
/// </summary>
public sealed class WorkoutNotFoundException : DomainException
{
    public long TelegramId { get; }
    public DateTime Date { get; }

    public WorkoutNotFoundException(long telegramId, DateTime date)
        : base($"Workout for user {telegramId} on {date:yyyy-MM-dd} not found", "WORKOUT_NOT_FOUND")
    {
        TelegramId = telegramId;
        Date = date;
    }
}

/// <summary>
/// Исключение: тренировка уже существует
/// </summary>
public sealed class WorkoutAlreadyExistsException : DomainException
{
    public long TelegramId { get; }
    public DateTime Date { get; }

    public WorkoutAlreadyExistsException(long telegramId, DateTime date)
        : base($"Workout for user {telegramId} on {date:yyyy-MM-dd} already exists", "WORKOUT_ALREADY_EXISTS")
    {
        TelegramId = telegramId;
        Date = date;
    }
}

/// <summary>
/// Исключение: тренировка в прошлом
/// </summary>
public sealed class WorkoutInPastException : DomainException
{
    public DateTime Date { get; }

    public WorkoutInPastException(DateTime date)
        : base($"Cannot create workout in the past. Date: {date:yyyy-MM-dd}", "WORKOUT_IN_PAST")
    {
        Date = date;
    }
}