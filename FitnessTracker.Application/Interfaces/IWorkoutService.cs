using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Application.Interfaces;

public interface IWorkoutService
{
    Task<Workout?> StartWorkoutAsync(long userId, int? programDayId = null);
    Task<Workout?> EndWorkoutAsync(int workoutId);
    Task<Workout?> GetCurrentWorkoutAsync(long userId);
    Task<List<Workout>> GetWorkoutHistoryAsync(long userId, DateTime? from = null, DateTime? to = null);
    Task<Workout?> GetWorkoutByIdAsync(int workoutId);
    Task<WorkoutExercise?> AddExerciseToWorkoutAsync(int workoutId, int exerciseId, int order);
}