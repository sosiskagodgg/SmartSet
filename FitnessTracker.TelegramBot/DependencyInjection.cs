using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Infrastructure;
using FitnessTracker.TelegramBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Telegram.Bot;
using static Telegram.Bot.TelegramBotClient;

namespace FitnessTracker.TelegramBot;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramBot(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ========== 1. Telegram API ==========
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = configuration["TelegramBot:Token"]
                ?? throw new InvalidOperationException("TelegramBot:Token not found in configuration");
            return new TelegramBotClient(token);
        });

        // Адаптер (единственная точка доступа к Telegram API)
        services.AddSingleton<ITelegramBotAdapter, TelegramBotAdapter>();

        // ========== 2. Core Services ==========
        services.AddSingleton<UserStateService>();     // Хранение состояний
        services.AddSingleton<UpdateRouter>();         // Роутинг апдейтов
        services.AddSingleton<UpdateHandler>();        // Обработчик апдейтов
        services.AddHostedService<BotService>();       // Фоновый сервис

        // ========== 3. Auto-register all handlers ==========
        RegisterHandlers(services);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Находим все классы-хендлеры
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) ||
                       typeof(ICallbackHandler).IsAssignableFrom(t) ||
                       typeof(IStateHandler).IsAssignableFrom(t))
            .ToList();

        foreach (var type in handlerTypes)
        {
            // Регистрируем как self (на всякий случай)
            services.AddSingleton(type);

            // Регистрируем по интерфейсам
            if (typeof(IMessageHandler).IsAssignableFrom(type))
                services.AddSingleton(typeof(IMessageHandler), type);

            if (typeof(ICallbackHandler).IsAssignableFrom(type))
                services.AddSingleton(typeof(ICallbackHandler), type);

            if (typeof(IStateHandler).IsAssignableFrom(type))
                services.AddSingleton(typeof(IStateHandler), type);
        }
    }
}