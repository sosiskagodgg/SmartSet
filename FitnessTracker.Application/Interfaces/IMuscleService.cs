// FitnessTracker.Application/Interfaces/IMuscleService.cs
using FitnessTracker.Domain.Entities;

namespace FitnessTracker.Application.Interfaces;

public interface IMuscleService
{
    Task<List<Muscle>> GetUserMusclesAsync(long userId);
    Task<bool> UpdateMuscleRecoveryAsync(long muscleId, double percentage);
    Task<List<Muscle>> GetRecoveredMusclesAsync(long userId);
    Task<List<Muscle>> GetFatiguedMusclesAsync(long userId);
    Task<bool> ResetAllMusclesAsync(long userId); // Вместо Task
}