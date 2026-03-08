using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Application.Services;

namespace FitnessTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Ручная регистрация ВСЕХ сервисов
        RegisterServices(services);

        return services;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Явно и понятно: каждый сервис регистрируем руками
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IUserWorkoutService, UserWorkoutService>();
        services.AddScoped <IUserParametersService, UserParametersService>();
        // В FitnessTracker.TelegramBot/Program.cs или DependencyInjection.cs
        services.AddScoped<UserRegistrationService>();
        // Добавляй сюда новые сервисы по мере создания
        // services.AddScoped<IExerciseService, ExerciseService>();
        // services.AddScoped<IMuscleService, MuscleService>();
        // services.AddScoped<IProgramService, ProgramService>();
        // services.AddScoped<IStatisticsService, StatisticsService>();
    }
}