using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;
public interface IWorkoutRepository
{
    Task<Workout?> GetByIdAsync(long id);
    Task<Workout?> GetCurrentWorkoutAsync(long userId);
    Task<List<Workout>> GetUserWorkoutsAsync(long userId, DateTime? from = null, DateTime? to = null);
    Task<bool> AddAsync(Workout workout);
    Task<bool> UpdateAsync(Workout workout);
    Task<bool> DeleteAsync(long id);
}