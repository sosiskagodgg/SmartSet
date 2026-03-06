// FitnessTracker.Domain.Entities/ProgramDay.cs
namespace FitnessTracker.Domain.Entities;

public class ProgramDay
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public ProgramTemplate? Program { get; set; }
    public int DayNumber { get; set; } // 1-7
    public string? Name { get; set; } // "День ног", "Отдых" и т.д.
    public bool IsRestDay { get; set; }

    public ICollection<ProgramDayExercise> Exercises { get; set; } = new List<ProgramDayExercise>();
}