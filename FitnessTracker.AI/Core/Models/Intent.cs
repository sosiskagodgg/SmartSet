namespace FitnessTracker.AI.Core.Models;

public enum IntentType
{
    Unknown,    // Неизвестная команда
    Help,       // Справка
    Cancel,     // Отмена
    Custom      // Кастомная команда (для плагинов)
}

public class Intent
{
    public IntentType Type { get; set; }
    public string? CustomIntentName { get; set; } // Для кастомных команд
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public Intent(IntentType type, double confidence)
    {
        Type = type;
        Confidence = confidence;
    }

    public bool IsCustom => Type == IntentType.Custom && !string.IsNullOrEmpty(CustomIntentName);
}