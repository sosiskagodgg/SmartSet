// FitnessTracker.AI/DependencyInjection.cs (дополненная версия)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Registry;
using FitnessTracker.AI.Infrastructure.GigaChat;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.AI.Registry;
using FitnessTracker.AI.Services;

namespace FitnessTracker.AI;

/// <summary>
/// Регистрация всех сервисов AI-слоя.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddFitnessAI(this IServiceCollection services, IConfiguration configuration)
    {
        // Конфигурация GigaChat
        services.Configure<GigaChatConfig>(configuration.GetSection("GigaChat"));

        // HTTP-клиент для GigaChat
        services.AddHttpClient<GigaChatProvider>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });

        // Регистрируем AI провайдер
        services.AddScoped<IAiProvider, GigaChatProvider>();

        // Реестр плагинов
        services.AddSingleton<IPluginRegistry, PluginRegistry>();

        // Процессор сообщений
        services.AddScoped<IMessageProcessor, MessageProcessor>();

        // Регистрируем публичные сервисы
        services.AddScoped<QuestionsAIService>();
        services.AddScoped<UserParametersAIService>();
        services.AddScoped<WorkoutGenerationService>();

        // Автоматическая регистрация всех плагинов
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IFitnessPlugin))
            .AddClasses(classes => classes.AssignableTo<IFitnessPlugin>())
            .As<IFitnessPlugin>()
            .WithScopedLifetime());

        return services;
    }
}