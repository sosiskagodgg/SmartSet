// FitnessTracker.Domain.Entities/Exercise.cs
using System;
using System.Collections.Generic;

namespace FitnessTracker.Domain.Entities;

public enum ExerciseCategory
{
    Unknown,
    Strength,
    Cardio,
    Stretching,
    Other
}

public class Exercise
{
    public int Id { get; set; } // В БД INT
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? MET { get; set; } // Для кардио
    public ExerciseCategory Category { get; set; } // Вместо ExerciseType
    public bool IsCustom { get; set; }
    public long? UserId { get; set; } // BIGINT
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }

    // Связь с мышцами через ExerciseMuscle
    public ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();
}

// Силовое упражнение (наследование не обязательно, можно просто использовать Exercise)
public class StrengthExercise : Exercise
{
    public StrengthExercise()
    {
        Category = ExerciseCategory.Strength;
    }
}

// Кардио упражнение
public class CardioExercise : Exercise
{
    public CardioExercise()
    {
        Category = ExerciseCategory.Cardio;
    }
}