// FitnessTracker.Application/Common/Exceptions/UserExceptions.cs
using FitnessTracker.Application.Common.Exceptions;

namespace FitnessTracker.Application.Common.Exceptions;

/// <summary>
/// Пользователь не найден
/// </summary>
public sealed class UserNotFoundException : ApplicationException
{
    public long TelegramId { get; }

    public UserNotFoundException(long telegramId)
        : base($"User with TelegramId {telegramId} not found", "USER_NOT_FOUND")
    {
        TelegramId = telegramId;
    }
}

/// <summary>
/// Пользователь уже существует
/// </summary>
public sealed class UserAlreadyExistsException : ApplicationException
{
    public long TelegramId { get; }

    public UserAlreadyExistsException(long telegramId)
        : base($"User with TelegramId {telegramId} already exists", "USER_ALREADY_EXISTS")
    {
        TelegramId = telegramId;
    }
}

/// <summary>
/// Неверные данные пользователя
/// </summary>
public sealed class InvalidUserDataException : ApplicationException
{
    public InvalidUserDataException(string message)
        : base(message, "INVALID_USER_DATA")
    {
    }
}