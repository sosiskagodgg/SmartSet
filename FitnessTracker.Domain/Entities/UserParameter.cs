// FitnessTracker.Domain/Entities/UserParameter.cs
namespace FitnessTracker.Domain.Entities;

public class UserParameter
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }

    // Физические параметры
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; } // male, female, other

    // Уровни
    public string? ActivityLevel { get; set; } // sedentary, light, moderate, very, extra
    public string? ExperienceLevel { get; set; } // beginner, intermediate, advanced

    // Цели
    public string[]? FitnessGoals { get; set; }

    // Метаданные
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool IsCurrent { get; set; }
}