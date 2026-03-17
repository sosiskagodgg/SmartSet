// FitnessTracker.Domain/Entities/UserWorkout.cs
using FitnessTracker.Domain.Common;
using FitnessTracker.Domain.Exceptions;
using FitnessTracker.Domain.Entities.Exercises;
using System.Text.Json;

namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Тренировка пользователя (шаблон).
/// Хранит запланированные упражнения по дням программы.
/// </summary>
public class UserWorkout : AggregateRoot<(long TelegramId, int DayNumber)>
{
    // Поля для EF Core
    private long _telegramId;
    private int _dayNumber;
    private string _exercisesJson = "[]";

    // Публичные свойства
    public long TelegramId
    {
        get => _telegramId;
        private set => _telegramId = value;
    }

    public int DayNumber
    {
        get => _dayNumber;
        private set => _dayNumber = value;
    }

    public string Name { get; private set; }

    // JSON поле для хранения
    public string ExercisesJson
    {
        get => _exercisesJson;
        private set => _exercisesJson = value ?? "[]";
    }

    // Не маппится в БД
    private List<Exercise> _exercises = new();
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    // Для EF Core
    private UserWorkout() : base()
    {
        Name = string.Empty;
    }

    private UserWorkout(long telegramId, int dayNumber, string name, IEnumerable<Exercise> exercises)
        : base((telegramId, dayNumber))
    {
        _telegramId = telegramId;
        _dayNumber = dayNumber;
        Name = name;
        SetExercises(exercises?.ToList() ?? new List<Exercise>());
    }

    public static UserWorkout Create(long telegramId, int dayNumber, string name, IEnumerable<Exercise> exercises)
    {
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId must be positive", nameof(telegramId));

        if (dayNumber <= 0)
            throw new InvalidDayNumberException(dayNumber);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workout name cannot be empty", nameof(name));

        return new UserWorkout(telegramId, dayNumber, name.Trim(), exercises ?? Array.Empty<Exercise>());
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Workout name cannot be empty", nameof(newName));
        Name = newName.Trim();
        UpdateJson();
    }

    public void UpdateExercises(IEnumerable<Exercise> newExercises)
    {
        _exercises.Clear();
        _exercises.AddRange(newExercises ?? Array.Empty<Exercise>());
        UpdateJson();
    }

    public void AddExercise(Exercise exercise)
    {
        if (exercise == null)
            throw new ArgumentNullException(nameof(exercise));
        _exercises.Add(exercise);
        UpdateJson();
    }

    public void RemoveExercise(Guid exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new UserWorkoutNotFoundException(TelegramId, DayNumber);
        _exercises.Remove(exercise);
        UpdateJson();
    }

    // Внутренний метод для установки упражнений из репозитория
    internal void SetExercises(List<Exercise> exercises)
    {
        _exercises = exercises ?? new List<Exercise>();
        UpdateJson();
    }

    private void UpdateJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new ExerciseJsonConverter() }
        };
        ExercisesJson = JsonSerializer.Serialize(_exercises, options);
    }

    public bool HasExercises => _exercises.Any();
    public int ExerciseCount => _exercises.Count;
}