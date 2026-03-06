using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    Task<User?> GetByIdAsync(long id);

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    Task<List<User>> GetAllAsync(int limit = 50);

    /// <summary>
    /// Создать пользователя
    /// Возвращает созданного пользователя или null при ошибке
    /// </summary>
    Task<User?> CreateAsync(User user);

    /// <summary>
    /// Обновить пользователя
    /// Возвращает true при успехе, иначе false
    /// </summary>
    Task<bool> UpdateAsync(User user);

    /// <summary>
    /// Удалить пользователя по ID
    /// Возвращает true при успехе, иначе false
    /// </summary>
    Task<bool> DeleteAsync(long id);
}
