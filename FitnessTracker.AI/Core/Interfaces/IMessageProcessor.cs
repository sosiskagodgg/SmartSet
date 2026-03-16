// FitnessTracker.AI/Core/Interfaces/IMessageProcessor.cs
using FitnessTracker.AI.Core.Models;

namespace FitnessTracker.AI.Core.Interfaces;

/// <summary>
/// Главный процессор сообщений от пользователя
/// </summary>
public interface IMessageProcessor
{
    /// <summary>
    /// Обработать входящее сообщение
    /// </summary>
    /// <param name="userId">Telegram ID пользователя</param>
    /// <param name="message">Текст сообщения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<ProcessingResult> ProcessAsync(long userId, string message, CancellationToken cancellationToken = default);
}