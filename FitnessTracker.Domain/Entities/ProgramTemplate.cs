// FitnessTracker.Domain.Entities/ProgramTemplate.cs
namespace FitnessTracker.Domain.Entities;

public class ProgramTemplate
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<ProgramDay> Days { get; set; } = new List<ProgramDay>();
}