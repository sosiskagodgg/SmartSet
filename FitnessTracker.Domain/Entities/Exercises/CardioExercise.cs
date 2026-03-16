// FitnessTracker.Domain/Entities/Exercises/CardioExercise.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Enums;

namespace FitnessTracker.Domain.Entities.Exercises;

/// <summary>
/// Кардио упражнение (бег, велосипед, эллипс и т.д.)
/// </summary>
public class CardioExercise : Exercise
{
    /// <summary>
    /// Длительность в минутах
    /// </summary>
    public int DurationMinutes { get; private set; }

    /// <summary>
    /// Дистанция в км (опционально)
    /// </summary>
    public float? DistanceKm { get; private set; }

    /// <summary>
    /// Средний пульс (опционально)
    /// </summary>
    public int? AvgHeartRate { get; private set; }

    /// <summary>
    /// Интенсивность
    /// </summary>
    public CardioIntensity Intensity { get; private set; }

    /// <summary>
    /// Тип кардио (LISS/HIIT)
    /// </summary>
    public CardioType CardioType { get; private set; }

    /// <summary>
    /// Дискриминатор типа (для JSON)
    /// </summary>
    public override ExerciseType Type => ExerciseType.Cardio;

    // Для EF Core
    protected CardioExercise() : base() { }

    // FitnessTracker.Domain/Entities/Exercises/CardioExercise.cs
    public CardioExercise(
        string name,
        float met,
        int durationMinutes,
        CardioIntensity intensity = CardioIntensity.Moderate,
        CardioType cardioType = CardioType.LISS,
        float? distanceKm = null,
        int? avgHeartRate = null) : base(name, met)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationMinutes));  // ← ИСПРАВЛЕНО

        DurationMinutes = durationMinutes;
        Intensity = intensity;
        CardioType = cardioType;
        DistanceKm = distanceKm;
        AvgHeartRate = avgHeartRate;
    }



    /// <summary>
    /// Обновить длительность
    /// </summary>
    public void UpdateDuration(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(minutes));
        DurationMinutes = minutes;
    }

    /// <summary>
    /// Обновить интенсивность
    /// </summary>
    public void UpdateIntensity(CardioIntensity intensity)
    {
        Intensity = intensity;
    }

    /// <summary>
    /// Обновить дистанцию
    /// </summary>
    public void UpdateDistance(float? distanceKm)
    {
        if (distanceKm.HasValue && distanceKm <= 0)
            throw new ArgumentException("Distance must be positive", nameof(distanceKm));
        DistanceKm = distanceKm;
    }
}