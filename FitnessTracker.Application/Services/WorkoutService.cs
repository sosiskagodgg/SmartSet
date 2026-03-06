using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepository;
    private readonly IExerciseSetRepository _setRepository;
    private readonly IExerciseLibraryRepository _exerciseRepository;
    private readonly ILogger<WorkoutService> _logger;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IWorkoutExerciseRepository workoutExerciseRepository,
        IExerciseSetRepository setRepository,
        IExerciseLibraryRepository exerciseRepository,
        ILogger<WorkoutService> logger)
    {
        _workoutRepository = workoutRepository;
        _workoutExerciseRepository = workoutExerciseRepository;
        _setRepository = setRepository;
        _exerciseRepository = exerciseRepository;
        _logger = logger;
    }

    public async Task<Workout?> StartWorkoutAsync(long userId, int? programDayId = null)
    {
        try
        {
            _logger.LogInformation("Starting workout for user {UserId}", userId);

            // Проверяем, нет ли уже активной тренировки
            var current = await _workoutRepository.GetCurrentWorkoutAsync(userId);
            if (current != null)
            {
                _logger.LogWarning("User {UserId} already has active workout {WorkoutId}", userId, current.Id);
                return current;
            }

            var workout = new Workout
            {
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Status = WorkoutStatus.Active,
                ProgramDayId = programDayId
            };

            var result = await _workoutRepository.AddAsync(workout);
            return result ? workout : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting workout for user {UserId}", userId);
            return null;
        }
    }

    public async Task<Workout?> EndWorkoutAsync(int workoutId)
    {
        try
        {
            _logger.LogInformation("Ending workout {WorkoutId}", workoutId);
            var workout = await _workoutRepository.GetByIdAsync(workoutId);

            if (workout == null)
            {
                _logger.LogWarning("Workout {WorkoutId} not found", workoutId);
                return null;
            }

            workout.EndedAt = DateTime.UtcNow;
            workout.Status = WorkoutStatus.Completed;

            var result = await _workoutRepository.UpdateAsync(workout);
            return result ? workout : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending workout {WorkoutId}", workoutId);
            return null;
        }
    }

    public async Task<Workout?> GetCurrentWorkoutAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting current workout for user {UserId}", userId);
            return await _workoutRepository.GetCurrentWorkoutAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current workout for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Workout>> GetWorkoutHistoryAsync(long userId, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            _logger.LogDebug("Getting workout history for user {UserId}", userId);
            return await _workoutRepository.GetUserWorkoutsAsync(userId, from, to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout history for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Workout?> GetWorkoutByIdAsync(int workoutId)
    {
        try
        {
            _logger.LogDebug("Getting workout by id {WorkoutId}", workoutId);
            return await _workoutRepository.GetByIdAsync(workoutId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout by id {WorkoutId}", workoutId);
            throw;
        }
    }

    public async Task<WorkoutExercise?> AddExerciseToWorkoutAsync(int workoutId, int exerciseId, int order)
    {
        try
        {
            _logger.LogInformation("Adding exercise {ExerciseId} to workout {WorkoutId}", exerciseId, workoutId);

            var workout = await _workoutRepository.GetByIdAsync(workoutId);
            if (workout == null)
            {
                _logger.LogWarning("Workout {WorkoutId} not found", workoutId);
                return null;
            }

            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
            if (exercise == null)
            {
                _logger.LogWarning("Exercise {ExerciseId} not found", exerciseId);
                return null;
            }

            var workoutExercise = new WorkoutExercise
            {
                WorkoutId = workoutId,
                ExerciseId = exerciseId,
                Order = order
            };

            var result = await _workoutExerciseRepository.AddAsync(workoutExercise);
            return result ? workoutExercise : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding exercise to workout {WorkoutId}", workoutId);
            return null;
        }
    }
}