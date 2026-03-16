// FitnessTracker.Domain/Exceptions/UserExceptions.cs
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.Exceptions;

/// <summary>
/// Исключение: пользователь не найден
/// </summary>
public sealed class UserNotFoundException : DomainException
{
    public long TelegramId { get; }

    public UserNotFoundException(long telegramId)
        : base($"User with TelegramId {telegramId} not found", "USER_NOT_FOUND")
    {
        TelegramId = telegramId;
    }
}

/// <summary>
/// Исключение: пользователь уже существует
/// </summary>
public sealed class UserAlreadyExistsException : DomainException
{
    public long TelegramId { get; }

    public UserAlreadyExistsException(long telegramId)
        : base($"User with TelegramId {telegramId} already exists", "USER_ALREADY_EXISTS")
    {
        TelegramId = telegramId;
    }
}

/// <summary>
/// Исключение: неверный статус подписки
/// </summary>
public sealed class InvalidSubscriptionStatusException : DomainException
{
    public string Status { get; }

    public InvalidSubscriptionStatusException(string status)
        : base($"Invalid subscription status: '{status}'. Allowed values: active, inactive, expired, cancelled",
               "INVALID_SUBSCRIPTION_STATUS")
    {
        Status = status;
    }
}

/// <summary>
/// Исключение: неверная дата подписки
/// </summary>
public sealed class InvalidSubscriptionDateException : DomainException
{
    public InvalidSubscriptionDateException(string message)
        : base(message, "INVALID_SUBSCRIPTION_DATE")
    {
    }
}