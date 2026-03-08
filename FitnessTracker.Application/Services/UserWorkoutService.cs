using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class UserWorkoutService : IUserWorkoutService
{
    private readonly IUserWorkoutRepository _userWorkoutRepository;

    public UserWorkoutService(IUserWorkoutRepository userWorkoutRepository)
    {
        _userWorkoutRepository = userWorkoutRepository;
    }

    public async Task<UserWorkout?> GetUserWorkoutAsync(User user, int dayNumber, CancellationToken ct = default)
    {
        var allWorkouts = await _userWorkoutRepository.GetAllAsync(int.MaxValue, ct);
        return allWorkouts.FirstOrDefault(w => w.TelegramId == user.TelegramId && w.DayNumber == dayNumber);
    }

    public async Task<List<UserWorkout>> GetAllUserWorkoutsAsync(User user, CancellationToken ct = default)
    {
        var allWorkouts = await _userWorkoutRepository.GetAllAsync(int.MaxValue, ct);
        return allWorkouts
            .Where(w => w.TelegramId == user.TelegramId)
            .OrderBy(w => w.DayNumber)
            .ToList();
    }

    public async Task<UserWorkout> CreateOrUpdateUserWorkoutAsync(User user, int dayNumber, string name, List<Exercise> exercises, CancellationToken ct = default)
    {
        var existing = await GetUserWorkoutAsync(user, dayNumber, ct);

        if (existing != null)
        {
            existing.Name = name;
            existing.Exercises = exercises;
            await _userWorkoutRepository.UpdateAsync(existing, ct);
            return existing;
        }

        var newWorkout = new UserWorkout
        {
            TelegramId = user.TelegramId,
            DayNumber = dayNumber,
            Name = name,
            Exercises = exercises
        };

        await _userWorkoutRepository.AddAsync(newWorkout, ct);
        return newWorkout;
    }

    public async Task AddExerciseToUserWorkoutAsync(UserWorkout userWorkout, Exercise exercise, CancellationToken ct = default)
    {
        userWorkout.Exercises.Add(exercise);
        await _userWorkoutRepository.UpdateAsync(userWorkout, ct);
    }

    public async Task RemoveExerciseFromUserWorkoutAsync(UserWorkout userWorkout, Exercise exercise, CancellationToken ct = default)
    {
        userWorkout.Exercises.Remove(exercise);
        await _userWorkoutRepository.UpdateAsync(userWorkout, ct);
    }

    public async Task DeleteUserWorkoutAsync(UserWorkout userWorkout, CancellationToken ct = default)
    {
        await _userWorkoutRepository.DeleteAsync(userWorkout, ct);
    }
}