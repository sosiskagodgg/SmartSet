// FitnessTracker.ConsoleTest/Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FitnessTracker.Infrastructure;
using FitnessTracker.Application;
using FitnessTracker.AI;
using FitnessTracker.ConsoleTest.TestScenarios;
using FitnessTracker.ConsoleTest.TestHelpers;

namespace FitnessTracker.ConsoleTest;

class Program
{
    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════╗
║     FITNESS TRACKER - КОНСОЛЬНЫЙ ТЕСТЕР (БЕЗ TELEGRAM)      ║
╚══════════════════════════════════════════════════════════════╝
");
        Console.ResetColor();

        try
        {
            // Настраиваем DI контейнер
            var services = new ServiceCollection();
            var configuration = BuildConfiguration();

            ConfigureServices(services, configuration);

            var serviceProvider = services.BuildServiceProvider();

            // Создаем тестового пользователя
            var testUserId = 123456789L; // Фиксированный ID для тестов
            var userManager = new TestUserManager(serviceProvider);
            await userManager.EnsureTestUserExistsAsync(testUserId);

            // Запускаем главное меню
            var mainMenu = new MainMenu(serviceProvider, testUserId);
            await mainMenu.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
            Console.WriteLine($"📌 StackTrace: {ex.StackTrace}");
            Console.ResetColor();
        }

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }

    static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Добавляем логирование в консоль
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Ставим Warning чтобы меньше спама
        });

        // Наши слои
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddFitnessAI(configuration);

        // Регистрируем тестовые сценарии
        services.AddScoped<AiTestScenario>();
        services.AddScoped<UserParametersTestScenario>();
        services.AddScoped<WorkoutGenerationScenario>();    // НОВЫЙ
        services.AddScoped<WorkoutExecutionScenario>();     // НОВЫЙ
    }
}