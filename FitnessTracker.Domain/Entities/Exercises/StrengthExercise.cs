// FitnessTracker.Domain/Entities/Exercises/StrengthExercise.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Enums;
using FitnessTracker.Domain.ValueObjects;

namespace FitnessTracker.Domain.Entities.Exercises;

/// <summary>
/// Силовое упражнение (со штангой, гантелями и т.д.)
/// </summary>
public class StrengthExercise : Exercise
{
    /// <summary>
    /// Количество подходов
    /// </summary>
    public int Sets { get; private set; }

    /// <summary>
    /// Количество повторений
    /// </summary>
    public int Reps { get; private set; }

    /// <summary>
    /// Вес (опционально, для упражнений с весом)
    /// </summary>
    public Weight? Weight { get; private set; }

    /// <summary>
    /// Группа мышц
    /// </summary>
    public string MuscleGroup { get; private set; }

    /// <summary>
    /// Тип упражнения (базовое/изолированное)
    /// </summary>
    public StrengthExerciseType StrengthExerciseType { get; private set; }

    /// <summary>
    /// Необходимое оборудование
    /// </summary>
    public Equipment Equipment { get; private set; }

    /// <summary>
    /// Дискриминатор типа (для JSON)
    /// </summary>
    public override ExerciseType Type => ExerciseType.Strength;

    // Для EF Core
    private StrengthExercise() : base()
    {
        MuscleGroup = string.Empty;
    }

    public StrengthExercise(
        string name,
        float met,
        int sets,
        int reps,
        string muscleGroup,
        StrengthExerciseType strengthExerciseType = StrengthExerciseType.Compound,
        Equipment equipment = Equipment.Bodyweight,
        decimal? weightKg = null) : base(name, met)
    {
        if (sets <= 0)
            throw new ArgumentException("Number of sets must be positive", nameof(sets));

        if (reps <= 0)
            throw new ArgumentException("Number of reps must be positive", nameof(reps));

        if (string.IsNullOrWhiteSpace(muscleGroup))
            throw new ArgumentException("Muscle group cannot be empty", nameof(muscleGroup));

        Sets = sets;
        Reps = reps;
        MuscleGroup = muscleGroup;
        StrengthExerciseType = strengthExerciseType;
        Equipment = equipment;

        if (weightKg.HasValue)
            Weight = Weight.Create(weightKg.Value);
    }

    /// <summary>
    /// Обновить вес
    /// </summary>
    public void UpdateWeight(decimal? newWeightKg)
    {
        Weight = newWeightKg.HasValue
            ? Weight.Create(newWeightKg.Value)
            : null;
    }

    /// <summary>
    /// Обновить подходы и повторения
    /// </summary>
    public void UpdateSetsAndReps(int newSets, int newReps)
    {
        if (newSets <= 0)
            throw new ArgumentException("Number of sets must be positive", nameof(newSets));

        if (newReps <= 0)
            throw new ArgumentException("Number of reps must be positive", nameof(newReps));

        Sets = newSets;
        Reps = newReps;
    }

    /// <summary>
    /// Обновить группу мышц
    /// </summary>
    public void UpdateMuscleGroup(string newMuscleGroup)
    {
        if (string.IsNullOrWhiteSpace(newMuscleGroup))
            throw new ArgumentException("Muscle group cannot be empty", nameof(newMuscleGroup));

        MuscleGroup = newMuscleGroup;
    }

    /// <summary>
    /// Обновить тип упражнения
    /// </summary>
    public void UpdateStrengthExerciseType(StrengthExerciseType newType)
    {
        StrengthExerciseType = newType;
    }

    /// <summary>
    /// Обновить оборудование
    /// </summary>
    public void UpdateEquipment(Equipment newEquipment)
    {
        Equipment = newEquipment;
    }
}