// FitnessTracker.Infrastructure/Repositories/ExerciseSetRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class ExerciseSetRepository : IExerciseSetRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<ExerciseSetRepository> _logger;

    public ExerciseSetRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<ExerciseSetRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<ExerciseSet?> GetByIdAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting set by id {Id}", id);
            throw;
        }
    }

    public async Task<List<ExerciseSet>> GetByWorkoutExerciseIdAsync(long workoutExerciseId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ExerciseSets
                .Where(s => s.WorkoutExerciseId == workoutExerciseId)
                .OrderBy(s => s.SetNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sets for workout exercise {WorkoutExerciseId}", workoutExerciseId);
            throw;
        }
    }

    public async Task<List<ExerciseSet>> GetUserSetsAsync(long userId, long? exerciseId = null, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.ExerciseSets
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Exercise)
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Workout)
                .Where(s => s.WorkoutExercise.Workout.UserId == userId);

            if (exerciseId.HasValue)
                query = query.Where(s => s.WorkoutExercise.ExerciseId == exerciseId.Value);

            if (from.HasValue)
                query = query.Where(s => s.CompletedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(s => s.CompletedAt <= to.Value);

            return await query
                .OrderByDescending(s => s.CompletedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sets for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddAsync(ExerciseSet set)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            set.CompletedAt = DateTime.UtcNow;

            await context.ExerciseSets.AddAsync(set);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding set");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(ExerciseSet set)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.ExerciseSets.Update(set);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating set {Id}", set.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var set = await context.ExerciseSets.FindAsync(id);
            if (set == null)
                return false;

            context.ExerciseSets.Remove(set);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting set {Id}", id);
            return false;
        }
    }
}