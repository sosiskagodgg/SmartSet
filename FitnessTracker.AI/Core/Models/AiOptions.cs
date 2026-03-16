// FitnessTracker.AI/Core/Models/AiOptions.cs
namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Опции для запроса к AI провайдеру
/// </summary>
public record AiOptions
{
    /// <summary>
    /// Название модели (зависит от провайдера)
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Температура (креативность) ответа 0.0-1.0
    /// </summary>
    public double Temperature { get; init; } = 0.3;

    /// <summary>
    /// Максимальное количество токенов в ответе
    /// </summary>
    public int MaxTokens { get; init; } = 1000;

    /// <summary>
    /// Системный промпт (инструкция для AI)
    /// </summary>
    public string? SystemPrompt { get; init; }

    public static AiOptions Default => new();
}