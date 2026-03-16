// FitnessTracker.Domain/Entities/Exercises/RunningExercise.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Enums;

namespace FitnessTracker.Domain.Entities.Exercises;

/// <summary>
/// Беговое упражнение (специализированное кардио)
/// </summary>
public class RunningExercise : CardioExercise
{
    /// <summary>
    /// Темп (мин/км)
    /// </summary>
    public float? Pace { get; private set; }

    /// <summary>
    /// Поверхность
    /// </summary>
    public RunningSurface Surface { get; private set; }

    /// <summary>
    /// Набор высоты (м)
    /// </summary>
    public int? ElevationGain { get; private set; }

    /// <summary>
    /// Дискриминатор типа (для JSON)
    /// </summary>
    public override ExerciseType Type => ExerciseType.Running;

    private RunningExercise() : base() { }

    public RunningExercise(
        string name,
        float met,
        int durationMinutes,
        float? distanceKm = null,
        RunningSurface surface = RunningSurface.Treadmill,
        CardioIntensity intensity = CardioIntensity.Moderate,
        int? avgHeartRate = null,
        int? elevationGain = null)
        : base(name, met, durationMinutes, intensity, CardioType.LISS, distanceKm, avgHeartRate)
    {
        Surface = surface;
        ElevationGain = elevationGain;

        if (distanceKm.HasValue && distanceKm.Value > 0 && durationMinutes > 0)
        {
            Pace = (float)durationMinutes / distanceKm.Value;
        }
    }

    /// <summary>
    /// Обновить поверхность
    /// </summary>
    public void UpdateSurface(RunningSurface surface)
    {
        Surface = surface;
    }

    /// <summary>
    /// Обновить набор высоты
    /// </summary>
    public void UpdateElevationGain(int? elevationGain)
    {
        if (elevationGain.HasValue && elevationGain < 0)
            throw new ArgumentException("Elevation gain cannot be negative", nameof(elevationGain));  // ← ИСПРАВЛЕНО
        ElevationGain = elevationGain;
    }

    /// <summary>
    /// Пересчитать темп
    /// </summary>
    private void RecalculatePace()
    {
        if (DistanceKm.HasValue && DistanceKm.Value > 0 && DurationMinutes > 0)
        {
            Pace = (float)DurationMinutes / DistanceKm.Value;
        }
    }
}