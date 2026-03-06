using Microsoft.Extensions.Logging;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.DTOs;

namespace FitnessTracker.Application.Services;

public class WorkoutSessionService : IWorkoutSessionService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepository;
    private readonly IExerciseSetRepository _setRepository;
    private readonly ILogger<WorkoutSessionService> _logger;

    public WorkoutSessionService(
        IWorkoutRepository workoutRepository,
        IWorkoutExerciseRepository workoutExerciseRepository,
        IExerciseSetRepository setRepository,
        ILogger<WorkoutSessionService> logger)
    {
        _workoutRepository = workoutRepository;
        _workoutExerciseRepository = workoutExerciseRepository;
        _setRepository = setRepository;
        _logger = logger;
    }

    public async Task<WorkoutSessionDto> GetCurrentSessionAsync(long userId)
    {
        try
        {
            _logger.LogDebug("Getting current session for user {UserId}", userId);

            var workout = await _workoutRepository.GetCurrentWorkoutAsync(userId);
            if (workout == null)
            {
                return new WorkoutSessionDto(); // Пустая сессия
            }

            var exercises = await _workoutExerciseRepository.GetByWorkoutIdAsync(workout.Id);

            var totalSets = exercises.Sum(e => e.Sets?.Count ?? 0);
            var completedSets = exercises.Sum(e => e.Sets?.Count(s => s.IsCompleted) ?? 0);
            var completedExercises = exercises.Count(e => e.Sets?.Any(s => s.IsCompleted) == true);

            var currentExercise = exercises
                .Where(e => e.Sets?.Any(s => !s.IsCompleted) == true)
                .OrderBy(e => e.Order)
                .FirstOrDefault();

            var session = new WorkoutSessionDto
            {
                WorkoutId = workout.Id,
                StartedAt = workout.StartedAt,
                TotalExercises = exercises.Count,
                CompletedExercises = completedExercises,
                TotalSets = totalSets,
                CompletedSets = completedSets,
                Exercises = exercises.Select(MapToDto).ToList()
            };

            if (currentExercise != null)
            {
                session.CurrentExercise = MapToDto(currentExercise);
            }

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WorkoutExercise?> GetCurrentExerciseAsync(long workoutId)
    {
        try
        {
            _logger.LogDebug("Getting current exercise for workout {WorkoutId}", workoutId);

            var exercises = await _workoutExerciseRepository.GetByWorkoutIdAsync(workoutId);

            return exercises
                .Where(e => e.Sets?.Any(s => !s.IsCompleted) == true)
                .OrderBy(e => e.Order)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current exercise for workout {WorkoutId}", workoutId);
            throw;
        }
    }

    public async Task<int> GetNextSetNumberAsync(long workoutExerciseId)
    {
        try
        {
            _logger.LogDebug("Getting next set number for workout exercise {WorkoutExerciseId}", workoutExerciseId);

            var sets = await _setRepository.GetByWorkoutExerciseIdAsync(workoutExerciseId);
            return (sets.Max(s => (int?)s.SetNumber) ?? 0) + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next set number for workout exercise {WorkoutExerciseId}", workoutExerciseId);
            throw;
        }
    }

    public async Task<WorkoutSummaryDto> GetWorkoutSummaryAsync(long workoutId)
    {
        try
        {
            _logger.LogDebug("Getting summary for workout {WorkoutId}", workoutId);

            var workout = await _workoutRepository.GetByIdAsync(workoutId);
            if (workout == null)
            {
                throw new ArgumentException($"Workout {workoutId} not found");
            }

            var exercises = await _workoutExerciseRepository.GetByWorkoutIdAsync(workoutId);

            var duration = workout.EndedAt.HasValue
                ? (workout.EndedAt.Value - workout.StartedAt).Minutes
                : (DateTime.UtcNow - workout.StartedAt).Minutes;

            var summary = new WorkoutSummaryDto
            {
                WorkoutId = workout.Id,
                StartedAt = workout.StartedAt,
                EndedAt = workout.EndedAt,
                DurationMinutes = duration,
                TotalExercises = exercises.Count,
                TotalSets = exercises.Sum(e => e.Sets?.Count ?? 0),
                TotalVolume = exercises.Sum(e => e.Sets?
                    .Where(s => s.Weight.HasValue && s.Reps.HasValue)
                    .Sum(s => (double)(s.Weight!.Value * s.Reps!.Value)) ?? 0),
                Exercises = new List<ExerciseSummaryDto>()
            };

            foreach (var ex in exercises)
            {
                summary.Exercises.Add(new ExerciseSummaryDto
                {
                    ExerciseName = ex.Exercise?.Name ?? "Unknown",
                    Sets = ex.Sets?.Count ?? 0,
                    TotalReps = ex.Sets?.Sum(s => s.Reps ?? 0) ?? 0,
                    MaxWeight = (double)(ex.Sets?.Where(s => s.Weight.HasValue)
                        .Max(s => (decimal?)s.Weight) ?? 0),
                    Volume = (double)(ex.Sets?
                        .Where(s => s.Weight.HasValue && s.Reps.HasValue)
                        .Sum(s => s.Weight!.Value * s.Reps!.Value) ?? 0)
                });
            }

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workout summary for workout {WorkoutId}", workoutId);
            throw;
        }
    }

    private WorkoutExerciseDto MapToDto(WorkoutExercise exercise)
    {
        return new WorkoutExerciseDto
        {
            WorkoutExerciseId = exercise.Id,
            ExerciseName = exercise.Exercise?.Name ?? "Unknown",
            Order = exercise.Order,
            CompletedSets = exercise.Sets?.Count(s => s.IsCompleted) ?? 0,
            TotalSets = exercise.Sets?.Count ?? 0,
            Sets = exercise.Sets?.Select(s => new SetDto
            {
                SetId = s.Id,
                SetNumber = s.SetNumber,
                Reps = s.Reps,
                Weight = s.Weight,
                IsCompleted = s.IsCompleted
            }).OrderBy(s => s.SetNumber).ToList() ?? new()
        };
    }
}