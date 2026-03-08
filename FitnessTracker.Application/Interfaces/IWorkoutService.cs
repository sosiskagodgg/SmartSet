using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IWorkoutService : IService  // ← Наследуем маркер
{
    Task<Workout?> GetWorkoutByIdAsync(long id, CancellationToken ct = default);
    Task<List<Workout>> GetUserWorkoutsAsync(User user, int limit = 50, CancellationToken ct = default);
    Task<Workout> CreateWorkoutAsync(User user, DateTime date, List<Exercise> exercises, CancellationToken ct = default);
    Task AddExerciseToWorkoutAsync(Workout workout, Exercise exercise, CancellationToken ct = default);
    Task RemoveExerciseFromWorkoutAsync(Workout workout, Exercise exercise, CancellationToken ct = default);
    Task DeleteWorkoutAsync(Workout workout, CancellationToken ct = default);
}