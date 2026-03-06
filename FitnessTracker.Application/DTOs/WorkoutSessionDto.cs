namespace FitnessTracker.Application.DTOs;
public class WorkoutSessionDto
{
    public long WorkoutId { get; set; }
    public DateTime StartedAt { get; set; }
    public int TotalExercises { get; set; }
    public int CompletedExercises { get; set; }
    public int TotalSets { get; set; }
    public int CompletedSets { get; set; }
    public WorkoutExerciseDto? CurrentExercise { get; set; }
    public List<WorkoutExerciseDto> Exercises { get; set; } = new();
}

public class WorkoutExerciseDto
{
    public long WorkoutExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Order { get; set; }
    public int CompletedSets { get; set; }
    public int TotalSets { get; set; }
    public List<SetDto> Sets { get; set; } = new();
}

public class SetDto
{
    public long SetId { get; set; }
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public decimal? Weight { get; set; }
    public bool IsCompleted { get; set; }
}

public class WorkoutSummaryDto
{
    public long WorkoutId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalExercises { get; set; }
    public int TotalSets { get; set; }
    public double TotalVolume { get; set; }
    public List<ExerciseSummaryDto> Exercises { get; set; } = new();
}

public class ExerciseSummaryDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int TotalReps { get; set; }
    public double MaxWeight { get; set; }
    public double Volume { get; set; }
}