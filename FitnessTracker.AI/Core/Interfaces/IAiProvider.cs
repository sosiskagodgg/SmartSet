// FitnessTracker.AI/Core/Interfaces/IAiProvider.cs
using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Interfaces;

/// <summary>
/// Провайдер AI (абстракция над конкретной нейросетью: GigaChat, YandexGPT, OpenAI и т.д.)
/// </summary>
public interface IAiProvider
{
    /// <summary>
    /// Отправить запрос и получить текстовый ответ
    /// </summary>
    /// <param name="prompt">Текст запроса</param>
    /// <param name="options">Дополнительные опции (модель, температура, макс токенов)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<AiResponse> AskAsync(string prompt, AiOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить запрос и получить структурированный ответ (JSON)
    /// </summary>
    /// <typeparam name="T">Тип, в который десериализовать ответ</typeparam>
    /// <param name="prompt">Текст запроса</param>
    /// <param name="options">Дополнительные опции</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<AiResponse<T>> AskStructuredAsync<T>(string prompt, AiOptions? options = null, CancellationToken cancellationToken = default) where T : class;
}