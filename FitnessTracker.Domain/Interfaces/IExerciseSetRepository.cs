using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;

public interface IExerciseSetRepository
{
    Task<ExerciseSet?> GetByIdAsync(long id);
    Task<List<ExerciseSet>> GetByWorkoutExerciseIdAsync(long workoutExerciseId);
    Task<List<ExerciseSet>> GetUserSetsAsync(long userId, long? exerciseId = null, DateTime? from = null, DateTime? to = null);
    Task<bool> AddAsync(ExerciseSet set);
    Task<bool> UpdateAsync(ExerciseSet set);
    Task<bool> DeleteAsync(long id);
}