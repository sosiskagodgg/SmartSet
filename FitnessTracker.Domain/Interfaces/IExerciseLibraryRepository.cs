using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Domain.Interfaces;

public interface IExerciseLibraryRepository
{
    Task<Exercise?> GetByIdAsync(int id); // int, не long!
    Task<List<Exercise>> GetAllBaseExercisesAsync();
    Task<List<Exercise>> GetUserCustomExercisesAsync(long userId);
    Task<Exercise?> GetByNameAsync(string name, long? userId = null);
    Task<bool> AddAsync(Exercise exercise);
    Task<bool> UpdateAsync(Exercise exercise);
    Task<bool> DeleteAsync(Exercise exercise); // Принимает упражнение, а не id!
}