namespace FitnessTracker.AI.Configuration;

public class GigaChatConfig
{
    public string AuthorizationKey { get; set; } = string.Empty;
    public string Scope { get; set; } = "GIGACHAT_API_PERS";
    public string Model { get; set; } = "GigaChat:latest";
    public double Temperature { get; set; } = 0.3;
    public int MaxTokens { get; set; } = 500;
}