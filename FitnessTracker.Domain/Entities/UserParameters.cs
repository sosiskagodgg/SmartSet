// FitnessTracker.Domain/Entities/UserParameters.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Exceptions;

namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Параметры пользователя.
/// Является частью агрегата User (не корень, т.к. не может существовать без User).
/// </summary>
public class UserParameters : Entity<long> // TelegramId = long
{
    // Приватные сеттеры для EF Core
    public int? Height { get; private set; }
    public decimal? Weight { get; private set; }
    public decimal? BodyFat { get; private set; }
    public string? Experience { get; private set; }
    public string? Goals { get; private set; }

    // Для EF Core
    private UserParameters() : base() { }

    private UserParameters(long telegramId) : base(telegramId) { }

    /// <summary>
    /// Фабричный метод для создания параметров пользователя.
    /// Создает пустые параметры, которые потом заполняются через UpdatePhysicalMetrics.
    /// </summary>
    public static UserParameters Create(long telegramId)
    {
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId must be positive", nameof(telegramId));

        return new UserParameters(telegramId);
    }

    /// <summary>
    /// Обновление физических показателей с валидацией
    /// </summary>
    public void UpdatePhysicalMetrics(int? height = null, decimal? weight = null, decimal? bodyFat = null)
    {
        // Валидация роста
        if (height.HasValue && (height < 100 || height > 250))
            throw new InvalidHeightException(height.Value);

        // Валидация веса
        if (weight.HasValue && (weight < 20 || weight > 300))
            throw new InvalidWeightException(weight.Value);

        // Валидация процента жира
        if (bodyFat.HasValue && (bodyFat < 3 || bodyFat > 60))
            throw new InvalidBodyFatException(bodyFat.Value);

        // Обновляем только переданные значения
        Height = height ?? Height;
        Weight = weight ?? Weight;
        BodyFat = bodyFat ?? BodyFat;
    }

    /// <summary>
    /// Обновление уровня опыта
    /// </summary>
    public void UpdateExperience(string? experience)
    {
        if (experience != null)
        {
            var allowedLevels = new[] { "beginner", "intermediate", "advanced" };
            if (!allowedLevels.Contains(experience.ToLowerInvariant()))
                throw new InvalidExperienceLevelException(experience);
        }

        Experience = experience?.ToLowerInvariant();
    }

    /// <summary>
    /// Обновление целей
    /// </summary>
    public void UpdateGoals(string? goals)
    {
        Goals = goals;
    }

    /// <summary>
    /// Очистка всех параметров (сброс к значениям по умолчанию)
    /// </summary>
    public void Clear()
    {
        Height = null;
        Weight = null;
        BodyFat = null;
        Experience = null;
        Goals = null;
    }

    // Вычисляемые свойства (не хранятся в БД)
    public bool IsBeginner => Experience == "beginner";
    public bool IsIntermediate => Experience == "intermediate";
    public bool IsAdvanced => Experience == "advanced";
    public bool HasAnyData => Height.HasValue || Weight.HasValue || BodyFat.HasValue ||
                              !string.IsNullOrEmpty(Experience) || !string.IsNullOrEmpty(Goals);
}