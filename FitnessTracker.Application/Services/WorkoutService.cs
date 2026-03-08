using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;

    public WorkoutService(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Workout?> GetWorkoutByIdAsync(long id, CancellationToken ct = default)
    {
        return await _workoutRepository.GetByIdAsync(id, ct);
    }

    public async Task<List<Workout>> GetUserWorkoutsAsync(User user, int limit = 50, CancellationToken ct = default)
    {
        var allWorkouts = await _workoutRepository.GetAllAsync(limit, ct);
        return allWorkouts
            .Where(w => w.TelegramId == user.TelegramId)
            .OrderByDescending(w => w.Date)
            .ToList();
    }

    public async Task<Workout> CreateWorkoutAsync(User user, DateTime date, List<Exercise> exercises, CancellationToken ct = default)
    {
        var workout = new Workout
        {
            TelegramId = user.TelegramId,
            Date = date,
            Exercises = exercises
        };

        await _workoutRepository.AddAsync(workout, ct);
        return workout;
    }

    public async Task AddExerciseToWorkoutAsync(Workout workout, Exercise exercise, CancellationToken ct = default)
    {
        workout.Exercises.Add(exercise);
        await _workoutRepository.UpdateAsync(workout, ct);
    }

    public async Task RemoveExerciseFromWorkoutAsync(Workout workout, Exercise exercise, CancellationToken ct = default)
    {
        workout.Exercises.Remove(exercise);
        await _workoutRepository.UpdateAsync(workout, ct);
    }

    public async Task DeleteWorkoutAsync(Workout workout, CancellationToken ct = default)
    {
        await _workoutRepository.DeleteAsync(workout, ct);
    }
}