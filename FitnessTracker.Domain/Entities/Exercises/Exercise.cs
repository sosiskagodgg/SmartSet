// FitnessTracker.Domain/Entities/Exercises/Exercise.cs
using System.Text.Json.Serialization;
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.Entities.Exercises;

/// <summary>
/// Базовый класс для всех упражнений
/// </summary>
[JsonDerivedType(typeof(StrengthExercise), typeDiscriminator: "strength")]
[JsonDerivedType(typeof(CardioExercise), typeDiscriminator: "cardio")]
[JsonDerivedType(typeof(RunningExercise), typeDiscriminator: "running")]
[JsonDerivedType(typeof(StaticExercise), typeDiscriminator: "static")]
public abstract class Exercise : Entity<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public float MET { get; private set; }

    [JsonIgnore]
    public abstract ExerciseType Type { get; }

    // Добавляем конструктор по умолчанию для сериализации
    protected Exercise() : base(Guid.NewGuid())
    {
        Name = string.Empty;
    }

    protected Exercise(string name, float met) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Exercise name cannot be empty", nameof(name));

        if (met <= 0)
            throw new ArgumentException("MET must be positive", nameof(met));

        Name = name;
        MET = met;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }
}