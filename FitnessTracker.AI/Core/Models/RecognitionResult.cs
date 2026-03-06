namespace FitnessTracker.AI.Core.Models;

public class RecognitionResult
{
    public Intent? Intent { get; set; }
    public List<Entity> Entities { get; set; } = new();
    public string OriginalMessage { get; set; } = string.Empty;
    public double Confidence => Intent?.Confidence ?? 0;
    public Dictionary<string, object> Metadata { get; set; } = new();

    public bool IsSuccess => Intent != null && Intent.Confidence > 0.5;
    public bool HasEntities => Entities.Any();
}