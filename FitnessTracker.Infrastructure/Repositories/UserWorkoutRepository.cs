// FitnessTracker.Infrastructure/Repositories/UserWorkoutRepository.cs
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с тренировками пользователя (шаблонами).
/// Использует Entity Framework Core для доступа к данным.
/// </summary>
public class UserWorkoutRepository : IUserWorkoutRepository
{
    /// <summary>
    /// Контекст базы данных
    /// </summary>
    protected readonly FitnessDbContext Context;

    /// <summary>
    /// DbSet для работы с тренировками пользователя
    /// </summary>
    protected readonly DbSet<UserWorkout> DbSet;

    /// <summary>
    /// Конструктор с внедрением зависимости контекста базы данных
    /// </summary>
    /// <param name="context">Контекст базы данных FitnessDbContext</param>
    public UserWorkoutRepository(FitnessDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<UserWorkout>();
    }

    /// <inheritdoc />
    public virtual async Task<UserWorkout?> GetByIdAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default)
    {
        // Ищем тренировку по составному ключу
        return await DbSet
            .FirstOrDefaultAsync(uw => uw.TelegramId == telegramId && uw.DayNumber == dayNumber, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<UserWorkout>> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        // Получаем все тренировки пользователя, отсортированные по номеру дня
        return await DbSet
            .Where(uw => uw.TelegramId == telegramId)
            .OrderBy(uw => uw.DayNumber)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default)
    {
        // Проверяем существование тренировки (более эффективно, чем загрузка всей сущности)
        return await DbSet
            .AnyAsync(uw => uw.TelegramId == telegramId && uw.DayNumber == dayNumber, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        if (workout == null)
            throw new ArgumentNullException(nameof(workout));

        // Добавляем тренировку в контекст
        await DbSet.AddAsync(workout, cancellationToken);

        // Сохраняем изменения в базе данных
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        if (workout == null)
            throw new ArgumentNullException(nameof(workout));

        // Помечаем тренировку как измененную
        DbSet.Update(workout);

        // Сохраняем изменения в базе данных
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default)
    {
        // Сначала находим тренировку
        var workout = await GetByIdAsync(telegramId, dayNumber, cancellationToken);

        if (workout != null)
        {
            // Удаляем её из контекста
            DbSet.Remove(workout);

            // Сохраняем изменения в базе данных
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}