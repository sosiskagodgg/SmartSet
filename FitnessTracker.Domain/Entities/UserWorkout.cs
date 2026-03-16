// FitnessTracker.Domain/Entities/UserWorkout.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Exceptions;
using FitnessTracker.Domain.Entities.Exercises;

namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Тренировка пользователя (шаблон).
/// Хранит запланированные упражнения по дням программы.
/// </summary>
public class UserWorkout : AggregateRoot<Guid>
{
    public long TelegramId { get; private set; }
    public int DayNumber { get; private set; }
    public string Name { get; private set; }

    // ИСПРАВЛЕНО: используем List<Exercise> вместо IReadOnlyCollection для JSON сериализации
    public List<Exercise> Exercises { get; private set; } = new();

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModified { get; private set; }

    // Для EF Core
    private UserWorkout() : base(Guid.NewGuid())
    {
        Name = string.Empty;
    }

    private UserWorkout(Guid id, long telegramId, int dayNumber, string name, IEnumerable<Exercise> exercises)
        : base(id)
    {
        TelegramId = telegramId;
        DayNumber = dayNumber;
        Name = name;
        Exercises = exercises.ToList();
        CreatedAt = DateTime.UtcNow;
    }

    public static UserWorkout Create(long telegramId, int dayNumber, string name, IEnumerable<Exercise> exercises)
    {
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId must be positive", nameof(telegramId));

        if (dayNumber <= 0)
            throw new InvalidDayNumberException(dayNumber);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workout name cannot be empty", nameof(name));

        return new UserWorkout(
            Guid.NewGuid(),
            telegramId,
            dayNumber,
            name.Trim(),
            exercises ?? Array.Empty<Exercise>());
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Workout name cannot be empty", nameof(newName));

        Name = newName.Trim();
        LastModified = DateTime.UtcNow;
    }

    public void UpdateExercises(IEnumerable<Exercise> newExercises)
    {
        Exercises.Clear();
        Exercises.AddRange(newExercises ?? Array.Empty<Exercise>());
        LastModified = DateTime.UtcNow;
    }

    public void AddExercise(Exercise exercise)
    {
        if (exercise == null)
            throw new ArgumentNullException(nameof(exercise));

        Exercises.Add(exercise);
        LastModified = DateTime.UtcNow;
    }

    public void RemoveExercise(Guid exerciseId)
    {
        var exercise = Exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new UserWorkoutNotFoundException(TelegramId, DayNumber);

        Exercises.Remove(exercise);
        LastModified = DateTime.UtcNow;
    }

    public bool HasExercises => Exercises.Any();
    public int ExerciseCount => Exercises.Count;
}