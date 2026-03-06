// FitnessTracker.Domain.Entities/Workout.cs
namespace FitnessTracker.Domain.Entities;

public class Workout
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Notes { get; set; }
    public WorkoutStatus Status { get; set; }
    public int? ProgramDayId { get; set; }
    public ProgramDay? ProgramDay { get; set; }

    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}

public enum WorkoutStatus
{
    Active,
    Completed,
    Cancelled
}