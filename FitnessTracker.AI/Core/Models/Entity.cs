namespace FitnessTracker.AI.Core.Models;

public class Entity
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
    public Dictionary<string, object> Metadata { get; set; } = new();

    public T? GetValue<T>()
    {
        try
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch
        {
            return default;
        }
    }
}