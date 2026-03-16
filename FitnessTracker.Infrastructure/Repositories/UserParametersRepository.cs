// FitnessTracker.Infrastructure/Repositories/UserParametersRepository.cs
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с параметрами пользователя
/// </summary>
public class UserParametersRepository : BaseRepository<UserParameters, long>, IUserParametersRepository
{
    public UserParametersRepository(FitnessDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<UserParameters?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(up => up.Id == telegramId, cancellationToken);
    }

    // Все базовые методы (GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync, ExistsAsync)
    // наследуются от BaseRepository<UserParameters, long>
}