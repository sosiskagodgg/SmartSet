
using FitnessTracker.Application.DTOs;
namespace FitnessTracker.Application.Interfaces;

public interface IStatisticsService
{
    Task<DailyStatsDto> GetTodayStatsAsync(long userId);
    Task<DailyStatsDto> GetYesterdayStatsAsync(long userId);
    Task<WeeklyStatsDto> GetLastWeekStatsAsync(long userId);
    Task<List<PersonalRecordDto>> GetPersonalRecordsAsync(long userId);
    Task<ExerciseProgressDto> GetExerciseProgressAsync(long userId, int exerciseId, DateTime? from = null, DateTime? to = null);
    Task<double> GetTotalVolumeAsync(long userId, DateTime from, DateTime to);
}