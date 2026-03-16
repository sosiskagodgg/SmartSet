// FitnessTracker.Domain/Entities/Workout.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Exceptions;
using FitnessTracker.Domain.Entities.Exercises;

namespace FitnessTracker.Domain.Entities;

public static class WorkoutStatus
{
    public const string Planned = "planned";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
    public const string InProgress = "inprogress";

    public static readonly string[] All = { Planned, Completed, Cancelled, InProgress };
    public static bool IsValid(string status) => All.Contains(status);
}

/// <summary>
/// Ежедневная тренировка.
/// </summary>
public class Workout : Entity<Guid>  // ← Изменили на Guid
{
    // Явные свойства, соответствующие SQL
    public long TelegramId { get; private set; }
    public DateTime Date { get; private set; }

    private readonly List<Exercise> _exercises = new();
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    public string? Notes { get; private set; }
    public string Status { get; private set; }
    public TimeSpan? TotalDuration { get; private set; }
    public int? TotalCaloriesBurned { get; private set; }

    // Для EF Core
    private Workout() : base(Guid.NewGuid())
    {
        Status = WorkoutStatus.Planned;
    }

    private Workout(long telegramId, DateTime date, IEnumerable<Exercise> exercises)
        : base(Guid.NewGuid())
    {
        TelegramId = telegramId;
        Date = date;
        _exercises.AddRange(exercises);
        Status = WorkoutStatus.Planned;
        RecalculateMetrics();
    }

    public static Workout Create(long telegramId, DateTime date, IEnumerable<Exercise> exercises)
    {
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId must be positive", nameof(telegramId));

        if (date.Date < DateTime.UtcNow.Date)
            throw new WorkoutInPastException(date);

        return new Workout(telegramId, date, exercises ?? Array.Empty<Exercise>());
    }

    public Workout UpdateExercises(IEnumerable<Exercise> newExercises)
    {
        var updated = new Workout(TelegramId, Date, newExercises ?? Array.Empty<Exercise>());
        updated.Notes = Notes;
        updated.Status = Status;
        return updated;
    }

    public void Complete(string? notes = null)
    {
        if (!_exercises.Any())
        {
            throw new WorkoutNotFoundException(TelegramId, Date);
        }

        Status = WorkoutStatus.Completed;
        Notes = notes;
    }

    public void Cancel(string reason)
    {
        Status = WorkoutStatus.Cancelled;
        Notes = reason;
    }

    public bool HasExercises => _exercises.Any();
    public int ExerciseCount => _exercises.Count;

    private void RecalculateMetrics()
    {
        TotalDuration = GetTotalDuration();
        TotalCaloriesBurned = GetEstimatedCalories();
    }

    private TimeSpan GetTotalDuration()
    {
        var totalMinutes = _exercises
            .OfType<CardioExercise>()
            .Sum(e => e.DurationMinutes);

        return TimeSpan.FromMinutes(totalMinutes);
    }

    private int GetEstimatedCalories()
    {
        return (int)_exercises
            .OfType<CardioExercise>()
            .Sum(e => e.MET * e.DurationMinutes * 3.5 / 200 * 60);
    }
}