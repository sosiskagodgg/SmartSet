// FitnessTracker.AI/Core/Models/ProcessingResult.cs
namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Результат обработки сообщения
/// </summary>
public record ProcessingResult
{
    /// <summary>
    /// Текст ответа пользователю
    /// </summary>
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// Отправить ли как Markdown (Telegram)
    /// </summary>
    public bool UseMarkdown { get; init; }

    /// <summary>
    /// Дополнительные данные (например, для логирования)
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Был ли использован AI или плагин
    /// </summary>
    public string Source { get; init; } = "unknown";

    public static ProcessingResult FromPlugin(PluginResult pluginResult)
    {
        return new ProcessingResult
        {
            Response = pluginResult.Message,
            UseMarkdown = pluginResult.UseMarkdown,
            Metadata = pluginResult.Metadata,
            Source = $"plugin:{pluginResult.PluginId}"
        };
    }

    public static ProcessingResult FromAi(AiResponse aiResponse)
    {
        return new ProcessingResult
        {
            Response = aiResponse.Content ?? "⚠️ AI не дал ответа.",
            UseMarkdown = true,
            Metadata = new Dictionary<string, object>
            {
                ["model"] = aiResponse.ModelUsed ?? "unknown",
                ["tokens"] = aiResponse.TokensUsed ?? 0,
                ["duration"] = aiResponse.Duration?.TotalMilliseconds ?? 0
            },
            Source = "ai"
        };
    }

    public static ProcessingResult Error(string message) =>
        new() { Response = message, Source = "error" };
}