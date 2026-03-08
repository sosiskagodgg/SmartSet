// FitnessTracker.Infrastructure/Repositories/BaseRepository.cs
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FitnessTracker.Infrastructure.Repositories;

/// <summary>
/// Базовая реализация репозитория с общими операциями
/// </summary>
public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly FitnessDbContext _context;  // ← конкретный тип, не абстрактный DbContext
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(FitnessDbContext context)  // ← конкретный тип в конструкторе
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }
}