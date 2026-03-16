// FitnessTracker.TelegramBot/Program.cs
using FitnessTracker.AI;
using FitnessTracker.Application;
using FitnessTracker.Infrastructure;
using FitnessTracker.TelegramBot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("🚀 Запуск бота...");
Console.WriteLine($"🕒 {DateTime.Now:HH:mm:ss} - Начало инициализации");

try
{
    var builder = Host.CreateDefaultBuilder(args);

    builder.ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();
    });

    builder.ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddFitnessAI(configuration);
        services.AddTelegramBot(configuration);
    });

    Console.WriteLine("🏗️ Построение хоста...");
    var host = builder.Build();
    Console.WriteLine("✅ Хост построен");

    Console.WriteLine("\n🚀 Запуск хоста...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Критическая ошибка: {ex.Message}");
    Console.WriteLine($"📌 StackTrace: {ex.StackTrace}");
}

Console.WriteLine("\n🛑 Бот остановлен");
Console.ReadLine();