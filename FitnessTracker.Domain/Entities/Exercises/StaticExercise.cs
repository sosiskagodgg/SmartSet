// FitnessTracker.Domain/Entities/Exercises/StaticExercise.cs (исправленный)
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Enums;

namespace FitnessTracker.Domain.Entities.Exercises;

/// <summary>
/// Статическое упражнение (планка, растяжка и т.д.)
/// </summary>
public class StaticExercise : Exercise
{
    /// <summary>
    /// Длительность удержания в секундах
    /// </summary>
    public int HoldSeconds { get; private set; }  // ← просто int, не Duration

    /// <summary>
    /// Количество подходов
    /// </summary>
    public int Sets { get; private set; }

    /// <summary>
    /// Тип статического упражнения
    /// </summary>
    public StaticType StaticType { get; private set; }

    /// <summary>
    /// Дискриминатор типа (для JSON)
    /// </summary>
    public override ExerciseType Type => ExerciseType.Static;

    // Для EF Core
    private StaticExercise() : base() { }

    public StaticExercise(
        string name,
        float met,
        int holdSeconds,
        int sets,
        StaticType staticType = StaticType.Plank)
        : base(name, met)
    {
        if (holdSeconds <= 0)
            throw new ArgumentException("Hold duration must be positive", nameof(holdSeconds));

        if (sets <= 0)
            throw new ArgumentException("Number of sets must be positive", nameof(sets));

        HoldSeconds = holdSeconds;
        Sets = sets;
        StaticType = staticType;
    }

    /// <summary>
    /// Обновить количество подходов
    /// </summary>
    public void UpdateSets(int newSets)
    {
        if (newSets <= 0)
            throw new ArgumentException("Number of sets must be positive", nameof(newSets));

        Sets = newSets;
    }

    /// <summary>
    /// Обновить длительность удержания
    /// </summary>
    public void UpdateHoldDuration(int newSeconds)
    {
        if (newSeconds <= 0)
            throw new ArgumentException("Hold duration must be positive", nameof(newSeconds));

        HoldSeconds = newSeconds;
    }

    /// <summary>
    /// Обновить тип упражнения
    /// </summary>
    public void UpdateStaticType(StaticType newType)
    {
        StaticType = newType;
    }
}