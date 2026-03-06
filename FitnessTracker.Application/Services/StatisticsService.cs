using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.DTOs;

namespace FitnessTracker.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IWorkoutStatisticsRepository _statisticsRepository;
    private readonly IExerciseSetRepository _setRepository;
    private readonly IExerciseLibraryRepository _exerciseRepository;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        IWorkoutStatisticsRepository statisticsRepository,
        IExerciseSetRepository setRepository,
        IExerciseLibraryRepository exerciseRepository,
        ILogger<StatisticsService> logger)
    {
        _statisticsRepository = statisticsRepository;
        _setRepository = setRepository;
        _exerciseRepository = exerciseRepository;
        _logger = logger;
    }

    public async Task<DailyStatsDto> GetTodayStatsAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting today's stats for user {UserId}", userId);
            var sets = await _statisticsRepository.GetTodaySetsAsync(userId);
            return MapToDailyStats(sets, DateTime.UtcNow.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's stats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<DailyStatsDto> GetYesterdayStatsAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting yesterday's stats for user {UserId}", userId);
            var sets = await _statisticsRepository.GetYesterdaySetsAsync(userId);
            return MapToDailyStats(sets, DateTime.UtcNow.Date.AddDays(-1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yesterday's stats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WeeklyStatsDto> GetLastWeekStatsAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting last week stats for user {UserId}", userId);
            var dailySets = await _statisticsRepository.GetLastWeekSetsAsync(userId);

            var weeklyStats = new WeeklyStatsDto
            {
                WeekStart = DateTime.UtcNow.Date.AddDays(-7),
                TotalWorkouts = dailySets.Count,
                TotalVolume = 0,
                TotalDurationMinutes = 0,
                DailyStats = new Dictionary<DateTime, DailyStatsDto>()
            };

            foreach (var (date, sets) in dailySets)
            {
                var daily = MapToDailyStats(sets, date);
                weeklyStats.DailyStats[date] = daily;
                weeklyStats.TotalVolume += daily.TotalVolume;
                weeklyStats.TotalDurationMinutes += daily.TotalDurationMinutes;
            }

            return weeklyStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last week stats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<PersonalRecordDto>> GetPersonalRecordsAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting personal records for user {UserId}", userId);
            var records = await _statisticsRepository.GetPersonalRecordsAsync(userId);

            var result = new List<PersonalRecordDto>();

            foreach (var (exerciseId, (maxWeight, maxReps)) in records)
            {
                var exercise = await _exerciseRepository.GetByIdAsync((int)exerciseId);
                if (exercise == null) continue;

                // Находим сет с максимальным весом
                var sets = await _setRepository.GetUserSetsAsync(userId, exerciseId);
                var bestWeightSet = sets.Where(s => s.Weight.HasValue)
                    .OrderByDescending(s => s.Weight)
                    .FirstOrDefault();

                var bestRepsSet = sets.Where(s => s.Reps.HasValue)
                    .OrderByDescending(s => s.Reps)
                    .FirstOrDefault();

                if (bestWeightSet != null)
                {
                    result.Add(new PersonalRecordDto
                    {
                        ExerciseName = exercise.Name,
                        Weight = (double)bestWeightSet.Weight!,
                        Reps = bestWeightSet.Reps ?? 0,
                        AchievedAt = bestWeightSet.CompletedAt ?? DateTime.UtcNow,
                        SetId = bestWeightSet.Id
                    });
                }

                if (bestRepsSet != null && (bestWeightSet == null || bestRepsSet.Id != bestWeightSet.Id))
                {
                    result.Add(new PersonalRecordDto
                    {
                        ExerciseName = exercise.Name,
                        Weight = (double)(bestRepsSet.Weight ?? 0),
                        Reps = bestRepsSet.Reps ?? 0,
                        AchievedAt = bestRepsSet.CompletedAt ?? DateTime.UtcNow,
                        SetId = bestRepsSet.Id
                    });
                }
            }

            return result.OrderByDescending(r => r.AchievedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal records for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ExerciseProgressDto> GetExerciseProgressAsync(long userId, int exerciseId, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            _logger.LogDebug("Getting progress for exercise {ExerciseId} for user {UserId}", exerciseId, userId);

            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
            if (exercise == null)
            {
                throw new ArgumentException($"Exercise {exerciseId} not found");
            }

            var sets = await _setRepository.GetUserSetsAsync(userId, exerciseId, from, to);

            var progress = new ExerciseProgressDto
            {
                ExerciseId = exerciseId,
                ExerciseName = exercise.Name,
                Progress = new List<ProgressPointDto>()
            };

            var groupedByDate = sets.GroupBy(s => s.CompletedAt!.Value.Date);

            foreach (var day in groupedByDate.OrderBy(g => g.Key))
            {
                var maxWeight = day.Where(s => s.Weight.HasValue).Max(s => (double?)s.Weight);
                var maxReps = day.Where(s => s.Reps.HasValue).Max(s => s.Reps);
                var volume = day.Where(s => s.Weight.HasValue && s.Reps.HasValue)
                    .Sum(s => (double)(s.Weight!.Value * s.Reps!.Value));

                progress.Progress.Add(new ProgressPointDto
                {
                    Date = day.Key,
                    MaxWeight = maxWeight,
                    MaxReps = maxReps,
                    Volume = volume
                });
            }

            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise progress for user {UserId}", userId);
            throw;
        }
    }

    public async Task<double> GetTotalVolumeAsync(long userId, DateTime from, DateTime to)
    {
        try
        {
            _logger.LogDebug("Getting total volume for user {UserId} from {From} to {To}", userId, from, to);
            return await _statisticsRepository.GetTotalVolumeAsync(userId, from, to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total volume for user {UserId}", userId);
            throw;
        }
    }

    private DailyStatsDto MapToDailyStats(List<ExerciseSet> sets, DateTime date)
    {
        var stats = new DailyStatsDto
        {
            Date = date,
            TotalExercises = sets.Select(s => s.WorkoutExercise.ExerciseId).Distinct().Count(),
            TotalSets = sets.Count,
            TotalVolume = sets.Where(s => s.Weight.HasValue && s.Reps.HasValue)
                .Sum(s => (double)(s.Weight!.Value * s.Reps!.Value)),
            TotalDurationMinutes = sets.Sum(s => s.DurationSeconds ?? 0) / 60,
            Exercises = new List<ExerciseStatsDto>()
        };

        var exercises = sets.GroupBy(s => s.WorkoutExercise.ExerciseId);

        foreach (var ex in exercises)
        {
            var exercise = ex.First().WorkoutExercise.Exercise;
            stats.Exercises.Add(new ExerciseStatsDto
            {
                ExerciseName = exercise?.Name ?? "Unknown",
                Sets = ex.Count(),
                TotalReps = ex.Sum(s => s.Reps ?? 0),
                MaxWeight = (double)(ex.Where(s => s.Weight.HasValue).Max(s => (decimal?)s.Weight) ?? 0),
                Volume = (double)ex.Where(s => s.Weight.HasValue && s.Reps.HasValue)
                    .Sum(s => s.Weight!.Value * s.Reps!.Value)
            });
        }

        return stats;
    }
}