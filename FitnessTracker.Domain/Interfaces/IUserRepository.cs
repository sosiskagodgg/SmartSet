using System.Collections.Generic;
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    Task<User?> GetByIdAsync(long id);

    Task<List<User>> GetAllAsync(int limit = 50);

    Task<User?> GetByUserName(string userName);

    Task<bool> AddAsync(User user);

    Task<bool> Update(User user);

    Task<bool> Delete(User user);
}