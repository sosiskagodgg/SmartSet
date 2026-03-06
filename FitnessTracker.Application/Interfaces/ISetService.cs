using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Application.Interfaces;

public interface ISetService
{
    Task<ExerciseSet?> LogSetAsync(int workoutExerciseId, int setNumber, int? reps, decimal? weight, int? durationSeconds, decimal? distanceMeters);
    Task<ExerciseSet?> UpdateSetAsync(int setId, int? reps, decimal? weight, int? durationSeconds, decimal? distanceMeters);
    Task<bool> DeleteSetAsync(int setId);
    Task<List<ExerciseSet>> GetSetsByWorkoutExerciseAsync(int workoutExerciseId);
    Task<List<ExerciseSet>> GetUserSetsAsync(long userId, int? exerciseId = null, DateTime? from = null, DateTime? to = null);
}