// FitnessTracker.Infrastructure/Repositories/BaseRepository.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Базовая реализация репозитория с общими CRUD операциями
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
/// <typeparam name="TId">Тип идентификатора</typeparam>
public abstract class BaseRepository<T, TId> : IBaseRepository<T, TId>
    where T : Entity<TId>
    where TId : notnull
{
    protected readonly FitnessDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(FitnessDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id.Equals(id), cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[BaseRepository] Adding entity of type {typeof(T).Name}");
        Console.WriteLine($"[BaseRepository] Entity Id: {entity.Id}");

        if (entity is User user)
        {
            Console.WriteLine($"[BaseRepository] User CreatedAt: {user.CreatedAt}, Kind: {user.CreatedAt.Kind}");
            Console.WriteLine($"[BaseRepository] User UpdatedAt: {user.UpdatedAt}, Kind: {user.UpdatedAt.Kind}");
        }

        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"[BaseRepository] Entity added successfully");
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
}