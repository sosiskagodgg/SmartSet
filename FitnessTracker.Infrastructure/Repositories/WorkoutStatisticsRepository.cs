// FitnessTracker.Infrastructure/Repositories/WorkoutStatisticsRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class WorkoutStatisticsRepository : IWorkoutStatisticsRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<WorkoutStatisticsRepository> _logger;

    public WorkoutStatisticsRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<WorkoutStatisticsRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<ExerciseSet>> GetTodaySetsAsync(long userId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Exercise)
                .Where(s => s.WorkoutExercise.Workout.UserId == userId &&
                           s.CompletedAt >= today &&
                           s.CompletedAt < tomorrow)
                .OrderBy(s => s.CompletedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's sets for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ExerciseSet>> GetYesterdaySetsAsync(long userId)
    {
        try
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var today = DateTime.UtcNow.Date;

            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Exercise)
                .Where(s => s.WorkoutExercise.Workout.UserId == userId &&
                           s.CompletedAt >= yesterday &&
                           s.CompletedAt < today)
                .OrderBy(s => s.CompletedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yesterday's sets for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Dictionary<DateTime, List<ExerciseSet>>> GetLastWeekSetsAsync(long userId)
    {
        try
        {
            var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            await using var context = await _contextFactory.CreateDbContextAsync();
            var sets = await context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Exercise)
                .Where(s => s.WorkoutExercise.Workout.UserId == userId &&
                           s.CompletedAt >= weekAgo &&
                           s.CompletedAt < tomorrow)
                .ToListAsync();

            return sets
                .GroupBy(s => s.CompletedAt!.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last week sets for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Dictionary<long, (double maxWeight, int maxReps)>> GetPersonalRecordsAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var sets = await context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                .Where(s => s.WorkoutExercise.Workout.UserId == userId &&
                           s.Weight.HasValue &&
                           s.Reps.HasValue)
                .ToListAsync();

            var records = new Dictionary<long, (double maxWeight, int maxReps)>();

            foreach (var set in sets)
            {
                var exerciseId = set.WorkoutExercise.ExerciseId;

                if (!records.ContainsKey(exerciseId))
                {
                    records[exerciseId] = (0, 0);
                }

                var current = records[exerciseId];

                if (set.Weight.HasValue && (double)set.Weight.Value > current.maxWeight)
                {
                    records[exerciseId] = ((double)set.Weight.Value, current.maxReps);
                }

                if (set.Reps.HasValue && set.Reps.Value > current.maxReps)
                {
                    records[exerciseId] = (current.maxWeight, set.Reps.Value);
                }
            }

            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal records for user {UserId}", userId);
            throw;
        }
    }

    public async Task<double> GetTotalVolumeAsync(long userId, DateTime from, DateTime to)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var sets = await context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                .Where(s => s.WorkoutExercise.Workout.UserId == userId &&
                           s.CompletedAt >= from &&
                           s.CompletedAt <= to &&
                           s.Weight.HasValue &&
                           s.Reps.HasValue)
                .ToListAsync();

            return sets.Sum(s => (double)(s.Weight.Value * s.Reps.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total volume for user {UserId}", userId);
            throw;
        }
    }
}