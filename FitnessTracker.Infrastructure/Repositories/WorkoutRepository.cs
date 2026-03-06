// FitnessTracker.Infrastructure/Repositories/WorkoutRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<WorkoutRepository> _logger;

    public WorkoutRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<WorkoutRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Workout?> GetByIdAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Workouts
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Exercise)
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout by id {Id}", id);
            throw;
        }
    }

    public async Task<Workout?> GetCurrentWorkoutAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Workouts
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Exercise)
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .Where(w => w.UserId == userId && w.Status == WorkoutStatus.Active)
                .OrderByDescending(w => w.StartedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current workout for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Workout>> GetUserWorkoutsAsync(long userId, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Workouts
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Exercise)
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .Where(w => w.UserId == userId);

            if (from.HasValue)
                query = query.Where(w => w.StartedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(w => w.StartedAt <= to.Value);

            return await query
                .OrderByDescending(w => w.StartedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workouts for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddAsync(Workout workout)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Workouts.AddAsync(workout);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding workout for user {UserId}", workout.UserId);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Workout workout)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Workouts.Update(workout);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workout {Id}", workout.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workout = await context.Workouts.FindAsync(id);
            if (workout == null)
                return false;

            context.Workouts.Remove(workout);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workout {Id}", id);
            return false;
        }
    }
}