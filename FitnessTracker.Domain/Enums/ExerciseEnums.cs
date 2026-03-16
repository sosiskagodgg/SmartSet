// FitnessTracker.Domain/Enums/ExerciseEnums.cs
namespace FitnessTracker.Domain.Enums;

/// <summary>
/// Интенсивность кардио
/// </summary>
public enum CardioIntensity
{
    Low,
    Moderate,
    High
}

/// <summary>
/// Тип кардио
/// </summary>
public enum CardioType
{
    LISS,
    HIIT
}

/// <summary>
/// Тип статического упражнения
/// </summary>
public enum StaticType
{
    Plank,
    Stretching,
    Yoga,
    Balance,
    WallSit,
    HollowHold
}

/// <summary>
/// Тип силового упражнения
/// </summary>
public enum StrengthExerciseType
{
    Compound,
    Isolation
}

/// <summary>
/// Оборудование
/// </summary>
public enum Equipment
{
    Bodyweight,
    Barbell,
    Dumbbell,
    Machine,
    Cable,
    Kettlebell,
    Resistance,
    Smith,
    PullUpBar,
    ParallelBars
}

/// <summary>
/// Поверхность для бега
/// </summary>
public enum RunningSurface
{
    Treadmill,
    Track,
    Road,
    Trail,
    TrailHill
}