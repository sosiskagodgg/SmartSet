using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class SetService : ISetService
{
    private readonly IExerciseSetRepository _setRepository;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepository;
    private readonly ILogger<SetService> _logger;

    public SetService(
        IExerciseSetRepository setRepository,
        IWorkoutExerciseRepository workoutExerciseRepository,
        ILogger<SetService> logger)
    {
        _setRepository = setRepository;
        _workoutExerciseRepository = workoutExerciseRepository;
        _logger = logger;
    }

    public async Task<ExerciseSet?> LogSetAsync(int workoutExerciseId, int setNumber, int? reps, decimal? weight, int? durationSeconds, decimal? distanceMeters)
    {
        try
        {
            _logger.LogInformation("Logging set for workout exercise {WorkoutExerciseId}", workoutExerciseId);

            var workoutExercise = await _workoutExerciseRepository.GetByIdAsync(workoutExerciseId);
            if (workoutExercise == null)
            {
                _logger.LogWarning("Workout exercise {WorkoutExerciseId} not found", workoutExerciseId);
                return null;
            }

            var set = new ExerciseSet
            {
                WorkoutExerciseId = workoutExerciseId,
                SetNumber = setNumber,
                Reps = reps,
                Weight = weight,
                DurationSeconds = durationSeconds,
                DistanceMeters = distanceMeters,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };

            var result = await _setRepository.AddAsync(set);
            return result ? set : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging set for workout exercise {WorkoutExerciseId}", workoutExerciseId);
            return null;
        }
    }

    public async Task<ExerciseSet?> UpdateSetAsync(int setId, int? reps, decimal? weight, int? durationSeconds, decimal? distanceMeters)
    {
        try
        {
            _logger.LogInformation("Updating set {SetId}", setId);

            var set = await _setRepository.GetByIdAsync(setId);
            if (set == null)
            {
                _logger.LogWarning("Set {SetId} not found", setId);
                return null;
            }

            if (reps.HasValue) set.Reps = reps;
            if (weight.HasValue) set.Weight = weight;
            if (durationSeconds.HasValue) set.DurationSeconds = durationSeconds;
            if (distanceMeters.HasValue) set.DistanceMeters = distanceMeters;

            var result = await _setRepository.UpdateAsync(set);
            return result ? set : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating set {SetId}", setId);
            return null;
        }
    }

    public async Task<bool> DeleteSetAsync(int setId)
    {
        try
        {
            _logger.LogInformation("Deleting set {SetId}", setId);
            return await _setRepository.DeleteAsync(setId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting set {SetId}", setId);
            return false;
        }
    }

    public async Task<List<ExerciseSet>> GetSetsByWorkoutExerciseAsync(int workoutExerciseId)
    {
        try
        {
            _logger.LogDebug("Getting sets for workout exercise {WorkoutExerciseId}", workoutExerciseId);
            return await _setRepository.GetByWorkoutExerciseIdAsync(workoutExerciseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sets for workout exercise {WorkoutExerciseId}", workoutExerciseId);
            throw;
        }
    }

    public async Task<List<ExerciseSet>> GetUserSetsAsync(long userId, int? exerciseId = null, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            _logger.LogDebug("Getting sets for user {UserId}", userId);
            return await _setRepository.GetUserSetsAsync(userId, exerciseId, from, to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sets for user {UserId}", userId);
            throw;
        }
    }
}