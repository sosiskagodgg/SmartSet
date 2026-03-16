// FitnessTracker.AI/Services/MessageProcessor.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Registry;

namespace FitnessTracker.AI.Services;

/// <summary>
/// Реализация основного процессора сообщений.
/// Сначала ищет подходящий плагин, если нет — отправляет запрос в AI.
/// </summary>
public class MessageProcessor : IMessageProcessor
{
    private readonly IPluginRegistry _pluginRegistry;
    private readonly IAiProvider _aiProvider;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(
        IPluginRegistry pluginRegistry,
        IAiProvider aiProvider,
        ILogger<MessageProcessor> logger)
    {
        _pluginRegistry = pluginRegistry;
        _aiProvider = aiProvider;
        _logger = logger;
    }

    public async Task<ProcessingResult> ProcessAsync(long userId, string message, CancellationToken cancellationToken = default)
    {
        // Используем AiMessageContext, а не старый MessageContext
        var context = new AiMessageContext
        {
            UserId = userId,
            Message = message,
            Metadata = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow
            }
        };

        try
        {
            // Шаг 1: ищем плагин, который может обработать сообщение
            var plugin = await _pluginRegistry.FindForMessageAsync(context, cancellationToken);
            if (plugin != null)
            {
                _logger.LogInformation("Message handled by plugin {PluginId}", plugin.Id);
                var pluginResult = await plugin.ExecuteAsync(context, cancellationToken);
                return ProcessingResult.FromPlugin(pluginResult);
            }

            // Шаг 2: если плагинов нет, отправляем в AI
            _logger.LogInformation("No plugin found, falling back to AI");
            var aiResponse = await _aiProvider.AskAsync(
                message,
                new AiOptions { SystemPrompt = "Ты — дружелюбный фитнес-ассистент. Отвечай коротко и по существу." },
                cancellationToken
            );

            return ProcessingResult.FromAi(aiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from user {UserId}", userId);
            return ProcessingResult.Error("Произошла внутренняя ошибка. Попробуйте позже.");
        }
    }
}