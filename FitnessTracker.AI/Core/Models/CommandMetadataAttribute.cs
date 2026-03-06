namespace FitnessTracker.AI.Core.Models;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandMetadataAttribute : Attribute
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public double ConfidenceThreshold { get; set; } = 0.7;
    public string[] Aliases { get; set; } = Array.Empty<string>();
}