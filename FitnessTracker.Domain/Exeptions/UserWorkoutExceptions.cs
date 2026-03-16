// FitnessTracker.Domain/Exceptions/UserWorkoutExceptions.cs
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.Exceptions;

/// <summary>
/// Исключение: тренировка пользователя не найдена
/// </summary>
public sealed class UserWorkoutNotFoundException : DomainException
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
/// Исключение: тренировка пользователя уже существует
/// </summary>
public sealed class UserWorkoutAlreadyExistsException : DomainException
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

/// <summary>
/// Исключение: неверный номер дня
/// </summary>
public sealed class InvalidDayNumberException : DomainException
{
    public int DayNumber { get; }

    public InvalidDayNumberException(int dayNumber)
        : base($"Day number must be positive. Received: {dayNumber}", "INVALID_DAY_NUMBER")
    {
        DayNumber = dayNumber;
    }
}