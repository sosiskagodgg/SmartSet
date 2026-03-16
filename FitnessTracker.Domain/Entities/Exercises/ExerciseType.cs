// FitnessTracker.Domain/Entities/Exercises/ExerciseType.cs
namespace FitnessTracker.Domain.Entities.Exercises;

/// <summary>
/// Тип упражнения (для дискриминатора в JSON)
/// </summary>
public enum ExerciseType
{
    Strength = 1,
    Cardio = 2,
    Running = 3,
    Static = 4
}