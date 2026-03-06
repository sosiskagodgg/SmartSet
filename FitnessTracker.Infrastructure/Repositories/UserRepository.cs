using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class UserRepository : IUserRepository
{
    private readonly IDbContextFactory<FitnessDbContext> _contextFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IDbContextFactory<FitnessDbContext> contextFactory,
        ILogger<UserRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        try
        {
            // СОЗДАЕМ НОВЫЙ КОНТЕКСТ ДЛЯ КАЖДОГО ЗАПРОСА
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id {UserId}", id);
            throw;
        }
    }

    public async Task<List<User>> GetAllAsync(int limit = 50)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<bool> AddAsync(User user)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Users.AddAsync(user);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user");
            return false;
        }
    }

    public async Task<bool> Update(User user)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Users.Update(user);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return false;
        }
    }
    public async Task<User?> GetByUserName(string userName)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users
                .FirstOrDefaultAsync(u => u.Username == userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username {UserName}", userName);
            throw;
        }
    }
    public async Task<bool> Delete(User user)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Users.Remove(user);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return false;
        }
    }
}