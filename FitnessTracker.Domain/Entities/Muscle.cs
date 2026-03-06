namespace FitnessTracker.Domain.Entities;

public class Muscle
{
    public int Id { get; set; }
    // All fields except Id can be null in DB
    public string? Name { get; set; }
    public float Stamina { get; set; } = 100; // Не nullable
    public float Strength { get; set; } = 100;
    public float PercentageOfRecovery { get; set; } = 100;
    public TimeSpan? RecoveryTime { get; set; }
    public long? UserId { get; set; }
    public User? User { get; set; }

    public Muscle(string name, TimeSpan? recoveryTime)
    {
        Name = name;
        RecoveryTime = recoveryTime;
        Stamina = 100;
        Strength = 100;
        PercentageOfRecovery = 100;
    }
}