namespace FitnessTracker.Domain.Entities;
public class WorkoutTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // beginner, intermediate, advanced
    public int DaysPerWeek { get; set; }
    public string Goal { get; set; } = string.Empty; // strength, hypertrophy, etc.
    public List<TemplateDay> Days { get; set; } = new();
}

public class TemplateDay
{
    public int DayNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TemplateExercise> Exercises { get; set; } = new();
}

public class TemplateExercise
{
    public string Name { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int RepsMin { get; set; }
    public int RepsMax { get; set; }
    public bool IsTimed { get; set; } = false;
}