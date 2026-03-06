using FitnessTracker.AI.Commands.Base;
using FitnessTracker.AI.Configuration;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Orchestration;
using FitnessTracker.AI.Recognition.Classifiers;
using FitnessTracker.AI.Recognition.Recognizers;
using FitnessTracker.AI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.AI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFitnessTrackerAI(this IServiceCollection services, IConfiguration configuration)
    {
        // Конфигурация
        services.Configure<GigaChatConfig>(configuration.GetSection("GigaChat"));
        services.Configure<RecognitionConfig>(configuration.GetSection("Recognition"));

        // HTTP клиенты
        services.AddHttpClient<GigaChatEntityRecognizer>();

        // Токен сервис
        services.AddSingleton<IGigaChatTokenService, GigaChatTokenService>();

        // Registry
        services.AddSingleton<ICommandRegistry, CommandRegistryService>();

        services.AddScoped<IGroupClassifier, GroupClassifier>();
        services.AddHttpClient<GroupClassifier>();


        // Classifiers
        services.AddScoped<IIntentClassifier, CommandRegistryClassifier>();
        services.AddScoped<IIntentClassifier, GigaChatIntentClassifier>();
        services.AddScoped<IIntentClassifier, HelpClassifier>();

        // Recognizers
        services.AddScoped<IEntityRecognizer, RegexEntityRecognizer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RegexEntityRecognizer>>();
            return new RegexEntityRecognizer(logger);
        });
        services.AddScoped<IEntityRecognizer, GigaChatEntityRecognizer>();

        // Orchestration
        services.AddScoped<CommandOrchestrator>();
        services.AddScoped<RecognitionPipeline>();

        // АВТОМАТИЧЕСКИ РЕГИСТРИРУЕМ ВСЕ КОМАНДЫ
        services.AddAllCommandsFromAssembly(typeof(HelpCommand).Assembly);

        return services;
    }

    public static IServiceCollection AddCommand<T>(this IServiceCollection services)
        where T : class, ICommand
    {
        services.AddScoped<T>();
        services.AddScoped<ICommand, T>(sp => sp.GetRequiredService<T>());

        services.AddSingleton<Action<IServiceProvider>>(sp =>
        {
            var registry = sp.GetRequiredService<ICommandRegistry>();
            registry.Register<T>();

            var logger = sp.GetRequiredService<ILogger<ICommandRegistry>>();
            logger.LogInformation("✅ Команда {CommandName} зарегистрирована в реестре", typeof(T).Name);
        });

        return services;
    }

    public static IServiceCollection AddAllCommandsFromAssembly(
        this IServiceCollection services,
        System.Reflection.Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t))
            .ToList();

        // Временный логгер для начального вывода
        Console.WriteLine($"🔍 Найдено команд в сборке {assembly.GetName().Name}: {commandTypes.Count}");

        foreach (var commandType in commandTypes)
        {
            services.AddScoped(commandType);
            services.AddScoped(typeof(ICommand), commandType);
            Console.WriteLine($"  📦 Добавлена команда: {commandType.Name}");

            // Добавляем в реестр через Action
            services.AddSingleton<Action<IServiceProvider>>(sp =>
            {
                var registry = sp.GetRequiredService<ICommandRegistry>();

                // Используем рефлексию для вызова RegisterCommand
                var method = typeof(ICommandRegistry).GetMethod("RegisterCommand");
                if (method != null)
                {
                    method.Invoke(registry, new[] { commandType });
                }
                else
                {
                    // Если нет метода RegisterCommand, используем Register<T> через MakeGenericMethod
                    var registerMethod = typeof(ICommandRegistry).GetMethod("Register");
                    if (registerMethod != null)
                    {
                        var genericMethod = registerMethod.MakeGenericMethod(commandType);
                        genericMethod.Invoke(registry, null);
                    }
                }
            });
        }

        return services;
    }
}