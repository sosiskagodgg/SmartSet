namespace FitnessTracker.AI.Core.Models;

public class EntityDefinition
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public string? DefaultValue { get; set; }
    public List<string>? PossibleValues { get; set; }
    public string? ValidationPattern { get; set; } // Regex для валидации
}