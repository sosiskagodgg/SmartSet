// FitnessTracker.Infrastructure/Repositories/WorkoutRepository.cs
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для работы с ежедневными тренировками (выполненными).
/// Использует Entity Framework Core для доступа к данным.
/// </summary>
public class WorkoutRepository : IWorkoutRepository
{
    /// <summary>
    /// Контекст базы данных
    /// </summary>
    protected readonly FitnessDbContext Context;

    /// <summary>
    /// DbSet для работы с тренировками
    /// </summary>
    protected readonly DbSet<Workout> DbSet;

    /// <summary>
    /// Конструктор с внедрением зависимости контекста базы данных
    /// </summary>
    /// <param name="context">Контекст базы данных FitnessDbContext</param>
    public WorkoutRepository(FitnessDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<Workout>();
    }

    /// <inheritdoc />
    public virtual async Task<Workout?> GetByIdAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default)
    {
        // Ищем тренировку по составному ключу (TelegramId + Date)
        return await DbSet.FindAsync(new object[] { telegramId, date }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<Workout>> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        // Получаем все тренировки пользователя, отсортированные по дате (сначала новые)
        return await DbSet
            .Where(w => w.TelegramId == telegramId)
            .OrderByDescending(w => w.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<Workout>> GetByDateRangeAsync(
        long telegramId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Проверяем корректность диапазона
        if (startDate > endDate)
            throw new ArgumentException("Start date must be less than or equal to end date");

        // Получаем тренировки за указанный период, отсортированные по дате
        return await DbSet
            .Where(w => w.TelegramId == telegramId && w.Date >= startDate && w.Date <= endDate)
            .OrderBy(w => w.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default)
    {
        // Проверяем существование тренировки (более эффективно, чем загрузка всей сущности)
        return await DbSet.AnyAsync(w => w.TelegramId == telegramId && w.Date == date, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        if (workout == null)
            throw new ArgumentNullException(nameof(workout));

        // Добавляем тренировку в контекст
        await DbSet.AddAsync(workout, cancellationToken);

        // Сохраняем изменения в базе данных
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        if (workout == null)
            throw new ArgumentNullException(nameof(workout));

        // Помечаем тренировку как измененную
        DbSet.Update(workout);

        // Сохраняем изменения в базе данных
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default)
    {
        // Сначала находим тренировку
        var workout = await GetByIdAsync(telegramId, date, cancellationToken);

        if (workout != null)
        {
            // Удаляем её из контекста
            DbSet.Remove(workout);

            // Сохраняем изменения в базе данных
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}