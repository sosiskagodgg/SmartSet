namespace FitnessTracker.AI.Configuration;

public class RecognitionConfig
{
    public bool UseGigaChat { get; set; } = true;
    public bool UseRegex { get; set; } = true;
    public double DefaultConfidenceThreshold { get; set; } = 0.6;
    public int MaxRecognitionAttempts { get; set; } = 3;
    public Dictionary<string, List<string>> EntityPatterns { get; set; } = new();
}