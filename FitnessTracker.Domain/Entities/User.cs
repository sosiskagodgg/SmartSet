namespace FitnessTracker.Domain.Entities;

public class User
{
    public long Id { get; set; }              // Это Telegram ID
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    public ICollection<Muscle> Muscles { get; set; } = new List<Muscle>();
}