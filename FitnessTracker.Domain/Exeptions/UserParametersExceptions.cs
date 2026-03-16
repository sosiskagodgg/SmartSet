// FitnessTracker.Domain/Exceptions/UserParametersExceptions.cs
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.Exceptions;

/// <summary>
/// Исключение: параметры пользователя не найдены
/// </summary>
public sealed class UserParametersNotFoundException : DomainException
{
    public long TelegramId { get; }

    public UserParametersNotFoundException(long telegramId)
        : base($"Parameters for user {telegramId} not found", "USER_PARAMETERS_NOT_FOUND")
    {
        TelegramId = telegramId;
    }
}

/// <summary>
/// Исключение: неверное значение роста
/// </summary>
public sealed class InvalidHeightException : DomainException
{
    public int Height { get; }

    public InvalidHeightException(int height)
        : base($"Height must be between 100 and 250 cm. Received: {height}", "INVALID_HEIGHT")
    {
        Height = height;
    }
}

/// <summary>
/// Исключение: неверное значение веса
/// </summary>
public sealed class InvalidWeightException : DomainException
{
    public decimal Weight { get; }

    public InvalidWeightException(decimal weight)
        : base($"Weight must be between 20 and 300 kg. Received: {weight}", "INVALID_WEIGHT")
    {
        Weight = weight;
    }
}

/// <summary>
/// Исключение: неверный процент жира
/// </summary>
public sealed class InvalidBodyFatException : DomainException
{
    public decimal BodyFat { get; }

    public InvalidBodyFatException(decimal bodyFat)
        : base($"Body fat must be between 3% and 60%. Received: {bodyFat}", "INVALID_BODY_FAT")
    {
        BodyFat = bodyFat;
    }
}

/// <summary>
/// Исключение: неверный уровень опыта
/// </summary>
public sealed class InvalidExperienceLevelException : DomainException
{
    public string Experience { get; }

    public InvalidExperienceLevelException(string experience)
        : base($"Experience level must be 'beginner', 'intermediate', or 'advanced'. Received: '{experience}'",
               "INVALID_EXPERIENCE_LEVEL")
    {
        Experience = experience;
    }
}