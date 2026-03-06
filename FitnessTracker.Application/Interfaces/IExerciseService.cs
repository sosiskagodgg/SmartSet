using FitnessTracker.Application.DTOs;
using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Application.Interfaces;

public interface IExerciseService
{
    Task<List<Exercise>> GetAllBaseExercisesAsync();
    Task<List<Exercise>> GetUserCustomExercisesAsync(long userId);
    Task<Exercise?> GetExerciseByIdAsync(int id);
    Task<Exercise?> GetExerciseByNameAsync(string name, long? userId = null);
    Task<Exercise?> CreateCustomExerciseAsync(long userId, string name, string? description, ExerciseCategory category);
    Task<List<Exercise>> GetExercisesByMuscleAsync(int muscleId);
    Task<bool> DeleteCustomExerciseAsync(int exerciseId, long userId);
}