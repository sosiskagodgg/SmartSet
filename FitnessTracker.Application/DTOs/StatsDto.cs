namespace FitnessTracker.Application.DTOs;
public class DailyStatsDto
{
    public DateTime Date { get; set; }
    public int TotalExercises { get; set; }
    public int TotalSets { get; set; }
    public double TotalVolume { get; set; }
    public int TotalDurationMinutes { get; set; }
    public List<ExerciseStatsDto> Exercises { get; set; } = new();
}

public class ExerciseStatsDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int TotalReps { get; set; }
    public double MaxWeight { get; set; }
    public double Volume { get; set; }
}

public class WeeklyStatsDto
{
    public DateTime WeekStart { get; set; }
    public int TotalWorkouts { get; set; }
    public double TotalVolume { get; set; }
    public int TotalDurationMinutes { get; set; }
    public Dictionary<DateTime, DailyStatsDto> DailyStats { get; set; } = new();
}

public class PersonalRecordDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public double Weight { get; set; }
    public int Reps { get; set; }
    public DateTime AchievedAt { get; set; }
    public long SetId { get; set; }
}

public class ExerciseProgressDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public List<ProgressPointDto> Progress { get; set; } = new();
}

public class ProgressPointDto
{
    public DateTime Date { get; set; }
    public double? MaxWeight { get; set; }
    public int? MaxReps { get; set; }
    public double? Volume { get; set; }
}