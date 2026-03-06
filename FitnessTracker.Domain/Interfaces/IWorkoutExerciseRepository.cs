using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Domain.Interfaces;

public interface IWorkoutExerciseRepository
{
    Task<WorkoutExercise?> GetByIdAsync(long id);
    Task<List<WorkoutExercise>> GetByWorkoutIdAsync(long workoutId);
    Task<bool> AddAsync(WorkoutExercise exercise);
    Task<bool> UpdateAsync(WorkoutExercise exercise);
    Task<bool> DeleteAsync(long id);
}