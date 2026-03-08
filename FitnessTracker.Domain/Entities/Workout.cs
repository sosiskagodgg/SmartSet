namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Представляет тренировку пользователя
/// </summary>
public class Workout
{

    public long TelegramId { get; set; }

    public DateTime Date { get; set; }

    public List<Exercise> Exercises { get; set; } = new();
}