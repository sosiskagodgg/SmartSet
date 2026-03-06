using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;

public interface IWorkoutStatisticsRepository
{
    Task<List<ExerciseSet>> GetTodaySetsAsync(long userId);
    Task<List<ExerciseSet>> GetYesterdaySetsAsync(long userId);
    Task<Dictionary<DateTime, List<ExerciseSet>>> GetLastWeekSetsAsync(long userId);
    Task<Dictionary<long, (double maxWeight, int maxReps)>> GetPersonalRecordsAsync(long userId);
    Task<double> GetTotalVolumeAsync(long userId, DateTime from, DateTime to);
}