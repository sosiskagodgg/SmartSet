namespace FitnessTracker.AI.Core.Models;

public class CommandContext
{
    public string OriginalMessage { get; init; } = string.Empty;
    public Intent? Intent { get; init; }
    public List<Entity> Entities { get; init; } = new();
    public long UserId { get; init; }
    public Dictionary<string, object> SessionData { get; init; } = new();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public T? GetEntityValue<T>(string entityType)
    {
        var entity = Entities.FirstOrDefault(e => e.Type.Equals(entityType, StringComparison.OrdinalIgnoreCase));
        return entity != null ? entity.GetValue<T>() : default;
    }

    public bool HasEntity(string entityType)
    {
        return Entities.Any(e => e.Type.Equals(entityType, StringComparison.OrdinalIgnoreCase));
    }

    public List<Entity> GetEntitiesByType(string entityType)
    {
        return Entities.Where(e => e.Type.Equals(entityType, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}