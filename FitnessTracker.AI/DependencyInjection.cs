// FitnessTracker.AI/DependencyInjection.cs

using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Registry;
using FitnessTracker.AI.Infrastructure.GigaChat;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.AI.Registry;
using FitnessTracker.AI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessTracker.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddFitnessAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GigaChatConfig>(configuration.GetSection("GigaChat"));

        // Регистрируем handler как Transient
        services.AddTransient<GigaChatHttpClientHandler>();

        // ЯВНО указываем базовый адрес для токенов
        services.AddHttpClient<GigaChatProvider>("gigachat_token", client =>
        {
            client.BaseAddress = new Uri("https://ngw.devices.sberbank.ru:9443/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler<GigaChatHttpClientHandler>();

        // Основной клиент для API
        services.AddHttpClient<GigaChatProvider>("gigachat_api", client =>
        {
            client.BaseAddress = new Uri("https://gigachat.devices.sberbank.ru/api/v1/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .ConfigurePrimaryHttpMessageHandler<GigaChatHttpClientHandler>();

        services.AddScoped<IAiProvider>(sp =>
        {
            var tokenClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("gigachat_token");
            var apiClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("gigachat_api");
            var config = sp.GetRequiredService<IOptions<GigaChatConfig>>();
            var logger = sp.GetRequiredService<ILogger<GigaChatProvider>>();

            return new GigaChatProvider(config, logger, tokenClient, apiClient);
        });

        // РЕГИСТРАЦИЯ ПУБЛИЧНЫХ СЕРВИСОВ
        services.AddScoped<QuestionsAIService>();
        services.AddScoped<UserParametersAIService>();
        services.AddScoped<WorkoutGenerationService>();

        // Регистрация процессора сообщений и реестра плагинов
        services.AddSingleton<IPluginRegistry, PluginRegistry>();
        services.AddScoped<IMessageProcessor, MessageProcessor>();



        return services;
    }
}