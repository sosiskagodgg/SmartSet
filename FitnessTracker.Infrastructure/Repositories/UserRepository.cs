// FitnessTracker.Infrastructure/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User, long>, IUserRepository
{
    public UserRepository(FitnessDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Id == telegramId, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.SubscriptionStatus == "active" && u.SubscriptionEndDate > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Id == telegramId, cancellationToken);
    }
}