// FitnessTracker.Application/Services/UserWorkoutService.cs
using FitnessTracker.Application.Common.Exceptions;
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services;

/// <summary>
/// Сервис для работы с тренировками пользователя (шаблонами)
/// </summary>
public class UserWorkoutService : IUserWorkoutService
{
    private readonly IUserWorkoutRepository _userWorkoutRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserWorkoutService> _logger;

    public UserWorkoutService(
        IUserWorkoutRepository userWorkoutRepository,
        IUserRepository userRepository,
        ILogger<UserWorkoutService> logger)
    {
        _userWorkoutRepository = userWorkoutRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserWorkout?> GetUserWorkoutAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default)
    {
        return await _userWorkoutRepository.GetByIdAsync(telegramId, dayNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserWorkout>> GetAllUserWorkoutsAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _userWorkoutRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserWorkout> CreateUserWorkoutAsync(
        long telegramId,
        int dayNumber,
        string name,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating workout for user {TelegramId} on day {DayNumber}", telegramId, dayNumber);

        var userExists = await _userRepository.ExistsByTelegramIdAsync(telegramId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            throw new UserNotFoundException(telegramId);
        }

        var existing = await _userWorkoutRepository.ExistsAsync(telegramId, dayNumber, cancellationToken);
        if (existing)
        {
            _logger.LogWarning("Workout for user {TelegramId} on day {DayNumber} already exists", telegramId, dayNumber);
            throw new UserWorkoutAlreadyExistsException(telegramId, dayNumber);
        }

        var workout = UserWorkout.Create(telegramId, dayNumber, name, exercises);
        await _userWorkoutRepository.AddAsync(workout, cancellationToken);

        _logger.LogInformation("Workout created for user {TelegramId} on day {DayNumber}", telegramId, dayNumber);
        return workout;
    }

    /// <inheritdoc />
    public async Task<UserWorkout> UpdateUserWorkoutAsync(
        long telegramId,
        int dayNumber,
        string name,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating workout for user {TelegramId} on day {DayNumber}", telegramId, dayNumber);

        var workout = await _userWorkoutRepository.GetByIdAsync(telegramId, dayNumber, cancellationToken);
        if (workout == null)
        {
            _logger.LogWarning("Workout for user {TelegramId} on day {DayNumber} not found", telegramId, dayNumber);
            throw new UserWorkoutNotFoundException(telegramId, dayNumber);
        }

        if (workout.Name != name)
            workout.Rename(name);

        workout.UpdateExercises(exercises);
        await _userWorkoutRepository.UpdateAsync(workout, cancellationToken);

        _logger.LogInformation("Workout updated for user {TelegramId} on day {DayNumber}", telegramId, dayNumber);
        return workout;
    }

    /// <inheritdoc />
    public async Task<UserWorkout> CreateOrUpdateUserWorkoutAsync(
        long telegramId,
        int dayNumber,
        string name,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userWorkoutRepository.ExistsAsync(telegramId, dayNumber, cancellationToken);
        return existing
            ? await UpdateUserWorkoutAsync(telegramId, dayNumber, name, exercises, cancellationToken)
            : await CreateUserWorkoutAsync(telegramId, dayNumber, name, exercises, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteUserWorkoutAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting workout for user {TelegramId} on day {DayNumber}", telegramId, dayNumber);

        var workout = await _userWorkoutRepository.GetByIdAsync(telegramId, dayNumber, cancellationToken);
        if (workout == null)
        {
            _logger.LogWarning("Workout for user {TelegramId} on day {DayNumber} not found", telegramId, dayNumber);
            throw new UserWorkoutNotFoundException(telegramId, dayNumber);
        }

        await _userWorkoutRepository.DeleteAsync(telegramId, dayNumber, cancellationToken);
        _logger.LogInformation("Workout deleted for user {TelegramId} on day {DayNumber}", telegramId, dayNumber);
    }

    /// <inheritdoc />
    public async Task<bool> UserWorkoutExistsAsync(long telegramId, int dayNumber, CancellationToken cancellationToken = default)
    {
        return await _userWorkoutRepository.ExistsAsync(telegramId, dayNumber, cancellationToken);
    }
}