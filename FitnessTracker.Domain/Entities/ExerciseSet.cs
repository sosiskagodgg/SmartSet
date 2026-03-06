// FitnessTracker.Domain.Entities/ExerciseSet.cs
namespace FitnessTracker.Domain.Entities;

public class ExerciseSet
{
    public int Id { get; set; }
    public int WorkoutExerciseId { get; set; }
    public WorkoutExercise? WorkoutExercise { get; set; }
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public decimal? Weight { get; set; }
    public int? DurationSeconds { get; set; } // Для кардио
    public decimal? DistanceMeters { get; set; } // Для кардио
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
}