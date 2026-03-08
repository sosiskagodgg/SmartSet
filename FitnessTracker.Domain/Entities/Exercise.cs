namespace FitnessTracker.Domain.Entities;

public class Exercise
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public float MET {  get; set; }

    public Exercise(string name, float MET)
    {
        Name = name;
        this. MET = MET;
    }
}
