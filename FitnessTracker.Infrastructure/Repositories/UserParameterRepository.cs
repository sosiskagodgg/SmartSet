// FitnessTracker.Infrastructure/Repositories/UserParameterRepository.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class UserParameterRepository : IUserParameterRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<UserParameterRepository> _logger;

    public UserParameterRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<UserParameterRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<UserParameter?> GetByIdAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserParameters.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user parameter by id {Id}", id);
            throw;
        }
    }

    public async Task<List<UserParameter>> GetByUserIdAsync(long userId, bool onlyCurrent = true)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.UserParameters.Where(p => p.UserId == userId);

            if (onlyCurrent)
            {
                query = query.Where(p => p.IsCurrent);
            }

            return await query.OrderByDescending(p => p.RecordedAt).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parameters for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserParameter?> GetCurrentByUserIdAsync(long userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.UserParameters
                .Where(p => p.UserId == userId && p.IsCurrent)
                .OrderByDescending(p => p.RecordedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current parameters for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AddAsync(UserParameter parameter)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Если новый параметр текущий - снимаем флаг с остальных
            if (parameter.IsCurrent)
            {
                var currentParams = await context.UserParameters
                    .Where(p => p.UserId == parameter.UserId && p.IsCurrent)
                    .ToListAsync();

                foreach (var p in currentParams)
                {
                    p.IsCurrent = false;
                }
            }

            await context.UserParameters.AddAsync(parameter);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user parameter");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(UserParameter parameter)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.UserParameters.Update(parameter);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user parameter");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var param = await context.UserParameters.FindAsync(id);
            if (param == null) return false;

            context.UserParameters.Remove(param);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user parameter {Id}", id);
            return false;
        }
    }
}