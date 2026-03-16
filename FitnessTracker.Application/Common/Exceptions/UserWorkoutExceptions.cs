// FitnessTracker.Application/Common/Exceptions/UserWorkoutExceptions.cs
using FitnessTracker.Application.Common.Exceptions;

namespace FitnessTracker.Application.Common.Exceptions;

/// <summary>
/// Тренировка пользователя не найдена
/// </summary>
public sealed class UserWorkoutNotFoundException : ApplicationException
{
    public long TelegramId { get; }
    public int DayNumber { get; }

    public UserWorkoutNotFoundException(long telegramId, int dayNumber)
        : base($"Workout for user {telegramId} on day {dayNumber} not found", "USER_WORKOUT_NOT_FOUND")
    {
        TelegramId = telegramId;
        DayNumber = dayNumber;
    }
}

/// <summary>
/// Тренировка пользователя уже существует
/// </summary>
public sealed class UserWorkoutAlreadyExistsException : ApplicationException
{
    public long TelegramId { get; }
    public int DayNumber { get; }

    public UserWorkoutAlreadyExistsException(long telegramId, int dayNumber)
        : base($"Workout for user {telegramId} on day {dayNumber} already exists", "USER_WORKOUT_ALREADY_EXISTS")
    {
        TelegramId = telegramId;
        DayNumber = dayNumber;
    }
}