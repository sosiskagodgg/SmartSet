using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.Application.Services;
using FitnessTracker.Application.Interfaces;

namespace FitnessTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Регистрируем сервисы приложения
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserParameterService, UserParameterService>(); // Добавить
                                                                           // Exercise services
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IMuscleService, MuscleService>();

        // Workout services
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<ISetService, SetService>();
        services.AddScoped<IWorkoutSessionService, WorkoutSessionService>();

        // Program services
        services.AddScoped<IProgramService, ProgramService>();

        // Statistics
        services.AddScoped<IStatisticsService, StatisticsService>();

        services.AddSingleton<IWorkoutTemplateService, WorkoutTemplateService>();
        return services;
    }
}