// FitnessTracker.Domain.Entities/WorkoutExercise.cs
namespace FitnessTracker.Domain.Entities;

public class WorkoutExercise
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public Workout? Workout { get; set; }
    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
    public int Order { get; set; }
    public string? Notes { get; set; }

    public ICollection<ExerciseSet> Sets { get; set; } = new List<ExerciseSet>();
}