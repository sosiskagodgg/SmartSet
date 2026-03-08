using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Репозиторий для работы с пользователями
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(FitnessDbContext context) : base(context)
    {
    }

    // Можно добавить специфичные методы, например:
    // public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    // {
    //     return await _dbSet.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
    // }
}