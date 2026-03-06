using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.AI.Recognition.Recognizers;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.AI.Extensions;

public static class EntityRecognizerExtensions
{
    /// <summary>
    /// Регистрирует стандартные regex паттерны для распознавания
    /// </summary>
    public static IServiceCollection AddDefaultRegexPatterns(this IServiceCollection services)
    {
        services.AddSingleton<RegexEntityRecognizer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RegexEntityRecognizer>>();
            var recognizer = new RegexEntityRecognizer(logger);

            // Здесь можно зарегистрировать базовые паттерны,
            // но они будут перезаписываться командами
            recognizer.RegisterPattern("number", @"\b\d+(?:[.,]\d+)?\b", 0.7);

            return recognizer;
        });

        return services;
    }
}