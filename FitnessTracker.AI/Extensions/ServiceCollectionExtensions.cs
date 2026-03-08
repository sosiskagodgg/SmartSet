using FitnessTracker.AI.Commands.Base;
using FitnessTracker.AI.Configuration;
using FitnessTracker.AI.Core.Attributes;
using FitnessTracker.AI.Core.Interfaces;
using FitnessTracker.AI.Core.Orchestration;
using FitnessTracker.AI.Core.Registry;
using FitnessTracker.AI.Core.Router;
using FitnessTracker.AI.PublicServices;
using FitnessTracker.AI.Recognition.Classifiers;
using FitnessTracker.AI.Recognition.Recognizers;
using FitnessTracker.AI.Services;
using FitnessTracker.AI.Services.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FitnessTracker.AI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFitnessTrackerAI(this IServiceCollection services, IConfiguration configuration)
    {
        // Конфигурация
        services.Configure<GigaChatConfig>(configuration.GetSection("GigaChat"));
        services.Configure<RecognitionConfig>(configuration.GetSection("Recognition"));

        // HTTP клиенты
        services.AddScoped<GigaChatEntityRecognizer>();

        // Токен сервис
        services.AddSingleton<IGigaChatTokenService, GigaChatTokenService>();

        // Registry
        services.AddSingleton<ICommandRegistry, CommandRegistryService>();

        services.AddScoped<IGroupClassifier, GroupClassifier>();
        services.AddHttpClient<GroupClassifier>();
        services.AddScoped<FitnessAIRouter>();

        // Автоматически регистрируем публичные сервисы
        services.AddPublicServices(typeof(PublicServiceBase<>).Assembly);

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


        services.AddSingleton<DirectionRegistry>();

        // Регистрируем классификатор
        services.AddScoped<DirectionClassifier>();
        services.AddScoped<UserParametersAIService>();

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
    /// <summary>
    /// Автоматически регистрирует все публичные сервисы из сборки
    /// </summary>
    public static IServiceCollection AddPublicServices(this IServiceCollection services, System.Reflection.Assembly assembly)
    {
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                   t.GetInterfaces().Any(i =>
                       i.IsGenericType &&
                       i.GetGenericTypeDefinition() == typeof(IPublicService<,>)))
            .ToList();

        foreach (var type in serviceTypes)
        {
            services.AddScoped(type);

            // Регистрируем по интерфейсу
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType &&
                       i.GetGenericTypeDefinition() == typeof(IPublicService<,>));

            foreach (var iface in interfaces)
            {
                services.AddScoped(iface, type);
            }

            var attribute = type.GetCustomAttribute<PublicServiceAttribute>();
            var serviceName = attribute?.Name ?? type.Name.Replace("Service", "");

            Console.WriteLine($"✅ Public service registered: {serviceName} -> {type.Name}");
        }

        return services;
    }
}