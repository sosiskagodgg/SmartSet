using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseLibraryRepository _exerciseRepository;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(
        IExerciseLibraryRepository exerciseRepository,
        ILogger<ExerciseService> logger)
    {
        _exerciseRepository = exerciseRepository;
        _logger = logger;
    }

    public async Task<List<Exercise>> GetAllBaseExercisesAsync()
    {
        try
        {
            _logger.LogDebug("Getting all base exercises");
            return await _exerciseRepository.GetAllBaseExercisesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting base exercises");
            throw;
        }
    }

    public async Task<List<Exercise>> GetUserCustomExercisesAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting custom exercises for user {UserId}", userId);
            return await _exerciseRepository.GetUserCustomExercisesAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom exercises for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting exercise by id {Id}", id);
            return await _exerciseRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise by id {Id}", id);
            throw;
        }
    }

    public async Task<Exercise?> GetExerciseByNameAsync(string name, long? userId = null)
    {
        try
        {
            _logger.LogDebug("Getting exercise by name {Name}", name);
            return await _exerciseRepository.GetByNameAsync(name, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercise by name {Name}", name);
            throw;
        }
    }

    public async Task<Exercise?> CreateCustomExerciseAsync(long userId, string name, string? description, ExerciseCategory category)
    {
        try
        {
            _logger.LogInformation("Creating custom exercise {Name} for user {UserId}", name, userId);

            var existing = await _exerciseRepository.GetByNameAsync(name, userId);
            if (existing != null)
            {
                _logger.LogWarning("Exercise {Name} already exists for user {UserId}", name, userId);
                return null;
            }

            var exercise = new Exercise
            {
                Name = name,
                Description = description,
                Category = category,
                IsCustom = true,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _exerciseRepository.AddAsync(exercise);
            return result ? exercise : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating custom exercise for user {UserId}", userId);
            return null;
        }
    }

    public async Task<List<Exercise>> GetExercisesByMuscleAsync(int muscleId)
    {
        try
        {
            _logger.LogDebug("Getting exercises for muscle {MuscleId}", muscleId);
            var all = await _exerciseRepository.GetAllBaseExercisesAsync();
            return all.Where(e => e.ExerciseMuscles?.Any(em => em.MuscleId == muscleId) == true).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exercises for muscle {MuscleId}", muscleId);
            throw;
        }
    }

    public async Task<bool> DeleteCustomExerciseAsync(int exerciseId, long userId)
    {
        try
        {
            _logger.LogInformation("Deleting custom exercise {ExerciseId} for user {UserId}", exerciseId, userId);
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);

            if (exercise == null || !exercise.IsCustom || exercise.UserId != userId)
            {
                _logger.LogWarning("Exercise {ExerciseId} not found or not owned by user {UserId}", exerciseId, userId);
                return false;
            }

            return await _exerciseRepository.DeleteAsync(exercise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom exercise {ExerciseId}", exerciseId);
            return false;
        }
    }
}