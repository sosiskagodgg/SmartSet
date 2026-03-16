// FitnessTracker.AI/Core/Models/AiResponse.cs
namespace FitnessTracker.AI.Core.Models;

/// <summary>
/// Ответ от AI провайдера (текстовый)
/// </summary>
public record AiResponse
{
    /// <summary>
    /// Успешен ли запрос
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Текст ответа
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Ошибка, если есть
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Время выполнения
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Использованная модель
    /// </summary>
    public string? ModelUsed { get; init; }

    /// <summary>
    /// Потрачено токенов (если провайдер отдаёт)
    /// </summary>
    public int? TokensUsed { get; init; }

    public static AiResponse Success(string content, string? model = null, int? tokens = null, TimeSpan? duration = null) =>
        new() { IsSuccess = true, Content = content, ModelUsed = model, TokensUsed = tokens, Duration = duration };

    public static AiResponse Failure(string error, TimeSpan? duration = null) =>
        new() { IsSuccess = false, Error = error, Duration = duration };
}

/// <summary>
/// Ответ от AI провайдера со структурированными данными
/// </summary>
public record AiResponse<T> : AiResponse where T : class
{
    /// <summary>
    /// Десериализованные данные
    /// </summary>
    public T? Data { get; init; }

    public static AiResponse<T> Success(T data, string content, string? model = null, int? tokens = null, TimeSpan? duration = null) =>
        new() { IsSuccess = true, Data = data, Content = content, ModelUsed = model, TokensUsed = tokens, Duration = duration };

    public new static AiResponse<T> Failure(string error, TimeSpan? duration = null) =>
        new() { IsSuccess = false, Error = error, Duration = duration };
}