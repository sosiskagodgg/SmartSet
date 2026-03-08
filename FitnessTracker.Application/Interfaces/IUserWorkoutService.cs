using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IUserWorkoutService : IService  // ← Наследуем маркер
{
    Task<UserWorkout?> GetUserWorkoutAsync(User user, int dayNumber, CancellationToken ct = default);
    Task<List<UserWorkout>> GetAllUserWorkoutsAsync(User user, CancellationToken ct = default);
    Task<UserWorkout> CreateOrUpdateUserWorkoutAsync(User user, int dayNumber, string name, List<Exercise> exercises, CancellationToken ct = default);
    Task AddExerciseToUserWorkoutAsync(UserWorkout userWorkout, Exercise exercise, CancellationToken ct = default);
    Task RemoveExerciseFromUserWorkoutAsync(UserWorkout userWorkout, Exercise exercise, CancellationToken ct = default);
    Task DeleteUserWorkoutAsync(UserWorkout userWorkout, CancellationToken ct = default);
}