// FitnessTracker.AI.Core/Router/FitnessAIRouter.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Models;
using FitnessTracker.AI.Core.Orchestration;
namespace FitnessTracker.AI.Core.Router;

/// <summary>
/// Простой роутер - использует твою существующую систему
/// </summary>
public class FitnessAIRouter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FitnessAIRouter> _logger;

    public FitnessAIRouter(
        IServiceProvider serviceProvider,
        ILogger<FitnessAIRouter> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Режим 1: AI сам решает что делать (использует твой CommandOrchestrator)
    /// </summary>
    public async Task<CommandResult> ProcessWithAIAsync(
        string message,
        long userId,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<CommandOrchestrator>();

        return await orchestrator.ProcessAsync(message, userId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Режим 2: Прямой вызов публичного сервиса (когда знаешь что хочешь)
    /// </summary>
    public async Task<TResponse> ExecuteServiceAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        using var scope = _serviceProvider.CreateScope();

        // Ищем сервис по имени
        var serviceType = FindServiceType(serviceName);
        if (serviceType == null)
        {
            throw new InvalidOperationException($"Service '{serviceName}' not found");
        }

        var service = scope.ServiceProvider.GetRequiredService(serviceType);

        // Вызываем ExecuteAsync через рефлексию (один раз при первом вызове)
        var method = serviceType.GetMethod("ExecuteAsync");
        if (method == null)
        {
            throw new InvalidOperationException($"Service '{serviceName}' has no ExecuteAsync method");
        }

        var result = await (Task<TResponse>)method.Invoke(service, new object[] { request, cancellationToken });

        return result;
    }

    /// <summary>
    /// Упрощенный вызов сервиса без параметров
    /// </summary>
    public async Task<TResponse> ExecuteServiceAsync<TResponse>(
        string serviceName,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await ExecuteServiceAsync<EmptyRequest, TResponse>(serviceName, new EmptyRequest(), cancellationToken);
    }

    private Type? FindServiceType(string serviceName)
    {
        // Ищем во всех зарегистрированных сервисах, которые реализуют IPublicService
        var publicServices = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IPublicService).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in publicServices)
        {
            // Пытаемся создать экземпляр чтобы получить ServiceName (не идеально, но для простоты)
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var instance = scope.ServiceProvider.GetService(type) as IPublicService;
                if (instance?.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return type;
                }
            }
            catch
            {
                // Игнорируем, пробуем следующий
            }
        }

        return null;
    }
}