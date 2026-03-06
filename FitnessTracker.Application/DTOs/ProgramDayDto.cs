namespace FitnessTracker.Application.DTOs;
public class ProgramDayDto
{
    public int DayNumber { get; set; }
    public string? Name { get; set; }
    public bool IsRestDay { get; set; }
    public List<ProgramDayExerciseDto> Exercises { get; set; } = new();
}

public class ProgramDayExerciseDto
{
    public int ExerciseId { get; set; }
    public int Order { get; set; }
    public int? TargetSets { get; set; }
    public int? TargetRepsMin { get; set; }
    public int? TargetRepsMax { get; set; }
    public decimal? TargetWeight { get; set; }
}