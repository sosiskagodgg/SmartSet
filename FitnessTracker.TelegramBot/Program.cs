using System.Reflection;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Infrastructure;
using FitnessTracker.TelegramBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Telegram клиент
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = context.Configuration["TelegramBot:Token"]
                ?? throw new Exception("Token not found");
            return new TelegramBotClient(token);
        });

        // Адаптер
        services.AddSingleton<ITelegramBotAdapter, TelegramBotAdapter>();

        // Core сервисы
        services.AddSingleton<UserStateService>();
        services.AddSingleton<UpdateRouter>();
        services.AddSingleton<UpdateHandler>();
        services.AddHostedService<BotService>();

        // Регистрация хендлеров
        var assembly = Assembly.GetExecutingAssembly();
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) ||
                       typeof(ICallbackHandler).IsAssignableFrom(t) ||
                       typeof(IStateHandler).IsAssignableFrom(t));

        foreach (var type in handlerTypes)
        {
            if (typeof(IMessageHandler).IsAssignableFrom(type))
                services.AddSingleton(typeof(IMessageHandler), type);
            if (typeof(ICallbackHandler).IsAssignableFrom(type))
                services.AddSingleton(typeof(ICallbackHandler), type);
            if (typeof(IStateHandler).IsAssignableFrom(type))
                services.AddSingleton(typeof(IStateHandler), type);
        }
    });

var host = builder.Build();


await host.RunAsync();