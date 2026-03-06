// FitnessTracker.Application/Interfaces/IUserParameterService.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IUserParameterService
{
    Task<UserParameter?> GetCurrentAsync(long userId);
    Task<List<UserParameter>> GetHistoryAsync(long userId, int limit = 10);
    Task<UserParameter?> AddOrUpdateAsync(long userId, UserParameter parameter);
    Task<bool> DeleteAsync(long id);
}