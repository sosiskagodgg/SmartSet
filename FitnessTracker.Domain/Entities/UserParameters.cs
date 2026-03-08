// FitnessTracker.Domain/Entities/UserParameters.cs
namespace FitnessTracker.Domain.Entities;

/// <summary>
/// Параметры пользователя
/// </summary>
public class UserParameters
{
    public long TelegramId { get; set; } // Primary Key
    public int? Height { get; set; } // рост в см
    public decimal? Weight { get; set; } // вес в кг
    public decimal? BodyFat { get; set; } // процент жира
    public string? Experience { get; set; } // опыт: beginner/intermediate/advanced
    public string? Goals { get; set; } // цели в свободном формате
}