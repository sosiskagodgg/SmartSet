// FitnessTracker.Domain.Entities/WorkoutStatistics.cs (для удобства)
namespace FitnessTracker.Domain.Entities;

public class WorkoutStatistics
{
    public long UserId { get; set; }
    public DateTime Date { get; set; }
    public int TotalExercises { get; set; }
    public int TotalSets { get; set; }
    public decimal TotalVolume { get; set; } // Сумма вес * повторения
    public int TotalDurationMinutes { get; set; }
    public List<ExerciseSet> Sets { get; set; } = new();
}