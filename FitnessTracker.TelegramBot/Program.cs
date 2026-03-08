using FitnessTracker.AI.Extensions;
using FitnessTracker.Application;
using FitnessTracker.Infrastructure;
using FitnessTracker.TelegramBot.Abstractions;
using FitnessTracker.TelegramBot.Handlers.Base;
using FitnessTracker.TelegramBot.Infrastructure;
using FitnessTracker.TelegramBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Добавляем Infrastructure (репозитории, DbContext)
        services.AddInfrastructure(context.Configuration);

        // Добавляем Application (сервисы)
        services.AddApplication();

        services.AddFitnessTrackerAI(context.Configuration);

        // Telegram клиент
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = context.Configuration["TelegramBot:Token"]
                ?? throw new Exception("Token not found");
            return new TelegramBotClient(token);
        });

        // Адаптер
        services.AddSingleton<ITelegramBotAdapter, TelegramBotAdapter>();

        // Core сервисы Telegram бота
        services.AddSingleton<UserStateService>();
        services.AddSingleton<UpdateRouter>();
        services.AddSingleton<UpdateHandler>();
        services.AddHostedService<BotService>();

        // Регистрация хендлеров (оставляем как есть - они в этой же сборке)
        var assembly = typeof(Program).Assembly;
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