namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Представляет тренировку пользователя
/// </summary>
public class UserWorkout
{

    public long TelegramId { get; set; }

    public int DayNumber { get; set; }

    public string Name { get; set; }

    public List<Exercise> Exercises { get; set; } = new();
}