// FitnessTracker.Domain/Interfaces/IUserParameterRepository.cs
using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;

public interface IUserParameterRepository
{
    Task<UserParameter?> GetByIdAsync(long id);
    Task<List<UserParameter>> GetByUserIdAsync(long userId, bool onlyCurrent = true);
    Task<UserParameter?> GetCurrentByUserIdAsync(long userId);
    Task<bool> AddAsync(UserParameter parameter);
    Task<bool> UpdateAsync(UserParameter parameter);
    Task<bool> DeleteAsync(long id);
}