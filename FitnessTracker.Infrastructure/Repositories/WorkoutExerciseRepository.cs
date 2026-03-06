// FitnessTracker.Infrastructure/Repositories/WorkoutExerciseRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class WorkoutExerciseRepository : IWorkoutExerciseRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<WorkoutExerciseRepository> _logger;

    public WorkoutExerciseRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<WorkoutExerciseRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<WorkoutExercise?> GetByIdAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.WorkoutExercises
                .Include(we => we.Exercise)
                .Include(we => we.Sets)
                .FirstOrDefaultAsync(we => we.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout exercise by id {Id}", id);
            throw;
        }
    }

    public async Task<List<WorkoutExercise>> GetByWorkoutIdAsync(long workoutId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.WorkoutExercises
                .Include(we => we.Exercise)
                .Include(we => we.Sets)
                .Where(we => we.WorkoutId == workoutId)
                .OrderBy(we => we.Order)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercises for workout {WorkoutId}", workoutId);
            throw;
        }
    }

    public async Task<bool> AddAsync(WorkoutExercise exercise)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.WorkoutExercises.AddAsync(exercise);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding exercise to workout {WorkoutId}", exercise.WorkoutId);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(WorkoutExercise exercise)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.WorkoutExercises.Update(exercise);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workout exercise {Id}", exercise.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var exercise = await context.WorkoutExercises.FindAsync(id);
            if (exercise == null)
                return false;

            context.WorkoutExercises.Remove(exercise);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workout exercise {Id}", id);
            return false;
        }
    }
}