using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class ExerciseLibraryRepository : IExerciseLibraryRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<ExerciseLibraryRepository> _logger;

    public ExerciseLibraryRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<ExerciseLibraryRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Exercises  // ← Exercises с большой буквы
                .Include(e => e.ExerciseMuscles)
                    .ThenInclude(em => em.Muscle)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise by id {Id}", id);
            throw;
        }
    }

    public async Task<List<Exercise>> GetAllBaseExercisesAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Exercises  // ← Exercises с большой буквы
                .Include(e => e.ExerciseMuscles)
                    .ThenInclude(em => em.Muscle)
                .Where(e => !e.IsCustom)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting base exercises");
            throw;
        }
    }

    public async Task<List<Exercise>> GetUserCustomExercisesAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Exercises  // ← Exercises с большой буквы
                .Include(e => e.ExerciseMuscles)
                    .ThenInclude(em => em.Muscle)
                .Where(e => e.IsCustom && e.UserId == userId)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom exercises for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Exercise?> GetByNameAsync(string name, long? userId = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Exercises  // ← Exercises с большой буквы
                .Include(e => e.ExerciseMuscles)
                    .ThenInclude(em => em.Muscle)
                .Where(e => e.Name.ToLower() == name.ToLower());

            if (userId.HasValue)
            {
                query = query.Where(e => !e.IsCustom || e.UserId == userId);
            }
            else
            {
                query = query.Where(e => !e.IsCustom);
            }

            return await query.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise by name {Name}", name);
            throw;
        }
    }

    public async Task<bool> AddAsync(Exercise exercise)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.Exercises  // ← Exercises с большой буквы
                .FirstOrDefaultAsync(e => e.Name.ToLower() == exercise.Name.ToLower()
                    && (exercise.IsCustom ? e.UserId == exercise.UserId : !e.IsCustom));

            if (existing != null)
                return false;

            exercise.CreatedAt = DateTime.UtcNow;

            await context.Exercises.AddAsync(exercise);  // ← Exercises с большой буквы
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding exercise {Name}", exercise.Name);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Exercise exercise)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Exercises.Update(exercise);  // ← Exercises с большой буквы
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exercise {Id}", exercise.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Exercise exercise)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Exercises.Remove(exercise);  // ← Exercises с большой буквы
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exercise {Id}", exercise.Id);
            return false;
        }
    }

    // Дополнительный метод если нужен по ID
    public async Task<bool> DeleteByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var exercise = await context.Exercises.FindAsync(id);  // ← Exercises с большой буквы
            if (exercise == null)
                return false;

            context.Exercises.Remove(exercise);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exercise by id {Id}", id);
            return false;
        }
    }
}