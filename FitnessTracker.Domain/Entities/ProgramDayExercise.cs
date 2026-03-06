// FitnessTracker.Domain.Entities/ProgramDayExercise.cs
namespace FitnessTracker.Domain.Entities;

public class ProgramDayExercise
{
    public int Id { get; set; }
    public int ProgramDayId { get; set; }
    public ProgramDay? ProgramDay { get; set; }
    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
    public int Order { get; set; }
    public int? TargetSets { get; set; }
    public int? TargetRepsMin { get; set; }
    public int? TargetRepsMax { get; set; }
    public decimal? TargetWeight { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public decimal? TargetDistanceMeters { get; set; }
    public string? Notes { get; set; }
}