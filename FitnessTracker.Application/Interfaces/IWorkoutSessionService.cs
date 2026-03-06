using FitnessTracker.Application.DTOs;
using FitnessTracker.Domain.Entities;
namespace FitnessTracker.Application.Interfaces;

public interface IWorkoutSessionService
{
    Task<WorkoutSessionDto> GetCurrentSessionAsync(long userId);
    Task<WorkoutExercise?> GetCurrentExerciseAsync(long workoutId);
    Task<int> GetNextSetNumberAsync(long workoutExerciseId);
    Task<WorkoutSummaryDto> GetWorkoutSummaryAsync(long workoutId);
}