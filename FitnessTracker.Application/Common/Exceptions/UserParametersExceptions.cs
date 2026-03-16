// FitnessTracker.Application/Common/Exceptions/UserParametersExceptions.cs
using FitnessTracker.Application.Common.Exceptions;

namespace FitnessTracker.Application.Common.Exceptions;

/// <summary>
/// Параметры пользователя не найдены
/// </summary>
public sealed class UserParametersNotFoundException : ApplicationException
{
    public long TelegramId { get; }

    public UserParametersNotFoundException(long telegramId)
        : base($"Parameters for user {telegramId} not found", "USER_PARAMETERS_NOT_FOUND")
    {
        TelegramId = telegramId;
    }
}