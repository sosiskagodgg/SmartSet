namespace FitnessTracker.Domain.Interfaces
{
    /// <summary>
    /// Базовый интерфейс репозитория с общими операциями
    /// </summary>
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(int lemit = 50,CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    }
}