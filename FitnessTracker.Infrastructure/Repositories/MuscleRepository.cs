using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class MuscleRepository : IMuscleRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<MuscleRepository> _logger;

    public MuscleRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<MuscleRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Muscle?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Muscles.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting muscle by id {Id}", id);
            throw;
        }
    }

    public async Task<List<Muscle>> GetMusclesByUserIdAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Muscles
                .Where(m => m.UserId == userId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting muscles for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddAsync(Muscle muscle) // Должен возвращать Task<bool>
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Muscles.AddAsync(muscle);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding muscle");
            return false;
        }
    }

    public async Task<bool> Update(Muscle muscle) // Должен возвращать Task<bool>
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Muscles.Update(muscle);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating muscle {Id}", muscle.Id);
            return false;
        }
    }

    public async Task<bool> Delete(Muscle muscle) // Должен возвращать Task<bool>
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Muscles.Remove(muscle);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting muscle {Id}", muscle.Id);
            return false;
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            return false;
        }
    }
}