using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

public interface IMuscleRepository
{
    Task<List<Muscle>> GetMusclesByUserIdAsync(long userId);
    Task<Muscle?> GetByIdAsync(int id);
    Task<bool> AddAsync(Muscle muscle);
    Task<bool> Update(Muscle muscle);
    Task<bool> Delete(Muscle muscle);
    Task<bool> SaveChangesAsync();
}