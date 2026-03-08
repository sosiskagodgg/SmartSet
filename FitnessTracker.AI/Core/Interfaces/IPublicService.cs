// FitnessTracker.AI.Core/Interfaces/IPublicService.cs

namespace FitnessTracker.AI.Core.Interfaces;

/// <summary>
/// Маркерный интерфейс для всех публичных сервисов
/// </summary>
public interface IPublicService
{
    string ServiceName { get; }
}

/// <summary>
/// Публичный сервис с запросом и ответом
/// </summary>
public interface IPublicService<in TRequest, TResponse> : IPublicService
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Публичный сервис без параметров
/// </summary>
public interface IPublicService<TResponse> : IPublicService<EmptyRequest, TResponse>
{
}

public class EmptyRequest { }