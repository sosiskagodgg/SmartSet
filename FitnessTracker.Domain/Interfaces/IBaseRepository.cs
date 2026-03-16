// FitnessTracker.Domain/Interfaces/IBaseRepository.cs
using FitnessTracker.Domain.Common;

namespace FitnessTracker.Domain.Interfaces;

/// <summary>
/// Базовый интерфейс репозитория для всех сущностей.
/// Содержит общие CRUD операции.
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
/// <typeparam name="TId">Тип идентификатора</typeparam>
public interface IBaseRepository<T, TId> where T : Entity<TId> where TId : notnull
{
    /// <summary>
    /// Получить сущность по идентификатору
    /// </summary>
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все сущности с ограничением по количеству
    /// </summary>
    /// <param name="limit">Максимальное количество записей (по умолчанию 50)</param>
    Task<IReadOnlyList<T>> GetAllAsync(int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование сущности по идентификатору
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую сущность
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить существующую сущность
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить сущность
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}