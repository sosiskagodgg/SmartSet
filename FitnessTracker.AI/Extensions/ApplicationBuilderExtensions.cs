using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Recognition.Recognizers;

namespace FitnessTracker.AI.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Инициализирует AI компоненты при старте приложения
    /// </summary>
    public static async Task UseFitnessTrackerAIAsync(this IServiceProvider serviceProvider)
    {
        try
        {
            // Создаем временный логгер без generic параметра
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FitnessTracker.AI.Initialization");

            // Проверяем наличие команд
            var registry = serviceProvider.GetRequiredService<ICommandRegistry>();
            var commands = registry.GetAllCommands().ToList();

            logger.LogInformation("FitnessTracker.AI initialized with {CommandCount} commands", commands.Count);

            // Регистрируем базовые regex паттерны если нужно
            var regexRecognizer = serviceProvider.GetService<RegexEntityRecognizer>();
            if (regexRecognizer != null)
            {
                // Можно добавить базовые паттерны
                logger.LogDebug("RegexEntityRecognizer configured");
            }
        }
        catch (Exception ex)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FitnessTracker.AI.Initialization");
            logger.LogError(ex, "Error initializing FitnessTracker.AI");
        }
    }
}