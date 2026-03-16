// FitnessTracker.AI/Core/Interfaces/IPublicService.cs
namespace FitnessTracker.AI.Core.Interfaces;

/// <summary>
/// Маркерный интерфейс для всех публичных сервисов
/// </summary>
public interface IPublicService
{
    /// <summary>
    /// Имя сервиса (уникальное)
    /// </summary>
    string ServiceName { get; }
}

/// <summary>
/// Публичный сервис с запросом и ответом
/// </summary>
public interface IPublicService<in TRequest, TResponse> : IPublicService
{
    /// <summary>
    /// Выполнить сервис
    /// </summary>
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Публичный сервис без параметров
/// </summary>
public interface IPublicService<TResponse> : IPublicService<EmptyRequest, TResponse>
{
}

/// <summary>
/// Пустой запрос для сервисов без параметров
/// </summary>
public record EmptyRequest();