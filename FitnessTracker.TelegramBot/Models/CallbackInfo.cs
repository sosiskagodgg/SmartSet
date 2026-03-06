namespace FitnessTracker.TelegramBot.Models;

public class CallbackInfo
{
    public string Prefix { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string[] Data { get; set; } = Array.Empty<string>();
    public string FullData { get; set; } = string.Empty;
}