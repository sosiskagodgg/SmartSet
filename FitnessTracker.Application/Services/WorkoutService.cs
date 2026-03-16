// FitnessTracker.Application/Services/WorkoutService.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Entities.Exercises;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Common.Interfaces;
using FitnessTracker.Application.Common.Exceptions;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<WorkoutService> _logger;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IUserRepository userRepository,
        ILogger<WorkoutService> logger)
    {
        _workoutRepository = workoutRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Workout?> GetWorkoutByDateAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _workoutRepository.GetByIdAsync(telegramId, date, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Workout?> GetTodayWorkoutAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await GetWorkoutByDateAsync(telegramId, today, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Workout>> GetUserWorkoutsAsync(
        long telegramId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
        return workouts.Take(limit).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Workout>> GetWorkoutsByDateRangeAsync(
        long telegramId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _workoutRepository.GetByDateRangeAsync(telegramId, startDate, endDate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Workout> CreateWorkoutAsync(
        long telegramId,
        DateTime date,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating workout for user {TelegramId} on {Date}", telegramId, date);

        var userExists = await _userRepository.ExistsByTelegramIdAsync(telegramId, cancellationToken);
        if (!userExists)
        {
            _logger.LogWarning("User with TelegramId {TelegramId} not found", telegramId);
            throw new UserNotFoundException(telegramId);
        }

        var existing = await _workoutRepository.ExistsAsync(telegramId, date, cancellationToken);
        if (existing)
        {
            _logger.LogWarning("Workout for user {TelegramId} on {Date} already exists", telegramId, date);
            throw new WorkoutAlreadyExistsException(telegramId, date);
        }

        var workout = Workout.Create(telegramId, date, exercises);
        await _workoutRepository.AddAsync(workout, cancellationToken);

        _logger.LogInformation("Workout created for user {TelegramId} on {Date}", telegramId, date);
        return workout;
    }

    /// <inheritdoc />
    public async Task<Workout> UpdateWorkoutAsync(
        long telegramId,
        DateTime date,
        IEnumerable<Exercise> exercises,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating workout for user {TelegramId} on {Date}", telegramId, date);

        var existingWorkout = await _workoutRepository.GetByIdAsync(telegramId, date, cancellationToken);
        if (existingWorkout == null)
        {
            _logger.LogWarning("Workout for user {TelegramId} on {Date} not found", telegramId, date);
            throw new WorkoutNotFoundException(telegramId, date);
        }

        var updatedWorkout = existingWorkout.UpdateExercises(exercises);

        await _workoutRepository.DeleteAsync(telegramId, date, cancellationToken);
        await _workoutRepository.AddAsync(updatedWorkout, cancellationToken);

        _logger.LogInformation("Workout updated for user {TelegramId} on {Date}", telegramId, date);
        return updatedWorkout;
    }

    /// <inheritdoc />
    public async Task DeleteWorkoutAsync(long telegramId, DateTime date, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting workout for user {TelegramId} on {Date}", telegramId, date);

        var workout = await _workoutRepository.GetByIdAsync(telegramId, date, cancellationToken);
        if (workout == null)
        {
            _logger.LogWarning("Workout for user {TelegramId} on {Date} not found", telegramId, date);
            throw new WorkoutNotFoundException(telegramId, date);
        }

        await _workoutRepository.DeleteAsync(telegramId, date, cancellationToken);
        _logger.LogInformation("Workout deleted for user {TelegramId} on {Date}", telegramId, date);
    }

    /// <inheritdoc />
    public async Task<bool> HasWorkoutTodayAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _workoutRepository.ExistsAsync(telegramId, today, cancellationToken);
    }
}