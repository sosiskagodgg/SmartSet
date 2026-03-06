using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application.Services;

public class MuscleService : IMuscleService
{
    private readonly IMuscleRepository _muscleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MuscleService> _logger;

    public MuscleService(IMuscleRepository muscleRepository, IUserRepository userRepository, ILogger<MuscleService> logger)
    {
        _muscleRepository = muscleRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<Muscle>> GetUserMusclesAsync(long userId)
    {
        _logger.LogDebug("Loading muscles for user {UserId}", userId);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when loading muscles", userId);
            return new List<Muscle>();
        }

        var muscles = await _muscleRepository.GetMusclesByUserIdAsync(userId);
        _logger.LogInformation("Loaded {Count} muscles for user {UserId}", muscles.Count, userId);
        return muscles;
    }

    public async Task<List<Muscle>> AssignDefaultMusclesToUserAsync(long userId)
    {
        _logger.LogInformation("Assigning default muscles to user {UserId}", userId);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found when assigning default muscles", userId);
            return new List<Muscle>();
        }

        var existing = await _muscleRepository.GetMusclesByUserIdAsync(userId);
        var existingNames = new HashSet<string>(existing.Select(m => m.Name), StringComparer.OrdinalIgnoreCase);

        var templates = await _muscleRepository.GetMusclesByUserIdAsync(0);

        var added = new List<Muscle>();

        foreach (var t in templates)
        {
            if (existingNames.Contains(t.Name))
                continue;

            var newMuscle = new Muscle(t.Name, t.RecoveryTime)
            {
                Strength = t.Strength,
                Stamina = t.Stamina,
                PercentageOfRecovery = t.PercentageOfRecovery,
                UserId = userId,
                User = null
            };

            await _muscleRepository.AddAsync(newMuscle);
            added.Add(newMuscle);
        }

        if (added.Count > 0)
        {
            var saved = await _muscleRepository.SaveChangesAsync();
            if (!saved)
            {
                _logger.LogError("Failed to save assigned muscles for user {UserId}", userId);
                return new List<Muscle>();
            }
        }

        return added;
    }

    public async Task<bool> UpdateMuscleRecoveryAsync(long muscleId, double percentage)
    {
        try
        {
            _logger.LogDebug("Updating recovery for muscle {MuscleId} to {Percentage}%", muscleId, percentage);

            var muscle = await _muscleRepository.GetByIdAsync((int)muscleId); // Convert long to int
            if (muscle == null)
            {
                _logger.LogWarning("Muscle {MuscleId} not found", muscleId);
                return false;
            }

            muscle.PercentageOfRecovery = (float)percentage;

            // Используем Update вместо UpdateAsync
            var updated = await _muscleRepository.Update(muscle);
            if (!updated)
            {
                _logger.LogError("Failed to update recovery for muscle {MuscleId}", muscleId);
                return false;
            }

            _logger.LogInformation("Updated recovery for muscle {MuscleId} to {Percentage}%", muscleId, percentage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recovery for muscle {MuscleId}", muscleId);
            return false;
        }
    }

    public async Task<List<Muscle>> GetRecoveredMusclesAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting recovered muscles for user {UserId}", userId);

            var muscles = await _muscleRepository.GetMusclesByUserIdAsync(userId);
            var recovered = muscles.Where(m => m.PercentageOfRecovery >= 90).ToList();

            _logger.LogInformation("Found {Count} recovered muscles for user {UserId}", recovered.Count, userId);
            return recovered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recovered muscles for user {UserId}", userId);
            return new List<Muscle>();
        }
    }

    public async Task<List<Muscle>> GetFatiguedMusclesAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting fatigued muscles for user {UserId}", userId);

            var muscles = await _muscleRepository.GetMusclesByUserIdAsync(userId);
            var fatigued = muscles.Where(m => m.PercentageOfRecovery < 30).ToList();

            _logger.LogInformation("Found {Count} fatigued muscles for user {UserId}", fatigued.Count, userId);
            return fatigued;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fatigued muscles for user {UserId}", userId);
            return new List<Muscle>();
        }
    }

    // Изменяем возвращаемый тип на Task<bool> вместо Task
    public async Task<bool> ResetAllMusclesAsync(long userId)
    {
        try
        {
            _logger.LogInformation("Resetting all muscles for user {UserId}", userId);

            var muscles = await _muscleRepository.GetMusclesByUserIdAsync(userId);

            foreach (var muscle in muscles)
            {
                muscle.PercentageOfRecovery = 100;
                await _muscleRepository.Update(muscle); // Используем Update вместо UpdateAsync
            }

            _logger.LogInformation("Reset {Count} muscles for user {UserId}", muscles.Count, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting muscles for user {UserId}", userId);
            return false;
        }
    }
}