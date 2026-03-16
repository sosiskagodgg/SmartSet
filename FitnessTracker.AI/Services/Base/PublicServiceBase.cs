// FitnessTracker.AI/Services/Base/PublicServiceBase.cs
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;

namespace FitnessTracker.AI.Services.Base;

/// <summary>
/// Базовый класс для публичных сервисов
/// Просто наследуйся и реализуй ExecuteInternalAsync
/// </summary>
public abstract class PublicServiceBase<TRequest, TResponse> : IPublicService<TRequest, TResponse>
{
    protected readonly ILogger Logger;
    public abstract string ServiceName { get; }

    protected PublicServiceBase(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Executing public service: {ServiceName}", ServiceName);
            var result = await ExecuteInternalAsync(request, cancellationToken);
            Logger.LogInformation("Service {ServiceName} completed", ServiceName);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in service {ServiceName}", ServiceName);
            throw;
        }
    }

    protected abstract Task<TResponse> ExecuteInternalAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Для сервисов без параметров
/// </summary>
public abstract class PublicServiceBase<TResponse> : PublicServiceBase<EmptyRequest, TResponse>
{
    protected PublicServiceBase(ILogger logger) : base(logger)
    {
    }

    public Task<TResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(new EmptyRequest(), cancellationToken);
    }
}