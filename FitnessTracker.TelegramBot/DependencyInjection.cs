// FitnessTracker.TelegramBot/DependencyInjection.cs
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Infrastructure;
using FitnessTracker.TelegramBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace FitnessTracker.TelegramBot;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        // Настраиваем HttpClient с таймаутом
        services.AddHttpClient("telegram_bot")
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30); // Увеличиваем таймаут
            });

        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = configuration["TelegramBot:Token"]
                ?? throw new InvalidOperationException("TelegramBot:Token not found");

            var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("telegram_bot");

            var options = new TelegramBotClientOptions(token);
            return new TelegramBotClient(options, httpClient);
        });

        services.AddSingleton<ITelegramBotAdapter, TelegramBotAdapter>();
        services.AddSingleton<Bot>();
        services.AddSingleton<IUserStateManager>(sp => sp.GetRequiredService<Bot>());
        services.AddHostedService<BotBackgroundService>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(ICommand))
            .AddClasses(classes => classes.AssignableTo<ICommand>())
            .As<ICommand>()
            .WithSingletonLifetime());

        return services;
    }
}