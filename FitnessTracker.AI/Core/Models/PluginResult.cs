// FitnessTracker.AI/Core/Models/PluginResult.cs
namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Результат выполнения плагина
/// </summary>
public record PluginResult
{
    /// <summary>
    /// ID плагина, который выполнился
    /// </summary>
    public string PluginId { get; init; } = string.Empty;

    /// <summary>
    /// Сообщение для пользователя
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Использовать ли Markdown в Telegram
    /// </summary>
    public bool UseMarkdown { get; init; }

    /// <summary>
    /// Дополнительные данные
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    public static PluginResult Success(string pluginId, string message, bool useMarkdown = false)
    {
        return new PluginResult
        {
            PluginId = pluginId,
            Message = message,
            UseMarkdown = useMarkdown
        };
    }

    public static PluginResult Error(string pluginId, string error)
    {
        return new PluginResult
        {
            PluginId = pluginId,
            Message = $"❌ {error}",
            UseMarkdown = false,
            Metadata = { ["error"] = error }
        };
    }
}