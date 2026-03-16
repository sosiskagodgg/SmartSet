// FitnessTracker.Application/Common/Exceptions/WorkoutExceptions.cs
using FitnessTracker.Application.Common.Exceptions;

namespace FitnessTracker.Application.Common.Exceptions;

/// <summary>
/// Тренировка не найдена
/// </summary>
public sealed class WorkoutNotFoundException : ApplicationException
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
/// Тренировка уже существует
/// </summary>
public sealed class WorkoutAlreadyExistsException : ApplicationException
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